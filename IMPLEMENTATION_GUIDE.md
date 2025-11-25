# El Mansour Syndic Manager - Implementation Guide

## Overview

This guide provides step-by-step instructions for implementing the El Mansour Syndic Manager application based on the architecture documentation.

## Implementation Phases

### Phase 1: Foundation (Week 1-2)

#### 1.1 Project Setup

1. **Create Solution Structure**
   ```bash
   dotnet new sln -n ElMansourSyndicManager
   dotnet new wpf -n ElMansourSyndicManager -o src/ElMansourSyndicManager
   dotnet new classlib -n ElMansourSyndicManager.Core -o src/ElMansourSyndicManager.Core
   dotnet new classlib -n ElMansourSyndicManager.Infrastructure -o src/ElMansourSyndicManager.Infrastructure
   dotnet new classlib -n ElMansourSyndicManager.ViewModels -o src/ElMansourSyndicManager.ViewModels
   dotnet new classlib -n ElMansourSyndicManager.Views -o src/ElMansourSyndicManager.Views
   dotnet new classlib -n ElMansourSyndicManager.Utilities -o src/ElMansourSyndicManager.Utilities
   ```

2. **Add Project References**
   ```bash
   dotnet add src/ElMansourSyndicManager reference src/ElMansourSyndicManager.ViewModels
   dotnet add src/ElMansourSyndicManager reference src/ElMansourSyndicManager.Views
   dotnet add src/ElMansourSyndicManager.Infrastructure reference src/ElMansourSyndicManager.Core
   dotnet add src/ElMansourSyndicManager.ViewModels reference src/ElMansourSyndicManager.Core
   ```

3. **Install NuGet Packages**
   ```bash
   # Main Application
   dotnet add src/ElMansourSyndicManager package MaterialDesignThemes
   dotnet add src/ElMansourSyndicManager package MaterialDesignColors
   dotnet add src/ElMansourSyndicManager package CommunityToolkit.Mvvm
   
   # Infrastructure
   dotnet add src/ElMansourSyndicManager.Infrastructure package Microsoft.EntityFrameworkCore.Sqlite
   dotnet add src/ElMansourSyndicManager.Infrastructure package Supabase
   dotnet add src/ElMansourSyndicManager.Infrastructure package QuestPDF
   dotnet add src/ElMansourSyndicManager.Infrastructure package BCrypt.Net-Next
   dotnet add src/ElMansourSyndicManager.Infrastructure package Serilog
   dotnet add src/ElMansourSyndicManager.Infrastructure package Serilog.Sinks.File
   dotnet add src/ElMansourSyndicManager.Infrastructure package AutoMapper
   ```

#### 1.2 Database Setup

1. **Create Database Schema**
   - Implement SQLite database creation script
   - Create all tables as per DATABASE_SCHEMA.md
   - Add indexes and constraints

2. **Seed Initial Data**
   - Insert all houses (A, B, C, D, E buildings)
   - Create default Admin user
   - Insert default settings

3. **Database Context**
   - Create SqliteDbContext
   - Configure Entity Framework (if using)
   - Or use Dapper for data access

#### 1.3 Authentication Foundation

1. **Password Hasher**
   - Implement PBKDF2 hashing
   - Create PasswordHasher class
   - Add unit tests

2. **Authentication Service**
   - Implement IAuthenticationService
   - JWT token generation
   - Session management

3. **Login UI**
   - Create LoginWindow
   - Create LoginViewModel
   - Wire up authentication

---

### Phase 2: Core Features (Week 3-4)

#### 2.1 Payment Module

1. **Payment Service**
   - Implement IPaymentService
   - Payment recording logic
   - Payment retrieval methods
   - Unpaid houses detection

2. **Payment Repository**
   - Implement IPaymentRepository
   - CRUD operations
   - Query methods

3. **Payment UI**
   - PaymentPage (record payment)
   - PaymentListPage (view payments)
   - Payment ViewModels

4. **PDF Receipt Generation**
   - Implement IPdfService
   - Receipt template
   - Signature integration

#### 2.2 User Management Module

1. **User Service**
   - Implement IUserService
   - User CRUD operations
   - Signature management

