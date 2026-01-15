# Guide Métier : Gestion Financière et Exports

## 1. Modèle Financier Avancé

### Calcul Automatique des Arriérés
Le système calcule désormais automatiquement la situation financière de chaque résident en temps réel.

**Logique de Calcul (FIFO) :**
1.  **Dû Théorique :** Cumul des cotisations mensuelles depuis la date d'entrée (ou création de la fiche) jusqu'à aujourd'hui.
2.  **Paiements Réels :** Somme de tous les paiements validés.
3.  **Affectation :** Les paiements "remplissent" les mois dûs en commençant par le plus ancien.
    *   *Exemple :* Si un résident doit Janvier (200), Février (200) et Mars (200), et qu'il paie 500 :
        *   Janvier : Payé (reste 300)
        *   Février : Payé (reste 100)
        *   Mars : Partiellement Payé (Reste à payer : 100).

### États Financiers (Comptes Résidents)
Chaque résident a un "Solde" :
*   **Positif (Bleu)** : Le résident a payé en avance.
*   **Nul (Vert)** : Le résident est à jour.
*   **Négatif (Orange/Rouge)** : Le résident a des dettes.
    *   *Orange* : Retard léger (< 3 mois).
    *   *Rouge* : Retard critique (3 mois ou plus).

---

## 2. Exports de Données

L'application supporte désormais l'export natif vers Excel (.xlsx) et CSV.

### Utilisation dans le Code

Injectez `IExportService` dans vos ViewModels.

**Exemple d'Export Excel :**
```csharp
public async Task ExportDataAsync()
{
    // 1. Récupérer les données
    var data = await _financialService.GetAllResidentsFinancialStateAsync();
    
    // 2. Générer le fichier
    var excelBytes = _exportService.ExportToExcel(data, "État Comptes");
    
    // 3. Sauvegarder (Dialogue Windows)
    var saveFileDialog = new SaveFileDialog 
    { 
        Filter = "Excel file (*.xlsx)|*.xlsx", 
        FileName = $"Etat_Financier_{DateTime.Now:yyyyMMdd}.xlsx" 
    };
    
    if (saveFileDialog.ShowDialog() == true)
    {
        File.WriteAllBytes(saveFileDialog.FileName, excelBytes);
        _dialogService.ShowMessage("Export réussi !");
    }
}
```

---

## 3. Bonnes Pratiques Comptables (Syndic)

### 3.1 Tracez tout
Ne supprimez jamais un paiement erroné s'il a déjà été reçu/banque. Créez un paiement "négatif" ou une écriture de régularisation pour garder une trace d'audit. (Le système Soft-Delete de l'application gère cela partiellement).

### 3.2 Gestion des Avances
Si un résident paie pour l'année entière :
-   Le système encaissera la somme totale.
-   Son solde sera positif.
-   Chaque mois futur, le montant dû augmentera, "consommant" l'avance automatiquement.
-   **Avantage :** Pas besoin d'intervention manuelle chaque mois.

### 3.3 Clôture et Arriérés
Lors de l'assemblée générale, utilisez le rapport **"État des Arriérés"** généré par `IFinancialService` pour présenter les dettes recouvrables.

---

## 4. Structure des Données (Technique)

### DTOs Clés
*   `ResidentFinancialStateDto` : Résumé d'un compte (Solde, Total Dû, Total Payé).
*   `UnpaidMonthDto` : Détail d'un impayé (Mois concerné, Montant restant, Jours de retard).

### Services
*   `IFinancialService` : Cerveau financier.
*   `IExportService` : Moteur de génération de fichiers (basé sur ClosedXML pour Excel).

