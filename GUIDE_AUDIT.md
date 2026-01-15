# Guide Complet : Système de Journal d'Audit

## Vue d'Ensemble

Le système d'audit d'**El Mansour Syndic Manager** enregistre automatiquement toutes les actions importantes des utilisateurs pour assurer la traçabilité, la sécurité et la conformité.

---

## 1. Principe : Les 5W (Who, What, When, Where, Why)

| Question | Champ | Description |
|----------|-------|-------------|
| **Who** (Qui) | `UserId`, `Username`, `UserRole` | Identité complète de l'utilisateur |
| **What** (Quoi) | `Action`, `EntityType`, `EntityId` | Action effectuée et cible |
| **When** (Quand) | `Timestamp` | Date et heure précises (UTC) |
| **Where** (Où) | `IpAddress`, `MachineName` | Localisation technique |
| **Why** (Pourquoi) | `Details`, `OldValues`, `NewValues` | Contexte et changements |

---

## 2. Modèle de Données

### Entité AuditLog

```csharp
public class AuditLog : BaseEntity
{
    // QUI
    public string UserId { get; set; }
    public string Username { get; set; }
    public string UserRole { get; set; }
    
    // QUOI
    public string Action { get; set; }        // Create, Update, Delete, Login, etc.
    public string EntityType { get; set; }    // Payment, House, User, etc.
    public string EntityId { get; set; }
    public string? OldValues { get; set; }    // JSON avant modification
    public string? NewValues { get; set; }    // JSON après modification
    public string Details { get; set; }
    
    // QUAND
    public DateTime Timestamp { get; set; }
    
    // OÙ
    public string? IpAddress { get; set; }
    public string? MachineName { get; set; }
    
    // CONTEXTE
    public string Severity { get; set; }      // Info, Warning, Error, Critical
    public bool IsSuccessful { get; set; }
    public string? ErrorMessage { get; set; }
    public int DurationMs { get; set; }       // Performance tracking
}
```

---

## 3. Actions Auditées

### Authentification
- ✅ **Login** : Connexion réussie
- ⚠️ **LoginFailed** : Tentative de connexion échouée
- ✅ **Logout** : Déconnexion
- 🔒 **PasswordChanged** : Changement de mot de passe

### Opérations CRUD
- ✅ **Create** : Création d'entité
- ✅ **Read** : Consultation (pour données sensibles uniquement)
- ✅ **Update** : Modification
- 🗑️ **Delete** : Suppression

### Actions Métier Critiques
- 💰 **PaymentReceived** : Enregistrement de paiement
- ❌ **PaymentCancelled** : Annulation de paiement
- 📄 **ReceiptGenerated** : Génération de reçu
- 📧 **ReceiptSent** : Envoi de reçu par email
- 📊 **ReportExported** : Export de rapport
- 💾 **BackupCreated** : Sauvegarde créée
- ♻️ **BackupRestored** : Restauration effectuée
- 👤 **UserCreated** : Nouvel utilisateur
- 🔐 **UserRoleChanged** : Changement de rôle (sécurité)

---

## 4. Niveaux de Sévérité

| Niveau | Couleur | Usage | Exemples |
|--------|---------|-------|----------|
| **Info** | 🟢 Vert | Actions normales | Login, Create, Update |
| **Warning** | 🟡 Jaune | Actions inhabituelles | LoginFailed, Tentatives multiples |
| **Error** | 🟠 Orange | Erreurs récupérables | Validation échouée, Fichier manquant |
| **Critical** | 🔴 Rouge | Erreurs critiques | Violation de sécurité, Corruption de données |

---

## 5. Implémentation Automatique

### A. Enregistrement Manuel

```csharp
public class PaymentService
{
    private readonly IAuditService _auditService;
    
    public async Task<Payment> CreatePaymentAsync(CreatePaymentDto dto)
    {
        var payment = // ... création du paiement
        
        // Audit manuel
        await _auditService.LogAsync(
            action: AuditAction.PaymentReceived,
            entityType: AuditEntityType.Payment,
            entityId: payment.Id.ToString(),
            details: $"Paiement de {payment.Amount} TND pour {payment.HouseCode}",
            newValues: JsonSerializer.Serialize(payment),
            severity: AuditSeverity.Info
        );
        
        return payment;
    }
}
```

### B. Enregistrement avec Mesure de Performance

```csharp
public async Task<Report> GenerateReportAsync(string month)
{
    return await _auditService.LogWithTimingAsync(
        action: AuditAction.ReportExported,
        entityType: AuditEntityType.Report,
        entityId: month,
        operation: async () =>
        {
            // Génération du rapport (peut être long)
            var report = await _reportService.GenerateAsync(month);
            return report;
        }
    );
    // L'audit enregistre automatiquement la durée d'exécution
}
```

