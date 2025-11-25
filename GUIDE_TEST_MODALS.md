# 🧪 Guide de Test des Modals - ElMansourSyndicManager

## 📋 Objectif
Tester tous les formulaires modals de l'application pour vérifier :
- ✅ L'affichage correct des formulaires
- ✅ Les liaisons bidirectionnelles (bindings)
- ✅ Les validations
- ✅ La sauvegarde des données
- ✅ L'annulation sans sauvegarde

---

## 🚀 Préparation

### 1. Lancer l'application
```powershell
cd "c:\Users\adamk\Desktop\raisidance application\src\ElMansourSyndicManager"
dotnet run
```

### 2. Se connecter
- **Code Maison** : Utiliser un code existant (ex: A1, B2, etc.)
- **Code d'authentification** : Le code à 6 chiffres associé

---

## 📝 Tests à Effectuer

### ✅ Test 1 : PaymentsView - Création de Paiement

#### Étapes :
1. **Navigation** : Cliquer sur "Paiements" dans le menu
2. **Ouverture du formulaire** : Cliquer sur "Nouveau Paiement"
3. **Vérifications visuelles** :
   - [ ] Le formulaire s'affiche au centre avec un fond semi-transparent
   - [ ] Titre : "Nouveau Paiement"
   - [ ] Tous les champs sont visibles

4. **Remplir le formulaire** :
   - [ ] Sélectionner une maison dans la liste déroulante
   - [ ] Vérifier que le montant se remplit automatiquement
   - [ ] Modifier le montant si nécessaire
   - [ ] Sélectionner une date de paiement

5. **Test de validation** :
   - [ ] Essayer de soumettre sans sélectionner de maison
   - [ ] Vérifier qu'un message d'erreur s'affiche
   - [ ] Essayer avec un montant à 0
   - [ ] Vérifier qu'un message d'erreur s'affiche

6. **Test de sauvegarde** :
   - [ ] Remplir tous les champs correctement
   - [ ] Cliquer sur "Créer le Paiement"
   - [ ] Vérifier qu'un message de succès s'affiche
   - [ ] Vérifier que le formulaire se ferme
   - [ ] Vérifier que le nouveau paiement apparaît dans la liste

7. **Test d'annulation** :
   - [ ] Ouvrir à nouveau le formulaire
   - [ ] Remplir quelques champs
   - [ ] Cliquer sur "Annuler"
   - [ ] Vérifier que le formulaire se ferme sans sauvegarder

#### ✅ Résultat attendu :
- Formulaire fonctionnel avec validation
- Données sauvegardées correctement
- Annulation sans effet

---

### ✅ Test 2 : UsersView - Gestion des Utilisateurs

#### Étapes :
1. **Navigation** : Cliquer sur "Utilisateurs" dans le menu
2. **Test de création** :
   - [ ] Cliquer sur "Nouvel Utilisateur"
   - [ ] Vérifier le titre : "Nouvel Utilisateur"
   - [ ] Remplir les champs :
     - Prénom : "Test"
     - Nom : "Utilisateur"
     - Code Maison : "TEST1"
     - Mot de passe : "123456" (6 chiffres)
     - Rôle : Sélectionner "Resident"
     - Cocher "Compte actif"
   - [ ] Cliquer sur "Enregistrer"
   - [ ] Vérifier que l'utilisateur apparaît dans la liste

3. **Test de modification** :
   - [ ] Sélectionner l'utilisateur créé
   - [ ] Clic droit → "Modifier"
   - [ ] Vérifier le titre : "Modifier Utilisateur"
   - [ ] Vérifier que les champs sont pré-remplis
   - [ ] Vérifier que le champ "Code Maison" est désactivé (grisé)
   - [ ] Vérifier que le champ "Mot de passe" n'est PAS visible
   - [ ] Modifier le prénom
   - [ ] Cliquer sur "Enregistrer"
   - [ ] Vérifier que les modifications sont sauvegardées

