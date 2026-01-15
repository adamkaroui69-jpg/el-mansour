# 📦 Guide d'Installation - El Mansour Syndic Manager

## 🎯 **Pour l'Utilisateur Final**

### **Installation en 3 Clics** ⚡

1. **Télécharger** le fichier `ElMansourSyndicManager-Setup.exe`
2. **Double-cliquer** sur le fichier
3. **Suivre** l'assistant d'installation

**C'est tout !** ✅

---

## 📋 **Prérequis Automatiques**

L'installateur vérifie et installe automatiquement :
- ✅ .NET 8.0 Runtime (si absent)
- ✅ Droits d'installation
- ✅ Espace disque (minimum 200 MB)

**Aucune action manuelle requise** 👍

---

## 🚀 **Processus d'Installation Détaillé**

### **Étape 1 : Téléchargement**

**Où télécharger ?**
- Site officiel : `www.elmansour-syndic.tn/telechargement`
- Email de l'administrateur
- Clé USB fournie

**Fichier à télécharger** :
```
ElMansourSyndicManager-Setup.exe
Taille : ~50 MB
```

---

### **Étape 2 : Lancement de l'Installation**

1. **Double-cliquez** sur `ElMansourSyndicManager-Setup.exe`

2. **Windows SmartScreen** peut afficher un avertissement :
   ```
   "Windows a protégé votre PC"
   ```
   → Cliquez sur **"Informations complémentaires"**
   → Puis **"Exécuter quand même"**

3. **Contrôle de compte d'utilisateur (UAC)** :
   ```
   "Voulez-vous autoriser cette application à apporter des modifications ?"
   ```
   → Cliquez sur **"Oui"**

---

### **Étape 3 : Assistant d'Installation**

**Écran 1 : Bienvenue**
```
┌─────────────────────────────────────┐
│  El Mansour Syndic Manager          │
│  Version 2.0                         │
│                                      │
│  Bienvenue dans l'assistant          │
│  d'installation                      │
│                                      │
│  [Suivant]  [Annuler]               │
└─────────────────────────────────────┘
```
→ Cliquez sur **"Suivant"**

---

**Écran 2 : Licence**
```
┌─────────────────────────────────────┐
│  Contrat de Licence                  │
│                                      │
│  [Texte de la licence...]            │
│                                      │
│  ☑ J'accepte les termes              │
│                                      │
│  [Précédent]  [Suivant]  [Annuler]  │
└─────────────────────────────────────┘
```
→ Cochez **"J'accepte"**
→ Cliquez sur **"Suivant"**

---

**Écran 3 : Dossier d'Installation**
```
┌─────────────────────────────────────┐
│  Emplacement d'installation          │
│                                      │
│  C:\Program Files\ElMansourSyndic\   │
│  [Parcourir...]                      │
│                                      │
│  💡 Recommandé : Garder par défaut   │
│                                      │
│  [Précédent]  [Suivant]  [Annuler]  │
└─────────────────────────────────────┘
```
→ **Recommandation** : Laissez le chemin par défaut
→ Cliquez sur **"Suivant"**

---

**Écran 4 : Raccourcis**
```
┌─────────────────────────────────────┐
│  Créer des raccourcis                │
│                                      │
│  ☑ Bureau                            │
│  ☑ Menu Démarrer                     │
│  ☐ Démarrage automatique             │
│                                      │
│  [Précédent]  [Installer]  [Annuler] │
└─────────────────────────────────────┘
```
→ Cochez les options souhaitées
→ Cliquez sur **"Installer"**

---

**Écran 5 : Installation en cours**
```
┌─────────────────────────────────────┐
│  Installation en cours...            │
│                                      │
│  ████████████░░░░░░░░░░░  60%        │
│                                      │
│  Installation de .NET Runtime...     │
│                                      │
│  Veuillez patienter...               │
└─────────────────────────────────────┘
```
⏱️ **Durée** : 2-5 minutes

---

**Écran 6 : Terminé**
```
┌─────────────────────────────────────┐
│  Installation terminée ! ✅           │
│                                      │
│  El Mansour Syndic Manager           │
│  a été installé avec succès.         │
│                                      │
│  ☑ Lancer l'application              │
│                                      │
│  [Terminer]                          │
└─────────────────────────────────────┘
```
→ Cochez **"Lancer l'application"** (optionnel)
→ Cliquez sur **"Terminer"**

