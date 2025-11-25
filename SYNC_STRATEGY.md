# El Mansour Syndic Manager - Synchronization Strategy

## Overview

The application implements a bidirectional synchronization system between local SQLite database and cloud Supabase database, ensuring data consistency, offline capability, and conflict resolution.

---

## 1. Sync Architecture

### 1.1 Architecture Diagram

```
┌─────────────────────────────────────────────────────────────┐
│                    Local SQLite Database                    │
│  ┌──────────┐  ┌──────────┐  ┌──────────┐  ┌──────────┐  │
│  │  Users   │  │ Payments │  │Maintenance│  │AuditLogs │  │
│  └────┬─────┘  └────┬─────┘  └────┬─────┘  └────┬─────┘  │
│       │             │             │             │          │
│       └─────────────┴─────────────┴─────────────┴──────────┘
│                          │                                    │
│                          │ Sync Engine                        │
│                          │                                    │
│       ┌──────────────────▼──────────────────┐               │
│       │         Conflict Resolver             │               │
│       └──────────────────┬──────────────────┘               │
│                          │                                    │
└──────────────────────────┼────────────────────────────────────┘
                           │
                           │ HTTPS
                           │
┌──────────────────────────▼────────────────────────────────────┐
│                  Cloud Supabase Database                      │
│  ┌──────────┐  ┌──────────┐  ┌──────────┐  ┌──────────┐   │
│  │  Users   │  │ Payments │  │Maintenance│  │AuditLogs │   │
│  └──────────┘  └──────────┘  └──────────┘  └──────────┘   │
│                                                               │
│  ┌──────────────────────────────────────────────────────┐   │
│  │         Real-time Subscriptions (Postgres)            │   │
│  └──────────────────────────────────────────────────────┘   │
└───────────────────────────────────────────────────────────────┘
```

### 1.2 Sync Components

1. **SyncEngine**: Orchestrates sync process
2. **ConflictResolver**: Handles conflicts
3. **OfflineQueueManager**: Manages offline changes
4. **CloudClient**: Supabase API client
5. **LocalRepository**: SQLite data access

---

## 2. Sync Process

### 2.1 Sync Flow

```
┌─────────────┐
│  Start Sync │
└──────┬──────┘
       │
       ▼
┌─────────────────┐
│ Check Connection│
└──────┬──────────┘
       │
       ├─── No ──► Queue for later
       │
       └─── Yes
             │
             ▼
      ┌──────────────┐
      │  Push Phase  │
      └──────┬───────┘
             │
             ▼
      ┌──────────────────┐
      │ Get Local Changes│
      └──────┬───────────┘
             │
             ▼
      ┌──────────────────┐
      │ Push to Cloud     │
      └──────┬───────────┘
             │
             ▼
      ┌──────────────┐
      │  Pull Phase  │
      └──────┬───────┘
             │
             ▼
      ┌──────────────────┐
      │ Get Cloud Changes│
      └──────┬───────────┘
             │
             ▼
      ┌──────────────────┐
      │ Detect Conflicts │
      └──────┬───────────┘
             │
             ▼
      ┌──────────────────┐
      │ Resolve Conflicts│
      └──────┬───────────┘
             │
             ▼
      ┌──────────────────┐
      │ Apply Changes    │
      └──────┬───────────┘
             │
             ▼
      ┌──────────────────┐
      │ Update Sync Status│
      └──────────────────┘
```

### 2.2 Sync Phases

#### Phase 1: Push (Local → Cloud)
1. Query SyncQueue for pending changes
2. Group by entity type
3. Push each entity to cloud
4. Update SyncStatus to 'Synced'
5. Store CloudId in local database
6. Remove from SyncQueue on success

#### Phase 2: Pull (Cloud → Local)
1. Get last sync timestamp
2. Query cloud for changes since last sync
3. For each cloud change:
   - Check if local version exists
   - Compare timestamps
   - Detect conflicts
   - Apply changes or resolve conflicts
4. Update LastSyncAt timestamp

---

## 3. Conflict Resolution

### 3.1 Conflict Detection

**Conflict occurs when**:
- Same entity modified locally and in cloud
- Local `UpdatedAt` ≠ Cloud `UpdatedAt`
- Both changes after last sync

**Conflict Types**:
1. **Update-Update**: Both sides modified
2. **Update-Delete**: One side updated, other deleted
3. **Delete-Update**: One side deleted, other updated

