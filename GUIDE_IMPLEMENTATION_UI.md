# 🎨 Guide d'Implémentation - Charte Graphique

## 📋 Table des Matières
1. [Configuration Initiale](#1-configuration-initiale)
2. [Page Login Modernisée](#2-page-login-modernisée)
3. [Dashboard](#3-dashboard)
4. [Liste de Paiements](#4-liste-de-paiements)
5. [Formulaires](#5-formulaires)
6. [Switch Mode Clair/Sombre](#6-switch-mode-clairsombre)
7. [Exemples de Code](#7-exemples-de-code)

---

## 1. Configuration Initiale

### Étape 1 : Mettre à jour App.xaml

```xml
<Application x:Class="ElMansourSyndicManager.App"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:materialDesign="http://materialdesigninxaml.net/winfx/xaml/themes">
    <Application.Resources>
        <ResourceDictionary>
            <ResourceDictionary.MergedDictionaries>
                <!-- Material Design Base -->
                <ResourceDictionary Source="pack://application:,,,/MaterialDesignThemes.Wpf;component/Themes/MaterialDesignTheme.Dark.xaml" />
                <ResourceDictionary Source="pack://application:,,,/MaterialDesignThemes.Wpf;component/Themes/MaterialDesignTheme.Defaults.xaml" />
                
                <!-- Couleurs Primary & Accent -->
                <ResourceDictionary Source="pack://application:,,,/MaterialDesignColors;component/Themes/Recommended/Primary/MaterialDesignColor.Blue.xaml" />
                <ResourceDictionary Source="pack://application:,,,/MaterialDesignColors;component/Themes/Recommended/Accent/MaterialDesignColor.Teal.xaml" />
                
                <!-- NOTRE DESIGN SYSTEM (IMPORTANT : En dernier pour override) -->
                <ResourceDictionary Source="/Resources/DesignSystem.xaml" />
                <ResourceDictionary Source="/Resources/ProfessionalStyles.xaml" />
                
            </ResourceDictionary.MergedDictionaries>
        </ResourceDictionary>
    </Application.Resources>
</Application>
```

---

## 2. Page Login Modernisée

### Design
- **Fond** : Dégradé subtil ou image de fond professionnelle
- **Carte centrale** : Elevation 8, max-width 400px
- **Logo** : 64px, centré
- **Champs** : Outlined style, espacement généreux
- **Bouton** : Primary, pleine largeur

### Code XAML

```xml
<Window x:Class="ElMansourSyndicManager.Views.LoginView"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:materialDesign="http://materialdesigninxaml.net/winfx/xaml/themes"
        Title="El Mansour Syndic Manager" 
        Height="600" Width="900"
        WindowStartupLocation="CenterScreen"
        WindowStyle="None"
        ResizeMode="NoResize"
        Background="{DynamicResource MaterialDesignPaper}">
    
    <Grid>
        <!-- Fond avec dégradé subtil -->
        <Grid.Background>
            <LinearGradientBrush StartPoint="0,0" EndPoint="1,1">
                <GradientStop Color="#1E1E1E" Offset="0"/>
                <GradientStop Color="#2D2D2D" Offset="1"/>
            </LinearGradientBrush>
        </Grid.Background>

        <!-- Carte de Login Centrée -->
        <materialDesign:Card MaxWidth="400" 
                             VerticalAlignment="Center" 
                             HorizontalAlignment="Center"
                             Padding="32"
                             UniformCornerRadius="12"
                             materialDesign:ElevationAssist.Elevation="Dp8">
            <StackPanel>
                <!-- Logo et Titre -->
                <materialDesign:PackIcon Kind="AccountBalance" 
                                        Width="64" Height="64"
                                        HorizontalAlignment="Center"
                                        Foreground="{StaticResource PrimaryBrush}"
                                        Margin="0,0,0,16"/>
                
                <TextBlock Text="El Mansour" 
                          Style="{StaticResource PageTitle}"
                          TextAlignment="Center"
                          FontSize="28"
                          Margin="0,0,0,4"/>
                
                <TextBlock Text="Syndic Manager" 
                          Style="{StaticResource SecondaryText}"
                          TextAlignment="Center"
                          FontSize="14"
                          Margin="0,0,0,32"/>

                <!-- Champ Identifiant -->
                <TextBox materialDesign:HintAssist.Hint="Identifiant"
                        Style="{StaticResource ProfessionalTextBox}"
                        Margin="0,0,0,16"
                        Text="{Binding Username, UpdateSourceTrigger=PropertyChanged}">
                    <TextBox.InputBindings>
                        <KeyBinding Key="Enter" Command="{Binding LoginCommand}"/>
                    </TextBox.InputBindings>
                </TextBox>

                <!-- Champ Mot de Passe -->
                <PasswordBox materialDesign:HintAssist.Hint="Mot de passe"
                            Style="{StaticResource ProfessionalPasswordBox}"
                            Margin="0,0,0,24"
                            materialDesign:PasswordBoxAssist.Password="{Binding Password, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}">
                    <PasswordBox.InputBindings>
                        <KeyBinding Key="Enter" Command="{Binding LoginCommand}"/>
                    </PasswordBox.InputBindings>
                </PasswordBox>

                <!-- Message d'Erreur -->
                <TextBlock Text="{Binding ErrorMessage}"
                          Foreground="{StaticResource ErrorBrush}"
                          TextWrapping="Wrap"
                          Margin="0,0,0,16"
                          Visibility="{Binding HasError, Converter={StaticResource BooleanToVisibilityConverter}}"/>

                <!-- Bouton Connexion -->
                <Button Content="SE CONNECTER"
                       Style="{StaticResource PrimaryButton}"
                       Command="{Binding LoginCommand}"
                       IsDefault="True"
                       Height="48"
                       FontSize="15"/>

                <!-- Lien Mot de Passe Oublié -->
                <Button Content="Mot de passe oublié ?"
                       Style="{StaticResource MaterialDesignFlatButton}"
                       HorizontalAlignment="Center"
                       Margin="0,16,0,0"
                       Foreground="{StaticResource PrimaryBrush}"/>
            </StackPanel>
        </materialDesign:Card>

        <!-- Version en bas à droite -->
        <TextBlock Text="Version 1.0.0"
                  Style="{StaticResource SecondaryText}"
                  HorizontalAlignment="Right"
                  VerticalAlignment="Bottom"
                  Margin="16"/>
    </Grid>
</Window>
```

---

## 3. Dashboard

### Structure
- **Header** : Titre + actions
- **KPI Cards** : 4 cartes en grille
- **Graphiques** : 2 colonnes
- **Liste récente** : DataGrid compact

### Code XAML

```xml
<UserControl x:Class="ElMansourSyndicManager.Views.DashboardView"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:materialDesign="http://materialdesigninxaml.net/winfx/xaml/themes">
    
    <ScrollViewer VerticalScrollBarVisibility="Auto">
        <StackPanel Margin="24">
            
            <!-- Header -->
            <Grid Margin="0,0,0,24">
                <TextBlock Text="Tableau de Bord" Style="{StaticResource PageTitle}"/>
                <StackPanel Orientation="Horizontal" HorizontalAlignment="Right">
                    <Button Content="EXPORTER" Style="{StaticResource SecondaryButton}" Margin="0,0,8,0"/>
                    <Button Content="ACTUALISER" Style="{StaticResource PrimaryButton}"/>
                </StackPanel>
            </Grid>

            <!-- KPI Cards -->
            <Grid Margin="0,0,0,32">
                <Grid.ColumnDefinitions>
                    <ColumnDefinition Width="*"/>
                    <ColumnDefinition Width="*"/>
                    <ColumnDefinition Width="*"/>
                    <ColumnDefinition Width="*"/>
                </Grid.ColumnDefinitions>

                <!-- Total Encaissé -->
                <materialDesign:Card Grid.Column="0" Style="{StaticResource KPICard}" Margin="0,0,16,0">
                    <StackPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,0,0,8">
                            <materialDesign:PackIcon Kind="CashMultiple" 
                                                    Width="24" Height="24"
                                                    Foreground="{StaticResource SuccessBrush}"
                                                    VerticalAlignment="Center"/>
                            <TextBlock Text="Total Encaissé" 
                                      Style="{StaticResource SecondaryText}"
                                      Margin="8,0,0,0"
                                      VerticalAlignment="Center"/>
                        </StackPanel>
                        <TextBlock Text="{Binding TotalEncaisse, StringFormat='{}{0:N0} TND'}" 
                                  FontSize="28"
                                  FontWeight="SemiBold"
                                  Foreground="{StaticResource SuccessBrush}"/>
                        <TextBlock Text="+12% ce mois" 
                                  Style="{StaticResource SecondaryText}"
                                  Margin="0,4,0,0"/>
                    </StackPanel>
                </materialDesign:Card>

                <!-- Arriérés -->
                <materialDesign:Card Grid.Column="1" Style="{StaticResource KPICard}" Margin="0,0,16,0">
                    <StackPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,0,0,8">
                            <materialDesign:PackIcon Kind="AlertCircle" 
                                                    Width="24" Height="24"
                                                    Foreground="{StaticResource WarningBrush}"
                                                    VerticalAlignment="Center"/>
                            <TextBlock Text="Arriérés" 
                                      Style="{StaticResource SecondaryText}"
                                      Margin="8,0,0,0"
                                      VerticalAlignment="Center"/>
                        </StackPanel>
                        <TextBlock Text="{Binding TotalArriere, StringFormat='{}{0:N0} TND'}" 
                                  FontSize="28"
                                  FontWeight="SemiBold"
                                  Foreground="{StaticResource WarningBrush}"/>
                        <TextBlock Text="15 résidents" 
                                  Style="{StaticResource SecondaryText}"
                                  Margin="0,4,0,0"/>
                    </StackPanel>
                </materialDesign:Card>

                <!-- Paiements en Attente -->
                <materialDesign:Card Grid.Column="2" Style="{StaticResource KPICard}" Margin="0,0,16,0">
                    <StackPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,0,0,8">
                            <materialDesign:PackIcon Kind="ClockOutline" 
                                                    Width="24" Height="24"
                                                    Foreground="{StaticResource InfoBrush}"
                                                    VerticalAlignment="Center"/>
                            <TextBlock Text="En Attente" 
                                      Style="{StaticResource SecondaryText}"
                                      Margin="8,0,0,0"
                                      VerticalAlignment="Center"/>
                        </StackPanel>
                        <TextBlock Text="{Binding PaiementsEnAttente}" 
                                  FontSize="28"
                                  FontWeight="SemiBold"
                                  Foreground="{StaticResource InfoBrush}"/>
                        <TextBlock Text="paiements" 
                                  Style="{StaticResource SecondaryText}"
                                  Margin="0,4,0,0"/>
                    </StackPanel>
                </materialDesign:Card>

                <!-- Taux de Recouvrement -->
                <materialDesign:Card Grid.Column="3" Style="{StaticResource KPICard}">
                    <StackPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,0,0,8">
                            <materialDesign:PackIcon Kind="ChartLine" 
                                                    Width="24" Height="24"
                                                    Foreground="{StaticResource PrimaryBrush}"
                                                    VerticalAlignment="Center"/>
                            <TextBlock Text="Taux de Recouvrement" 
                                      Style="{StaticResource SecondaryText}"
                                      Margin="8,0,0,0"
                                      VerticalAlignment="Center"/>
                        </StackPanel>
                        <TextBlock Text="{Binding TauxRecouvrement, StringFormat='{}{0:N1}%'}" 
                                  FontSize="28"
                                  FontWeight="SemiBold"
                                  Foreground="{StaticResource PrimaryBrush}"/>
                        <ProgressBar Value="{Binding TauxRecouvrement}" 
                                    Maximum="100"
                                    Style="{StaticResource ProfessionalProgressBar}"
                                    Margin="0,8,0,0"/>
                    </StackPanel>
                </materialDesign:Card>
            </Grid>

            <!-- Graphiques et Listes -->
            <Grid>
                <Grid.ColumnDefinitions>
                    <ColumnDefinition Width="2*"/>
                    <ColumnDefinition Width="*"/>
                </Grid.ColumnDefinitions>

                <!-- Graphique Évolution -->
                <materialDesign:Card Grid.Column="0" Style="{StaticResource ProfessionalCard}" Margin="0,0,16,0">
                    <StackPanel>
                        <TextBlock Text="Évolution des Encaissements" Style="{StaticResource CardTitle}"/>
                        <!-- Insérer votre graphique ici (LiveCharts, etc.) -->
                        <Border Height="300" Background="#F5F5F5" CornerRadius="4">
                            <TextBlock Text="Graphique" 
                                      HorizontalAlignment="Center" 
                                      VerticalAlignment="Center"
                                      Foreground="#999"/>
                        </Border>
                    </StackPanel>
                </materialDesign:Card>

                <!-- Derniers Paiements -->
                <materialDesign:Card Grid.Column="1" Style="{StaticResource ProfessionalCard}">
                    <StackPanel>
                        <TextBlock Text="Derniers Paiements" Style="{StaticResource CardTitle}"/>
                        <ListBox ItemsSource="{Binding DerniersPaiements}" 
                                BorderThickness="0"
                                MaxHeight="300">
                            <ListBox.ItemTemplate>
                                <DataTemplate>
                                    <StackPanel Margin="0,8">
                                        <TextBlock Text="{Binding ResidentName}" FontWeight="Medium"/>
                                        <TextBlock Text="{Binding Montant, StringFormat='{}{0:N0} TND'}" 
                                                  Foreground="{StaticResource SuccessBrush}"/>
                                        <TextBlock Text="{Binding Date, StringFormat='dd/MM/yyyy'}" 
                                                  Style="{StaticResource SecondaryText}"/>
                                    </StackPanel>
                                </DataTemplate>
                            </ListBox.ItemTemplate>
                        </ListBox>
                    </StackPanel>
                </materialDesign:Card>
            </Grid>
        </StackPanel>
    </ScrollViewer>
</UserControl>
```

---

## 4. Liste de Paiements

### Fonctionnalités
- **Filtres** : Date, Statut, Résident
- **DataGrid** : Lignes aérées, alternance de couleurs
- **Actions** : Icônes avec tooltips
- **États** : Badges colorés

### Code XAML

```xml
<UserControl x:Class="ElMansourSyndicManager.Views.PaymentsView"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:materialDesign="http://materialdesigninxaml.net/winfx/xaml/themes">
    
    <Grid Margin="24">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
        </Grid.RowDefinitions>

        <!-- Header -->
        <Grid Grid.Row="0" Margin="0,0,0,24">
            <TextBlock Text="Paiements" Style="{StaticResource PageTitle}"/>
            <StackPanel Orientation="Horizontal" HorizontalAlignment="Right">
                <Button Content="EXPORTER" Style="{StaticResource SecondaryButton}" Margin="0,0,8,0"/>
                <Button Content="+ NOUVEAU PAIEMENT" Style="{StaticResource PrimaryButton}"/>
            </StackPanel>
        </Grid>

        <!-- Filtres -->
        <materialDesign:Card Grid.Row="1" Padding="16" Margin="0,0,0,16">
            <Grid>
                <Grid.ColumnDefinitions>
                    <ColumnDefinition Width="*"/>
                    <ColumnDefinition Width="*"/>
                    <ColumnDefinition Width="*"/>
                    <ColumnDefinition Width="Auto"/>
                </Grid.ColumnDefinitions>

                <DatePicker Grid.Column="0" 
                           materialDesign:HintAssist.Hint="Date Début"
                           Style="{StaticResource ProfessionalDatePicker}"
                           Margin="0,0,16,0"/>

                <DatePicker Grid.Column="1" 
                           materialDesign:HintAssist.Hint="Date Fin"
                           Style="{StaticResource ProfessionalDatePicker}"
                           Margin="0,0,16,0"/>

                <ComboBox Grid.Column="2" 
                         materialDesign:HintAssist.Hint="Statut"
                         Style="{StaticResource ProfessionalComboBox}"
                         Margin="0,0,16,0">
                    <ComboBoxItem Content="Tous"/>
                    <ComboBoxItem Content="Payé"/>
                    <ComboBoxItem Content="En attente"/>
                    <ComboBoxItem Content="En retard"/>
                </ComboBox>

                <Button Grid.Column="3" 
                       Content="FILTRER" 
                       Style="{StaticResource PrimaryButton}"/>
            </Grid>
        </materialDesign:Card>

        <!-- DataGrid -->
        <DataGrid Grid.Row="2" 
                 Style="{StaticResource ProfessionalDataGrid}"
                 ItemsSource="{Binding Paiements}">
            
            <DataGrid.Columns>
                <!-- Date -->
                <DataGridTextColumn Header="Date" 
                                   Binding="{Binding Date, StringFormat='dd/MM/yyyy'}"
                                   Width="120"/>

                <!-- Résident -->
                <DataGridTextColumn Header="Résident" 
                                   Binding="{Binding ResidentName}"
                                   Width="*"/>

                <!-- Mois -->
                <DataGridTextColumn Header="Mois" 
                                   Binding="{Binding Mois}"
                                   Width="120"/>

                <!-- Montant -->
                <DataGridTextColumn Header="Montant" 
                                   Binding="{Binding Montant, StringFormat='{}{0:N2} TND'}"
                                   Width="120">
                    <DataGridTextColumn.ElementStyle>
                        <Style TargetType="TextBlock">
                            <Setter Property="FontWeight" Value="SemiBold"/>
                            <Setter Property="Foreground" Value="{StaticResource SuccessBrush}"/>
                        </Style>
                    </DataGridTextColumn.ElementStyle>
                </DataGridTextColumn>

                <!-- Statut -->
                <DataGridTemplateColumn Header="Statut" Width="120">
                    <DataGridTemplateColumn.CellTemplate>
                        <DataTemplate>
                            <Border Style="{StaticResource StatusBadge}">
                                <Border.Style>
                                    <Style TargetType="Border" BasedOn="{StaticResource StatusBadge}">
                                        <Style.Triggers>
                                            <DataTrigger Binding="{Binding Status}" Value="Paid">
                                                <Setter Property="Background" Value="{StaticResource SuccessBrush}"/>
                                            </DataTrigger>
                                            <DataTrigger Binding="{Binding Status}" Value="Pending">
                                                <Setter Property="Background" Value="{StaticResource WarningBrush}"/>
                                            </DataTrigger>
                                            <DataTrigger Binding="{Binding Status}" Value="Overdue">
                                                <Setter Property="Background" Value="{StaticResource ErrorBrush}"/>
                                            </DataTrigger>
                                        </Style.Triggers>
                                    </Style>
                                </Border.Style>
                                <TextBlock Text="{Binding StatusText}" 
                                          Foreground="White" 
                                          FontSize="12"
                                          FontWeight="Medium"/>
                            </Border>
                        </DataTemplate>
                    </DataGridTemplateColumn.CellTemplate>
                </DataGridTemplateColumn>

                <!-- Actions -->
                <DataGridTemplateColumn Header="Actions" Width="150">
                    <DataGridTemplateColumn.CellTemplate>
                        <DataTemplate>
                            <StackPanel Orientation="Horizontal">
                                <Button Style="{StaticResource IconButton}" 
                                       ToolTip="Voir le reçu"
                                       Margin="0,0,4,0">
                                    <materialDesign:PackIcon Kind="Eye" Width="18" Height="18"/>
                                </Button>
                                <Button Style="{StaticResource IconButton}" 
                                       ToolTip="Imprimer"
                                       Margin="0,0,4,0">
                                    <materialDesign:PackIcon Kind="Printer" Width="18" Height="18"/>
                                </Button>
                                <Button Style="{StaticResource IconButton}" 
                                       ToolTip="Télécharger"
                                       Margin="0,0,4,0">
                                    <materialDesign:PackIcon Kind="Download" Width="18" Height="18"/>
                                </Button>
                                <Button Style="{StaticResource IconButton}" 
                                       ToolTip="Envoyer par email">
                                    <materialDesign:PackIcon Kind="Email" Width="18" Height="18"/>
                                </Button>
                            </StackPanel>
                        </DataTemplate>
                    </DataGridTemplateColumn.CellTemplate>
                </DataGridTemplateColumn>
            </DataGrid.Columns>
        </DataGrid>
    </Grid>
</UserControl>
```

---

## 5. Formulaires

### Exemple : Ajouter un Paiement

```xml
<materialDesign:Card Padding="24" MaxWidth="600">
    <StackPanel>
        <TextBlock Text="Nouveau Paiement" Style="{StaticResource SectionTitle}"/>

        <ComboBox materialDesign:HintAssist.Hint="Résident *"
                 Style="{StaticResource ProfessionalComboBox}"
                 Margin="0,0,0,16"
                 ItemsSource="{Binding Residents}"
                 SelectedItem="{Binding SelectedResident}"/>

        <TextBox materialDesign:HintAssist.Hint="Mois (YYYY-MM) *"
                Style="{StaticResource ProfessionalTextBox}"
                Margin="0,0,0,16"
                Text="{Binding Mois}"/>

        <TextBox materialDesign:HintAssist.Hint="Montant (TND) *"
                Style="{StaticResource ProfessionalTextBox}"
                Margin="0,0,0,16"
                Text="{Binding Montant}"/>

        <DatePicker materialDesign:HintAssist.Hint="Date de Paiement"
                   Style="{StaticResource ProfessionalDatePicker}"
                   Margin="0,0,0,16"
                   SelectedDate="{Binding DatePaiement}"/>

        <TextBox materialDesign:HintAssist.Hint="Numéro de Référence"
                Style="{StaticResource ProfessionalTextBox}"
                Margin="0,0,0,24"
                Text="{Binding Reference}"/>

        <StackPanel Orientation="Horizontal" HorizontalAlignment="Right">
            <Button Content="ANNULER" 
                   Style="{StaticResource SecondaryButton}" 
                   Margin="0,0,8,0"
                   Command="{Binding CancelCommand}"/>
            <Button Content="ENREGISTRER" 
                   Style="{StaticResource PrimaryButton}"
                   Command="{Binding SaveCommand}"/>
        </StackPanel>
    </StackPanel>
</materialDesign:Card>
```

---

## 6. Switch Mode Clair/Sombre

### Dans MainViewModel.cs

```csharp
using MaterialDesignThemes.Wpf;

public class MainViewModel : ViewModelBase
{
    private readonly PaletteHelper _paletteHelper = new PaletteHelper();
    private bool _isDarkMode = true;

    public bool IsDarkMode
    {
        get => _isDarkMode;
        set
        {
            if (SetProperty(ref _isDarkMode, value))
            {
                ApplyTheme(value);
            }
        }
    }

    private void ApplyTheme(bool isDark)
    {
        var theme = _paletteHelper.GetTheme();
        theme.SetBaseTheme(isDark ? Theme.Dark : Theme.Light);
        _paletteHelper.SetTheme(theme);
    }
}
```

### Dans MainWindow.xaml (Toggle Button)

```xml
<ToggleButton Style="{StaticResource MaterialDesignSwitchToggleButton}"
             IsChecked="{Binding IsDarkMode}"
             ToolTip="Mode Sombre / Clair"
             Margin="16,0">
    <ToggleButton.Content>
        <StackPanel Orientation="Horizontal">
            <materialDesign:PackIcon Kind="WeatherNight" 
                                    Width="20" Height="20"
                                    Margin="0,0,8,0"/>
            <TextBlock Text="Mode Sombre"/>
        </StackPanel>
    </ToggleButton.Content>
</ToggleButton>
```

---

## 7. Exemples de Code

### Notification Snackbar

```csharp
// Dans votre ViewModel
_dialogService.ShowMessage("Paiement enregistré avec succès !", "ANNULER", () => 
{
    // Action d'annulation
});
```

### Dialog de Confirmation

```csharp
var confirmed = await _dialogService.ShowConfirmationAsync(
    "Êtes-vous sûr de vouloir supprimer ce paiement ?",
    "Confirmation",
    "SUPPRIMER",
    "ANNULER"
);

if (confirmed)
{
    // Supprimer
}
```

### Badge de Statut Dynamique

```xml
<Border>
    <Border.Style>
        <Style TargetType="Border" BasedOn="{StaticResource StatusBadge}">
            <Style.Triggers>
                <DataTrigger Binding="{Binding Status}" Value="Paid">
                    <Setter Property="Background" Value="{StaticResource SuccessBrush}"/>
                </DataTrigger>
                <DataTrigger Binding="{Binding Status}" Value="Overdue">
                    <Setter Property="Background" Value="{StaticResource ErrorBrush}"/>
                </DataTrigger>
            </Style.Triggers>
        </Style>
    </Border.Style>
    <TextBlock Text="{Binding StatusText}" Foreground="White"/>
</Border>
```

---

## 📋 Checklist d'Implémentation

### Configuration
- [ ] DesignSystem.xaml ajouté et référencé
- [ ] ProfessionalStyles.xaml ajouté et référencé
- [ ] App.xaml mis à jour avec les bonnes références

### Pages
- [ ] Login modernisé
- [ ] Dashboard avec KPI cards
- [ ] Listes avec DataGrid professionnel
- [ ] Formulaires avec styles cohérents

### Composants
- [ ] Boutons Primary/Secondary/Danger utilisés
- [ ] Cartes avec elevation
- [ ] Badges de statut colorés
- [ ] Icônes Material Design

### UX
- [ ] Mode sombre/clair fonctionnel
- [ ] Snackbar pour notifications
- [ ] Dialogs pour confirmations
- [ ] Animations subtiles

---

**Votre application aura maintenant un design professionnel, cohérent et agréable à utiliser ! 🎨**
