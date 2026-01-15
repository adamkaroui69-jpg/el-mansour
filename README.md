# El Mansour - Gestion de Syndic

Application de gestion complète pour syndicats de copropriété.

## 🎯 Fonctionnalités Principales

- **Gestion des Résidents** : Fiches complètes avec coordonnées et historique
- **Encaissement des Paiements** : Suivi mensuel des cotisations
- **Calcul Automatique des Arriérés** : Détection des impayés en temps réel
- **Génération de Reçus** : Reçus PDF professionnels avec logo
- **Rapports Financiers** : Tableaux de bord et exports Excel/CSV
- **Sauvegarde Automatique** : Protection de vos données
- **Gestion Multi-Utilisateurs** : Rôles et permissions configurables

## 💻 Installation

### Prérequis
- Windows 10/11
- .NET 8.0 Runtime (téléchargé automatiquement si absent)

### Installation Rapide
1. Téléchargez le fichier `ElMansourSyndicManager-Setup.exe`
2. Double-cliquez pour lancer l'installation
3. Suivez l'assistant d'installation
4. Lancez l'application depuis le menu Démarrer

### Première Connexion
**Compte administrateur par défaut :**
- Identifiant : `admin`
- Mot de passe : `admin123`

⚠️ **Important** : Changez ce mot de passe dès la première connexion !

## 🚀 Démarrage Rapide

### 1. Ajouter des Résidents
1. Cliquez sur **"Résidents"** dans le menu
2. Cliquez sur **"+ Nouveau Résident"**
3. Remplissez les informations (Code Maison, Nom, Cotisation mensuelle)
4. Enregistrez

### 2. Encaisser un Paiement
1. Allez dans **"Paiements"**
2. Cliquez sur **"+ Nouveau Paiement"**
3. Sélectionnez le résident et le mois concerné
4. Entrez le montant et validez
5. Un reçu est généré automatiquement

### 3. Consulter les Arriérés
1. Ouvrez **"Rapports"**
2. Sélectionnez **"État des Comptes"**
3. Les résidents en retard apparaissent en orange/rouge

### 4. Exporter des Données
1. Dans n'importe quelle vue (Paiements, Résidents, etc.)
2. Cliquez sur **"Exporter"**
3. Choisissez Excel (.xlsx) ou CSV
4. Sélectionnez l'emplacement de sauvegarde

## 🔒 Sauvegardes

### Sauvegarde Automatique
- L'application sauvegarde automatiquement chaque jour à 2h du matin
- Les sauvegardes sont stockées dans : `C:\ProgramData\ElMansourSyndic\Backups`

### Sauvegarde Manuelle
1. Allez dans **"Paramètres"** → **"Sauvegardes"**
2. Cliquez sur **"Sauvegarder Maintenant"**
3. Attendez la confirmation

### Restauration
1. Allez dans **"Paramètres"** → **"Sauvegardes"**
2. Sélectionnez une sauvegarde dans l'historique
3. Cliquez sur **"Restaurer"**
4. ⚠️ L'application redémarrera automatiquement

## 📊 Base de Données

L'application utilise SQL Server (LocalDB ou distant).

**Emplacement par défaut** : `C:\ProgramData\ElMansourSyndic\ElMansourDB.mdf`

Pour utiliser un serveur SQL distant, modifiez la connexion dans :
`appsettings.json` → `ConnectionStrings` → `DefaultConnection`

## 🆘 Problèmes Courants

### L'application ne démarre pas
- Vérifiez que .NET 8.0 est installé
- Exécutez en tant qu'administrateur (clic droit → "Exécuter en tant qu'administrateur")

### Impossible de se connecter
- Utilisez les identifiants par défaut : `admin` / `admin123`
- Si oublié, contactez le support ou réinstallez

### Les reçus ne s'affichent pas
- Vérifiez que le logo `logo png.png` est présent dans le dossier d'installation
- Redémarrez l'application

### Erreur de base de données
- Vérifiez que SQL Server LocalDB est installé
- Restaurez une sauvegarde récente

## 📞 Support

Pour toute question ou problème :
- Email : support@elmansour-syndic.local
- Documentation complète : Consultez les guides dans le dossier d'installation

## 📝 Licence

© 2026 El Mansour Syndic Manager - Tous droits réservés
