# Résumé - Système de Mise à Jour Automatique

## ✅ Ce qui a été fait

### 1. Installation de la bibliothèque AutoUpdater.NET
- Ajout du package `AutoUpdater.NET.Official` au projet
- Cette bibliothèque permet de vérifier et installer automatiquement les mises à jour

### 2. Configuration de l'application
- Modification de `MainWindow.xaml.cs` pour vérifier les mises à jour au démarrage
- L'application pointe maintenant vers : `https://raw.githubusercontent.com/adamkaroui69-jpg/el-mansour/main/update.xml`

### 3. Création du script automatique `publish-update.ps1`
Ce script fait tout automatiquement :
- ✅ Détecte votre dépôt GitHub
- ✅ Incrémente automatiquement la version (ex: 1.0.0 → 1.0.1)
- ✅ Compile l'application
- ✅ Crée l'installateur avec Inno Setup
- ✅ Met à jour le fichier `update.xml` avec les bons liens
- ✅ Envoie tout sur GitHub

### 4. Configuration GitHub
- Dépôt configuré : `https://github.com/adamkaroui69-jpg/el-mansour`
- Tous les fichiers ont été envoyés sur GitHub
- Version actuelle : **1.0.3**

## 📝 Comment utiliser le système

### Pour publier une nouvelle mise à jour :

1. **Faites vos modifications** dans le code
2. **Lancez le script** :
   ```powershell
   ./publish-update.ps1
   ```
3. **C'est tout !** Le script fait le reste automatiquement

### Ce qui se passe ensuite :
- Le fichier `update.xml` est mis à jour sur GitHub avec la nouvelle version
- Au prochain lancement, tous vos utilisateurs verront une notification de mise à jour
- Ils peuvent cliquer pour télécharger et installer la nouvelle version

## 📂 Fichiers importants

### `update.xml`
Ce fichier indique quelle est la dernière version disponible :
```xml
<item>
    <version>1.0.3.0</version>
    <url>https://raw.githubusercontent.com/adamkaroui69-jpg/el-mansour/main/installer-output/setup.exe</url>
    <changelog>https://raw.githubusercontent.com/adamkaroui69-jpg/el-mansour/main/CHANGELOG.md</changelog>
    <mandatory>false</mandatory>
</item>
```

### `publish-update.ps1`
Le script qui automatise tout le processus de publication.

### `GUIDE_MISE_A_JOUR.md`
Documentation complète du système de mise à jour.

## ⚠️ Important

### Votre dépôt doit être PUBLIC
Pour que vos utilisateurs puissent télécharger les mises à jour sans authentification, votre dépôt GitHub doit être **public**.

Pour vérifier/changer cela :
1. Allez sur https://github.com/adamkaroui69-jpg/el-mansour
2. Settings → Danger Zone → Change repository visibility

### Fichiers volumineux
GitHub a averti que certains fichiers sont volumineux (>50MB) :
- `deployment/Output/ElMansourSyndicManager_1.0.0.0.msix` (78.74 MB)
- `publish/ElMansourSyndicManager.exe` (78.02 MB)

Ce n'est pas bloquant, mais si vous voulez optimiser :
- Vous pouvez utiliser Git LFS (Large File Storage)
- Ou héberger les gros fichiers ailleurs (ex: GitHub Releases)

## 🎯 Exemple de workflow

### Scénario : Vous corrigez un bug

1. Vous modifiez le code
2. Vous testez localement
3. Vous exécutez : `./publish-update.ps1`
4. Le script :
   - Change la version de 1.0.3 → 1.0.4
   - Compile l'application
   - Crée le setup.exe
   - Met à jour update.xml
   - Envoie tout sur GitHub
5. Vos utilisateurs reçoivent la notification au prochain lancement

## 📞 En cas de problème

### L'upload GitHub est lent
C'est normal, les fichiers sont volumineux (~300 MB). Soyez patient.

### Erreur "git push rejected"
Utilisez : `git push origin main --force` (avec précaution)

### Les utilisateurs ne voient pas la mise à jour
Vérifiez que :
- Le dépôt est public
- Le fichier `update.xml` est bien à jour sur GitHub
- L'URL dans `MainWindow.xaml.cs` est correcte

## 🎉 Résultat final

Vous avez maintenant un système professionnel de mise à jour automatique !
- ✅ Plus besoin de désinstaller/réinstaller manuellement
- ✅ Les utilisateurs sont notifiés automatiquement
- ✅ Tout le processus est automatisé avec un seul script
- ✅ Versionning automatique

---

**Version actuelle** : 1.0.3  
**Dépôt GitHub** : https://github.com/adamkaroui69-jpg/el-mansour  
**Dernière mise à jour** : 25 novembre 2025