4. **Test des converters** :
   - [ ] En mode création : vérifier que le champ "Mot de passe" est visible
   - [ ] En mode édition : vérifier que le champ "Mot de passe" est caché
   - [ ] En mode création : vérifier que le champ "Code Maison" est actif
   - [ ] En mode édition : vérifier que le champ "Code Maison" est désactivé

#### ✅ Résultat attendu :
- Création et modification fonctionnelles
- Converters fonctionnent correctement
- Champs conditionnels s'affichent/masquent selon le mode

---

### ✅ Test 3 : ExpensesView - Gestion des Dépenses

#### Étapes :
1. **Navigation** : Cliquer sur "Dépenses" dans le menu
2. **Création d'une dépense** :
   - [ ] Cliquer sur "Nouvelle Dépense"
   - [ ] Remplir les champs :
     - Description : "Réparation ascenseur"
     - Catégorie : "Maintenance"
     - Montant : "500"
     - Date : Sélectionner la date du jour
     - Notes : "Intervention urgente"
   - [ ] Cliquer sur "Enregistrer"
   - [ ] Vérifier que la dépense apparaît dans la liste
   - [ ] Vérifier que le montant s'affiche avec "TND"

3. **Modification d'une dépense** :
   - [ ] Sélectionner la dépense créée
   - [ ] Clic droit → "Modifier"
   - [ ] Modifier le montant
   - [ ] Cliquer sur "Enregistrer"
   - [ ] Vérifier les modifications

#### ✅ Résultat attendu :
- Création et modification fonctionnelles
- Montant affiché avec la devise TND

---

### ✅ Test 4 : MaintenanceView - Gestion de la Maintenance

#### Étapes :
1. **Navigation** : Cliquer sur "Maintenance" dans le menu
2. **Création d'une demande** :
   - [ ] Cliquer sur "Nouvelle Demande"
   - [ ] Remplir les champs :
     - Description : "Fuite d'eau appartement 3B"
     - Type : "Plumbing"
     - Priorité : "Urgent"
     - Coût Estimé : "300"
     - Assigné à : "Plombier Mohamed"
     - Date Prévue : Sélectionner demain
     - Notes : "Intervention rapide nécessaire"
   - [ ] Vérifier que le champ "Statut" n'est PAS visible (mode création)
   - [ ] Cliquer sur "Enregistrer"
   - [ ] Vérifier que la demande apparaît dans la liste

3. **Modification d'une demande** :
   - [ ] Sélectionner la demande créée
   - [ ] Clic droit → "Modifier"
   - [ ] Vérifier que le champ "Statut" EST visible (mode édition)
   - [ ] Changer le statut à "InProgress"
   - [ ] Modifier le coût
   - [ ] Cliquer sur "Enregistrer"
   - [ ] Vérifier les modifications

#### ✅ Résultat attendu :
- Champ "Statut" visible uniquement en mode édition
- Toutes les données sauvegardées correctement
- Coût affiché avec TND

---

### ✅ Test 5 : DocumentsView - Upload de Documents

#### Étapes :
1. **Navigation** : Cliquer sur "Documents" dans le menu
2. **Upload d'un document** :
   - [ ] Cliquer sur "Uploader"
   - [ ] Cliquer sur le bouton "..." pour parcourir
   - [ ] Sélectionner un fichier (PDF, image, etc.)
   - [ ] Vérifier que le chemin s'affiche dans le champ
   - [ ] Sélectionner une catégorie : "General"
   - [ ] Ajouter une description : "Règlement intérieur"
   - [ ] Cliquer sur "Uploader"
   - [ ] Vérifier que le document apparaît dans la liste

3. **Recherche et filtrage** :
   - [ ] Utiliser la barre de recherche
   - [ ] Filtrer par catégorie
   - [ ] Vérifier que les résultats sont corrects