---

## 🎉 **Premier Lancement**

### **Configuration Initiale Automatique**

Au premier lancement, l'application :

1. **Crée automatiquement** les dossiers nécessaires :
   ```
   C:\Users\VotreNom\AppData\Local\ElMansourSyndic\
   ├── data\
   │   ├── local.db
   │   ├── documents\
   │   ├── backups\
   │   └── logs\
   ```

2. **Affiche l'assistant de configuration** :

**Écran 1 : Bienvenue**
```
┌─────────────────────────────────────┐
│  🎉 Bienvenue !                      │
│                                      │
│  C'est votre première utilisation.   │
│  Créons votre compte administrateur. │
│                                      │
│  [Commencer]                         │
└─────────────────────────────────────┘
```

**Écran 2 : Compte Administrateur**
```
┌─────────────────────────────────────┐
│  Créer le compte administrateur      │
│                                      │
│  Nom d'utilisateur : [________]      │
│  Mot de passe :      [________]      │
│  Confirmer :         [________]      │
│  Email :             [________]      │
│                                      │
│  [Précédent]  [Créer]                │
└─────────────────────────────────────┘
```

**Écran 3 : Informations Syndic**
```
┌─────────────────────────────────────┐
│  Informations du Syndic (optionnel)  │
│                                      │
│  Nom :     [El Mansour Syndic]       │
│  Adresse : [________________]        │
│  Téléphone:[________________]        │
│  Email :   [________________]        │
│                                      │
│  [Ignorer]  [Enregistrer]            │
└─────────────────────────────────────┘
```

**Écran 4 : Terminé**
```
┌─────────────────────────────────────┐
│  ✅ Configuration terminée !          │
│                                      │
│  Votre application est prête.        │
│                                      │
│  💡 Conseil : Activez les            │
│     sauvegardes automatiques         │
│     dans Paramètres > Sauvegardes    │
│                                      │
│  [Commencer]                         │
└─────────────────────────────────────┘
```

---

## 🔄 **Mise à Jour de l'Application**

### **Méthode Automatique (Recommandée)**

**L'application vérifie automatiquement les mises à jour au démarrage.**

**Si une mise à jour est disponible** :
```
┌─────────────────────────────────────┐
│  🔔 Mise à jour disponible           │
│                                      │
│  Version actuelle : 2.0              │
│  Nouvelle version : 2.1              │
│                                      │
│  Nouveautés :                        │
│  • Amélioration des performances     │
│  • Correction de bugs                │
│  • Nouvelles fonctionnalités         │
│                                      │
│  ⚠️ Vos données seront préservées    │
│                                      │
│  [Mettre à jour]  [Plus tard]        │
└─────────────────────────────────────┘
```

**Processus de mise à jour** :

1. **Sauvegarde automatique** de vos données
2. **Téléchargement** de la nouvelle version
3. **Installation** automatique
4. **Redémarrage** de l'application
5. **Vérification** de l'intégrité des données

⏱️ **Durée** : 2-3 minutes
✅ **Données préservées** : 100%

---

### **Méthode Manuelle**

**Si vous préférez mettre à jour manuellement** :

1. **Téléchargez** la nouvelle version
2. **Lancez** l'installateur
3. **Sélectionnez** "Mettre à jour" (pas "Nouvelle installation")
4. **Suivez** l'assistant

**⚠️ Important** : Ne désinstallez PAS l'ancienne version avant !

---

## 🗑️ **Désinstallation**

### **Méthode Standard**

**Windows 10/11** :

1. **Paramètres Windows** (touche Windows + I)
2. **Applications** → **Applications et fonctionnalités**
3. Cherchez **"El Mansour Syndic Manager"**
4. Cliquez sur **"Désinstaller"**
5. Confirmez

**Ou via Panneau de configuration** :

1. **Panneau de configuration**
2. **Programmes** → **Désinstaller un programme**
3. Sélectionnez **"El Mansour Syndic Manager"**
4. Cliquez sur **"Désinstaller"**

---

### **Conservation des Données**

**Lors de la désinstallation, vous avez le choix** :

