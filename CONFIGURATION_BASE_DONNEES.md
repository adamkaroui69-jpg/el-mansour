# 🔄 Configuration Base de Données - El Mansour Syndic Manager

## 📊 Comprendre les Deux Modes

L'application peut fonctionner en **deux modes différents**:

### Mode 1: SQLite Local (Chaque PC a sa propre base de données)
- ✅ **Avantage**: Fonctionne sans internet
- ❌ **Inconvénient**: Les données ne sont PAS partagées entre les PCs
- 📁 **Fichier**: `data\local.db` (local sur chaque PC)

### Mode 2: SQL Server Distant (Tous les PCs partagent la même base de données)
- ✅ **Avantage**: Données synchronisées sur tous les PCs
- ✅ **Avantage**: Plusieurs utilisateurs peuvent travailler ensemble
- ❌ **Inconvénient**: Nécessite une connexion internet
- 🌐 **Serveur**: ElMansourDB.mssql.somee.com

---

## ⚙️ Configuration Actuelle

**Mode activé**: **SQL Server Distant** ✅

Tous les PCs qui installent l'application vont:
- Se connecter au serveur distant `ElMansourDB.mssql.somee.com`
- Partager les mêmes données (paiements, maisons, dépenses, etc.)
- Voir les modifications en temps réel

---

## 🔧 Comment Changer de Mode

### Pour Passer en Mode SQLite Local:

1. Ouvrir le fichier: `C:\Program Files\ElMansourSyndicManager\appsettings.json`
2. Modifier la ligne:
   ```json
   "DatabaseProvider": "SqlServer"
   ```
   En:
   ```json
   "DatabaseProvider": "Sqlite"
   ```
3. Redémarrer l'application

### Pour Passer en Mode SQL Server Distant:

1. Ouvrir le fichier: `C:\Program Files\ElMansourSyndicManager\appsettings.json`
2. Modifier la ligne:
   ```json
   "DatabaseProvider": "Sqlite"
   ```
   En:
   ```json
   "DatabaseProvider": "SqlServer"
   ```
3. Redémarrer l'application

---

## 📋 Informations de Connexion SQL Server

**Serveur**: ElMansourDB.mssql.somee.com  
**Base de données**: ElMansourDB  
**Utilisateur**: adamos666_SQLLogin_1  
**Mot de passe**: 5kyk7ensh8  

⚠️ **Important**: Ces informations sont déjà configurées dans l'application.

---

## 🎯 Recommandation

Pour votre cas d'utilisation (plusieurs PCs):

✅ **Utilisez SQL Server Distant** (configuration actuelle)

**Pourquoi?**
- Tous les PCs voient les mêmes données
- Un utilisateur ajoute un paiement → visible sur tous les PCs
- Pas de duplication de données
- Gestion centralisée

---

## 🔍 Vérifier le Mode Actif

Pour savoir quel mode est actif:

1. Aller dans: `C:\Program Files\ElMansourSyndicManager\`
2. Ouvrir: `appsettings.json`
3. Regarder la ligne `"DatabaseProvider"`:
   - `"SqlServer"` = Mode distant (partagé)
   - `"Sqlite"` = Mode local (isolé)

---

## 🚀 Prochaines Étapes

1. **Recompiler l'installateur** avec Inno Setup
2. **Installer sur tous les PCs**
3. **Tous les PCs** se connecteront automatiquement au serveur SQL Server
4. **Les données seront partagées** entre tous les utilisateurs

---

## ⚠️ Notes Importantes

### Connexion Internet Requise
- Les PCs doivent avoir accès à internet pour se connecter au serveur
- Si internet est coupé, l'application ne fonctionnera pas

### Limites du Serveur Gratuit (somee.com)
- Peut avoir des limitations de connexions simultanées
- Peut être lent parfois
- Pour une utilisation professionnelle intensive, envisager un serveur payant

### Sécurité
- Les identifiants SQL sont dans le fichier `appsettings.json`
- Pour plus de sécurité, utiliser des variables d'environnement
- Changer le mot de passe régulièrement

---

**Date**: 23 novembre 2025  
**Version**: 1.0.0  
**Mode actuel**: SQL Server Distant ✅
