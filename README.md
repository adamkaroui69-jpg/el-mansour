# El Mansour Syndic Manager

Application WPF moderne de gestion de syndic pour la résidence "El Mansour" développée en .NET 8.

## 📋 Table des Matières

- [Vue d'ensemble](#vue-densemble)
- [Fonctionnalités](#fonctionnalités)
- [Architecture](#architecture)
- [Structure du Projet](#structure-du-projet)
- [Prérequis](#prérequis)
- [Installation](#installation)
- [Configuration](#configuration)
- [Utilisation](#utilisation)
- [Documentation](#documentation)
- [Développement](#développement)
- [Sécurité](#sécurité)
- [Support](#support)

## 🏢 Vue d'ensemble

**El Mansour Syndic Manager** est une application de gestion complète pour la résidence "El Mansour", permettant de gérer les paiements mensuels, la maintenance, les rapports financiers et les utilisateurs.

### Structure de la Résidence

- **Résidence**: El Mansour
- **Bâtiments A, C, D, E**: 3 étages × 4 maisons = 12 maisons chacun
- **Bâtiment B**: 4 étages
  - 4ème étage: Bureau Syndic + Logement Concierge (2 unités spéciales)
  - Rez-de-chaussée: Magasins M02 et M03
- **Bâtiment A rez-de-chaussée**: Magasin M01

**Format des codes**: D01, A03, M01, etc.

## ✨ Fonctionnalités

### 💰 Gestion des Paiements
- Enregistrement des paiements mensuels fixes
- Suivi obligatoire des paiements
- Génération de reçus PDF avec signature
- Liste des maisons non payées
- Historique des paiements

### 🔧 Gestion de la Maintenance
- Création de demandes de maintenance
- Suivi des coûts
- Pièces justificatives (PDF/images)
- Statuts: En attente, En cours, Terminé

### 📊 Rapports Financiers
- Rapports mensuels détaillés
- Rapports annuels
- Statistiques et graphiques
- Export PDF/Excel
- Liste des maisons non payées

### 👥 Gestion des Utilisateurs
- 1 Administrateur
- Jusqu'à 4 Membres Syndic
- Authentification par code 6 chiffres
- Gestion des signatures PNG

### ☁️ Synchronisation Cloud
- Synchronisation bidirectionnelle avec Supabase
- Base de données locale SQLite
- Mode hors ligne complet
- Résolution de conflits automatique

### 📄 Gestion des Documents
- Upload de documents justificatifs
- Stockage local et cloud
- Visualisation de documents
- Gestion des versions

### 📝 Audit et Sécurité
- Journal d'audit complet
- Logs de toutes les actions
- Chiffrement des données sensibles
- Authentification sécurisée

### 💾 Sauvegarde
- Sauvegardes automatiques quotidiennes
- Sauvegardes manuelles
- Restauration de sauvegardes
- Chiffrement des sauvegardes

## 🏗️ Architecture

L'application suit une architecture MVVM (Model-View-ViewModel) avec séparation en couches:

```
┌─────────────────┐
│   Presentation  │ (Views, ViewModels)
├─────────────────┤
│    Services     │ (Business Logic)
├─────────────────┤
│  Data Access    │ (Repositories)
├─────────────────┤
│   Databases     │ (SQLite + Supabase)
└─────────────────┘
```

### Technologies

- **.NET 8**: Framework principal
- **WPF**: Interface utilisateur
- **Material Design in XAML Toolkit**: Composants UI
- **SQLite**: Base de données locale
- **Supabase**: Backend cloud
- **QuestPDF**: Génération de PDF
- **Entity Framework Core**: ORM (optionnel)

Voir [ARCHITECTURE.md](docs/ARCHITECTURE.md) pour plus de détails.

## 📁 Structure du Projet

```
ElMansourSyndicManager/
├── src/
│   ├── ElMansourSyndicManager/          # Application principale WPF
│   ├── ElMansourSyndicManager.Core/       # Domain & Interfaces
│   ├── ElMansourSyndicManager.Infrastructure/  # Implémentations
│   ├── ElMansourSyndicManager.ViewModels/ # ViewModels MVVM
│   ├── ElMansourSyndicManager.Views/     # Vues XAML
│   └── ElMansourSyndicManager.Utilities/ # Utilitaires
├── tests/                                 # Tests unitaires/intégration
├── docs/                                  # Documentation
├── scripts/                               # Scripts SQL/PS
└── resources/                             # Ressources (images, templates)
```

Voir [PROJECT_STRUCTURE.md](docs/PROJECT_STRUCTURE.md) pour la structure complète.

## 🔧 Prérequis

- **.NET 8 SDK** ou supérieur
- **Visual Studio 2022** (recommandé) ou **Visual Studio Code**
- **Windows 10/11** (WPF est Windows uniquement)
- **Compte Supabase** (pour le backend cloud)

## 🚀 Installation

### 1. Cloner le Repository

```bash
git clone https://github.com/your-repo/el-mansour-syndic-manager.git
cd el-mansour-syndic-manager
```

### 2. Restaurer les Packages NuGet

```bash
dotnet restore
```

### 3. Configurer la Base de Données

```bash
# Exécuter le script de création de base de données
sqlite3 data/database/elmansour.db < scripts/setup-database.sql

# Charger les données initiales
sqlite3 data/database/elmansour.db < scripts/seed-data.sql
```

### 4. Configurer Supabase

1. Créer un projet sur [Supabase](https://supabase.com)
2. Configurer les tables (voir `docs/DATABASE_SCHEMA.md`)
3. Configurer Row Level Security (RLS)
4. Obtenir l'URL et la clé API

### 5. Configurer l'Application

Éditer `appsettings.json`:

```json
{
  "Supabase": {
    "Url": "https://your-project.supabase.co",
    "AnonKey": "your-anon-key"
  },
  "Database": {
    "Path": "data/database/elmansour.db",
    "EncryptionKey": "your-encryption-key"
  },
  "Sync": {
    "IntervalMinutes": 5,
    "AutoSync": true
  }
}
```

### 6. Compiler et Exécuter

```bash
dotnet build
dotnet run --project src/ElMansourSyndicManager
```

## ⚙️ Configuration

### Utilisateur Administrateur par Défaut

- **Code Maison**: B40 (Bureau Syndic)
- **Code d'authentification**: 123456 (à changer lors de la première connexion)

### Configuration de la Synchronisation

- **Intervalle automatique**: 5 minutes (configurable)
- **Mode hors ligne**: Activé par défaut
- **Résolution de conflits**: Dernière écriture gagne (LWW)

Voir [SYNC_STRATEGY.md](docs/SYNC_STRATEGY.md) pour plus de détails.

## 📖 Utilisation

### Première Connexion

1. Lancer l'application
2. Entrer le code maison: `B40`
3. Entrer le code: `123456`
4. Changer le code immédiatement (recommandé)

### Enregistrer un Paiement

1. Naviguer vers **Paiements** → **Enregistrer un Paiement**
2. Sélectionner le code maison
3. Sélectionner le mois
4. Vérifier le montant (pré-rempli)
5. Entrer la date de paiement
6. Cliquer sur **Enregistrer**
7. Le reçu PDF est généré automatiquement

### Créer une Maintenance

1. Naviguer vers **Maintenance** → **Créer une Maintenance**
2. Remplir la description
3. Sélectionner le type
4. Entrer le coût
5. Ajouter des documents justificatifs (optionnel)
6. Cliquer sur **Enregistrer**

### Générer un Rapport

1. Naviguer vers **Rapports**
2. Sélectionner le type (Mensuel/Annuel)
3. Sélectionner la période
4. Cliquer sur **Générer**
5. Exporter en PDF ou Excel si nécessaire

## 📚 Documentation

- [Architecture](docs/ARCHITECTURE.md) - Architecture complète
- [Structure du Projet](docs/PROJECT_STRUCTURE.md) - Organisation des fichiers
- [Modules](docs/MODULES.md) - Documentation des modules
- [Schéma de Base de Données](docs/DATABASE_SCHEMA.md) - Structure des tables
- [Modèle de Sécurité](docs/SECURITY_MODEL.md) - Sécurité et authentification
- [Stratégie de Synchronisation](docs/SYNC_STRATEGY.md) - Synchronisation cloud
- [Navigation et UI](docs/NAVIGATION_UI.md) - Flux de navigation et wireframes

## 💻 Développement

### Structure des Branches

- `main`: Code de production
- `develop`: Développement actif
- `feature/*`: Nouvelles fonctionnalités
- `bugfix/*`: Corrections de bugs

### Standards de Code

- **C#**: Suivre les conventions Microsoft
- **XAML**: Indentation 4 espaces
- **Commentaires**: En français pour la documentation

### Tests

```bash
# Exécuter les tests unitaires
dotnet test tests/ElMansourSyndicManager.Tests.Unit

# Exécuter les tests d'intégration
dotnet test tests/ElMansourSyndicManager.Tests.Integration
```

## 🔒 Sécurité

- **Authentification**: Code 6 chiffres haché avec PBKDF2
- **Chiffrement**: Base de données SQLite chiffrée (SQLCipher)
- **HTTPS**: Toutes les communications cloud en HTTPS
- **Audit**: Journalisation de toutes les actions

Voir [SECURITY_MODEL.md](docs/SECURITY_MODEL.md) pour plus de détails.

## 🐛 Dépannage

### Problèmes de Synchronisation

1. Vérifier la connexion internet
2. Vérifier les credentials Supabase
3. Consulter les logs dans `logs/`
4. Forcer une synchronisation manuelle

### Problèmes de Base de Données

1. Vérifier que le fichier existe: `data/database/elmansour.db`
2. Vérifier les permissions
3. Restaurer depuis une sauvegarde si nécessaire

### Problèmes d'Authentification

1. Vérifier que l'utilisateur existe
2. Réinitialiser le code si nécessaire (Admin uniquement)
3. Vérifier les logs d'audit

## 📞 Support

Pour toute question ou problème:
- **Email**: support@elmansour-syndic.com
- **Issues**: [GitHub Issues](https://github.com/your-repo/issues)

## 📄 Licence

Ce projet est propriétaire. Tous droits réservés.

## 🙏 Remerciements

- Material Design in XAML Toolkit
- Supabase
- QuestPDF
- .NET Community

---

**Version**: 1.0.0  
**Dernière mise à jour**: 2024