2. **User Repository**
   - Implement IUserRepository
   - User queries

3. **User Management UI**
   - UserManagementPage
   - Add/Edit user dialogs
   - Signature upload

---

### Phase 3: Advanced Features (Week 5-6)

#### 3.1 Maintenance Module

1. **Maintenance Service**
   - Implement IMaintenanceService
   - Maintenance CRUD
   - Document attachment

2. **Maintenance Repository**
   - Implement IMaintenanceRepository
   - Maintenance queries

3. **Maintenance UI**
   - MaintenancePage
   - MaintenanceListPage
   - Document upload/view

#### 3.2 Reporting Module

1. **Report Service**
   - Implement IReportService
   - Monthly report generation
   - Yearly report generation
   - Statistics calculation

2. **Report UI**
   - ReportPage
   - Report viewer
   - Export functionality

3. **Charts and Graphs**
   - Integrate charting library
   - Monthly collection chart
   - Statistics visualization

---

### Phase 4: Integration (Week 7-8)

#### 4.1 Cloud Integration

1. **Supabase Setup**
   - Create Supabase project
   - Configure tables
   - Set up RLS policies
   - Configure storage buckets

2. **Cloud Client**
   - Implement ICloudClient
   - Supabase client wrapper
   - Real-time subscriptions

3. **Storage Service**
   - Implement IStorageService
   - File upload/download
   - Cloud storage integration

#### 4.2 Sync Engine

1. **Sync Service**
   - Implement ISyncService
   - Push/pull logic
   - Conflict detection

2. **Conflict Resolution**
   - Implement ConflictResolver
   - LWW strategy
   - Manual resolution UI

3. **Offline Queue**
   - Implement OfflineQueueManager
   - Queue management
   - Retry logic

#### 4.3 Audit Logging

1. **Audit Service**
   - Implement IAuditService
   - Activity logging
   - Log retrieval

2. **Audit Integration**
   - Add audit logging to all services
   - Log all critical operations

---

### Phase 5: Polish (Week 9-10)

#### 5.1 UI/UX Improvements

1. **Material Design**
   - Apply Material Design theme
   - Customize colors
   - Add icons

2. **Navigation**
   - Implement navigation system
   - Add navigation menu
   - Breadcrumbs

3. **Responsive Design**
   - Ensure responsive layout
   - Handle window resizing
   - Mobile-friendly (if needed)

#### 5.2 Backup/Restore

1. **Backup Service**
   - Implement backup functionality
   - Database backup
   - Document backup
   - Scheduled backups

2. **Restore Service**
   - Implement restore functionality
   - Backup validation
   - Restore process

#### 5.3 Testing

1. **Unit Tests**
   - Test all services
   - Test repositories
   - Test ViewModels

2. **Integration Tests**
   - Test database operations
   - Test sync functionality
   - Test end-to-end flows

3. **UI Tests** (Optional)
   - Test critical user flows
   - Test navigation

---

## Code Structure Examples

### Service Implementation Pattern

```csharp
public class PaymentService : IPaymentService
{
    private readonly IPaymentRepository _repository;
    private readonly IAuditService _auditService;
    private readonly IAuthenticationService _authService;
    
    public PaymentService(
        IPaymentRepository repository,
        IAuditService auditService,
        IAuthenticationService authService)
    {
        _repository = repository;
        _auditService = auditService;
        _authService = authService;
    }
    
    public async Task<PaymentDto> RecordPaymentAsync(PaymentDto payment)
    {
        // Validation
        if (string.IsNullOrWhiteSpace(payment.HouseCode))
            throw new ValidationException("House code is required");
        
        // Business logic
        var house = await _houseRepository.GetByCodeAsync(payment.HouseCode);
        if (house == null)
            throw new NotFoundException("House not found");
        
        // Create payment
        var entity = MapToEntity(payment);
        var result = await _repository.CreateAsync(entity);
        
        // Audit log
        await _auditService.LogActivityAsync(new ActivityLogDto
        {
            Action = ActionType.Create,
            EntityType = "Payment",
            EntityId = result.Id
        });
        
        return MapToDto(result);
    }
}
```

