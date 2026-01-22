# 🔄 Guide de Synchronisation Réseau (Multi-Postes)

Ce guide explique comment configurer El Mansour Syndic Manager pour que plusieurs ordinateurs partagent les mêmes données.

## 🛑 Le Problème Actuel
Par défaut, l'application stocke ses données **localement** sur le disque dur de chaque ordinateur.
- Ordinateur A : Base de données A (60 TND)
- Ordinateur B : Base de données B (3000 TND)

C'est pourquoi vous ne voyez pas les mêmes montants.

## ✅ La Solution : Base de Données Partagée
Pour que tout le monde voie la même chose, tous les ordinateurs doivent pointer vers le **même fichier** de base de données, situé sur un dossier partagé.

### Étape 1 : Préparer le Dossier Partagé
1. Sur l'ordinateur principal (celui qui a les données les plus complètes, ex: Ordinateur B), créez un dossier, par exemple `C:\SyndicData`.
2. Faites un clic droit sur ce dossier > **Propriétés** > **Partage**.
3. Partagez ce dossier avec les autres utilisateurs (Lecture/Écriture).
4. Notez le chemin réseau, ex: `\\ORDI-PRINCIPAL\SyndicData`.

### Étape 2 : Déplacer la Base de Données
1. Sur l'ordinateur B (celui avec les 3000 TND), allez dans `%AppData%\ElMansourSyndic\data` (tapez `%AppData%` dans la barre d'adresse de l'explorateur).
2. Copiez le fichier `local.db` (et `local.db-shm`, local.db-wal` si présents).
3. Collez ces fichiers dans le dossier partagé réseau (`\\ORDI-PRINCIPAL\SyndicData`).

### Étape 3 : Configurer Chaque Ordinateur
Sur **CHAQUE** ordinateur (y compris le principal) :

1. Allez dans le dossier d'installation de l'application (ex: `C:\Program Files\ElMansourSyndicManager`).
2. Ouvrez le fichier `appsettings.json` avec le Bloc-notes (en administrateur).
3. Modifiez la ligne `DefaultConnection` pour pointer vers le fichier partagé.

**Avant :**
```json
"ConnectionStrings": {
  "DefaultConnection": "Data Source=data/local.db"
}
```

**Après :**
```json
"ConnectionStrings": {
  "DefaultConnection": "Data Source=\\\\ORDI-PRINCIPAL\\SyndicData\\local.db"
}
```
*(Notez les doubles backslashes `\\` qui sont nécessaires en JSON)*

4. Enregistrez et relancez l'application.
5. Allez dans **Paramètres** > **Base de données** pour vérifier que le chemin est correct.

## ⚠️ Important
- Tous les PC doivent être connectés au même réseau.
- SQLite supporte l'accès concurrent, mais pour plus de 5 utilisateurs simultanés, envisagez SQL Server.
- Faites des sauvegardes régulières du fichier `local.db`.