### 3.2 Resolution Strategies

#### Strategy 1: Last-Write-Wins (LWW) - Default

```csharp
if (localEntity.UpdatedAt > cloudEntity.UpdatedAt)
{
    // Local is newer, push to cloud
    await PushToCloudAsync(localEntity);
}
else if (cloudEntity.UpdatedAt > localEntity.UpdatedAt)
{
    // Cloud is newer, update local
    await UpdateLocalAsync(cloudEntity);
}
else
{
    // Same timestamp, manual resolution required
    await FlagForManualResolutionAsync(conflict);
}
```

#### Strategy 2: Manual Resolution

For critical conflicts:
- Flag conflict in UI
- Show both versions
- Admin selects which version to keep
- Apply resolution
- Log resolution decision

### 3.3 Conflict Resolution Flow

```
┌──────────────────┐
│  Detect Conflict │
└────────┬─────────┘
         │
         ▼
┌──────────────────┐
│  Is Critical?    │
└────────┬─────────┘
         │
         ├─── No ──► Apply LWW
         │
         └─── Yes
               │
               ▼
      ┌──────────────────┐
      │ Flag for Manual  │
      │   Resolution     │
      └────────┬─────────┘
               │
               ▼
      ┌──────────────────┐
      │ Admin Resolves    │
      └────────┬─────────┘
               │
               ▼
      ┌──────────────────┐
      │ Apply Resolution │
      └──────────────────┘
```

---

## 4. Offline Support

### 4.1 Offline Queue

**SyncQueue Table** stores pending changes:
- Entity type and ID
- Operation (Create, Update, Delete)
- Serialized entity data
- Retry count
- Status (Pending, Processing, Failed, Completed)

### 4.2 Offline Operations

**All operations work offline**:
1. User performs action (e.g., record payment)
2. Save to local SQLite
3. Add to SyncQueue
4. Mark SyncStatus as 'Pending'
5. Continue working offline

**When connection restored**:
1. SyncEngine detects connection
2. Processes SyncQueue
3. Pushes all pending changes
4. Updates sync status

### 4.3 Offline Indicators

**UI Indicators**:
- Connection status icon
- Pending sync count
- Last sync timestamp
- Sync in progress indicator

---

## 5. Real-time Synchronization

### 5.1 Supabase Real-time

**Postgres Changes**:
- Subscribe to table changes
- Receive updates in real-time
- Apply changes to local database

**Implementation**:
```csharp
await _supabaseClient
    .From<Payment>()
    .On(ChannelEventType.Update, (sender, response) =>
    {
        // Update local database
        await UpdateLocalPaymentAsync(response.Model);
    })
    .Subscribe();
```

### 5.2 Real-time Events

**Subscribed Tables**:
- Payments
- Maintenance
- MaintenanceDocuments
- Users (Admin only)

**Event Types**:
- INSERT: New record created
- UPDATE: Record modified
- DELETE: Record deleted (soft delete)

---

## 6. Sync Status Tracking

### 6.1 SyncStatus Field

**Values**:
- `Pending`: Not yet synced
- `Syncing`: Currently syncing
- `Synced`: Successfully synced
- `Conflict`: Conflict detected
- `Failed`: Sync failed

### 6.2 Sync Metadata

**Fields**:
- `LastSyncAt`: Last successful sync timestamp
- `CloudId`: Cloud database ID (after sync)
- `SyncStatus`: Current sync status
- `SyncError`: Error message (if failed)

---

## 7. Sync Scheduling

### 7.1 Automatic Sync

**Triggers**:
- Every 5 minutes (configurable)
- On application startup
- After critical operations (payment, maintenance)
- When connection restored

### 7.2 Manual Sync

**User-Triggered**:
- Sync button in Settings
- Force sync option
- Sync status page

---

## 8. Error Handling

### 8.1 Sync Errors

**Error Types**:
- Network errors (retry with exponential backoff)
- Authentication errors (refresh token)
- Validation errors (log and skip)
- Conflict errors (flag for resolution)

### 8.2 Retry Strategy

**Exponential Backoff**:
```
Attempt 1: Immediate
Attempt 2: 1 second
Attempt 3: 2 seconds
Attempt 4: 4 seconds
Attempt 5: 8 seconds
Max attempts: 5
```

