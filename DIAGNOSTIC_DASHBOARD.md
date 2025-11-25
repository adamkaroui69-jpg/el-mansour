# Diagnostic du Problème Dashboard

## Problème Observé
- Le dashboard affiche 0 TND pour Total Collecté, Dépenses et Solde
- La maison A01 n'apparaît pas dans la liste des "Retards de Paiement"
- Les maisons A02-A09 apparaissent bien dans la liste

## Causes Possibles

### 1. La maison A01 n'existe pas dans la base de données
La maison A01 pourrait ne pas être enregistrée dans la table `Houses` de la base de données.

**Solution** : Vérifier dans le menu "Utilisateurs" (si vous êtes Admin) si la maison A01 existe.

### 2. La maison A01 n'est pas active
La maison A01 pourrait exister mais être marquée comme inactive.

**Solution** : Vérifier le statut de la maison A01 dans la base de données.

### 3. La maison A01 a déjà un paiement enregistré pour le mois en cours
Si un paiement avec le statut "Payé" existe déjà pour A01 pour le mois en cours, elle ne sera pas affichée dans les retards.

**Solution** : Vérifier dans le menu "Paiements" si un paiement existe déjà pour A01.

### 4. Les statistiques sont à 0 car il n'y a aucun paiement dans la base
Si aucun paiement n'a été enregistré dans la base de données, les statistiques seront à 0.

**Solution** : Vérifier qu'il y a bien des paiements enregistrés dans le menu "Paiements".

## Actions à Faire

### Étape 1 : Vérifier si A01 existe
1. Allez dans le menu **Utilisateurs** (nécessite droits Admin)
2. Cherchez la maison **A01**
3. Si elle n'existe pas, créez-la

### Étape 2 : Vérifier les paiements
1. Allez dans le menu **Paiements**
2. Sélectionnez le mois en cours
3. Vérifiez s'il y a des paiements enregistrés
4. Vérifiez si A01 a un paiement pour le mois en cours

### Étape 3 : Vérifier la base de données
Si le problème persiste, il faut vérifier directement la base de données.

**Emplacement de la base de données** :
- Sur le PC de l'utilisateur : `C:\Users\[NomUtilisateur]\AppData\Local\ElMansourSyndicManager\data\elmansour.db`
- Ou dans le dossier d'installation de l'application

## Solution Temporaire

Si vous voulez que A01 apparaisse dans les retards de paiement :
1. Assurez-vous que la maison A01 existe dans la base de données
2. Assurez-vous qu'elle est active
3. Assurez-vous qu'elle n'a PAS de paiement enregistré pour le mois en cours

## Besoin d'Aide ?

Si le problème persiste après avoir vérifié ces points, envoyez-moi :
1. Une capture d'écran du menu "Paiements" avec le mois en cours sélectionné
2. Une capture d'écran du menu "Utilisateurs" montrant la liste des maisons
3. Le fichier de log : `[Dossier de l'application]\data\logs\dashboard_debug.txt`

Ce fichier contient des informations de diagnostic qui m'aideront à identifier le problème.
