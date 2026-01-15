# FAQ - Questions Fréquentes

Réponses rapides aux questions les plus courantes.

## 🔐 Connexion et Compte

### Q : J'ai oublié mon mot de passe, que faire ?
**R :** Si vous êtes administrateur :
1. Utilisez le compte par défaut : `admin` / `admin123`
2. Si vous l'avez changé et oublié, vous devrez réinstaller l'application

Si vous êtes utilisateur standard, contactez votre administrateur.

### Q : Puis-je avoir plusieurs utilisateurs ?
**R :** Oui ! Allez dans **Paramètres** → **Utilisateurs** pour créer des comptes avec différents rôles (Administrateur, Gestionnaire, Consultant).

### Q : Comment changer mon mot de passe ?
**R :** Cliquez sur votre nom en haut à droite → **Mon Profil** → **Changer le mot de passe**.

---

## 👥 Gestion des Résidents

### Q : Puis-je importer une liste de résidents depuis Excel ?
**R :** Oui ! Préparez un fichier CSV avec les colonnes requises, puis dans **Résidents** → **Importer**.

### Q : Que signifie "Code Maison" ?
**R :** C'est l'identifiant unique de chaque logement (ex: A-101, B-205). Il ne doit jamais être modifié une fois créé.

### Q : Comment gérer un changement de propriétaire ?
**R :** 
1. Désactivez l'ancien résident (ne le supprimez pas !)
2. Créez un nouveau résident avec le même Code Maison
3. L'historique des paiements reste lié au Code Maison

### Q : Puis-je supprimer un résident ?
**R :** Oui, mais c'est déconseillé. Préférez le désactiver pour conserver l'historique.

---

## 💰 Paiements et Reçus

### Q : Comment corriger un paiement enregistré par erreur ?
**R :** 
- **Méthode recommandée** : Créez un paiement négatif (régularisation)
- **À éviter** : Supprimer le paiement (perte de traçabilité)

### Q : Le reçu ne s'affiche pas, pourquoi ?
**R :** Vérifiez que :
1. Le logo est présent dans le dossier d'installation
2. Vous avez les permissions d'écriture dans `C:\ProgramData\ElMansourSyndic\Receipts`
3. Un lecteur PDF est installé sur votre PC

### Q : Puis-je personnaliser le reçu ?
**R :** Actuellement, seul le logo est personnalisable. Le format du reçu est standardisé.

### Q : Comment envoyer un reçu par email ?
**R :** Cliquez sur l'icône email dans la liste des paiements. Vous devez avoir configuré les paramètres SMTP dans **Paramètres** → **Email**.

---

## 📊 Rapports et Arriérés

### Q : Comment sont calculés les arriérés ?
**R :** L'application utilise une logique FIFO (First In, First Out) :
- Chaque mois depuis la création du résident génère une dette
- Les paiements "remplissent" les mois les plus anciens en premier
- Le solde = Total payé - Total dû

Consultez [GUIDE_FINANCIER_EXPORT.md](GUIDE_FINANCIER_EXPORT.md) pour plus de détails.

### Q : Que signifient les couleurs dans l'état des comptes ?
**R :**
- 🟢 **Vert** : Résident à jour
- 🔵 **Bleu** : Résident en avance (a payé plusieurs mois)
- 🟠 **Orange** : Retard léger (1-2 mois)
- 🔴 **Rouge** : Retard critique (3+ mois)

### Q : Comment exporter un rapport ?
**R :** Dans n'importe quelle vue, cliquez sur **Exporter** → Choisissez Excel (.xlsx) ou CSV.

### Q : Puis-je filtrer les rapports par période ?
**R :** Oui, utilisez les filtres en haut de chaque vue (Date de début, Date de fin).

---

## 💾 Sauvegardes

### Q : Où sont stockées mes sauvegardes ?
**R :** Par défaut dans `C:\ProgramData\ElMansourSyndic\Backups`.

### Q : Combien de sauvegardes sont conservées ?
**R :** Les 30 dernières sauvegardes automatiques. Les plus anciennes sont supprimées automatiquement.

### Q : Comment faire une sauvegarde sur clé USB ?
**R :** 
1. Créez une sauvegarde manuelle dans l'application
2. Copiez le fichier depuis `C:\ProgramData\ElMansourSyndic\Backups` vers votre clé USB

Consultez [GUIDE_SAUVEGARDE_RESTAURATION.md](GUIDE_SAUVEGARDE_RESTAURATION.md) pour plus de détails.

### Q : La restauration supprime-t-elle mes données actuelles ?
**R :** Oui, mais une sauvegarde de sécurité de l'état actuel est créée automatiquement avant la restauration.

