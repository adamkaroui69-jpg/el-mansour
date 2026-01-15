# Architecture de Sécurité : Rôles et Permissions (RBAC)

## 1. Vue d'Ensemble
L'objectif est de remplacer la logique binaire "Admin/User" par un système flexible où :
1. Les actions sont protégées par des **Permissions** atomiques (ex: `Payments.Create`, `Reports.Export`).
2. Les Permissions sont regroupées dans des **Rôles** (ex: "Trésorier", "Secrétaire", "Auditeur").
3. Les Utilisateurs sont assignés à un **Rôle**.

## 2. Modèle de Données

Nous devons introduire deux nouvelles entités et modifier l'utilisateur.

### Entités (Core)

```csharp
// Représente un rôle (ex: "Administrateur", "Observateur")
public class Role : BaseEntity
{
    public string Name { get; set; }        // Unique
    public string Description { get; set; }
    public bool IsSystem { get; set; }      // True = Impossible à supprimer (ex: Admin)
    
    // Un rôle contient plusieurs permissions
    public ICollection<RolePermission> Permissions { get; set; } = new List<RolePermission>();
}

// Table de liaison Rôle <-> Permission
public class RolePermission
{
    public Guid RoleId { get; set; }
    public Role Role { get; set; }
    
    // On stocke le code de la permission (ex: "Payments.Create")
    public string PermissionCode { get; set; } 
}

// Mise à jour de l'utilisateur
public class User : BaseEntity
{
    // ... champs existants ...
    
    // Remplacement de "string Role" par une relation
    public Guid RoleId { get; set; }
    public Role Role { get; set; }
}
```

## 3. Catalogue des Permissions

Les permissions sont définies comme des constantes statiques dans le code (`Core.Domain.Constants.Permissions`) pour éviter les "magic strings".

| Module | Code Permission | Description |
|:-------|:---------------|:------------|
| **Paiements** | `Payments.View` | Voir la liste et détails |
| | `Payments.Create` | Encaisser un paiement |
| | `Payments.Edit` | Modifier un paiement (sauf validé) |
| | `Payments.Delete` | Supprimer (Annuler) un paiement |
| **Dépenses** | `Expenses.View` | Voir les dépenses |
| | `Expenses.Manage` | Créer/Modifier/Supprimer |
| **Utilisateurs** | `Users.View` | Voir la liste des résidents |
| | `Users.Manage` | Créer/Modifier/Supprimer des comptes |
| **Rapports** | `Reports.View` | Voir les graphiques |
| | `Reports.Export` | Exporter PDF/Excel |
| **Système** | `System.Settings` | Modifier les paramètres globaux |
| | `System.Backup` | Gérer les sauvegardes |
| | `System.Audit` | Voir les logs d'audit |

## 4. Implémentation de la Vérification

### Service d'Autorisation (`IPermissionService`)

Ce service vérifie si l'utilisateur courant possède la permission requise.

```csharp
public interface IPermissionService
{
    // Vérifie si l'utilisateur a la permission
    Task<bool> HasPermissionAsync(string userId, string permissionCode);
    
    // Vérifie et lève une exception ForbiddenException si échec
    Task EnforcePermissionAsync(string permissionCode);
    
    // Récupère toutes les permissions d'un utilisateur (pour le cache)
    Task<HashSet<string>> GetUserPermissionsAsync(string userId);
}
```

### Protection dans les Services (Backend Logic)

C'est la barrière de sécurité principale. Même si l'UI cache le bouton, le service doit bloquer l'appel.

```csharp
public async Task DeletePaymentAsync(Guid id)
{
    // 1. Audit de sécurité
    await _permissionService.EnforcePermissionAsync(Permissions.Payments.Delete);

    // 2. Logique métier...
}
```

### Protection dans les ViewModels (UI Logic)

Pour une meilleure UX, on désactive ou cache les éléments inaccessibles.

```csharp
// Dans un ViewModel
public bool CanDeletePayment => _permissionService.HasPermission(Permissions.Payments.Delete);

// Commande protégée
DeleteCommand = new RelayCommand(ExecuteDelete, () => CanDeletePayment);
```

## 5. Gestion des Permissions (Interface)

Une nouvelle vue "Gestion des Rôles" permettra de :
1. Créer un nouveau rôle (ex: "Stagiaire").
2. Cocher les permissions autorisées via une liste de cases à cocher groupées par Module.
3. Sauvegarder en base.

## 6. Bonnes Pratiques de Sécurité

1. **Deny by Default** : Si une permission n'est pas explicitement accordée, elle est refusée.
2. **Principe de Moindre Privilège** : Ne donner aux utilisateurs que les droits strictement nécessaires à leur tâche.
3. **Audit des Changements** : Toute modification des permissions d'un rôle doit être loguée avec sévérité "Critical" (via votre nouveau système d'audit).
4. **Fail Secure** : Si le service de permission échoue (ex: erreur DB), l'accès doit être refusé.
5. **SuperAdmin** : Le rôle "Admin" système doit avoir un booléen `IsSuperAdmin` qui by-passe les vérifications (filet de sécurité) ou recevoir automatiquement toutes les permissions à la création.

## 7. Plan de Migration

1. Créer les tables `Roles` et `RolePermissions`.
2. Insérer les rôles par défaut :
   - **Administrateur** : Toutes les permissions.
   - **Utilisateur (Standard)** : Permissions de lecture + Création Paiement simple.
3. Migrer les utilisateurs existants :
   - Si `User.Role == "Admin"` -> Assign RoleId Administrateur.
   - Sinon -> Assign RoleId Utilisateur.
4. Supprimer l'ancienne colonne `Role` (string).

_Signé: L'Architecte Sécurité Antigravity_
