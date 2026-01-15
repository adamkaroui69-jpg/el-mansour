# 🎨 Charte Graphique - El Mansour Syndic Manager

**Version** : 1.0  
**Date** : 15 janvier 2026  
**Type d'application** : Gestion de syndic (Desktop WPF)

---

## 1. PHILOSOPHIE DESIGN

### Principes Directeurs
- **Professionnalisme** : Couleurs sobres, mise en page structurée
- **Lisibilité** : Contraste élevé, typographie claire
- **Confort** : Espacement généreux, animations subtiles
- **Confiance** : Design cohérent, feedback clair

### Ton Visuel
- Sérieux mais accessible
- Moderne sans être tape-à-l'œil
- Rassurant pour une application financière

---

## 2. PALETTE DE COULEURS

### 🎨 Couleurs Principales

#### Primary (Bleu Professionnel)
```
Primary 500:     #1976D2  (Bleu Corporate)
Primary 400:     #42A5F5  (Bleu Clair)
Primary 600:     #1565C0  (Bleu Foncé)
Primary 700:     #0D47A1  (Bleu Très Foncé)
```
**Usage** : Boutons principaux, liens, éléments actifs

#### Secondary (Émeraude Élégant)
```
Secondary 500:   #00897B  (Émeraude)
Secondary 400:   #26A69A  (Émeraude Clair)
Secondary 600:   #00796B  (Émeraude Foncé)
```
**Usage** : Accents, badges, états de succès

#### Accent (Orange Discret)
```
Accent 500:      #FF6F00  (Orange)
Accent 400:      #FF8F00  (Orange Clair)
Accent 600:      #E65100  (Orange Foncé)
```
**Usage** : Alertes importantes, call-to-action secondaires

---

### 🌙 Mode Sombre (Recommandé pour usage prolongé)

#### Backgrounds
```
Surface:         #1E1E1E  (Fond principal)
Card:            #2D2D2D  (Cartes, panels)
Elevated:        #383838  (Éléments surélevés)
Dialog:          #424242  (Dialogues)
```

#### Textes
```
Primary Text:    #FFFFFF  (Texte principal - 100% opacité)
Secondary Text:  #B3B3B3  (Texte secondaire - 70% opacité)
Disabled Text:   #666666  (Texte désactivé - 40% opacité)
Hint Text:       #808080  (Texte d'aide - 50% opacité)
```

#### Dividers & Borders
```
Divider:         #404040  (Séparateurs)
Border:          #4A4A4A  (Bordures)
```

---

### ☀️ Mode Clair (Pour environnements lumineux)

#### Backgrounds
```
Surface:         #FAFAFA  (Fond principal)
Card:            #FFFFFF  (Cartes, panels)
Elevated:        #F5F5F5  (Éléments surélevés)
Dialog:          #FFFFFF  (Dialogues)
```

#### Textes
```
Primary Text:    #212121  (Texte principal - 87% opacité)
Secondary Text:  #757575  (Texte secondaire - 60% opacité)
Disabled Text:   #BDBDBD  (Texte désactivé - 38% opacité)
Hint Text:       #9E9E9E  (Texte d'aide - 50% opacité)
```

#### Dividers & Borders
```
Divider:         #E0E0E0  (Séparateurs)
Border:          #BDBDBD  (Bordures)
```

---

### 🎯 Couleurs Sémantiques

#### Succès (Vert)
```
Success:         #4CAF50  (Vert)
Success Light:   #81C784  (Vert Clair)
Success Dark:    #388E3C  (Vert Foncé)
```
**Usage** : Paiements validés, opérations réussies

#### Erreur (Rouge)
```
Error:           #F44336  (Rouge)
Error Light:     #E57373  (Rouge Clair)
Error Dark:      #D32F2F  (Rouge Foncé)
```
**Usage** : Erreurs, suppressions, alertes critiques

#### Avertissement (Orange)
```
Warning:         #FF9800  (Orange)
Warning Light:   #FFB74D  (Orange Clair)
Warning Dark:    #F57C00  (Orange Foncé)
```
**Usage** : Retards de paiement (1-2 mois), actions à confirmer

#### Information (Bleu Clair)
```
Info:            #2196F3  (Bleu Info)
Info Light:      #64B5F6  (Bleu Info Clair)
Info Dark:       #1976D2  (Bleu Info Foncé)
```
**Usage** : Messages informatifs, tooltips

---

### 💰 Couleurs Métier (Spécifiques Syndic)

#### États Financiers
```
Paid:            #4CAF50  (Vert - Payé)
Pending:         #FF9800  (Orange - En attente)
Overdue:         #F44336  (Rouge - En retard)
Advanced:        #2196F3  (Bleu - Avance de paiement)
```

