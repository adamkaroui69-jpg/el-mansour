# Guide Sauvegarde & Restauration

Guide complet pour protéger et récupérer vos données.

## 🛡️ Pourquoi Sauvegarder ?

Les sauvegardes protègent contre :
- ❌ Panne matérielle (disque dur défaillant)
- ❌ Erreur humaine (suppression accidentelle)
- ❌ Corruption de données
- ❌ Virus ou ransomware
- ❌ Mise à jour problématique

**Règle d'or** : Une sauvegarde par jour éloigne les problèmes pour toujours !

---

## 📦 Types de Sauvegardes

### 1. Sauvegarde Automatique (Recommandée)
- **Fréquence** : Quotidienne à 2h du matin
- **Emplacement** : `C:\ProgramData\ElMansourSyndic\Backups`
- **Rétention** : 30 dernières sauvegardes
- **Avantage** : Aucune action requise

### 2. Sauvegarde Manuelle
- **Quand** : Avant une opération importante (import massif, mise à jour, etc.)
- **Emplacement** : Même dossier que l'automatique
- **Avantage** : Contrôle total du moment

### 3. Sauvegarde Externe (Fortement Recommandée)
- **Fréquence** : Hebdomadaire ou mensuelle
- **Emplacement** : Clé USB, disque externe, cloud
- **Avantage** : Protection contre panne PC

---

## ⚙️ Configuration des Sauvegardes

### Activer la Sauvegarde Automatique

1. Ouvrez l'application
2. Allez dans **"Paramètres"** (icône engrenage ⚙️)
3. Cliquez sur l'onglet **"Sauvegardes"**
4. Activez **"Sauvegarde automatique"**
5. Choisissez la fréquence :
   - **Quotidien** ✅ (Recommandé)
   - Hebdomadaire
   - Mensuel
6. Cliquez sur **"Enregistrer"**

### Modifier l'Emplacement de Sauvegarde (Avancé)

Par défaut : `C:\ProgramData\ElMansourSyndic\Backups`

