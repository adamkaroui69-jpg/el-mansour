# Guide de Première Utilisation

Bienvenue dans El Mansour Syndic Manager ! Ce guide vous accompagne pas à pas pour configurer et utiliser l'application.

## 📋 Checklist de Démarrage

- [ ] Installation terminée
- [ ] Première connexion effectuée
- [ ] Mot de passe administrateur changé
- [ ] Logo de la résidence ajouté
- [ ] Premier résident créé
- [ ] Premier paiement enregistré
- [ ] Sauvegarde automatique configurée

---

## Étape 1 : Première Connexion (2 min)

### Se Connecter
1. Lancez l'application
2. Entrez les identifiants par défaut :
   - **Identifiant** : `admin`
   - **Mot de passe** : `admin123`
3. Cliquez sur **"Connexion"**

### Changer le Mot de Passe (OBLIGATOIRE)
1. Une fois connecté, cliquez sur votre nom en haut à droite
2. Sélectionnez **"Mon Profil"**
3. Cliquez sur **"Changer le mot de passe"**
4. Entrez un nouveau mot de passe sécurisé
5. **Notez-le dans un endroit sûr !**

---

## Étape 2 : Configuration Initiale (5 min)

### Ajouter le Logo de la Résidence
1. Allez dans **"Paramètres"** (icône engrenage)
2. Section **"Personnalisation"**
3. Cliquez sur **"Choisir le logo"**
4. Sélectionnez votre fichier PNG/JPG
5. Le logo apparaîtra sur tous les reçus

### Configurer les Sauvegardes
1. Dans **"Paramètres"** → **"Sauvegardes"**
2. Activez **"Sauvegarde automatique"**
3. Choisissez la fréquence : **"Quotidien"** (recommandé)
4. Cliquez sur **"Sauvegarder Maintenant"** pour créer la première sauvegarde

---

## Étape 3 : Ajouter les Résidents (10-30 min)

### Créer le Premier Résident
1. Cliquez sur **"Résidents"** dans le menu de gauche
2. Cliquez sur le bouton **"+ Nouveau Résident"**
3. Remplissez les informations :
   - **Code Maison** : Ex: `A-101` (identifiant unique)
   - **Code Bâtiment** : Ex: `A`
   - **Nom du Propriétaire** : Nom complet
   - **Téléphone** : Numéro de contact
   - **Email** : Adresse email (optionnel)
   - **Cotisation Mensuelle** : Ex: `200.00` TND
4. Cliquez sur **"Enregistrer"**

### Importer Plusieurs Résidents (Optionnel)
Si vous avez déjà une liste Excel :
1. Préparez un fichier CSV avec les colonnes :
   ```
   CodeMaison;CodeBatiment;NomProprietaire;Telephone;Email;CotisationMensuelle
   A-101;A;Ahmed Ben Ali;98765432;ahmed@email.com;200.00
   A-102;A;Fatma Trabelsi;91234567;fatma@email.com;200.00
   ```
2. Dans **"Résidents"**, cliquez sur **"Importer"**
3. Sélectionnez votre fichier CSV
4. Vérifiez l'aperçu et validez

---

## Étape 4 : Enregistrer les Premiers Paiements (5 min)

### Encaisser un Paiement
1. Allez dans **"Paiements"**
2. Cliquez sur **"+ Nouveau Paiement"**
3. Remplissez :
   - **Résident** : Sélectionnez dans la liste
   - **Mois** : Ex: `Janvier 2026`
   - **Montant** : Ex: `200.00`
   - **Date de Paiement** : Date du jour (par défaut)
   - **Numéro de Référence** : Numéro de chèque/virement (optionnel)
4. Cliquez sur **"Enregistrer"**
5. Un reçu PDF est généré automatiquement

### Imprimer ou Envoyer le Reçu
- **Imprimer** : Cliquez sur l'icône imprimante
- **Télécharger** : Cliquez sur l'icône téléchargement
- **Envoyer par Email** : Cliquez sur l'icône email (si configuré)