#### Priorités
```
High Priority:   #F44336  (Rouge)
Medium Priority: #FF9800  (Orange)
Low Priority:    #4CAF50  (Vert)
```

---

## 3. TYPOGRAPHIE

### Police Principale
**Roboto** (incluse dans Material Design)
- Excellente lisibilité
- Professionnelle
- Optimisée pour les écrans

### Hiérarchie Typographique

#### Titres de Page (H1)
```
Font: Roboto
Weight: Light (300)
Size: 32px
Color: Primary Text
Letter Spacing: -0.5px
```

#### Titres de Section (H2)
```
Font: Roboto
Weight: Regular (400)
Size: 24px
Color: Primary Text
Letter Spacing: 0px
```

#### Sous-titres (H3)
```
Font: Roboto
Weight: Medium (500)
Size: 20px
Color: Primary Text
Letter Spacing: 0.15px
```

#### Titres de Cartes (H4)
```
Font: Roboto
Weight: Medium (500)
Size: 16px
Color: Primary Text
Letter Spacing: 0.15px
```

#### Corps de Texte
```
Font: Roboto
Weight: Regular (400)
Size: 14px
Color: Primary Text
Line Height: 20px
Letter Spacing: 0.25px
```

#### Texte Secondaire
```
Font: Roboto
Weight: Regular (400)
Size: 12px
Color: Secondary Text
Line Height: 16px
Letter Spacing: 0.4px
```

#### Boutons
```
Font: Roboto
Weight: Medium (500)
Size: 14px
Color: White (sur fond coloré)
Letter Spacing: 1.25px (UPPERCASE)
```

#### Labels de Champs
```
Font: Roboto
Weight: Regular (400)
Size: 12px
Color: Secondary Text
Letter Spacing: 0.4px
```

---

## 4. ESPACEMENT & GRILLE

### Système d'Espacement (Multiple de 8)
```
XS:  4px   (Espacement minimal)
S:   8px   (Espacement petit)
M:   16px  (Espacement standard)
L:   24px  (Espacement large)
XL:  32px  (Espacement très large)
XXL: 48px  (Espacement section)
```

### Marges de Contenu
```
Page Margin:     24px
Card Padding:    16px
Section Spacing: 32px
```

### Grille
```
Colonnes:        12 colonnes
Gutter:          16px
Max Width:       1440px
```

---

## 5. COMPOSANTS UI

### Boutons

#### Primary Button
```
Background:      Primary 500
Text:            White
Height:          36px
Padding:         16px horizontal
Border Radius:   4px
Shadow:          Elevation 2
Hover:           Primary 600 + Elevation 4
Pressed:         Primary 700 + Elevation 8
Disabled:        #BDBDBD (gris)
```

#### Secondary Button (Outlined)
```
Background:      Transparent
Border:          1px Primary 500
Text:            Primary 500
Height:          36px
Padding:         16px horizontal
Border Radius:   4px
Hover:           Primary 50 background
Pressed:         Primary 100 background
```

#### Danger Button
```
Background:      Error
Text:            White
(Mêmes dimensions que Primary)
```

### Cartes (Cards)

#### Standard Card
```
Background:      Card
Border Radius:   8px
Shadow:          Elevation 2
Padding:         16px
Margin Bottom:   16px
```

#### Elevated Card (Hover)
```
Shadow:          Elevation 4
Transition:      0.3s ease
```

### DataGrid

#### En-têtes
```
Background:      Elevated
Text:            Primary Text
Font Weight:     Medium (500)
Height:          48px
Padding:         16px
Border Bottom:   1px Divider
```

#### Lignes
```
Height:          52px (aéré)
Padding:         12px 16px
Border Bottom:   1px Divider
Hover:           Primary 50 (mode clair) / #2A2A2A (mode sombre)
Selected:        Primary 100 (mode clair) / #3A3A3A (mode sombre)
```

#### Alternance de Lignes
```
Even Row:        Card
Odd Row:         Surface
```

### Champs de Saisie

#### Text Input
```
Height:          48px
Padding:         12px 16px
Border:          1px Border
Border Radius:   4px
Focus Border:    2px Primary 500
Label:           Floating label (Material Design)
Helper Text:     12px Secondary Text
Error State:     Error color + message
```

### Dialogs

#### Modal Dialog
```
Background:      Dialog
Border Radius:   8px
Shadow:          Elevation 24
Max Width:       600px
Padding:         24px
Title:           H3
Content:         Body text
Actions:         Right-aligned buttons
```

### Snackbar / Toast

