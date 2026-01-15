# 📦 Guide de Déploiement - El Mansour Syndic Manager

## 🎯 **Objectif**
Permettre l'installation et l'utilisation de l'application sur **plusieurs PC** sans modification du code.

---

## 📋 **Prérequis**

### **Sur chaque PC :**
- ✅ Windows 10/11
- ✅ .NET 8.0 Runtime (téléchargeable gratuitement)
- ✅ 100 MB d'espace disque minimum
- ✅ Droits d'écriture dans le dossier d'installation

---

## 🚀 **Installation sur un Nouveau PC**

### **Méthode 1 : Installation Propre (Premier PC)**

1. **Copier le dossier complet** de l'application
   ```
   ElMansourSyndicManager/
   ├── ElMansourSyndicManager.exe
   ├── appsettings.json
   ├── *.dll (tous les fichiers)
   └── (autres fichiers)
   ```

2. **Lancer l'application**
   - Double-cliquez sur `ElMansourSyndicManager.exe`
   - Les dossiers suivants seront créés automatiquement :
     ```
     data/
     ├── local.db (base de données)
     ├── documents/ (fichiers uploadés)
     ├── backups/ (sauvegardes automatiques)
     └── logs/ (journaux)
     ```

3. **Créer le premier utilisateur administrateur**
   - L'application détecte automatiquement le premier lancement
   - Suivez l'assistant de configuration

4. **C'est prêt !** ✅

---

### **Méthode 2 : Installation avec Données Existantes**

**Scénario** : Vous avez déjà des données sur un autre PC et voulez les transférer.

1. **Sur le PC source** :
   - Créez une sauvegarde complète :
     - Ouvrez l'application
     - Allez dans **Paramètres** → **Sauvegardes**
     - Cliquez sur **"Exporter Tout"**
     - Sauvegardez le fichier `.zip` sur une clé USB

2. **Sur le nouveau PC** :
   - Installez l'application (Méthode 1)
   - Copiez le fichier `.zip` dans le dossier `data/backups/`
   - Lancez l'application
   - Allez dans **Paramètres** → **Sauvegardes**
   - Cliquez sur **"Restaurer"** et sélectionnez votre backup

3. **Terminé !** Toutes vos données sont transférées ✅

---

## 🔧 **Configuration Personnalisée**

### **Modifier l'Emplacement des Données**

Éditez le fichier `appsettings.json` **AVANT** le premier lancement :

```json
{
  "ApplicationSettings": {
    "DataDirectory": "D:/MesDonnees/Syndic",
    "DocumentsDirectory": "D:/MesDonnees/Syndic/documents",
    "BackupsDirectory": "E:/Backups",
    "LogsDirectory": "D:/MesDonnees/Syndic/logs"
  }
}
```

**💡 Avantages** :
- Stocker les données sur un disque externe
- Séparer les backups sur un autre disque
- Faciliter les sauvegardes réseau

---

## 💾 **Stratégies de Sauvegarde**

### **Option 1 : Sauvegardes Automatiques (Recommandé)**

**Configuration par défaut** :
```json
{
  "ApplicationSettings": {
    "MaxBackupCount": 10,
    "AutoBackupEnabled": true,
    "AutoBackupIntervalHours": 24
  }
}
```

**Comportement** :
- Une sauvegarde automatique **chaque jour**
- Conservation des **10 dernières sauvegardes**
- Suppression automatique des anciennes

**Emplacement** : `data/backups/backup_YYYYMMDD_HHMMSS.db.zip`

---

### **Option 2 : Sauvegardes Manuelles**

1. Ouvrez l'application
2. **Paramètres** → **Sauvegardes**
3. Cliquez sur **"Créer une Sauvegarde Maintenant"**
4. Le fichier est créé dans `data/backups/`

**💡 Conseil** : Copiez régulièrement ce dossier sur une clé USB ou un cloud.

---

### **Option 3 : Export Complet**

Pour transférer **TOUT** (base de données + documents) :

1. **Paramètres** → **Sauvegardes**
2. **"Exporter Tout"**
3. Choisissez un emplacement (ex: clé USB)
4. Un fichier `.zip` contenant TOUT est créé

**Contenu du ZIP** :
```
backup_export.zip
├── local.db (base de données)
└── documents/
    ├── contrat_001.pdf
    ├── pv_2024.docx
    └── ...
```

---

## 🌐 **Utilisation Multi-PC**

### **⚠️ Important : Limitations SQLite**

SQLite **NE SUPPORTE PAS** :
- ❌ Accès simultané depuis plusieurs PC via réseau
- ❌ Modifications concurrentes
- ❌ Partage de fichier `.db` sur un serveur

### **✅ Solutions Recommandées**

#### **Solution 1 : PC Principal + Synchronisation**

**Configuration** :
- **PC Principal** : Utilise l'application normalement
- **PC Secondaire** : Utilise des sauvegardes synchronisées

**Processus** :
1. Sur PC Principal : Travaillez normalement
2. Fin de journée : Créez une sauvegarde
3. Copiez le backup sur un dossier partagé/cloud
4. Sur PC Secondaire : Restaurez le backup

