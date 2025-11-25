# Guide : Dépôt Privé avec GitHub Releases

## Pourquoi utiliser GitHub Releases ?

Avec un dépôt **privé**, les utilisateurs ne peuvent pas accéder directement aux fichiers. Mais les **GitHub Releases** permettent de rendre certains fichiers publics même dans un dépôt privé.

## 📋 Prérequis

### 1. Installer GitHub CLI

GitHub CLI (gh) est nécessaire pour créer des releases automatiquement.

**Installation avec winget** (recommandé) :
```powershell
winget install --id GitHub.cli
```

**Ou téléchargez depuis** : https://cli.github.com/

### 2. Authentification

Après installation, authentifiez-vous :
```powershell
gh auth login
```

Suivez les instructions :
1. Choisissez "GitHub.com"
2. Choisissez "HTTPS"
3. Choisissez "Login with a web browser"
4. Copiez le code et collez-le dans votre navigateur

## 🚀 Utilisation

### Pour publier une mise à jour avec un dépôt privé :

```powershell
./publish-update-private.ps1
```

### Ce que fait le script :

1. ✅ Incrémente la version
2. ✅ Compile l'application
3. ✅ Crée l'installateur
4. ✅ **Crée une Release GitHub publique** avec le fichier setup.exe
5. ✅ Met à jour `update.xml` avec l'URL de la release
6. ✅ Commit et push sur GitHub

### Différence avec le script public :

| Aspect | Dépôt Public | Dépôt Privé |
|--------|--------------|-------------|
| Script | `publish-update.ps1` | `publish-update-private.ps1` |
| Fichiers accessibles | Tous via Raw URL | Seulement les releases |
| Prérequis | Aucun | GitHub CLI (gh) |
| URL de téléchargement | `raw.githubusercontent.com/...` | `github.com/.../releases/download/...` |

## 📦 Comment ça fonctionne ?

### Avec un dépôt privé :

1. **Votre code reste privé** : Personne ne peut voir votre code source
2. **Les releases sont publiques** : Les fichiers `.exe` dans les releases sont téléchargeables par tous
3. **Le fichier `update.xml` reste accessible** : Il est dans le dépôt mais accessible via Raw URL

### Structure des URLs :

**Fichier update.xml** (dans le dépôt) :
```
https://raw.githubusercontent.com/adamkaroui69-jpg/el-mansour/main/update.xml
```

**Installateur** (dans les releases) :
```
https://github.com/adamkaroui69-jpg/el-mansour/releases/download/v1.0.4/ElMansourSyndicManager-Setup-v1.0.4.exe
```

## 🔐 Sécurité

### Avantages du dépôt privé :

- ✅ Votre code source reste confidentiel
- ✅ Seuls les collaborateurs peuvent voir le code
- ✅ Les utilisateurs peuvent quand même télécharger les mises à jour
- ✅ Vous contrôlez qui peut contribuer au projet

### Ce qui reste public :

- Les fichiers dans les **Releases** (setup.exe)
- Le fichier `update.xml` (via Raw URL)
- Les notes de version (changelog)

### Ce qui reste privé :

- Tout le code source
- L'historique Git
- Les issues et pull requests (si configuré)

## 🎯 Exemple complet

### Scénario : Vous voulez publier la version 1.0.4

1. **Faites vos modifications** dans le code

2. **Lancez le script** :
   ```powershell
   ./publish-update-private.ps1
   ```

3. **Le script crée automatiquement** :
   - Tag Git : `v1.0.4`
   - Release GitHub : "Version 1.0.4"
   - Fichier attaché : `ElMansourSyndicManager-Setup-v1.0.4.exe`

4. **Vos utilisateurs** :
   - Lancent l'application
   - Voient la notification de mise à jour
   - Cliquent sur "Télécharger"
   - Le fichier est téléchargé depuis GitHub Releases
   - Installation automatique

## 🔄 Migration depuis un dépôt public

Si vous avez déjà utilisé le script pour dépôt public :

1. **Rendez votre dépôt privé** sur GitHub :
   - Settings → Danger Zone → Change repository visibility → Make private

2. **Utilisez le nouveau script** :
   ```powershell
   ./publish-update-private.ps1
   ```

3. **C'est tout !** Les anciennes versions restent accessibles, et les nouvelles utilisent les releases.

## ❓ FAQ

### Q: Le fichier update.xml est-il accessible dans un dépôt privé ?
**R:** Oui ! Les fichiers accessibles via Raw URL restent accessibles même dans un dépôt privé, tant qu'on connaît l'URL exacte.

### Q: Les utilisateurs doivent-ils avoir un compte GitHub ?
**R:** Non ! Les releases publiques sont téléchargeables par tout le monde, sans authentification.

### Q: Puis-je supprimer une release ?
**R:** Oui, via GitHub ou avec `gh release delete v1.0.4`

### Q: Combien de releases puis-je créer ?
**R:** Illimité ! Mais chaque fichier est limité à 2 GB.

### Q: Puis-je rendre certaines releases privées ?
**R:** Non, toutes les releases sont publiques. Si vous voulez une version privée, ne créez pas de release.

## 🛠️ Commandes utiles

### Lister toutes les releases :
```powershell
gh release list
```

### Voir les détails d'une release :
```powershell
gh release view v1.0.4
```

### Supprimer une release :
```powershell
gh release delete v1.0.4
```

### Télécharger une release :
```powershell
gh release download v1.0.4
```

## 📞 En cas de problème

### Erreur : "gh: command not found"
→ Installez GitHub CLI : `winget install --id GitHub.cli`

### Erreur : "authentication required"
→ Authentifiez-vous : `gh auth login`

### Erreur : "release already exists"
→ La version existe déjà, incrémentez manuellement ou supprimez l'ancienne release

---

**Recommandation** : Utilisez un dépôt **privé** si votre code contient des informations sensibles (clés API, logique métier confidentielle). Sinon, un dépôt public est plus simple.
