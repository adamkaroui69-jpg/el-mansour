# ☁️ Guide de Synchronisation Cloud (Internet)

Pour synchroniser vos données automatiquement via Internet entre plusieurs utilisateurs, vous devez passer d'une base de données locale (SQLite) à une **base de données centralisée (SQL Server)**.

L'application supporte désormais nativement SQL Server.

## Étape 1 : Obtenir une Base de Données SQL Server
Vous avez besoin d'un hébergement pour votre base de données. Voici quelques options :

### Option A : Azure SQL (Recommandé)
- **Coût** : ~5€/mois (Basic DTU) ou Gratuit pendant 12 mois.
- **Fiabilité** : Maximale (Microsoft).
1. Créez un compte sur [Azure Portal](https://portal.azure.com).
2. Créez une ressource "SQL Database".
3. Configurez le serveur et notez la **Chaîne de connexion (Connection String)**.

### Option B : Hébergement Web Standard (ex: Plesk/cPanel)
Si vous avez déjà un site web, votre hébergeur propose peut-être des bases de données MS SQL.
1. Créez une base MSSQL dans votre panel.
2. Notez l'adresse du serveur, le nom de la base, l'utilisateur et le mot de passe.

---

## Étape 2 : Configurer l'Application

Une fois que vous avez votre chaîne de connexion (qui ressemble à ceci : `Server=tcp:monserveur.database.windows.net;Database=mabase;User ID=monuser;Password=monpassword;Encrypt=true;`), vous devez configurer chaque PC.

1. Allez dans le dossier d'installation de l'application (ex: `C:\Program Files\ElMansourSyndicManager`).
2. Ouvrez le fichier `appsettings.json` avec le Bloc-notes (en administrateur).
3. Modifiez deux lignes :
   - Changez `DatabaseProvider` de `"Sqlite"` à `"SqlServer"`.
   - Remplacez `DefaultConnection` par votre chaîne de connexion Cloud.

**Exemple de fichier configuré :**
```json
{
  "DatabaseProvider": "SqlServer",
  "ConnectionStrings": {
    "DefaultConnection": "Server=tcp:monserveur-syndic.database.windows.net,1433;Initial Catalog=ElMansourDB;Persist Security Info=False;User ID=admin-syndic;Password=MotDePasseComplexe123!;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
  },
  ...
}
```

## Étape 3 : Premier Lancement
Au premier lancement avec la nouvelle configuration, l'application va automatiquement créer les tables dans votre base de données Cloud vide.

⚠️ **Important : Migration des Données**
Si vous avez déjà des données en local (SQLite) que vous voulez envoyer sur le Cloud, cette opération n'est pas automatique.
Vous devrez resaisir les données ou contacter le développeur pour un script de migration.

## Résultat
Une fois configuré sur tous les PC :
- Quand l'utilisateur A ajoute un paiement, l'utilisateur B le voit instantanément (après rafraîchissement).
- Plus de conflits de fichiers.
- Vos données sont sécurisées dans le Cloud.