```
┌─────────────────────────────────────┐
│  Désinstallation                     │
│                                      │
│  Que voulez-vous faire de vos        │
│  données ?                           │
│                                      │
│  ○ Conserver mes données             │
│     (pour réinstaller plus tard)     │
│                                      │
│  ○ Supprimer toutes les données      │
│     (suppression complète)           │
│                                      │
│  [Annuler]  [Désinstaller]           │
└─────────────────────────────────────┘
```

**💡 Recommandation** : Choisissez **"Conserver"** si vous comptez réinstaller.

---

## 🆘 **Problèmes Courants et Solutions**

### **Problème 1 : "L'installation a échoué"**

**Causes possibles** :
- Antivirus bloque l'installation
- Droits insuffisants
- Espace disque insuffisant

**Solutions** :
1. **Désactivez temporairement** l'antivirus
2. **Clic droit** sur l'installateur → **"Exécuter en tant qu'administrateur"**
3. **Libérez** au moins 500 MB d'espace disque

---

### **Problème 2 : "L'application ne démarre pas"**

**Causes possibles** :
- .NET Runtime manquant
- Fichiers corrompus

**Solutions** :
1. **Réinstallez** .NET 8.0 Runtime :
   - Téléchargez depuis : `https://dotnet.microsoft.com/download/dotnet/8.0`
   - Installez la version **Desktop Runtime**

2. **Réparez** l'installation :
   - Panneau de configuration → Programmes
   - Sélectionnez l'application
   - Cliquez sur **"Réparer"**

---

### **Problème 3 : "Mes données ont disparu après mise à jour"**

**Solution** :
1. Allez dans **Paramètres** → **Sauvegardes**
2. Cliquez sur **"Restaurer une sauvegarde"**
3. Sélectionnez la sauvegarde la plus récente
4. Cliquez sur **"Restaurer"**

**💡 Les sauvegardes automatiques sont créées avant chaque mise à jour.**

---

### **Problème 4 : "Erreur de base de données verrouillée"**

**Cause** : L'application est déjà ouverte

**Solution** :
1. **Fermez** toutes les fenêtres de l'application
2. **Ouvrez** le Gestionnaire des tâches (Ctrl+Shift+Esc)
3. Cherchez **"ElMansourSyndicManager"**
4. Cliquez sur **"Fin de tâche"**
5. **Relancez** l'application

---

## 📂 **Emplacements des Fichiers**

### **Fichiers de l'Application**
```
C:\Program Files\ElMansourSyndic\
├── ElMansourSyndicManager.exe
├── appsettings.json
└── [autres fichiers]
```
**⚠️ Ne pas modifier ces fichiers**

### **Données Utilisateur**
```
C:\Users\VotreNom\AppData\Local\ElMansourSyndic\
├── data\
│   ├── local.db (base de données)
│   ├── documents\ (vos fichiers)
│   ├── backups\ (sauvegardes)
│   └── logs\ (journaux)
```
**✅ Vous pouvez sauvegarder ce dossier**

### **Raccourcis**
```
Bureau : ElMansour Syndic Manager.lnk
Menu Démarrer : El Mansour Syndic Manager
```

---

## 💾 **Sauvegarde Manuelle Avant Mise à Jour**

**Pour les utilisateurs prudents** :

1. **Ouvrez** l'application
2. **Paramètres** → **Sauvegardes**
3. **"Exporter Tout"**
4. **Enregistrez** sur une clé USB ou un disque externe
5. **Procédez** à la mise à jour

**En cas de problème** : Restaurez cette sauvegarde.

---

## ✅ **Checklist Post-Installation**

Après l'installation, vérifiez :

- [ ] L'application se lance correctement
- [ ] Vous pouvez vous connecter
- [ ] Les sauvegardes automatiques sont activées
- [ ] Un raccourci est présent sur le Bureau
- [ ] Les données de test s'affichent (si premier lancement)

---

## 📞 **Support Installation**

**En cas de problème persistant** :

📧 **Email** : support@elmansour-syndic.tn  
📞 **Téléphone** : +216 XX XXX XXX  
🌐 **Site web** : www.elmansour-syndic.tn/support

**Informations à fournir** :
- Version de Windows
- Message d'erreur exact (capture d'écran)
- Étape où le problème survient

---

**Version du Guide** : 2.0  
**Dernière mise à jour** : 2026-01-15  
**Compatibilité** : Windows 10/11
