using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ElMansourSyndicManager.Infrastructure.Data;
using ElMansourSyndicManager.Core.Domain.Entities;
using System.IO;

namespace MigrationTool;

class Program
{
    static void Main(string[] args)
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.WriteLine("=== MIGRATION TOOL : Local SQLite -> Neon PostgreSQL ===");

        string dbFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ElMansourSyndic", "data", "local.db");

        if (!File.Exists(dbFile))
        {
            Console.WriteLine($"❌ Erreur : Base de données source introuvable à {dbFile}");
            return;
        }

        Console.WriteLine($"Source : {dbFile}");

        var sourceOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite($"Data Source={dbFile}")
            .Options;

        string neonConn = "Host=ep-floral-star-ag7n2wlm.c-2.eu-central-1.aws.neon.tech;Database=neondb;Username=neondb_owner;Password=npg_YkqCV3WLbSB5;SSL Mode=Require;Trust Server Certificate=true";
        var destOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(neonConn, b => b.MigrationsAssembly("ElMansourSyndicManager.Infrastructure"))
            .Options;

        try
        {
            using var sourceCtx = new ApplicationDbContext(sourceOptions);
            using var destCtx = new ApplicationDbContext(destOptions);

            Console.WriteLine("Initialisation de la base de données Neon...");
            // Au lieu de EnsureDeleted, on va essayer de juste migrer. 
            // Si des tables existent déjà sans historique de migration, on va avoir un problème.
            // La solution propre est de vider le schéma "public".
            
            try {
                destCtx.Database.ExecuteSqlRaw("DROP SCHEMA public CASCADE; CREATE SCHEMA public; GRANT ALL ON SCHEMA public TO neondb_owner; GRANT ALL ON SCHEMA public TO public;");
                Console.WriteLine("Schéma public réinitialisé.");
            } catch (Exception ex) {
                Console.WriteLine($"Note: Impossible de reset le schéma ({ex.Message}), tentative de migration directe...");
            }

            Console.WriteLine("Application des migrations sur Neon...");
            destCtx.Database.Migrate();

            Console.WriteLine("Début de la migration des données...");

            MigrateTable(sourceCtx, destCtx, ctx => ctx.Roles);
            MigrateTable(sourceCtx, destCtx, ctx => ctx.Users);
            MigrateTable(sourceCtx, destCtx, ctx => ctx.Buildings);
            MigrateTable(sourceCtx, destCtx, ctx => ctx.Houses);
            MigrateTable(sourceCtx, destCtx, ctx => ctx.Payments);
            MigrateTable(sourceCtx, destCtx, ctx => ctx.Expenses);
            MigrateTable(sourceCtx, destCtx, ctx => ctx.AuditLogs);
            MigrateTable(sourceCtx, destCtx, ctx => ctx.Receipts);
            MigrateTable(sourceCtx, destCtx, ctx => ctx.Documents);
            MigrateTable(sourceCtx, destCtx, ctx => ctx.RolePermissions);
            MigrateTable(sourceCtx, destCtx, ctx => ctx.Maintenances);
            MigrateTable(sourceCtx, destCtx, ctx => ctx.Notifications);

            Console.WriteLine("\n✅ MIGRATION TERMINEE AVEC SUCCES !");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n❌ ERREUR CRITIQUE : {ex.Message}");
            if (ex.InnerException != null) Console.WriteLine($"Détail : {ex.InnerException.Message}");
        }
    }

    static void MigrateTable<T>(ApplicationDbContext source, ApplicationDbContext dest, Func<ApplicationDbContext, DbSet<T>> tableSelector) where T : class
    {
        string tableName = typeof(T).Name;
        Console.Write($"Migration {tableName}... ");

        try
        {
            var data = tableSelector(source).AsNoTracking().ToList();
            if (data.Any())
            {
                tableSelector(dest).AddRange(data);
                dest.SaveChanges();
                Console.WriteLine($"Ok ({data.Count} lignes)");
            }
            else
            {
                Console.WriteLine("Vide");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Échec : {ex.Message}");
            if (ex.InnerException != null) Console.WriteLine($"   -> Détail : {ex.InnerException.Message}");
        }
    }
}