### C. Enregistrement des Connexions

```csharp
// Dans AuthenticationService
public async Task<AuthenticationResult> LoginAsync(string username, string password)
{
    var user = await _userRepository.GetByUsernameAsync(username);
    
    if (user == null || !VerifyPassword(password, user.PasswordHash))
    {
        await _auditService.LogLoginAsync(
            userId: username,
            username: username,
            success: false,
            errorMessage: "Identifiants invalides"
        );
        return AuthenticationResult.Failed("Identifiants invalides");
    }
    
    await _auditService.LogLoginAsync(
        userId: user.Id.ToString(),
        username: user.Username,
        success: true
    );
    
    return AuthenticationResult.Success(user);
}
```

---

## 6. Interface Utilisateur

### Page Journal d'Audit

**Accès :** Menu principal → Journal d'Audit

**Fonctionnalités :**

#### Filtres Avancés
- 📅 **Période** : Date de début et fin
- 👤 **Utilisateur** : Filtrer par nom d'utilisateur
- ⚡ **Action** : Type d'action (Login, Create, Update, etc.)
- 📦 **Type d'entité** : Payment, House, User, etc.
- 🎯 **Sévérité** : Info, Warning, Error, Critical

#### Affichage
- **DataGrid** avec colonnes :
  - Date/Heure
  - Utilisateur + Rôle
  - **Badge de sévérité** (coloré)
  - Action
  - Type d'entité
  - Détails complets

#### Actions
- 🔍 **Rechercher** : Appliquer les filtres
- 🔄 **Rafraîchir** : Recharger les données
- 📥 **Exporter** : Export CSV/Excel (à implémenter)
- ♻️ **Réinitialiser** : Effacer tous les filtres

---

## 7. Bonnes Pratiques

### Performance

#### ✅ À Faire
1. **Indexer** les colonnes fréquemment filtrées :
   ```sql
   CREATE INDEX idx_audit_timestamp ON AuditLogs(Timestamp);
   CREATE INDEX idx_audit_userid ON AuditLogs(UserId);
   CREATE INDEX idx_audit_action ON AuditLogs(Action);
   ```

2. **Virtualisation** dans le DataGrid (déjà implémenté) :
   ```xml
   <DataGrid EnableRowVirtualization="True"
             VirtualizingPanel.IsVirtualizing="True"
             VirtualizingPanel.VirtualizationMode="Recycling"/>
   ```

3. **Pagination** : Limiter à 500 enregistrements par défaut

4. **Async/Await** : Ne jamais bloquer le thread UI

#### ❌ À Éviter
1. Ne pas auditer les actions de lecture (sauf données sensibles)
2. Ne pas stocker de mots de passe en clair dans `OldValues`/`NewValues`
3. Ne pas faire échouer l'opération principale si l'audit échoue
4. Ne pas auditer les actions du système d'audit lui-même (boucle infinie)

### Sécurité

#### Protection des Données Sensibles
```csharp
// ❌ MAUVAIS
await _auditService.LogAsync(
    action: "PasswordChanged",
    newValues: JsonSerializer.Serialize(new { Password = newPassword }) // DANGER!
);

// ✅ BON
await _auditService.LogAsync(
    action: "PasswordChanged",
    newValues: JsonSerializer.Serialize(new { PasswordChanged = true, Timestamp = DateTime.UtcNow })
);
```

#### Intégrité des Logs
- Les logs d'audit **ne doivent JAMAIS être modifiables** par les utilisateurs
- Seul un administrateur système peut les supprimer (avec audit de la suppression)
- Implémenter une signature numérique pour les logs critiques (optionnel)

### Rétention des Données

```csharp
// Nettoyage automatique des anciens logs (90 jours par défaut)
public async Task CleanOldLogsAsync(int retentionDays = 90)
{
    var cutoffDate = DateTime.UtcNow.AddDays(-retentionDays);
    var oldLogs = await _auditRepository.GetByDateRangeAsync(null, cutoffDate);
    
    foreach (var log in oldLogs)
    {
        await _auditRepository.DeleteAsync(log);
    }
    
    _logger.LogInformation("Cleaned {Count} audit logs older than {Days} days", 
        oldLogs.Count, retentionDays);
}
```

**Recommandation :** Exécuter ce nettoyage mensuellement via une tâche planifiée.

---

## 8. Cas d'Usage Avancés

### Détection d'Anomalies