---

## Étape 5 : Consulter les Rapports (3 min)

### Tableau de Bord
1. Cliquez sur **"Tableau de Bord"** (page d'accueil)
2. Vous verrez :
   - Total des encaissements du mois
   - Nombre de paiements en attente
   - Montant total des arriérés
   - Graphiques d'évolution

### État des Comptes
1. Allez dans **"Rapports"** → **"État des Comptes"**
2. Vous verrez pour chaque résident :
   - **Vert** : À jour
   - **Bleu** : En avance (a payé plusieurs mois)
   - **Orange** : Retard léger (1-2 mois)
   - **Rouge** : Retard critique (3+ mois)

### Exporter un Rapport
1. Dans n'importe quel rapport, cliquez sur **"Exporter"**
2. Choisissez le format :
   - **Excel** : Pour analyse approfondie
   - **CSV** : Pour import dans d'autres logiciels
3. Sélectionnez l'emplacement et enregistrez

---

## Étape 6 : Gestion Multi-Utilisateurs (Optionnel)

### Créer un Utilisateur
1. Allez dans **"Paramètres"** → **"Utilisateurs"**
2. Cliquez sur **"+ Nouvel Utilisateur"**
3. Remplissez :
   - **Nom d'utilisateur** : Ex: `comptable`
   - **Mot de passe** : Mot de passe temporaire
   - **Rôle** : Choisissez parmi :
     - **Administrateur** : Accès total
     - **Gestionnaire** : Paiements + Rapports
     - **Consultant** : Lecture seule
4. Enregistrez

### Configurer les Permissions (Avancé)
1. Dans **"Paramètres"** → **"Rôles et Permissions"**
2. Créez un rôle personnalisé
3. Cochez les permissions souhaitées
4. Assignez le rôle à un utilisateur

---

## 🎯 Bonnes Pratiques

### Quotidien
- ✅ Enregistrez les paiements le jour même
- ✅ Vérifiez les notifications (icône cloche)
- ✅ Imprimez ou envoyez les reçus immédiatement

### Hebdomadaire
- ✅ Consultez l'état des arriérés
- ✅ Relancez les résidents en retard (via email ou téléphone)
- ✅ Vérifiez le tableau de bord

### Mensuel
- ✅ Générez le rapport financier mensuel
- ✅ Exportez les données pour l'assemblée générale
- ✅ Vérifiez que la sauvegarde automatique fonctionne

### Annuel
- ✅ Archivez les données de l'année précédente
- ✅ Mettez à jour les cotisations mensuelles si nécessaire
- ✅ Créez une sauvegarde manuelle pour l'archivage

---

## ⚠️ Erreurs à Éviter

### ❌ Ne PAS supprimer un paiement validé
→ Si erreur, créez un paiement négatif (régularisation)

### ❌ Ne PAS modifier le Code Maison d'un résident
→ Créez un nouveau résident si déménagement

### ❌ Ne PAS désactiver les sauvegardes automatiques
→ Vos données sont précieuses !

### ❌ Ne PAS partager le mot de passe administrateur
→ Créez des comptes utilisateurs distincts

---

## 🆘 Besoin d'Aide ?

### Problème Technique
1. Consultez la section **"Problèmes Courants"** du README
2. Vérifiez les logs dans **"Paramètres"** → **"Journaux"**
3. Restaurez une sauvegarde si nécessaire

### Question Fonctionnelle
- Consultez le **GUIDE_UX_MODERNE.md** pour les fonctionnalités avancées
- Consultez le **GUIDE_FINANCIER_EXPORT.md** pour les calculs financiers

---

## ✅ Vous êtes Prêt !

Félicitations ! Vous maîtrisez maintenant les bases de l'application.

**Prochaines étapes suggérées :**
1. Importez tous vos résidents
2. Enregistrez les paiements des 3 derniers mois
3. Générez votre premier rapport mensuel
4. Configurez les notifications par email (optionnel)

Bonne gestion ! 🎉
