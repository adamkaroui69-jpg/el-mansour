# El Mansour Syndic Manager - Security Model

## Overview

The security model ensures data protection, user authentication, authorization, and secure communication between the application and cloud services.

---

## 1. Authentication

### 1.1 Authentication Method

**6-Digit Code Authentication**
- Users authenticate using their house code and a 6-digit numeric code
- No username/password complexity requirements
- Simple but secure for small user base

### 1.2 Password Hashing

**Algorithm**: PBKDF2 (Password-Based Key Derivation Function 2)
- **Iterations**: 10,000
- **Hash Algorithm**: SHA-256
- **Salt**: Unique 32-byte random salt per user
- **Output**: 256-bit (32-byte) hash

**Implementation**:
```csharp
public class PasswordHasher
{
    private const int Iterations = 10000;
    private const int SaltSize = 32;
    private const int HashSize = 32;

    public (string Hash, string Salt) HashPassword(string password)
    {
        var salt = GenerateSalt();
        var hash = PBKDF2(password, salt, Iterations, HashSize);
        return (Convert.ToBase64String(hash), Convert.ToBase64String(salt));
    }

    public bool VerifyPassword(string password, string hash, string salt)
    {
        var saltBytes = Convert.FromBase64String(salt);
        var hashBytes = Convert.FromBase64String(hash);
        var computedHash = PBKDF2(password, saltBytes, Iterations, HashSize);
        return SlowEquals(hashBytes, computedHash);
    }
}
```

### 1.3 Session Management

**JWT Tokens**
- **Expiration**: 24 hours
- **Refresh Token**: 7 days
- **Claims**: UserId, HouseCode, Role, Name, Surname
- **Storage**: Secure in-memory storage (not persisted)

**Token Structure**:
```json
{
  "sub": "user-id",
  "houseCode": "A01",
  "role": "SyndicMember",
  "name": "Ahmed",
  "surname": "Benali",
  "exp": 1234567890,
  "iat": 1234567890
}
```

### 1.4 Authentication Flow

```
1. User enters House Code + 6-digit code
2. Application retrieves user from database
3. Verify password hash
4. Generate JWT token
5. Store token in memory
6. Log authentication event
7. Redirect to dashboard
```

---

## 2. Authorization

### 2.1 Role-Based Access Control (RBAC)

**Roles**:
- **Admin**: Full system access
- **SyndicMember**: Limited access

### 2.2 Permission Matrix

| Feature | Admin | Syndic Member |
|---------|-------|---------------|
| View Dashboard | ✅ | ✅ |
| Record Payments | ✅ | ✅ |
| Generate Receipts | ✅ | ✅ |
| View Payments | ✅ | ✅ |
| Create Maintenance | ✅ | ✅ |
| Update Maintenance | ✅ | ✅ |
| View Maintenance | ✅ | ✅ |
| Generate Reports | ✅ | ✅ |
| View Reports | ✅ | ✅ |
| Manage Users | ✅ | ❌ |
| Delete Users | ✅ | ❌ |
| View Audit Logs | ✅ | ❌ |
| System Settings | ✅ | ❌ |
| Backup/Restore | ✅ | ❌ |
| Sync Management | ✅ | ❌ |

### 2.3 Authorization Implementation

**Attribute-Based Authorization**:
```csharp
[Authorize(Roles = "Admin")]
public class UserManagementViewModel : ViewModelBase
{
    // Only Admin can access
}

[Authorize(Roles = "Admin,SyndicMember")]
public class PaymentViewModel : ViewModelBase
{
    // Both roles can access
}
```

**Method-Level Authorization**:
```csharp
public class PaymentService
{
    public async Task DeletePaymentAsync(Guid paymentId)
    {
        if (!_currentUser.IsAdmin)
            throw new UnauthorizedException("Only admins can delete payments");
        
        // Delete logic
    }
}
```

---

## 3. Data Protection

### 3.1 Database Encryption

**SQLite Encryption**: SQLCipher
- **Algorithm**: AES-256
- **Key Derivation**: PBKDF2
- **Key Storage**: Windows Credential Manager or encrypted config file

**Implementation**:
```csharp
var connectionString = $"Data Source={dbPath};Password={encryptionKey}";
// SQLCipher encrypts database at rest
```

### 3.2 Sensitive Data Encryption

**Encrypted Fields**:
- Password hashes (already hashed)
- Cloud API keys
- Encryption keys

**Encryption Method**: AES-256-GCM
- **Key Management**: Windows DPAPI (Data Protection API)
- **IV**: Random per encryption

### 3.3 Document Access Control

**Document Permissions**:
- Users can only view documents they created or have permission for
- Admin can view all documents
- Documents stored with access metadata

**Implementation**:
```csharp
public async Task<Stream> GetDocumentAsync(Guid documentId)
{
    var document = await _documentRepository.GetByIdAsync(documentId);
    
    // Check permissions
    if (!_currentUser.IsAdmin && 
        document.UploadedBy != _currentUser.Id)
    {
        throw new UnauthorizedException();
    }
    
    return await _storageService.DownloadAsync(document.CloudStoragePath);
}
```