#### Notification
```
Background:      #323232 (mode sombre) / #323232 (mode clair aussi)
Text:            White
Height:          48px
Border Radius:   4px
Shadow:          Elevation 6
Position:        Bottom center
Duration:        4000ms (4 secondes)
Action Button:   Accent color
```

---

## 6. ICONOGRAPHIE

### Icônes Material Design
```
Library:         Material Design Icons
Size Standard:   24px
Size Small:      18px
Size Large:      36px
Color:           Secondary Text (par défaut)
Active Color:    Primary
```

### Icônes Métier
```
Paiements:       💰 AttachMoney
Dépenses:        📤 TrendingDown
Résidents:       👥 People
Maintenance:     🔧 Build
Documents:       📄 Description
Rapports:        📊 Assessment
Paramètres:      ⚙️ Settings
Tableau de Bord: 📈 Dashboard
```

---

## 7. ANIMATIONS & TRANSITIONS

### Principes
- **Subtiles** : Pas de distraction
- **Rapides** : 200-300ms
- **Naturelles** : Easing curves

### Transitions Standards
```
Hover:           200ms ease-out
Click:           100ms ease-in
Page Load:       300ms ease-in-out
Dialog Open:     250ms ease-out
Snackbar:        300ms ease-in-out
```

### Easing Curves
```
Standard:        cubic-bezier(0.4, 0.0, 0.2, 1)
Decelerate:      cubic-bezier(0.0, 0.0, 0.2, 1)
Accelerate:      cubic-bezier(0.4, 0.0, 1, 1)
```

---

## 8. ACCESSIBILITÉ

### Contraste Minimum
```
Texte Normal:    4.5:1 (WCAG AA)
Texte Large:     3:1 (WCAG AA)
Éléments UI:     3:1
```

### Focus Visible
```
Outline:         2px Primary 500
Offset:          2px
Border Radius:   4px
```

### Tailles Tactiles
```
Minimum:         44x44px (boutons, liens)
Recommandé:      48x48px
```

---

## 9. RESPONSIVE DESIGN

### Breakpoints
```
Mobile:          < 600px
Tablet:          600px - 960px
Desktop:         > 960px
Large Desktop:   > 1440px
```

### Adaptations
- **Mobile** : Navigation drawer, cartes empilées
- **Tablet** : Grille 2 colonnes
- **Desktop** : Grille 3-4 colonnes, sidebar fixe

---

## 10. ÉTATS VISUELS

### Hover
```
Opacity:         0.08 overlay
Cursor:          pointer
Transition:      200ms
```

### Active / Pressed
```
Opacity:         0.12 overlay
Shadow:          Elevation +2
```

### Disabled
```
Opacity:         0.38
Cursor:          not-allowed
```

### Loading
```
Spinner:         Primary color
Overlay:         0.5 opacity black
```

---

## 11. OMBRES (ELEVATION)

```
Elevation 0:     none
Elevation 1:     0 1px 3px rgba(0,0,0,0.12)
Elevation 2:     0 2px 4px rgba(0,0,0,0.14)
Elevation 4:     0 4px 8px rgba(0,0,0,0.16)
Elevation 6:     0 6px 12px rgba(0,0,0,0.18)
Elevation 8:     0 8px 16px rgba(0,0,0,0.20)
Elevation 12:    0 12px 24px rgba(0,0,0,0.22)
Elevation 16:    0 16px 32px rgba(0,0,0,0.24)
Elevation 24:    0 24px 48px rgba(0,0,0,0.28)
```

---

## 12. EXEMPLES D'APPLICATION

### Page Login
- Fond : Surface
- Carte centrale : Card + Elevation 4
- Logo : 64px
- Champs : Outlined style
- Bouton : Primary, pleine largeur

### Dashboard
- Cartes KPI : Card + Elevation 2
- Graphiques : Couleurs Primary/Secondary
- Grille : 4 colonnes (desktop)

### Liste de Paiements
- DataGrid : Lignes alternées
- États : Couleurs sémantiques (Paid/Pending/Overdue)
- Actions : Icônes + tooltips

---

## 13. CHECKLIST DE CONFORMITÉ

✅ Contraste texte/fond ≥ 4.5:1  
✅ Boutons ≥ 44x44px  
✅ Espacement cohérent (multiples de 8)  
✅ Animations ≤ 300ms  
✅ Focus visible sur tous les éléments interactifs  
✅ Couleurs sémantiques cohérentes  
✅ Typographie hiérarchisée  
✅ Ombres progressives  

---

**Cette charte garantit une expérience utilisateur professionnelle, confortable et accessible pour une utilisation quotidienne prolongée.**