---

## 🖥️ Technique

### Q : Quelle base de données utilise l'application ?
**R :** SQL Server LocalDB (inclus avec l'installation) ou SQL Server distant.

### Q : Puis-je utiliser l'application sur plusieurs PC ?
**R :** Oui, en configurant une base de données SQL Server partagée sur le réseau. Modifiez la chaîne de connexion dans `appsettings.json`.

### Q : L'application fonctionne-t-elle hors ligne ?
**R :** Oui, elle est 100% locale (sauf si vous utilisez une base distante).

### Q : Quelle est la configuration minimale requise ?
**R :**
- Windows 10/11
- 4 Go RAM
- 500 Mo d'espace disque
- .NET 8.0 Runtime (installé automatiquement)

---

## 🔧 Problèmes Courants

### Q : L'application ne démarre pas
**R :** 
1. Vérifiez que .NET 8.0 est installé
2. Exécutez en tant qu'administrateur (clic droit → "Exécuter en tant qu'administrateur")
3. Consultez les logs dans `C:\ProgramData\ElMansourSyndic\Logs`

### Q : Message "Erreur de connexion à la base de données"
**R :**
1. Vérifiez que SQL Server LocalDB est installé
2. Vérifiez les permissions sur `C:\ProgramData\ElMansourSyndic`
3. Essayez de restaurer une sauvegarde

### Q : L'application est lente
**R :**
1. Fermez les autres applications
2. Vérifiez l'espace disque disponible (minimum 1 Go libre)
3. Nettoyez les anciennes sauvegardes
4. Si vous avez beaucoup de données (1000+ résidents), contactez le support pour optimisation

### Q : Les icônes ne s'affichent pas
**R :** 
1. Redémarrez l'application
2. Vérifiez votre connexion internet (pour le chargement des polices Material Design)
3. Réinstallez l'application

---

## 📱 Fonctionnalités

### Q : Y a-t-il une application mobile ?
**R :** Pas encore, mais c'est prévu dans une prochaine version.

### Q : Puis-je envoyer des SMS aux résidents ?
**R :** Pas directement, mais vous pouvez exporter la liste avec les numéros de téléphone et utiliser un service externe.

### Q : L'application gère-t-elle plusieurs résidences ?
**R :** Non, une installation = une résidence. Pour gérer plusieurs résidences, installez l'application plusieurs fois dans des dossiers différents.

### Q : Puis-je personnaliser les montants de cotisation par résident ?
**R :** Oui ! Chaque résident a son propre champ "Cotisation Mensuelle" modifiable.

---

## 💡 Bonnes Pratiques

### Q : À quelle fréquence dois-je sauvegarder ?
**R :** 
- **Automatique** : Quotidienne (activée par défaut)
- **Manuelle** : Avant toute opération importante
- **Externe** : Hebdomadaire ou mensuelle sur clé USB

### Q : Dois-je supprimer les anciens paiements ?
**R :** Non ! Conservez tout l'historique pour la traçabilité et les audits.

### Q : Comment préparer l'assemblée générale ?
**R :**
1. Exportez l'état des comptes (Excel)
2. Générez le rapport financier annuel
3. Créez une sauvegarde manuelle
4. Imprimez les rapports nécessaires

---

## 🆘 Support

### Q : Comment contacter le support ?
**R :** Email : support@elmansour-syndic.local

### Q : Quelles informations fournir au support ?
**R :**
- Version de l'application (Paramètres → À propos)
- Message d'erreur exact (capture d'écran)
- Fichier de log récent
- Étapes pour reproduire le problème

### Q : Y a-t-il une formation disponible ?
**R :** Oui, consultez [GUIDE_PREMIERE_UTILISATION.md](GUIDE_PREMIERE_UTILISATION.md) pour un guide complet pas à pas.

---

## 📚 Documentation Complète

Pour plus d'informations, consultez :
- **[DOCUMENTATION.md](DOCUMENTATION.md)** - Index de toute la documentation
- **[README.md](README.md)** - Vue d'ensemble
- **[GUIDE_PREMIERE_UTILISATION.md](GUIDE_PREMIERE_UTILISATION.md)** - Guide détaillé
- **[GUIDE_SAUVEGARDE_RESTAURATION.md](GUIDE_SAUVEGARDE_RESTAURATION.md)** - Sauvegardes
- **[GUIDE_FINANCIER_EXPORT.md](GUIDE_FINANCIER_EXPORT.md)** - Gestion financière

---

**Votre question n'est pas listée ?** Consultez la documentation complète ou contactez le support.