---

## 4. Network Security

### 4.1 HTTPS Only

**All Cloud Communications**:
- Enforce HTTPS for all API calls
- Certificate pinning (optional)
- TLS 1.2 minimum

**Implementation**:
```csharp
var client = new SupabaseClient(url, key, new SupabaseOptions
{
    AutoConnectRealtime = true,
    AutoRefreshToken = true
});
// Supabase client uses HTTPS by default
```

### 4.2 API Key Security

**Storage**:
- Encrypted in configuration file
- Never logged or exposed in UI
- Rotated periodically

**Usage**:
- Sent in Authorization header
- Not stored in code
- Retrieved from secure storage

### 4.3 Request Validation

**Input Validation**:
- All user inputs validated
- SQL injection prevention (parameterized queries)
- XSS prevention (output encoding)
- File upload validation (type, size)

---

## 5. Audit and Logging

### 5.1 Audit Trail

**Logged Events**:
- Login/Logout
- Payment creation/update
- Maintenance creation/update
- User management
- Report generation
- Document upload/download
- Settings changes
- Backup/restore operations

**Audit Log Fields**:
- User ID
- Action type
- Entity type and ID
- Timestamp
- IP address
- Additional details (JSON)

### 5.2 Security Logging

**Security Events**:
- Failed login attempts
- Unauthorized access attempts
- Suspicious activities
- Data export operations

**Log Storage**:
- Local log files (encrypted)
- Cloud audit logs
- Retention: 1 year

---

## 6. Secure Coding Practices

### 6.1 Input Validation

```csharp
public class PaymentService
{
    public async Task RecordPaymentAsync(PaymentDto payment)
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(payment.HouseCode))
            throw new ValidationException("House code is required");
        
        if (payment.Amount <= 0)
            throw new ValidationException("Amount must be positive");
        
        // Sanitize input
        payment.Notes = SanitizeHtml(payment.Notes);
        
        // Process payment
    }
}
```

### 6.2 SQL Injection Prevention

**Parameterized Queries**:
```csharp
// ✅ Safe
var query = "SELECT * FROM Payments WHERE HouseCode = @houseCode";
var parameters = new { houseCode = userInput };

// ❌ Unsafe (never do this)
var query = $"SELECT * FROM Payments WHERE HouseCode = '{userInput}'";
```

### 6.3 XSS Prevention

**Output Encoding**:
```csharp
// In XAML
<TextBlock Text="{Binding Notes, Converter={StaticResource HtmlEncoder}}" />

// In code
var safeText = HttpUtility.HtmlEncode(userInput);
```

---

## 7. Backup Security

### 7.1 Backup Encryption

**Backup Files**:
- Encrypted using AES-256
- Password-protected archives
- Key stored separately

### 7.2 Backup Access Control

**Backup Permissions**:
- Only Admin can create backups
- Only Admin can restore backups
- Backup files stored in secure location

---

## 8. Cloud Security

### 8.1 Supabase Security

**Row Level Security (RLS)**:
- Enabled on all tables
- Policies restrict access based on user role
- Admin has full access
- Syndic Members have limited access

**Example RLS Policy**:
```sql
CREATE POLICY "Syndic members can view payments" ON payments
    FOR SELECT
    USING (
        auth.role() = 'authenticated' AND
        (auth.uid() IN (SELECT id FROM users WHERE role = 'Admin') OR
         auth.uid() = recorded_by)
    );
```

### 8.2 Storage Security

**Supabase Storage**:
- Private buckets for sensitive documents
- Signed URLs for temporary access
- Access control via RLS policies

---

## 9. Security Best Practices

### 9.1 Code Security
- ✅ No hardcoded secrets
- ✅ Secure password hashing
- ✅ Input validation
- ✅ Output encoding
- ✅ Parameterized queries
- ✅ Error handling (no sensitive data in errors)

### 9.2 Operational Security
- ✅ Regular security updates
- ✅ Backup encryption
- ✅ Access logging
- ✅ Session timeout
- ✅ Secure key storage

### 9.3 User Security
- ✅ Strong password policy (6-digit minimum)
- ✅ Session management
- ✅ Role-based access
- ✅ Audit trail

---

## 10. Security Checklist

### Development
- [ ] All passwords hashed with PBKDF2
- [ ] SQL injection prevention
- [ ] XSS prevention
- [ ] Input validation
- [ ] Error handling
- [ ] No sensitive data in logs

### Deployment
- [ ] Database encryption enabled
- [ ] HTTPS enforced
- [ ] API keys secured
- [ ] Backup encryption
- [ ] Access controls configured

### Maintenance
- [ ] Regular security audits
- [ ] Update dependencies
- [ ] Review audit logs
- [ ] Rotate API keys
- [ ] Test backup/restore

