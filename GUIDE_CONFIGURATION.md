# 🔧 Configuration de l'Application - El Mansour Syndic Manager

## 📋 **Vue d'Ensemble**

Ce fichier `appsettings.json` contient toute la configuration de l'application.  
**Vous pouvez modifier ces paramètres sans toucher au code source.**

---

## 📂 **Structure du Fichier**

```json
{
  "DatabaseProvider": "Sqlite",
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=data/local.db"
  },
  "ApplicationSettings": {
    "DataDirectory": "data",
    "DocumentsDirectory": "data/documents",
    "BackupsDirectory": "data/backups",
    "LogsDirectory": "data/logs",
    "MaxBackupCount": 10,
    "AutoBackupEnabled": true,
    "AutoBackupIntervalHours": 24
  },
  "CompanyInfo": {
    "Name": "El Mansour Syndic",
    "Address": "",
    "Phone": "",
    "Email": ""
  }
}
```

---

## 🗄️ **Section 1 : Base de Données**

### **DatabaseProvider**
- **Valeur actuelle** : `"Sqlite"`
- **Description** : Type de base de données utilisée
- **Options** : `"Sqlite"` (recommandé pour usage local)
- **Ne pas modifier** sauf si vous savez ce que vous faites

### **ConnectionStrings.DefaultConnection**
- **Valeur actuelle** : `"Data Source=data/local.db"`
- **Description** : Chemin vers le fichier de base de données
- **Chemins possibles** :
  - `"Data Source=data/local.db"` → Relatif (dans le dossier de l'application)
  - `"Data Source=C:/MesDocuments/syndic.db"` → Absolu (chemin complet)
  - `"Data Source=D:/Backup/database.db"` → Sur un autre disque

**💡 Conseil** : Utilisez un chemin relatif pour faciliter le déplacement de l'application.

---

## 📁 **Section 2 : Répertoires de l'Application**

### **DataDirectory**
- **Valeur** : `"data"`
- **Description** : Dossier principal pour toutes les données
- **Créé automatiquement** : ✅ Oui

### **DocumentsDirectory**
- **Valeur** : `"data/documents"`
- **Description** : Stockage des fichiers uploadés (contrats, PV, factures)
- **Créé automatiquement** : ✅ Oui

### **BackupsDirectory**
- **Valeur** : `"data/backups"`
- **Description** : Sauvegardes automatiques de la base de données
- **Créé automatiquement** : ✅ Oui

### **LogsDirectory**
- **Valeur** : `"data/logs"`
- **Description** : Fichiers de journalisation (logs)
- **Créé automatiquement** : ✅ Oui

**💡 Conseil** : Vous pouvez utiliser des chemins absolus si vous voulez stocker les données ailleurs :
```json
"DocumentsDirectory": "D:/SyndicDocuments",
"BackupsDirectory": "E:/Backups/Syndic"
```

---

## 💾 **Section 3 : Paramètres de Sauvegarde**

### **MaxBackupCount**
- **Valeur** : `10`
- **Description** : Nombre maximum de sauvegardes à conserver
- **Comportement** : Les plus anciennes sont supprimées automatiquement
- **Recommandation** : Entre 5 et 20

### **AutoBackupEnabled**
- **Valeur** : `true`
- **Description** : Active/désactive les sauvegardes automatiques
- **Options** :
  - `true` → Sauvegardes automatiques activées
  - `false` → Sauvegardes manuelles uniquement

### **AutoBackupIntervalHours**
- **Valeur** : `24`
- **Description** : Intervalle entre deux sauvegardes automatiques (en heures)
- **Exemples** :
  - `24` → Une fois par jour
  - `12` → Deux fois par jour
  - `168` → Une fois par semaine

---

## 🏢 **Section 4 : Informations de l'Entreprise**

### **CompanyInfo.Name**
- **Valeur** : `"El Mansour Syndic"`
- **Description** : Nom de votre syndic
- **Utilisé dans** : Reçus, rapports, en-têtes

### **CompanyInfo.Address**
- **Valeur** : `""` (vide par défaut)
- **Description** : Adresse complète du syndic
- **Exemple** : `"123 Avenue Mohammed V, Tunis 1000"`

### **CompanyInfo.Phone**
- **Valeur** : `""` (vide par défaut)
- **Description** : Numéro de téléphone
- **Exemple** : `"+216 71 123 456"`

### **CompanyInfo.Email**
- **Valeur** : `""` (vide par défaut)
- **Description** : Email de contact
- **Exemple** : `"contact@elmansour-syndic.tn"`

---

## 🚀 **Scénarios d'Utilisation**

### **Scénario 1 : Installation sur un nouveau PC**

1. Copiez le dossier complet de l'application
2. Lancez l'application → Les dossiers seront créés automatiquement
3. (Optionnel) Modifiez `appsettings.json` pour personnaliser les chemins

### **Scénario 2 : Déplacer les données sur un autre disque**

```json
{
  "ApplicationSettings": {
    "DataDirectory": "D:/SyndicData",
    "DocumentsDirectory": "D:/SyndicData/documents",
    "BackupsDirectory": "E:/Backups",
    "LogsDirectory": "D:/SyndicData/logs"
  }
}
```

### **Scénario 3 : Partage réseau (plusieurs PC)**

**⚠️ Important** : SQLite ne supporte pas les accès concurrents sur réseau.  
Pour plusieurs utilisateurs simultanés, contactez le support pour une migration vers SQL Server.

**Solution temporaire** : Utilisez un PC comme "serveur" et partagez les sauvegardes.

### **Scénario 4 : Sauvegarde sur clé USB**

```json
{
  "ApplicationSettings": {
    "BackupsDirectory": "F:/SyndicBackups"
  }
}
```

---

## 🔒 **Sécurité et Bonnes Pratiques**

### ✅ **À FAIRE**
- Sauvegarder régulièrement le fichier `appsettings.json`
- Tester les modifications sur une copie de l'application
- Conserver au moins 10 sauvegardes
- Stocker les backups sur un disque externe

### ❌ **À NE PAS FAIRE**
- Modifier `DatabaseProvider` sans savoir ce que vous faites
- Supprimer des sections entières du fichier
- Utiliser des caractères spéciaux dans les chemins (éviter : `é`, `à`, etc.)
- Partager la base de données SQLite sur un réseau

---

## 🆘 **Dépannage**

### **Problème : L'application ne démarre pas**
**Solution** : Vérifiez que `appsettings.json` est bien formaté (JSON valide)

### **Problème : Base de données introuvable**
**Solution** : Vérifiez le chemin dans `ConnectionStrings.DefaultConnection`

### **Problème : Erreur d'accès aux fichiers**
**Solution** : Vérifiez les permissions du dossier `data/`

### **Problème : Sauvegardes non créées**
**Solution** : Vérifiez que `AutoBackupEnabled` est à `true`

---

## 📞 **Support**

Pour toute question ou problème :
1. Consultez ce guide
2. Vérifiez les logs dans `data/logs/`
3. Contactez le support technique

---

**Version** : 2.0  
**Dernière mise à jour** : 2026-01-15
