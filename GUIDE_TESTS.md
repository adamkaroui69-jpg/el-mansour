# Guide d'Utilisation des Tests Unitaires

## 🚀 Démarrage Rapide

### Exécuter tous les tests
```powershell
.\run-tests.ps1
```

### Exécuter les tests avec détails
```powershell
.\run-tests.ps1 -ShowDetails
```

### Exécuter uniquement les tests de services
```powershell
.\run-tests.ps1 -TestType Services
```

### Exécuter uniquement les tests du domaine
```powershell
.\run-tests.ps1 -TestType Domain
```

### Exécution rapide (sans rebuild)
```powershell
.\run-tests.ps1 -TestType Quick
```

## 📁 Fichiers Importants

- **`run-tests.ps1`** - Script principal pour exécuter les tests
- **`tests/RAPPORT_TESTS.md`** - Rapport détaillé des tests
- **`tests/ElMansourSyndicManager.Tests.Unit/README.md`** - Documentation des tests

## 📊 Résultats Actuels

✅ **29 tests** - Tous réussis  
⏱️ **Durée** : ~1.2 secondes  
📈 **Taux de réussite** : 100%

## 🎯 Couverture

### Services (5 tests)
- PaymentService ✅

### Entités (24 tests)
- Payment ✅
- House ✅

## 📝 Prochaines Étapes

Pour améliorer la couverture des tests :

1. Ajouter des tests pour `AuthenticationService`
2. Ajouter des tests pour `UserService`
3. Ajouter des tests pour `MaintenanceService`
4. Ajouter des tests pour `ReportService`
5. Ajouter des tests pour les Repositories
6. Ajouter des tests pour les ViewModels

## 🛠️ Commandes Utiles

### Depuis Visual Studio
- Ouvrir **Test Explorer** (Test > Test Explorer)
- Cliquer sur "Run All" pour exécuter tous les tests
- Clic droit sur un test > "Debug" pour déboguer

### Depuis la ligne de commande
```powershell
# Exécuter tous les tests
cd tests\ElMansourSyndicManager.Tests.Unit
dotnet test

# Exécuter un test spécifique
dotnet test --filter "FullyQualifiedName~PaymentServiceTests"

# Avec couverture de code
dotnet test /p:CollectCoverage=true
```

## 📞 Support

Pour toute question :
- Consulter `tests/ElMansourSyndicManager.Tests.Unit/README.md`
- Consulter `tests/RAPPORT_TESTS.md`

---

**Dernière mise à jour** : 30 Novembre 2025