```csharp
// Détecter les tentatives de connexion suspectes
public async Task<List<AuditLog>> DetectSuspiciousLoginsAsync()
{
    var last24h = DateTime.UtcNow.AddHours(-24);
    var failedLogins = await _auditService.GetAuditLogsAsync(
        from: last24h,
        action: AuditAction.LoginFailed,
        severity: AuditSeverity.Warning
    );
    
    // Grouper par utilisateur
    var suspiciousUsers = failedLogins
        .GroupBy(l => l.Username)
        .Where(g => g.Count() >= 5) // 5 échecs ou plus
        .Select(g => g.Key)
        .ToList();
    
    if (suspiciousUsers.Any())
    {
        await _auditService.LogCriticalErrorAsync(
            action: "SuspiciousActivity",
            details: $"Tentatives de connexion multiples détectées pour : {string.Join(", ", suspiciousUsers)}"
        );
    }
    
    return failedLogins;
}
```

### Historique d'une Entité

```csharp
// Voir toutes les modifications d'un paiement
public async Task<List<AuditLog>> GetPaymentHistoryAsync(Guid paymentId)
{
    return await _auditService.GetEntityHistoryAsync(
        entityType: AuditEntityType.Payment,
        entityId: paymentId.ToString()
    );
}
```

### Rapport d'Activité Utilisateur

```csharp
// Générer un rapport d'activité pour un utilisateur
public async Task<UserActivityReport> GenerateUserActivityReportAsync(string userId, DateTime from, DateTime to)
{
    var logs = await _auditService.GetAuditLogsAsync(
        from: from,
        to: to,
        userId: userId
    );
    
    return new UserActivityReport
    {
        TotalActions = logs.Count,
        LoginCount = logs.Count(l => l.Action == AuditAction.Login),
        CreateCount = logs.Count(l => l.Action == AuditAction.Create),
        UpdateCount = logs.Count(l => l.Action == AuditAction.Update),
        DeleteCount = logs.Count(l => l.Action == AuditAction.Delete),
        ErrorCount = logs.Count(l => l.Severity == AuditSeverity.Error || l.Severity == AuditSeverity.Critical),
        MostActiveDay = logs.GroupBy(l => l.Timestamp.Date).OrderByDescending(g => g.Count()).First().Key
    };
}
```

---

## 9. Conformité et Réglementation

### RGPD (Protection des Données)
- ✅ Droit à l'oubli : Anonymiser les logs lors de la suppression d'un utilisateur
- ✅ Droit d'accès : Permettre à l'utilisateur de consulter ses propres logs
- ✅ Minimisation : Ne stocker que les données nécessaires

### Audit Trail (Piste d'Audit)
- ✅ Immuabilité : Les logs ne peuvent pas être modifiés
- ✅ Intégrité : Vérification par hash (optionnel)
- ✅ Disponibilité : Sauvegarde régulière des logs

---

## 10. Exemple Complet d'Intégration

### Scénario : Modification d'un Paiement

```csharp
public class PaymentService
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IAuditService _auditService;
    
    public async Task<Payment> UpdatePaymentAsync(Guid id, UpdatePaymentDto dto)
    {
        // 1. Récupérer l'ancien état
        var oldPayment = await _paymentRepository.GetByIdAsync(id);
        if (oldPayment == null) throw new NotFoundException("Payment not found");
        
        // 2. Appliquer les modifications
        oldPayment.Amount = dto.Amount;
        oldPayment.PaymentDate = dto.PaymentDate;
        oldPayment.Status = dto.Status;
        
        // 3. Sauvegarder
        await _paymentRepository.UpdateAsync(oldPayment);
        
        // 4. Auditer avec ancien/nouveau état
        await _auditService.LogAsync(
            action: AuditAction.Update,
            entityType: AuditEntityType.Payment,
            entityId: id.ToString(),
            details: $"Modification du paiement {oldPayment.HouseCode} - {oldPayment.Month}",
            oldValues: JsonSerializer.Serialize(new { 
                Amount = oldPayment.Amount, 
                Status = oldPayment.Status 
            }),
            newValues: JsonSerializer.Serialize(new { 
                Amount = dto.Amount, 
                Status = dto.Status 
            }),
            severity: AuditSeverity.Info
        );
        
        return oldPayment;
    }
}
```

---

## Conclusion

Le système d'audit garantit :
- 🔍 **Traçabilité complète** : Qui a fait quoi et quand
- 🛡️ **Sécurité** : Détection d'activités suspectes
- 📊 **Conformité** : Respect des réglementations (RGPD)
- 🐛 **Débogage** : Historique complet pour résoudre les problèmes
- 📈 **Analyse** : Statistiques d'utilisation et de performance

**Prochaines Améliorations :**
- Export automatique vers SIEM (Security Information and Event Management)
- Alertes en temps réel pour événements critiques
- Dashboard d'analyse des logs avec graphiques
- Signature numérique des logs pour garantir l'intégrité
