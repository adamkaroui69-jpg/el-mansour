# Guide de Gestion des Mises à Jour Automatiques

Ce guide explique comment configurer et publier des mises à jour pour que l'aapplication les détecte automatiquement.

## 1. Principe de Fonctionnement

L'application vérifie régulièrement une URL fixe pour voir si une nouvelle version est disponible :
- **URL de vérification** : `https://github.com/adamkaroui69-jpg/el-mansour/releases/latest/download/update.xml`
- **Fichier clé** : `update.xml`

Si la version dans `update.xml` est supérieure à la version installée, l'application propose à l'utilisateur de télécharger la mise à jour.

## 2. Créer une Nouvelle Mise à Jour

### Étape A : Compiler la nouvelle version
1. Ouvrez le projet dans votre terminal.
2. Lancez le script de construction de l'installateur :
   ```powershell
   ./build-installer.ps1
   ```
   Cela va générer un fichier comme `ElMansourSyndicManager-Setup-v2.1.0.exe`.

### Étape B : Préparer le fichier `update.xml`
Créez un fichier nommé `update.xml` avec le contenu suivant (adaptez le numéro de version) :

```xml
<?xml version="1.0" encoding="utf-8"?>
<update>
    <version>2.1.0</version>
    <url>https://github.com/adamkaroui69-jpg/el-mansour/releases/latest/download/ElMansourSyndicManager-Setup-v2.1.0.exe</url>
    <critical>false</critical>
    <notes>
        - Ajout de la sauvegarde automatique locale.
        - Correction de bugs d'affichage.
        - Amélioration des performances.
    </notes>
</update>
```

### Étape C : Publier sur GitHub
1. Allez sur votre dépôt GitHub > **Releases** > **Draft a new release**.
2. **Tag version** : `v2.1.0` (Doit correspondre à votre version logicielle).
3. **Titre** : "Version 2.1.0".
4. **Description** : Copiez les notes de version.
5. **Joindre les binaires** :
   - Glissez-déposez l'installateur `.exe` généré.
   - Glissez-déposez le fichier `update.xml`.
6. Cliquez sur **Publish release**.

## 3. Test de la Mise à Jour

1. Installez une ancienne version de l'application (ex: 2.0.0).
2. Lancez l'application.
3. Allez dans **Paramètres**.
4. Cliquez sur **"Vérifier les mises à jour"**.
5. Une fenêtre doit apparaître annonçant la version 2.1.0.
6. Cliquez sur "Oui" pour télécharger et lancer l'installateur.

## 4. Bonnes Pratiques

- **Compatibilité** : L'installateur Inno Setup est configuré pour ne pas écraser les données utilisateur (`local.db`, dossiers `App_Data`).
- **Sauvegarde** : Toujours conseiller aux utilisateurs de faire une sauvegarde manuelle avant une mise à jour majeure, même si le processus est automatisé.
- **Notes de version** : Soyez clair dans le fichier `update.xml` sur les changements importants.
