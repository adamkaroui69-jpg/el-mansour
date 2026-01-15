# Guide UX Moderne - El Mansour Syndic Manager

Ce guide détaille les patterns UX mis en place pour améliorer le confort d'utilisation quotidien de l'application.

## 1. Philosophie UX
- **Non-blocking** : L'utilisateur ne doit pas être interrompu sauf nécessité absolue.
- **Feedback Immédiat** : Chaque action doit avoir une réaction visible (Loading, Toast, Animation).
- **Sécurité** : Les actions destructives doivent être confirmées ou annulables.

## 2. Système de Notifications (Toasts/Snackbars)
Utilisez `IDialogService.ShowMessage` pour informer l'utilisateur de manière non intrusive.

### Quand l'utiliser :
- Confirmation de succès ("Paiement enregistré")
- Erreur mineure ("Impossible de connecter à l'imprimante")
- Information contextuelle ("Sauvegarde en cours...")

### Exemple d'implémentation (ViewModel) :
```csharp
public class MyViewModel : ViewModelBase
{
    private readonly IDialogService _dialogService;

    public MyViewModel(IDialogService dialogService)
    {
        _dialogService = dialogService;
    }

    public void SaveCommand()
    {
        // ... Logique de sauvegarde
        _dialogService.ShowMessage("Sauvegarde effectuée avec succès !");
    }
}
```

## 3. Confirmations Intelligentes
Évitez les `MessageBox` natifs bloquants et moches. Utilisez `IDialogService.ShowConfirmationAsync`.

### Quand l'utiliser :
- Suppression d'un élément important (Paiement, Utilisateur).
- Action irréversible (Clôture d'exercice).
- Quitter un formulaire avec des modifications non sauvegardées.

### Exemple :
```csharp
var confirmed = await _dialogService.ShowConfirmationAsync(
    "Êtes-vous sûr de vouloir supprimer ce paiement ? Cette action est irréversible.",
    "Supprimer le paiement",
    "Supprimer", // Bouton Confirmer (Rouge implicitement via style si possible, sinon texte clair)
    "Annuler"
);

if (confirmed)
{
    await _repository.DeleteAsync(id);
    _dialogService.ShowMessage("Paiement supprimé.");
}
```

## 4. Système Undo (Annuler)
Pour les suppressions fréquentes (ex: suppression d'une ligne dans une grille), préférez le "Soft Delete" avec option d'annulation immédiate via Snackbar.

### Pattern Recommandé :
1.  Masquer l'élément dans la liste (UI).
2.  Ne pas supprimer en base tout de suite (ou marquer IsDeleted=true).
3.  Afficher une Snackbar avec bouton "ANNULER".
4.  Si l'utilisateur clique "ANNULER", ré-afficher l'élément.
5.  Sinon, valider la suppression (après délai ou action suivante).

### Exemple :
```csharp
public void DeleteItem(ItemDto item)
{
    // 1. Suppression visuelle temporaire
    Items.Remove(item);

    // 2. Notification avec Action
    _dialogService.ShowMessage(
        "Élément supprimé", 
        "ANNULER", 
        () => 
        {
            // Action d'annulation
            Items.Add(item);
            // Annuler la suppression en base si elle avait été lancée
        }
    );
    
    // 3. Appel asynchrone réel (si pas de undo soft-delete purement UI)
    // _service.Delete(item); 
}
```

## 5. Indicateurs de Chargement
Utilisez les indicateurs de chargement de Material Design.

### Overlay Global (DialogHost)
Pour les chargements bloquants (Login, Initialisation lourde).

```xml
<materialDesign:DialogHost IsOpen="{Binding IsLoading}">
    <materialDesign:DialogHost.DialogContent>
        <ProgressBar Style="{StaticResource MaterialDesignCircularProgressBar}" 
                     Value="0" IsIndeterminate="True" />
    </materialDesign:DialogHost.DialogContent>
    <!-- Contenu -->
</materialDesign:DialogHost>
```

### Indicateur Inline (Boutons)
Pour les actions asynchrones (Sauvegarder).

```xml
<Button Command="{Binding SaveCommand}">
    <Grid>
        <TextBlock Text="Enregistrer" 
                   Visibility="{Binding IsSaving, Converter={StaticResource InverseBooleanToVisibilityConverter}}"/>
        <ProgressBar Style="{StaticResource MaterialDesignCircularProgressBar}" 
                     IsIndeterminate="True" 
                     Value="0" Width="20" Height="20"
                     Visibility="{Binding IsSaving, Converter={StaticResource BooleanToVisibilityConverter}}"/>
    </Grid>
</Button>
```

## 6. Bonnes Pratiques XAML (Material Design)

### Ombres et Profondeur
Utilisez les effets d'ombre pour hiérarchiser l'information.
```xml
<Border Effect="{StaticResource MaterialDesignShadowDepth1}">...</Border>
```

### Cartes (Cards)
Encapsulez le contenu groupé dans des cartes.
```xml
<materialDesign:Card Padding="16" Margin="8">
    <StackPanel>
        <TextBlock Style="{StaticResource MaterialDesignHeadline6TextBlock}" Text="Titre"/>
        <!-- Contenu -->
    </StackPanel>
</materialDesign:Card>
```

### Champs de saisie
Utilisez toujours les styles Material Design avec HintAssist.
```xml
<TextBox materialDesign:HintAssist.Hint="Nom d'utilisateur"
         Style="{StaticResource MaterialDesignFloatingHintTextBox}" />
```
