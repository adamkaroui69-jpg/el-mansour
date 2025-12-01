# Tests Unitaires - El Mansour Syndic Manager

## 📋 Vue d'ensemble

Ce projet contient les tests unitaires pour l'application El Mansour Syndic Manager. Les tests sont écrits avec **xUnit**, **Moq** et **FluentAssertions** pour garantir la qualité et la fiabilité du code.

## 🎯 Couverture des Tests

### Services Testés
- ✅ **PaymentService** - Gestion des paiements
  - Création de paiements
  - Validation des paiements
  - Gestion des maisons impayées
  - Suppression de paiements

### Entités Testées
- ✅ **Payment** - Entité de paiement
  - Initialisation et propriétés
  - Validation des formats
  - Statuts de paiement

- ✅ **House** - Entité de maison
  - Initialisation et propriétés
  - Codes de maison
  - Validation des bâtiments

## 🚀 Exécution des Tests

### Tous les tests
```powershell
dotnet test
```

### Tests avec détails
```powershell
dotnet test --logger "console;verbosity=detailed"
```

### Tests avec couverture de code
```powershell
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

### Tests d'un fichier spécifique
```powershell
dotnet test --filter "FullyQualifiedName~PaymentServiceTests"
```

## 📊 Résultats

**Nombre total de tests : 29**
- ✅ Réussis : 29
- ❌ Échecs : 0
- ⏭️ Ignorés : 0

**Taux de réussite : 100%**

## 🛠️ Technologies Utilisées

- **xUnit 2.9.3** - Framework de tests
- **Moq 4.20.70** - Bibliothèque de mocking
- **FluentAssertions 6.12.0** - Assertions fluides et lisibles
- **.NET 8.0** - Framework cible

## 📁 Structure des Tests

```
ElMansourSyndicManager.Tests.Unit/
├── Services/
│   └── PaymentServiceTests.cs
├── Domain/
│   ├── PaymentTests.cs
│   └── HouseTests.cs
└── ElMansourSyndicManager.Tests.Unit.csproj
```

## ✍️ Écriture de Nouveaux Tests

### Exemple de test de service

```csharp
[Fact]
public async Task MethodName_Scenario_ExpectedBehavior()
{
    // Arrange
    var mock = new Mock<IDependency>();
    mock.Setup(x => x.Method()).ReturnsAsync(expectedValue);
    var service = new Service(mock.Object);

    // Act
    var result = await service.MethodToTest();

    // Assert
    result.Should().Be(expectedValue);
    mock.Verify(x => x.Method(), Times.Once);
}
```

### Exemple de test d'entité

```csharp
[Fact]
public void Entity_Property_ShouldBehavior()
{
    // Arrange & Act
    var entity = new Entity
    {
        Property = value
    };

    // Assert
    entity.Property.Should().Be(value);
}
```

## 🎨 Conventions de Nommage

- **Nom du test** : `MethodName_Scenario_ExpectedBehavior`
- **Fichiers de test** : `{ClassName}Tests.cs`
- **Namespace** : `ElMansourSyndicManager.Tests.Unit.{Category}`

## 📝 Bonnes Pratiques

1. **Arrange-Act-Assert** : Structurer chaque test en 3 parties
2. **Un test = Un scénario** : Chaque test doit tester un seul comportement
3. **Tests isolés** : Les tests ne doivent pas dépendre les uns des autres
4. **Mocks explicites** : Utiliser Moq pour isoler les dépendances
5. **Assertions claires** : Utiliser FluentAssertions pour la lisibilité

## 🔍 Tests à Ajouter (Prochaines Étapes)

- [ ] **AuthenticationService** - Tests d'authentification
- [ ] **UserService** - Tests de gestion des utilisateurs
- [ ] **MaintenanceService** - Tests de gestion de la maintenance
- [ ] **ReportService** - Tests de génération de rapports
- [ ] **ReceiptService** - Tests de génération de reçus
- [ ] **Repositories** - Tests des repositories
- [ ] **ViewModels** - Tests des ViewModels

## 📈 Amélioration Continue

Pour augmenter la couverture de code :
1. Ajouter des tests pour les cas limites (edge cases)
2. Tester les scénarios d'erreur
3. Ajouter des tests d'intégration
4. Mesurer la couverture de code avec Coverlet

## 🐛 Débogage des Tests

### Dans Visual Studio
1. Ouvrir le Test Explorer (Test > Test Explorer)
2. Clic droit sur un test > Debug

### En ligne de commande
```powershell
# Exécuter un test spécifique en mode debug
dotnet test --filter "FullyQualifiedName~TestName" --logger "console;verbosity=detailed"
```

## 📞 Support

Pour toute question sur les tests :
- Consulter la documentation xUnit : https://xunit.net/
- Consulter la documentation Moq : https://github.com/moq/moq4
- Consulter la documentation FluentAssertions : https://fluentassertions.com/

---

**Dernière mise à jour** : 30 Novembre 2025  
**Version** : 1.0.0
