# El Mansour Syndic Manager - Testing Checklist

## Pre-Deployment Testing

### 1. Authentication Tests

- [ ] **Login with Admin**
  - House Code: `B-SYNDIC`
  - 6-digit Code: Default code works
  - Can access all features

- [ ] **Login with Syndic Member**
  - House Code: `A01`, `B01`, `C01`, or `D01`
  - 6-digit Code: Default code works
  - Cannot access Admin-only features

- [ ] **Change Password**
  - Admin can change own code
  - Admin can reset member codes
  - New code works for login

- [ ] **Invalid Login**
  - Wrong house code shows error
  - Wrong 6-digit code shows error
  - Error messages in French

### 2. Payment Tests

- [ ] **Create Payment**
  - Select house code
  - Enter amount
  - Select month
  - Save payment
  - Payment appears in list

- [ ] **Mark Payment as Paid**
  - Select unpaid payment
  - Mark as paid
  - Status updates
  - Receipt can be generated

- [ ] **Generate Receipt**
  - Click "Générer Reçu" for paid payment
  - PDF opens correctly
  - Signature overlay appears
  - Receipt saved locally
  - Receipt uploaded to cloud

- [ ] **View Receipt History**
  - Select house code
  - View all receipts
  - Download receipt
  - Print receipt

### 3. Reporting Tests

- [ ] **Generate Monthly Report**
  - Select month
  - Click "Générer le Rapport"
  - Report displays correctly
  - Statistics are accurate

- [ ] **Export Monthly Report to PDF**
  - Generate report
  - Click "Exporter PDF"
  - PDF opens correctly
  - All data present

- [ ] **Export Monthly Report to Excel**
  - Generate report
  - Click "Exporter Excel"
  - Excel opens correctly
  - All sheets present

- [ ] **Generate Yearly Report**
  - Select year
  - Generate report
  - Monthly breakdown correct
  - Totals accurate

### 4. Notification Tests

- [ ] **Unpaid House Notification**
  - Create unpaid payment
  - Notification appears
  - Toast notification shows
  - Can mark as read

- [ ] **Maintenance Notification**
  - Create maintenance task
  - Notification appears
  - Toast notification shows

- [ ] **View Notifications**
  - Open notifications page
  - All notifications listed
  - Filter by read/unread works
  - Filter by type works

- [ ] **Mark Notification as Read**
  - Click notification
  - Mark as read
  - Status updates
  - Unread count decreases

### 5. Backup Tests

- [ ] **Create Manual Backup**
  - Click "Créer une Sauvegarde"
  - Backup completes
  - Backup appears in history
  - Notification sent

- [ ] **Scheduled Backup**
  - Configure schedule
  - Wait for scheduled time
  - Backup runs automatically
  - Notification sent

- [ ] **Restore Backup**
  - Create backup
  - Restore from backup
  - Data restored correctly
  - Application restarts

- [ ] **Delete Old Backups**
  - Create multiple backups
  - Delete old backups
  - Only last N kept
  - Files deleted

### 6. Integration Tests

- [ ] **Cloud Sync**
  - Create payment locally
  - Sync to cloud
  - Verify in Supabase
  - Pull from cloud works

- [ ] **File Upload**
  - Upload document
  - File appears in cloud storage
  - Download works
  - File integrity verified

- [ ] **Offline Mode**
  - Disconnect internet
  - Create payment
  - Reconnect
  - Sync completes

### 7. UI/UX Tests

- [ ] **Navigation**
  - All pages accessible
  - Navigation drawer works
  - Selected item highlighted
  - Role-based filtering works

- [ ] **French Localization**
  - All text in French
  - Dates in French format
  - Currency in MAD
  - Error messages in French

- [ ] **Responsive Layout**
  - Window resize works
  - DataGrids scroll correctly
  - Cards display properly
  - Charts render correctly

### 8. Performance Tests

- [ ] **Large Dataset**
  - Load 100+ payments
  - Performance acceptable
  - No UI freezing
  - Memory usage reasonable

- [ ] **PDF Generation**
  - Generate 10+ receipts
  - Performance acceptable
  - No errors

- [ ] **Report Generation**
  - Generate yearly report
  - Performance acceptable
  - All data correct

### 9. Security Tests

- [ ] **Password Hashing**
  - Passwords hashed correctly
  - Salt unique per user
  - Cannot reverse hash

- [ ] **Backup Encryption**
  - Backup files encrypted
  - Cannot open without key
  - Decryption works

- [ ] **Cloud Security**
  - RLS policies enforced
  - Storage buckets private
  - API keys secured

### 10. Error Handling Tests

- [ ] **Network Errors**
  - Handle connection loss
  - Show user-friendly message
  - Retry mechanism works

- [ ] **File Errors**
  - Handle missing files
  - Handle permission errors
  - Show appropriate messages

- [ ] **Validation Errors**
  - Invalid input rejected
  - Error messages clear
  - French error messages

## Post-Deployment Validation

### Production Checklist

- [ ] All tests pass
- [ ] Cloud services deployed
- [ ] Database seeded
- [ ] Storage buckets created
- [ ] Edge functions scheduled
- [ ] RLS policies configured
- [ ] Encryption keys secured
- [ ] Logging configured
- [ ] Monitoring setup
- [ ] Backup schedule active

## Test Data

### Test Users

- Admin: `B-SYNDIC` / `123456`
- Member 1: `A01` / `111111`
- Member 2: `B01` / `222222`
- Member 3: `C01` / `333333`
- Member 4: `D01` / `444444`

### Test Payments

- Create payments for various houses
- Mix of paid/unpaid
- Different months
- Various amounts

### Test Maintenance

- Create maintenance tasks
- Different priorities
- Different statuses
- Assign to users

## Reporting Issues

When reporting issues, include:
- Steps to reproduce
- Expected behavior
- Actual behavior
- Screenshots
- Log files
- System information