**After max attempts**:
- Mark as Failed
- Log error
- Show notification to user
- Allow manual retry

---

## 9. Data Consistency

### 9.1 Consistency Guarantees

**Eventual Consistency**:
- Local changes immediately visible
- Cloud sync happens asynchronously
- Conflicts resolved automatically or manually

**Strong Consistency for**:
- User authentication
- Critical financial operations (with confirmation)

### 9.2 Validation

**Pre-Sync Validation**:
- Required fields present
- Foreign key constraints
- Business rule validation
- Data type validation

**Post-Sync Validation**:
- Verify sync success
- Compare record counts
- Validate data integrity

---

## 10. Performance Optimization

### 10.1 Incremental Sync

**Only sync changes**:
- Track last sync timestamp
- Query only modified records
- Reduce data transfer

### 10.2 Batch Operations

**Batch Updates**:
- Group multiple changes
- Send in single request
- Reduce API calls

### 10.3 Compression

**Large Data**:
- Compress JSON payloads
- Compress document uploads
- Reduce bandwidth usage

---

## 11. Sync Monitoring

### 11.1 Sync Status UI

**Dashboard Widget**:
- Connection status
- Last sync time
- Pending changes count
- Sync progress bar

### 11.2 Sync Logs

**Logging**:
- Sync start/end times
- Entities synced
- Conflicts detected
- Errors encountered

---

## 12. Implementation Example

### 12.1 SyncService Implementation

```csharp
public class SyncService : ISyncService
{
    public async Task<SyncResult> SyncAsync()
    {
        var result = new SyncResult();
        
        try
        {
            // Check connection
            if (!await IsConnectedAsync())
            {
                result.Status = SyncStatus.Offline;
                return result;
            }
            
            // Push phase
            var pushResult = await PushChangesAsync();
            result.PushedCount = pushResult.Count;
            
            // Pull phase
            var pullResult = await PullChangesAsync();
            result.PulledCount = pullResult.Count;
            result.ConflictsCount = pullResult.Conflicts.Count;
            
            // Update last sync time
            await UpdateLastSyncTimeAsync();
            
            result.Status = SyncStatus.Success;
        }
        catch (Exception ex)
        {
            result.Status = SyncStatus.Failed;
            result.Error = ex.Message;
            _logger.LogError(ex, "Sync failed");
        }
        
        return result;
    }
    
    private async Task<List<ConflictDto>> PullChangesAsync()
    {
        var lastSync = await GetLastSyncTimeAsync();
        var conflicts = new List<ConflictDto>();
        
        // Get cloud changes
        var cloudPayments = await _cloudClient
            .From<Payment>()
            .Where(p => p.UpdatedAt > lastSync)
            .Get();
        
        foreach (var cloudPayment in cloudPayments)
        {
            var localPayment = await _localRepository.GetByIdAsync(cloudPayment.Id);
            
            if (localPayment != null && 
                localPayment.UpdatedAt != cloudPayment.UpdatedAt)
            {
                // Conflict detected
                var conflict = new ConflictDto
                {
                    EntityType = "Payment",
                    EntityId = cloudPayment.Id,
                    LocalVersion = localPayment,
                    CloudVersion = cloudPayment
                };
                
                // Resolve automatically or flag for manual
                if (await ShouldAutoResolveAsync(conflict))
                {
                    await ResolveConflictAsync(conflict);
                }
                else
                {
                    conflicts.Add(conflict);
                }
            }
            else
            {
                // No conflict, apply cloud version
                await _localRepository.UpdateAsync(cloudPayment);
            }
        }
        
        return conflicts;
    }
}
```

---

## 13. Testing Sync

### 13.1 Test Scenarios

1. **Normal Sync**: Both sides in sync
2. **Local Changes**: Local changes pushed to cloud
3. **Cloud Changes**: Cloud changes pulled to local
4. **Conflicts**: Both sides modified
5. **Offline**: Operations work offline
6. **Connection Loss**: Sync resumes when connection restored

### 13.2 Test Data

- Create test entities locally
- Modify in cloud
- Verify sync
- Check conflict resolution

---

## Summary

The sync strategy ensures:
- ✅ Offline capability
- ✅ Real-time updates
- ✅ Conflict resolution
- ✅ Data consistency
- ✅ Error handling
- ✅ Performance optimization
- ✅ User visibility