### ViewModel Pattern

```csharp
public class PaymentViewModel : ViewModelBase
{
    private readonly IPaymentService _paymentService;
    private PaymentDto _payment;
    
    public PaymentViewModel(IPaymentService paymentService)
    {
        _paymentService = paymentService;
        RecordPaymentCommand = new RelayCommand(async () => await RecordPaymentAsync());
    }
    
    public PaymentDto Payment
    {
        get => _payment;
        set => SetProperty(ref _payment, value);
    }
    
    public ICommand RecordPaymentCommand { get; }
    
    private async Task RecordPaymentAsync()
    {
        try
        {
            await _paymentService.RecordPaymentAsync(Payment);
            // Show success message
            // Navigate to payment list
        }
        catch (Exception ex)
        {
            // Show error message
        }
    }
}
```

### Repository Pattern

```csharp
public class PaymentRepository : IPaymentRepository
{
    private readonly SqliteDbContext _context;
    
    public PaymentRepository(SqliteDbContext context)
    {
        _context = context;
    }
    
    public async Task<Payment> CreateAsync(Payment payment)
    {
        payment.Id = Guid.NewGuid();
        payment.CreatedAt = DateTime.UtcNow;
        payment.UpdatedAt = DateTime.UtcNow;
        
        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();
        
        return payment;
    }
    
    public async Task<Payment> GetByIdAsync(Guid id)
    {
        return await _context.Payments
            .FirstOrDefaultAsync(p => p.Id == id);
    }
}
```

---

## Dependency Injection Setup

```csharp
public partial class App : Application
{
    private ServiceProvider _serviceProvider;
    
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        
        var services = new ServiceCollection();
        ConfigureServices(services);
        
        _serviceProvider = services.BuildServiceProvider();
        
        var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }
    
    private void ConfigureServices(IServiceCollection services)
    {
        // Database
        services.AddDbContext<SqliteDbContext>(options =>
            options.UseSqlite("Data Source=data/database/elmansour.db"));
        
        // Repositories
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        // ... other repositories
        
        // Services
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        // ... other services
        
        // ViewModels
        services.AddTransient<DashboardViewModel>();
        services.AddTransient<PaymentViewModel>();
        // ... other ViewModels
        
        // Views
        services.AddTransient<MainWindow>();
        services.AddTransient<LoginWindow>();
        // ... other Views
    }
}
```

---

## Testing Strategy

### Unit Tests

```csharp
[TestClass]
public class PaymentServiceTests
{
    private Mock<IPaymentRepository> _mockRepository;
    private Mock<IAuditService> _mockAuditService;
    private PaymentService _service;
    
    [TestInitialize]
    public void Setup()
    {
        _mockRepository = new Mock<IPaymentRepository>();
        _mockAuditService = new Mock<IAuditService>();
        _service = new PaymentService(_mockRepository.Object, _mockAuditService.Object);
    }
    
    [TestMethod]
    public async Task RecordPaymentAsync_ValidPayment_ReturnsPayment()
    {
        // Arrange
        var payment = new PaymentDto { HouseCode = "A01", Amount = 1500 };
        _mockRepository.Setup(r => r.CreateAsync(It.IsAny<Payment>()))
            .ReturnsAsync(new Payment { Id = Guid.NewGuid() });
        
        // Act
        var result = await _service.RecordPaymentAsync(payment);
        
        // Assert
        Assert.IsNotNull(result);
        _mockRepository.Verify(r => r.CreateAsync(It.IsAny<Payment>()), Times.Once);
    }
}
```

---

## Deployment Checklist

- [ ] All tests passing
- [ ] Database migrations applied
- [ ] Supabase configured
- [ ] Environment variables set
- [ ] Backup mechanism tested
- [ ] Documentation complete
- [ ] User training materials ready
- [ ] Support contact information configured

---

## Summary

This implementation guide provides:
- ✅ Phased approach (10 weeks)
- ✅ Step-by-step instructions
- ✅ Code structure examples
- ✅ Testing strategy
- ✅ Deployment checklist

Follow this guide to implement the complete El Mansour Syndic Manager application.

