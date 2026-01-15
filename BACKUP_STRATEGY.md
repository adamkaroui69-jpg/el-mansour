# Stratégie de Sauvegarde et de Prévention de Perte de Données

Ce document détaille l'architecture mise en place pour garantir "Zéro Perte de Données" dans l'application **El Mansour Syndic Manager**.

## 1. Vue d'Ensemble

L'objectif est d'assurer la résilience des données face aux pannes matérielles, aux erreurs logicielles ou aux mauvaises manipulations.
Le système gère deux types de bases de données (SQLite Local et SQL Server Distant) et préserve également les fichiers documents (reçus, factures).

## 2. Types de Sauvegardes

### A. Sauvegarde Automatique (Planifiée)
- **Fréquence** : Configurable (par défaut : Quotidienne à l'arrêt ou via Timer).
- **Déclencheur** : 
  1. `App.OnExit` : Une sauvegarde rapide est tentée à chaque fermeture propre.
  2. `Timer` : Une tâche de fond s'exécute si l'application reste ouverte longtemps (ex: serveurs).
- **Rétention** : Les 10 dernières sauvegardes sont conservées (configurable).

### B. Sauvegarde Manuelle
- **Accessibilité** : Bouton "Sauvegarder maintenant" dans les Paramètres.
- **Usage** : Avant une intervention majeure ou une mise à jour.

## 3. Architecture des Fichiers de Sauvegarde

Les sauvegardes sont stockées sous forme d'archives ZIP chiffrées (`.zip.encrypted`) pour garantir la confidentialité (RGPD).

**Structure d'une archive déchiffrée :**
```
Backup_2026-01-15_120000/
├── metadata.json       # Méta-données (Date, Version App, Type, Hash)
├── database.db         # Copie de la base SQLite (ou Export JSON pour SQL Server)
└── files/              # Documents liés
    ├── Receipts/       # Reçus PDF générés
    ├── Documents/      # Scans et contrats
    └── reports/        # Rapports exportés
```

## 4. Stratégie Technique par Fournisseur de Données

### Cas 1 : SQLite (Local)
C'est le mode par défaut.
- **Défi** : Copier un fichier `.db` ouvert peut corrompre la copie si des écritures sont en cours (Mode WAL).
- **Solution** :
  1. Forcer un `Checkpoint` (écriture des journaux WAL dans la base principale).
  2. Utiliser le "Vacuum" ou une copie avec verrouillage partagé si possible, ou simplement copier `.db`, `.db-shm`, `.db-wal` si l'application est active.
  3. **Note** : Notre implémentation actuelle ferme les connexions ou s'assure d'être dans un état stable avant la copie.

### Cas 2 : SQL Server (Distant)
Pour une architecture client lourd avec base distante.
- **Défi** : Le client ne peut pas copier les fichiers `.mdf` du serveur.
- **Solution "Client-Side"** :
  1. Exporter les données critiques en JSON (via les Repositories).
  2. Avantage : Indépendant du moteur de BDD, facile à relire.
  3. Inconvénient : Plus lent pour les gros volumes.
- **Solution "Server-Side" (Recommandée)** :
  1. Exécuter une commande SQL `BACKUP DATABASE` sur le serveur.
  2. Nécessite des droits d'admin sur le serveur SQL.

## 5. Processus de Restauration

La restauration est une opération critique et destructive pour les données actuelles.

1. **Validation** : Vérification de l'intégrité de l'archive (Hash/Déchiffrement).
2. **Sauvegarde de précaution** : Une sauvegarde de l'état *actuel* est faite avant d'écraser.
3. **Remplacement** :
   - Fichiers : Suppression et remplacement du dossier `data/files`.
   - Base de données : Remplacement du fichier `.db` (SQLite) ou Réimportation des données (SQL Server).
4. **Redémarrage** : L'application doit redémarrer pour recharger les connexions.

## 6. Bonnes Pratiques de Sécurité

1.  **Chiffrement** : Toutes les archives sont chiffrées (AES-256) avec une clé dérivée (ne jamais stocker de données sensibles en clair).
2.  **Portabilité** : Les chemins sont relatifs (`data/backups`) pour faciliter le déplacement de l'installation.
3.  **Atomicité** : La restauration est "Tout ou Rien". Si une étape échoue, on revient en arrière.

---

## Exemple de Code (Interface Service)

```csharp
public interface IBackupService
{
    // Lance une sauvegarde (Auto ou Manuelle)
    Task<BackupHistoryDTO> RunBackupAsync(bool isAutomatic, CancellationToken token);
    
    // Restaure depuis une archive locale
    Task<bool> RestoreBackupAsync(string backupFilePath, CancellationToken token);
    
    // Configure la planification
    Task ScheduleBackupsAsync(bool enabled, TimeSpan? time, CancellationToken token);
}
```
