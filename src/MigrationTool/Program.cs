using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ElMansourSyndicManager.Infrastructure.Data;
using ElMansourSyndicManager.Core.Domain.Entities;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("=== MIGRATION TOOL : Local SQLite -> Supabase PostgreSQL ===");

        // Utiliser le fichier local trouvé
        string dbFile = @"C:\Users\adamk\Desktop\raisidance application\data\local.db";
        
        if (!File.Exists(dbFile))
        {
             // Fallback
             dbFile = @"C:\Users\adamk\Desktop\raisidance application\src\ElMansourSyndicManager\bin\Debug\net8.0-windows\data\local.db";
        }

        if (!File.Exists(dbFile))
        {
            Console.WriteLine("ERREUR: Impossible de trouver local.db");
            return;
        }

        Console.WriteLine($"Base de données source : {dbFile}");
        // PasserDirectement à la suite sans ZIP

        // 2. Connexion Source (SQLite)
        var sourceOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite($"Data Source={dbFile}")
            .Options;

        // 3. Connexion Destination (PostgreSQL Supabase)
        string pgConnectionString = "Host=db.kawppmcjxxbcosfyitfx.supabase.co;Port=5432;Database=postgres;Username=postgres;Password=sykcYExRhdnrjgQk;Include Error Detail=true";
        var destOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(pgConnectionString)
            .Options;

        using var sourceCtx = new ApplicationDbContext(sourceOptions);
        using var destCtx = new ApplicationDbContext(destOptions);

        Console.WriteLine("Connexion à Supabase...");
        destCtx.Database.EnsureCreated(); // S'assurer que les tables existent

        Console.WriteLine("Début de la migration des données...");

        MigrateTable(sourceCtx, destCtx, ctx => ctx.Roles);
        MigrateTable(sourceCtx, destCtx, ctx => ctx.Users);
        MigrateTable(sourceCtx, destCtx, ctx => ctx.Buildings);
        MigrateTable(sourceCtx, destCtx, ctx => ctx.Houses);
        MigrateTable(sourceCtx, destCtx, ctx => ctx.Payments);
        MigrateTable(sourceCtx, destCtx, ctx => ctx.Expenses);
        // Ajoutez d'autres tables si nécessaire (Receipts, Notifications, etc.)

        Console.WriteLine("Migration Terminée avec succès !");
    }

    static void MigrateTable<T>(ApplicationDbContext source, ApplicationDbContext dest, Func<ApplicationDbContext, DbSet<T>> tableSelector) where T : class
    {
        var tableName = typeof(T).Name;
        Console.Write($"Migration de {tableName}... ");
        try
        {
            var data = tableSelector(source).AsNoTracking().ToList();
            if (data.Any())
            {
                var destSet = tableSelector(dest);
                
                // Vérifier les doublons pour éviter les erreurs (basique, par ID si possible)
                // Ici on suppose une base vide ou on fait un AddRange simple
                // Pour éviter les conflits d'ID auto-incrément, sur Postgres il faut parfois reset les séqences, 
                // mais si on insère des IDs explicites, EF le gère généralement bien.
                
                foreach (var item in data)
                {
                    // Check if exists (optionnel, lent mais sûr)
                    // var exists = destSet.Find(GetKey(item)); 
                    // Pour l'instant on bourrine :
                    destSet.Add(item);
                }
                
                dest.SaveChanges();
                Console.WriteLine($"OK ({data.Count} enregistrements)");
            }
            else
            {
                Console.WriteLine("Vide (Ignoré)");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERREUR: {ex.Message}");
            if (ex.InnerException != null) Console.WriteLine($"  Detail: {ex.InnerException.Message}");
        }
    }
}
