# ☁️ RELEASE v2.1.1 - Cloud Sync & Fixes

**Version :** 2.1.1
**Date :** 22 Janvier 2026

## 🚀 Nouveautés

### Synchronisation Cloud Automatique
- Support ajouté pour **SQL Server**.
- Vous pouvez désormais héberger votre base de données sur le Cloud (Azure, etc.) et connecter tous vos utilisateurs dessus.
- Synchronisation en temps réel via Internet.
- Voir `GUIDE_CLOUD_SYNC.md` pour la mise en place.

## 🛠️ Correctifs

### Crash au Démarrage
- Résolution de l'erreur "Impossible de mettre à jour la base de données".
- L'application stocke désormais ses données locales dans le dossier utilisateur (`AppData`), ce qui évite les problèmes de droits d'administrateur.

### Configuration
- Ajout d'une section dans les Paramètres indiquant l'emplacement de la base de données et le mode (Local/Cloud).