4. **Ouverture d'un document** :
   - [ ] Sélectionner un document
   - [ ] Clic droit → "Ouvrir"
   - [ ] Vérifier que le document s'ouvre

#### ✅ Résultat attendu :
- Upload fonctionnel
- Recherche et filtrage opérationnels
- Ouverture de documents fonctionnelle

---

## 📊 Checklist Globale

### Validation Visuelle
- [ ] Tous les formulaires s'affichent correctement
- [ ] Les fonds semi-transparents sont visibles
- [ ] Les cartes sont centrées
- [ ] Les boutons sont bien alignés
- [ ] Les messages d'erreur s'affichent en rouge

### Validation Fonctionnelle
- [ ] Les données saisies sont bien liées au ViewModel
- [ ] Les validations fonctionnent
- [ ] Les sauvegardes persistent dans la base de données
- [ ] Les annulations ferment sans sauvegarder
- [ ] Les listes se rafraîchissent après sauvegarde

### Validation des Converters
- [ ] InverseBooleanConverter fonctionne (UsersView - Code Maison désactivé en édition)
- [ ] InverseBooleanToVisibilityConverter fonctionne (UsersView - Mot de passe caché en édition)
- [ ] BooleanToVisibilityConverter fonctionne (MaintenanceView - Statut visible en édition)

---

## 🐛 Problèmes Potentiels et Solutions

### Problème 1 : Le formulaire ne s'affiche pas
**Solution** : Vérifier que `IsFormVisible` est bien lié dans le XAML

### Problème 2 : Les données ne se sauvegardent pas
**Solution** : Vérifier les bindings `Mode=TwoWay`

### Problème 3 : Les validations ne fonctionnent pas
**Solution** : Vérifier les méthodes de validation dans les ViewModels

### Problème 4 : Les converters ne fonctionnent pas
**Solution** : Vérifier que les converters sont bien déclarés dans les ressources

---

## 📝 Rapport de Test

Après avoir effectué tous les tests, remplir ce tableau :

| Modal | Affichage | Création | Modification | Validation | Annulation | Statut |
|-------|-----------|----------|--------------|------------|------------|--------|
| PaymentsView | ⬜ | ⬜ | ⬜ | ⬜ | ⬜ | ⬜ |
| UsersView | ⬜ | ⬜ | ⬜ | ⬜ | ⬜ | ⬜ |
| ExpensesView | ⬜ | ⬜ | ⬜ | ⬜ | ⬜ | ⬜ |
| MaintenanceView | ⬜ | ⬜ | ⬜ | ⬜ | ⬜ | ⬜ |
| DocumentsView | ⬜ | ⬜ | N/A | ⬜ | ⬜ | ⬜ |

**Légende** :
- ✅ : Fonctionne correctement
- ⚠️ : Fonctionne avec des problèmes mineurs
- ❌ : Ne fonctionne pas
- N/A : Non applicable

---

## 🎯 Résultat Attendu Final

**Tous les modals doivent être ✅ dans toutes les catégories.**

Si vous rencontrez des problèmes, notez-les avec :
- Le modal concerné
- L'action effectuée
- Le comportement attendu
- Le comportement observé
- Les messages d'erreur éventuels

---

## 💡 Conseils

1. **Testez dans l'ordre** : Commencez par PaymentsView (le plus simple) pour vous familiariser
2. **Vérifiez la base de données** : Après chaque création, vérifiez que les données sont bien enregistrées
3. **Testez les cas limites** : Champs vides, montants négatifs, dates invalides, etc.
4. **Notez tout** : Même les petits problèmes d'UI peuvent être importants

---

## 🚀 Lancement Rapide

```powershell
# Lancer l'application
cd "c:\Users\adamk\Desktop\raisidance application\src\ElMansourSyndicManager"
dotnet run

# Ou compiler et lancer
dotnet build
.\bin\Debug\net8.0-windows\ElMansourSyndicManager.exe
```

**Bon test ! 🎉**