Pour changer :
1. Fermez l'application
2. Ouvrez le fichier `appsettings.json` (dans le dossier d'installation)
3. Modifiez la section :
   ```json
   "BackupSettings": {
     "BackupPath": "D:\\MesSauvegardes\\ElMansour"
   }
   ```
4. Enregistrez et relancez l'application

---

## 💾 Créer une Sauvegarde Manuelle

### Méthode 1 : Depuis l'Application

1. Ouvrez **"Paramètres"** → **"Sauvegardes"**
2. Cliquez sur le bouton **"Sauvegarder Maintenant"**
3. Attendez le message de confirmation (quelques secondes)
4. La sauvegarde apparaît dans l'historique

### Méthode 2 : Copie Manuelle de la Base de Données

**Emplacement de la base** : `C:\ProgramData\ElMansourSyndic\ElMansourDB.mdf`

1. **Fermez l'application** (important !)
2. Naviguez vers `C:\ProgramData\ElMansourSyndic\`
3. Copiez les fichiers :
   - `ElMansourDB.mdf`
   - `ElMansourDB_log.ldf`
4. Collez-les dans un dossier sécurisé (ex: clé USB)
5. Renommez avec la date : `ElMansourDB_2026-01-15.mdf`

---

## 🔄 Restaurer une Sauvegarde

### ⚠️ ATTENTION
La restauration **REMPLACE** toutes les données actuelles par celles de la sauvegarde.
**Une sauvegarde de sécurité de l'état actuel sera créée automatiquement.**

### Restauration depuis l'Application

1. Ouvrez **"Paramètres"** → **"Sauvegardes"**
2. Dans la liste **"Historique des sauvegardes"**, trouvez la sauvegarde souhaitée
3. Vérifiez la date et l'heure
4. Cliquez sur le bouton **"Restaurer"** (icône ↻)
5. Lisez attentivement l'avertissement
6. Cliquez sur **"Oui, Restaurer"**
7. Attendez la fin du processus (ne fermez pas l'application)
8. L'application redémarre automatiquement
9. Reconnectez-vous

### Restauration Manuelle (Si l'application ne démarre pas)

1. **Fermez l'application** si elle est ouverte
2. Naviguez vers `C:\ProgramData\ElMansourSyndic\Backups`
3. Trouvez le fichier de sauvegarde (ex: `Backup_2026-01-15_02-00-00.bak`)
4. Copiez-le dans un endroit sûr
5. Allez dans `C:\ProgramData\ElMansourSyndic\`
6. **Supprimez** (ou renommez) les fichiers :
   - `ElMansourDB.mdf`
   - `ElMansourDB_log.ldf`
7. Utilisez SQL Server Management Studio (SSMS) pour restaurer :
   ```sql
   RESTORE DATABASE ElMansourDB
   FROM DISK = 'C:\ProgramData\ElMansourSyndic\Backups\Backup_2026-01-15_02-00-00.bak'
   WITH REPLACE
   ```
8. Relancez l'application

---

## 📤 Exporter une Sauvegarde (Pour Archivage)

### Sauvegarde Complète sur Clé USB

**Fréquence recommandée** : Chaque fin de mois

1. Insérez une clé USB
2. Créez un dossier : `ElMansour_Backup_Janvier2026`
3. Copiez dedans :
   - Le dernier fichier de `C:\ProgramData\ElMansourSyndic\Backups`
   - Le logo (si personnalisé)
   - Les rapports Excel exportés (optionnel)
4. Éjectez la clé en toute sécurité
5. Rangez-la dans un endroit sûr

### Sauvegarde Cloud (Optionnel)

Utilisez OneDrive, Google Drive, ou Dropbox :

1. Installez l'application cloud sur votre PC
2. Créez un dossier synchronisé : `ElMansour_Backups`
3. Configurez l'application pour sauvegarder dans ce dossier :
   - Modifiez `appsettings.json` → `BackupPath`
   - Pointez vers `C:\Users\VotreNom\OneDrive\ElMansour_Backups`
4. Les sauvegardes seront automatiquement envoyées dans le cloud

---

## 🧪 Tester une Restauration

**Important** : Testez régulièrement que vos sauvegardes fonctionnent !

### Test Mensuel Recommandé

1. Notez l'état actuel (nombre de résidents, dernier paiement)
2. Créez une sauvegarde manuelle
3. Modifiez quelque chose (ex: ajoutez un résident test)
4. Restaurez la sauvegarde créée à l'étape 2
5. Vérifiez que le résident test a disparu
6. ✅ Si oui, vos sauvegardes fonctionnent !

---

## 📊 Gestion de l'Espace Disque

### Taille des Sauvegardes

- **Petite résidence** (< 50 résidents) : ~5-10 Mo par sauvegarde
- **Moyenne résidence** (50-200 résidents) : ~10-50 Mo
- **Grande résidence** (200+ résidents) : ~50-200 Mo

### Nettoyage Automatique

L'application conserve automatiquement les **30 dernières sauvegardes**.
Les plus anciennes sont supprimées automatiquement.

### Nettoyage Manuel

Si vous manquez d'espace :

1. Allez dans **"Paramètres"** → **"Sauvegardes"**
2. Dans l'historique, sélectionnez les anciennes sauvegardes
3. Cliquez sur **"Supprimer"** (icône poubelle 🗑️)
4. Confirmez

**Conseil** : Gardez au minimum :
- Les 7 derniers jours (quotidiennes)
- 1 sauvegarde par mois des 12 derniers mois
- 1 sauvegarde annuelle

---

## 🚨 Scénarios de Récupération

### Scénario 1 : Suppression Accidentelle
**Problème** : J'ai supprimé un résident par erreur ce matin.

**Solution** :
1. Restaurez la sauvegarde automatique d'hier soir
2. Vous perdrez les paiements d'aujourd'hui
3. Ressaisissez les paiements du jour

### Scénario 2 : Corruption de Données
**Problème** : L'application affiche des erreurs bizarres.

**Solution** :
1. Notez l'erreur exacte
2. Restaurez la dernière sauvegarde fonctionnelle
3. Si le problème persiste, contactez le support

### Scénario 3 : Changement de PC
**Problème** : Je veux transférer l'application sur un nouvel ordinateur.

**Solution** :
1. Sur l'ancien PC :
   - Créez une sauvegarde manuelle
   - Copiez le fichier sur une clé USB
2. Sur le nouveau PC :
   - Installez l'application
   - Lancez-la une première fois (pour créer la structure)
   - Fermez-la
   - Copiez le fichier de sauvegarde dans `C:\ProgramData\ElMansourSyndic\Backups`
   - Relancez et restaurez

### Scénario 4 : Panne Totale du PC
**Problème** : Mon PC ne démarre plus.

**Solution** :
- ✅ **Si vous avez une sauvegarde externe** : Installez sur un nouveau PC et restaurez
- ❌ **Si pas de sauvegarde externe** : Récupération difficile, contactez un technicien

**Morale** : Faites des sauvegardes externes régulières !

---

## ✅ Checklist de Sécurité

### Configuration Initiale
- [ ] Sauvegarde automatique activée
- [ ] Première sauvegarde manuelle créée
- [ ] Emplacement de sauvegarde vérifié
- [ ] Test de restauration effectué

### Routine Hebdomadaire
- [ ] Vérifier que les sauvegardes automatiques fonctionnent
- [ ] Copier la dernière sauvegarde sur clé USB (optionnel)

### Routine Mensuelle
- [ ] Tester une restauration
- [ ] Archiver la sauvegarde du mois sur support externe
- [ ] Nettoyer les anciennes sauvegardes si nécessaire

### Avant Opération Importante
- [ ] Créer une sauvegarde manuelle
- [ ] Vérifier l'espace disque disponible
- [ ] Noter l'heure de la sauvegarde

---

## 📞 Support

En cas de problème avec les sauvegardes :
1. Consultez les logs : **"Paramètres"** → **"Journaux"**
2. Vérifiez l'espace disque disponible
3. Essayez une restauration manuelle
4. Contactez le support technique

**Fichiers à fournir au support** :
- Capture d'écran de l'erreur
- Fichier de log (dans `C:\ProgramData\ElMansourSyndic\Logs`)
- Date et heure du problème

---

## 🎯 Résumé

**3 Règles d'Or** :
1. ✅ Activez la sauvegarde automatique
2. ✅ Faites une copie externe chaque mois
3. ✅ Testez vos restaurations régulièrement

**Vos données sont précieuses. Protégez-les !** 🛡️
