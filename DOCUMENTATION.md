# 📚 Documentation El Mansour Syndic Manager

Bienvenue dans la documentation complète de l'application.

## 🚀 Pour Commencer

### Nouveau sur l'application ?
1. **[README.md](README.md)** - Vue d'ensemble et installation rapide
2. **[GUIDE_PREMIERE_UTILISATION.md](GUIDE_PREMIERE_UTILISATION.md)** - Guide pas à pas pour démarrer

### Installation et Configuration
- Installation de l'application → [README.md](README.md#-installation)
- Première connexion → [GUIDE_PREMIERE_UTILISATION.md](GUIDE_PREMIERE_UTILISATION.md#étape-1--première-connexion-2-min)
- Configuration initiale → [GUIDE_PREMIERE_UTILISATION.md](GUIDE_PREMIERE_UTILISATION.md#étape-2--configuration-initiale-5-min)

---

## 📖 Guides Utilisateur

### Utilisation Quotidienne
- **[GUIDE_PREMIERE_UTILISATION.md](GUIDE_PREMIERE_UTILISATION.md)** - Toutes les fonctionnalités de base
  - Ajouter des résidents
  - Enregistrer des paiements
  - Générer des reçus
  - Consulter les rapports

### Fonctionnalités Avancées
- **[GUIDE_UX_MODERNE.md](GUIDE_UX_MODERNE.md)** - Interface et expérience utilisateur
  - Dialogues de confirmation
  - Notifications (Snackbar)
  - Système d'annulation (Undo)
  - Indicateurs de chargement

- **[GUIDE_FINANCIER_EXPORT.md](GUIDE_FINANCIER_EXPORT.md)** - Gestion financière
  - Calcul automatique des arriérés
  - États financiers des résidents
  - Export Excel et CSV
  - Bonnes pratiques comptables

---

## 🔒 Sécurité et Maintenance

### Sauvegardes
- **[GUIDE_SAUVEGARDE_RESTAURATION.md](GUIDE_SAUVEGARDE_RESTAURATION.md)** - Protection de vos données
  - Configuration des sauvegardes automatiques
  - Créer une sauvegarde manuelle
  - Restaurer des données
  - Scénarios de récupération

### Audit et Traçabilité
- **[GUIDE_AUDIT.md](GUIDE_AUDIT.md)** - Suivi des actions
  - Journal d'audit
  - Traçabilité des modifications
  - Rapports de conformité

---

## 🛠️ Guides Techniques

### Pour les Développeurs
- Architecture de l'application
- Structure du code
- Modèles de données
- Services et interfaces

### Base de Données
- Schéma de la base de données
- Connexion SQL Server
- Migrations
- Optimisation des performances

---

## 📋 Référence Rapide

### Raccourcis Clavier
| Raccourci | Action |
|-----------|--------|
| `Ctrl + N` | Nouveau (Résident/Paiement selon la vue) |
| `Ctrl + S` | Enregistrer |
| `Ctrl + P` | Imprimer |
| `Ctrl + E` | Exporter |
| `F5` | Actualiser |
| `Esc` | Annuler/Fermer dialogue |

### Identifiants par Défaut
- **Administrateur** : `admin` / `admin123`
- ⚠️ À changer immédiatement après installation !

### Emplacements Importants
- **Base de données** : `C:\ProgramData\ElMansourSyndic\ElMansourDB.mdf`
- **Sauvegardes** : `C:\ProgramData\ElMansourSyndic\Backups`
- **Logs** : `C:\ProgramData\ElMansourSyndic\Logs`
- **Reçus PDF** : `C:\ProgramData\ElMansourSyndic\Receipts`

---

## 🆘 Résolution de Problèmes

### Problèmes Courants
Consultez la section [Problèmes Courants](README.md#-problèmes-courants) du README.

### Erreurs Fréquentes

#### "Impossible de se connecter à la base de données"
→ Vérifiez que SQL Server LocalDB est installé
→ Consultez les logs dans `C:\ProgramData\ElMansourSyndic\Logs`

#### "Erreur lors de la génération du reçu"
→ Vérifiez que le logo est présent
→ Vérifiez les permissions du dossier `Receipts`

#### "Sauvegarde automatique échouée"
→ Vérifiez l'espace disque disponible
→ Consultez [GUIDE_SAUVEGARDE_RESTAURATION.md](GUIDE_SAUVEGARDE_RESTAURATION.md)

---

## 📞 Support

### Avant de Contacter le Support
1. ✅ Consultez la documentation pertinente
2. ✅ Vérifiez les logs d'erreur
3. ✅ Notez les étapes pour reproduire le problème
4. ✅ Faites une capture d'écran de l'erreur

### Informations à Fournir
- Version de l'application (voir Paramètres → À propos)
- Système d'exploitation (Windows 10/11)
- Message d'erreur exact
- Fichier de log récent

---

## 🎓 Formations et Tutoriels

### Vidéos (À venir)
- Installation et première utilisation (10 min)
- Gestion quotidienne des paiements (5 min)
- Génération de rapports mensuels (8 min)
- Sauvegardes et restauration (6 min)

### Webinaires
- Session de questions/réponses mensuelle
- Nouveautés et mises à jour
- Bonnes pratiques de gestion

---

## 📝 Notes de Version

### Version Actuelle : 1.0.0
- Gestion complète des résidents
- Encaissement et suivi des paiements
- Calcul automatique des arriérés
- Génération de reçus PDF
- Exports Excel/CSV
- Système de sauvegarde automatique
- Gestion des rôles et permissions
- Audit complet des actions

### Prochaines Fonctionnalités
- Notifications par email automatiques
- Application mobile (consultation)
- Intégration bancaire
- Signature électronique des reçus

---

## 🤝 Contribuer

Vous avez des suggestions d'amélioration ?
- Envoyez vos idées à : feedback@elmansour-syndic.local
- Participez aux sondages utilisateurs
- Testez les versions bêta

---

## 📄 Licence et Mentions Légales

© 2026 El Mansour Syndic Manager - Tous droits réservés

Cette application est destinée à un usage professionnel pour la gestion de syndicats de copropriété.

---

**Dernière mise à jour** : 15 janvier 2026