**💡 Avantages** :
- Simple à mettre en place
- Pas de risque de corruption
- Fonctionne avec n'importe quel PC

**⚠️ Inconvénient** :
- Pas de synchronisation en temps réel

---

#### **Solution 2 : Dossier Partagé avec Rotation**

**Configuration** :
```json
{
  "ApplicationSettings": {
    "DataDirectory": "\\\\SERVEUR\\Syndic\\data",
    "BackupsDirectory": "\\\\SERVEUR\\Syndic\\backups"
  }
}
```

**Règles** :
- ⚠️ **UN SEUL utilisateur à la fois**
- Fermer l'application avant qu'un autre utilisateur l'ouvre
- Utiliser un fichier "verrou" (voir ci-dessous)

---

#### **Solution 3 : Migration vers SQL Server (Professionnel)**

Pour un **vrai multi-utilisateurs simultané** :

**Contactez le support** pour migrer vers :
- SQL Server Express (gratuit, jusqu'à 10 utilisateurs)
- SQL Server Standard (usage professionnel)

**Avantages** :
- ✅ Accès simultané illimité
- ✅ Performances optimales
- ✅ Sauvegardes centralisées
- ✅ Sécurité renforcée

---

## 📂 **Structure des Dossiers**

```
ElMansourSyndicManager/
│
├── ElMansourSyndicManager.exe    ← Application principale
├── appsettings.json               ← Configuration (MODIFIABLE)
├── *.dll                          ← Bibliothèques (NE PAS MODIFIER)
│
└── data/                          ← Données (créé automatiquement)
    ├── local.db                   ← Base de données SQLite
    ├── documents/                 ← Fichiers uploadés
    │   ├── contrats/
    │   ├── pv/
    │   └── factures/
    ├── backups/                   ← Sauvegardes automatiques
    │   ├── backup_20260115_120000.db.zip
    │   ├── backup_20260114_120000.db.zip
    │   └── ...
    └── logs/                      ← Journaux d'événements
        ├── app_20260115.log
        └── ...
```

---

## 🔒 **Sécurité et Bonnes Pratiques**

### **✅ À FAIRE**

1. **Sauvegardes régulières**
   - Activez les sauvegardes automatiques
   - Copiez `data/backups/` sur un disque externe chaque semaine

2. **Protection des données**
   - Ne partagez pas le fichier `local.db` directement
   - Utilisez toujours les exports/imports de l'application

3. **Mises à jour**
   - Sauvegardez AVANT toute mise à jour
   - Testez sur une copie si possible

4. **Permissions**
   - Assurez-vous que l'utilisateur Windows a les droits d'écriture

---

### **❌ À NE PAS FAIRE**

1. ❌ Modifier `local.db` avec un éditeur externe
2. ❌ Copier `local.db` pendant que l'application tourne
3. ❌ Partager `local.db` sur un réseau avec SQLite
4. ❌ Supprimer le dossier `data/` sans sauvegarde
5. ❌ Modifier les fichiers `.dll`

---

## 🆘 **Dépannage**

### **Problème : "Base de données verrouillée"**

**Cause** : Un autre processus utilise la base de données

**Solution** :
1. Fermez toutes les instances de l'application
2. Redémarrez le PC
3. Relancez l'application

---

### **Problème : "Fichier introuvable"**

**Cause** : Chemin incorrect dans `appsettings.json`

**Solution** :
1. Ouvrez `appsettings.json`
2. Vérifiez les chemins dans `ApplicationSettings`
3. Utilisez des chemins absolus si nécessaire

---

### **Problème : "Erreur de permissions"**

**Cause** : Droits insuffisants sur le dossier

**Solution** :
1. Clic droit sur le dossier de l'application
2. **Propriétés** → **Sécurité**
3. Donnez les droits complets à votre utilisateur

---

### **Problème : Données perdues après mise à jour**

**Cause** : Dossier `data/` supprimé

**Solution** :
1. Restaurez la dernière sauvegarde depuis `data/backups/`
2. Si backups supprimés, contactez le support

---

## 📞 **Support et Assistance**

### **Ressources Disponibles**

1. **`GUIDE_CONFIGURATION.md`** - Configuration détaillée
2. **`GUIDE_UI_UX.md`** - Utilisation de l'interface
3. **Logs** - Consultez `data/logs/` pour diagnostics

### **Contact Support**

Pour assistance technique :
- 📧 Email : support@elmansour-syndic.tn
- 📞 Téléphone : +216 XX XXX XXX
- 🌐 Documentation : [lien vers documentation en ligne]

---

## ✅ **Checklist de Déploiement**

Avant de déployer sur un nouveau PC :

- [ ] .NET 8.0 Runtime installé
- [ ] Dossier complet copié
- [ ] `appsettings.json` configuré (si personnalisé)
- [ ] Premier lancement testé
- [ ] Utilisateur admin créé
- [ ] Sauvegarde automatique activée
- [ ] Test de restauration effectué
- [ ] Documentation fournie à l'utilisateur

---

**Version** : 2.0  
**Date** : 2026-01-15  
**Auteur** : Équipe El Mansour Syndic Manager
