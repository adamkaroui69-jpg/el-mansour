# ☁️ Guide de Synchronisation Cloud avec SUPABASE (PostgreSQL)

Pour synchroniser vos données gratuitement et en temps réel, nous recommandons **Supabase** (PostgreSQL).

## Étape 1 : Créer le Projet Supabase
1. Allez sur [database.new](https://database.new) (Connectez-vous avec GitHub).
2. Créez un nouveau projet (New Project).
   - **Name** : ElMansourSyndic
   - **Password** : Choisissez un mot de passe fort et NOTEZ-LE (vous en aurez besoin).
   - **Region** : Choisissez le plus proche (ex: Frankfurt ou Paris).
3. Attendez ~2 minutes que le projet soit vert ("Active").

## Étape 2 : Récupérer la Chaîne de Connexion
1. Dans votre tableau de bord Supabase, allez dans **Project Settings** (roue dentée en bas à gauche) > **Database**.
2. Cherchez la section **Connection String**.
3. Cliquez sur l'onglet **.NET** (ou URI) et copiez la valeur.
4. **Remplacez `[YOUR-PASSWORD]` par le mot de passe créé à l'étape 1.**

Elle doit ressembler à ceci :
`User Id=postgres.xxxx;Password=MonMotDePasse;Server=aws-0-eu-central-1.pooler.supabase.com;Port=5432;Database=postgres;`

## Étape 3 : Configurer l'Application

1. Allez dans le dossier d'installation de l'application.
2. Ouvrez le fichier `appsettings.json` avec le Bloc-notes (Admin).
3. Modifiez comme suit :

```json
{
  "DatabaseProvider": "PostgreSQL",
  "ConnectionStrings": {
    "DefaultConnection": "User Id=postgres.votre_id;Password=votre_mot_de_passe;Server=votre_serveur.pooler.supabase.com;Port=5432;Database=postgres;"
  },
  ...
}
```

## Étape 4 : Lancement
Lancez l'application. Elle va se connecter à Supabase et y créer les tables. C'est prêt !

## ⚠️ Migration des anciennes données
Si vous avez un fichier `local.db` (SQLite) avec des données importantes :
1. L'application ne transfère PAS automatiquement les données locales vers le Cloud.
2. Vous démarrez avec une base vide sur le Cloud.

Si vous avez besoin de récupérer vos anciennes données, contactez le support pour une migration manuelle.
