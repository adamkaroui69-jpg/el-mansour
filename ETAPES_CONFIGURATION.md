# Étapes de Configuration - Dépôt Privé

## ✅ Ce qui a été fait

1. ✅ Dépôt rendu privé sur GitHub
2. ✅ GitHub CLI installé (version 2.83.1)
3. ✅ Script `publish-update-private.ps1` créé
4. ✅ Guide complet `GUIDE_DEPOT_PRIVE.md` créé

## 🔄 Prochaines étapes (À FAIRE MAINTENANT)

### Étape 1 : Redémarrer le terminal

**IMPORTANT** : Fermez votre terminal PowerShell actuel et ouvrez-en un nouveau.
Cela permet de recharger le PATH et de reconnaître la commande `gh`.

### Étape 2 : Authentification GitHub

Dans le nouveau terminal, exécutez :

```powershell
gh auth login
```

Suivez les instructions :
1. Choisissez : **GitHub.com**
2. Choisissez : **HTTPS**
3. Choisissez : **Login with a web browser**
4. Appuyez sur Entrée
5. Un code s'affiche (ex: ABCD-1234)
6. Votre navigateur s'ouvre automatiquement
7. Collez le code et autorisez l'accès

### Étape 3 : Vérifier l'authentification

```powershell
gh auth status
```

Vous devriez voir :
```
✓ Logged in to github.com as adamkaroui69-jpg
```

### Étape 4 : Tester le système

Créez votre première release :

```powershell
cd "c:\Users\adamk\Desktop\raisidance application"
./publish-update-private.ps1
```

## 📋 Résumé des commandes

```powershell
# 1. Fermer et rouvrir le terminal

# 2. S'authentifier
gh auth login

# 3. Vérifier
gh auth status

# 4. Publier une mise à jour
cd "c:\Users\adamk\Desktop\raisidance application"
./publish-update-private.ps1
```

## 🎯 Ce qui va se passer

Quand vous exécuterez `./publish-update-private.ps1` :

1. ✅ Version incrémentée (1.0.3 → 1.0.4)
2. ✅ Application compilée
3. ✅ Installateur créé
4. ✅ **Release GitHub créée** (publique, même si le dépôt est privé)
5. ✅ Fichier setup.exe attaché à la release
6. ✅ `update.xml` mis à jour avec l'URL de la release
7. ✅ Commit et push sur GitHub

## 🔗 URLs importantes

Après la première release, vous aurez :

**Page des releases** :
https://github.com/adamkaroui69-jpg/el-mansour/releases

**URL de téléchargement** (exemple pour v1.0.4) :
https://github.com/adamkaroui69-jpg/el-mansour/releases/download/v1.0.4/ElMansourSyndicManager-Setup-v1.0.4.exe

**Fichier update.xml** :
https://raw.githubusercontent.com/adamkaroui69-jpg/el-mansour/main/update.xml

## ⚠️ Important

### Le fichier update.xml doit rester accessible

Même avec un dépôt privé, le fichier `update.xml` doit être accessible via l'URL Raw.
GitHub permet cela pour les dépôts privés si vous connaissez l'URL exacte.

Si vous avez des problèmes d'accès au `update.xml`, vous avez 2 options :

**Option A** : Héberger `update.xml` ailleurs (serveur web, Dropbox, etc.)

**Option B** : Utiliser GitHub Gist (public) pour héberger uniquement ce fichier

## 📞 Besoin d'aide ?

Si vous rencontrez un problème :

1. Vérifiez que vous avez bien fermé/rouvert le terminal
2. Vérifiez l'authentification : `gh auth status`
3. Consultez `GUIDE_DEPOT_PRIVE.md` pour plus de détails

---

**Prochaine action** : Fermez ce terminal, ouvrez-en un nouveau, et exécutez `gh auth login`
