# Guide de Mise à Jour Automatique

Ce guide explique comment configurer et utiliser le système de mise à jour automatique intégré à l'application **ElMansourSyndicManager**.

## 1. Fonctionnement

L'application utilise la bibliothèque **AutoUpdater.NET**. Au démarrage, elle vérifie un fichier XML situé à une adresse spécifique (URL ou chemin réseau). Si la version indiquée dans ce fichier est supérieure à la version actuelle de l'application installée, une fenêtre propose à l'utilisateur de faire la mise à jour.

## 2. Configuration Initiale

### Étape 1 : Définir l'emplacement du fichier de mise à jour

Dans le fichier `src/ElMansourSyndicManager/MainWindow.xaml.cs`, nous avons ajouté la ligne suivante :

```csharp
AutoUpdater.Start("https://votre-domaine.com/update.xml");
```

Vous devez remplacer `"https://votre-domaine.com/update.xml"` par l'emplacement réel où vous allez héberger ce fichier.
Cela peut être :
*   **Un site web** : `https://mon-syndic.com/updates/update.xml`
*   **Un dossier partagé (Réseau Local)** : `\\SERVEUR\Partage\Updates\update.xml`
*   **GitHub** : Le lien "Raw" vers un fichier sur GitHub.

### Étape 2 : Préparer le fichier XML

Un modèle `update_template.xml` a été créé à la racine du projet. Copiez-le et renommez-le en `update.xml`.

Contenu du fichier :
```xml
<item>
    <version>1.0.1.0</version>
    <url>https://votre-domaine.com/setup.exe</url>
    <changelog>https://votre-domaine.com/changelog.html</changelog>
    <mandatory>false</mandatory>
</item>
```

*   `<version>` : La nouvelle version de l'application (doit être supérieure à la version actuelle).
*   `<url>` : Le lien direct pour télécharger le nouvel installateur (`.exe`).
*   `<changelog>` : (Optionnel) Lien vers une page décrivant les changements.
*   `<mandatory>` : `true` pour obliger la mise à jour, `false` pour laisser le choix.

## 3. Procédure Automatisée (Recommandée)

Nous avons créé un script qui fait tout pour vous : incrémentation de version, construction, mise à jour du fichier XML et envoi sur GitHub.

### Pré-requis
1.  Avoir un compte GitHub et un dépôt pour ce projet.
2.  Avoir configuré le dépôt sur votre machine :
    ```powershell
    git remote add origin https://github.com/VOTRE_NOM/VOTRE_PROJET.git
    ```
    *Note : Le dépôt doit être **Public** pour que les utilisateurs puissent télécharger les mises à jour sans configuration complexe.*

### Lancer une mise à jour
Il suffit d'exécuter cette commande dans le terminal :

```powershell
./publish-update.ps1
```

Le script va :
1.  Détecter votre dépôt GitHub.
2.  Augmenter la version de l'application (ex: 1.0.0 -> 1.0.1).
3.  Créer le nouvel installateur.
4.  Mettre à jour `update.xml` avec les bons liens GitHub.
5.  Configurer l'application pour lire les mises à jour depuis GitHub (la première fois).
6.  Envoyer le tout sur GitHub.

Une fois terminé, vos utilisateurs recevront la mise à jour automatiquement au prochain lancement !

## 4. Procédure Manuelle (Si besoin)

Si vous ne pouvez pas utiliser le script automatique :

1.  **Augmenter la version** dans `ElMansourSyndicManager.csproj`.
2.  **Générer l'installateur** avec `build-installer.ps1`.
3.  **Mettre à jour `update.xml`** manuellement avec la nouvelle version et le lien vers le fichier `setup.exe`.
4.  **Uploader** le fichier `setup.exe` et `update.xml` sur votre hébergement.
