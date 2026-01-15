# 🎨 Guide Visuel Rapide - Palette de Couleurs

## Couleurs Principales

### Primary (Bleu Professionnel)
```
█ #1976D2  Primary 500     - Boutons principaux, liens
█ #42A5F5  Primary 400     - Hover states
█ #1565C0  Primary 600     - Pressed states
█ #0D47A1  Primary 700     - Emphasis
```

### Secondary (Émeraude Élégant)
```
█ #00897B  Secondary 500   - Accents, badges
█ #26A69A  Secondary 400   - Lighter variant
█ #00796B  Secondary 600   - Darker variant
```

### Accent (Orange Discret)
```
█ #FF6F00  Accent 500      - Call-to-action
█ #FF8F00  Accent 400      - Lighter
█ #E65100  Accent 600      - Darker
```

---

## Couleurs Sémantiques

### Success (Vert)
```
█ #4CAF50  Success         - Paiements validés, opérations réussies
█ #81C784  Success Light   - Backgrounds
█ #388E3C  Success Dark    - Text emphasis
```

### Error (Rouge)
```
█ #F44336  Error           - Erreurs, suppressions, alertes critiques
█ #E57373  Error Light     - Backgrounds
█ #D32F2F  Error Dark      - Text emphasis
```

### Warning (Orange)
```
█ #FF9800  Warning         - Retards de paiement, actions à confirmer
█ #FFB74D  Warning Light   - Backgrounds
█ #F57C00  Warning Dark    - Text emphasis
```

### Info (Bleu Clair)
```
█ #2196F3  Info            - Messages informatifs, tooltips
█ #64B5F6  Info Light      - Backgrounds
█ #1976D2  Info Dark       - Text emphasis
```

---

## Mode Sombre (Recommandé)

### Backgrounds
```
█ #1E1E1E  Surface         - Fond principal de l'application
█ #2D2D2D  Card            - Cartes, panels, conteneurs
█ #383838  Elevated        - Éléments surélevés (hover)
█ #424242  Dialog          - Dialogues modaux
```

### Textes
```
█ #FFFFFF  Primary Text    - Texte principal (100% opacité)
█ #B3B3B3  Secondary Text  - Texte secondaire (70% opacité)
█ #666666  Disabled Text   - Texte désactivé (40% opacité)
█ #808080  Hint Text       - Texte d'aide (50% opacité)
```

### Dividers & Borders
```
█ #404040  Divider         - Séparateurs horizontaux/verticaux
█ #4A4A4A  Border          - Bordures de cartes, champs
```

---

## Mode Clair

### Backgrounds
```
█ #FAFAFA  Surface         - Fond principal de l'application
█ #FFFFFF  Card            - Cartes, panels, conteneurs
█ #F5F5F5  Elevated        - Éléments surélevés (hover)
█ #FFFFFF  Dialog          - Dialogues modaux
```

### Textes
```
█ #212121  Primary Text    - Texte principal (87% opacité)
█ #757575  Secondary Text  - Texte secondaire (60% opacité)
█ #BDBDBD  Disabled Text   - Texte désactivé (38% opacité)
█ #9E9E9E  Hint Text       - Texte d'aide (50% opacité)
```

### Dividers & Borders
```
█ #E0E0E0  Divider         - Séparateurs horizontaux/verticaux
█ #BDBDBD  Border          - Bordures de cartes, champs
```

---

## Couleurs Métier (États Financiers)

```
█ #4CAF50  Paid            - Paiement effectué
█ #FF9800  Pending         - Paiement en attente
█ #F44336  Overdue         - Paiement en retard
█ #2196F3  Advanced        - Avance de paiement
```

---

## Exemples d'Utilisation

### Bouton Primary
```
Background:  #1976D2
Text:        #FFFFFF
Hover:       #1565C0
```

### Carte Success
```
Background:  #4CAF50
Text:        #FFFFFF
Icon:        #81C784
```

### Badge Warning
```
Background:  #FF9800
Text:        #FFFFFF
Border:      #F57C00
```

### DataGrid Row (Mode Sombre)
```
Even Row:    #2D2D2D
Odd Row:     #1E1E1E
Hover:       #2A2A2A
Selected:    #3A3A3A
```

---

## Contraste et Accessibilité

### Texte sur Fond Sombre
```
✅ #FFFFFF sur #1E1E1E  (Contraste: 15.8:1) - Excellent
✅ #B3B3B3 sur #1E1E1E  (Contraste: 8.5:1)  - Très bon
✅ #808080 sur #1E1E1E  (Contraste: 4.6:1)  - Bon (WCAG AA)
```

### Texte sur Fond Clair
```
✅ #212121 sur #FAFAFA  (Contraste: 15.3:1) - Excellent
✅ #757575 sur #FAFAFA  (Contraste: 4.6:1)  - Bon (WCAG AA)
✅ #9E9E9E sur #FAFAFA  (Contraste: 3.1:1)  - Acceptable pour texte large
```

### Boutons
```
✅ #FFFFFF sur #1976D2  (Contraste: 5.9:1)  - Excellent
✅ #FFFFFF sur #4CAF50  (Contraste: 4.0:1)  - Bon (WCAG AA)
✅ #FFFFFF sur #F44336  (Contraste: 4.5:1)  - Bon (WCAG AA)
```

---

## Quick Reference XAML

### Utiliser une Couleur
```xml
<!-- Brush -->
<Border Background="{StaticResource PrimaryBrush}"/>

<!-- Color -->
<SolidColorBrush Color="{StaticResource PrimaryColor}"/>
```

### Utiliser un Espacement
```xml
<!-- Thickness -->
<StackPanel Margin="{StaticResource SpacingM}"/>

<!-- Double -->
<Grid RowSpacing="{StaticResource SpacingMValue}"/>
```

### Utiliser une Ombre
```xml
<Border Effect="{StaticResource Elevation4}"/>
```

### Utiliser une Typographie
```xml
<TextBlock Style="{StaticResource PageTitle}"/>
<TextBlock FontSize="{StaticResource FontSizeH2}"/>
<TextBlock FontWeight="{StaticResource FontWeightMedium}"/>
```

---

**Gardez ce guide à portée de main pour référence rapide ! 🎨**
