# 🎯 Guide Rapide - Nouvelles Fonctionnalités

## 🆕 Vue "Rapports Financiers"

### Accès
1. Lancez l'application
2. Connectez-vous
3. Cliquez sur **"Rapports Financiers"** dans le menu latéral (icône 📊)

### Fonctionnalités

#### 1. **KPI Cards** (En haut de la page)

```
┌─────────────────────────────────────────────────────────────┐
│ 🔴 Total Arriérés    🟢 À Jour    🟠 En Retard    🔵 En Avance│
│    5,200 TND          45 rés.       15 rés.        10 rés.  │
└─────────────────────────────────────────────────────────────┘
```

**Interprétation** :
- **Rouge** : Montant total des arriérés
- **Vert** : Nombre de résidents à jour (solde = 0)
- **Orange** : Nombre de résidents en retard (solde < 0)
- **Bleu** : Nombre de résidents en avance (solde > 0)

#### 2. **Boutons d'Export**

**Export Excel** :
- Cliquez sur **[EXCEL]**
- Choisissez l'emplacement du fichier
- Le fichier `.xlsx` s'ouvre automatiquement

**Export CSV** :
- Cliquez sur **[CSV]**
- Choisissez l'emplacement du fichier
- Compatible Excel (séparateur `;`, UTF-8 BOM)

**Actualiser** :
- Cliquez sur **[ACTUALISER]** pour recharger les données

#### 3. **Recherche**

```
┌─────────────────────────────────────────────────────────┐
│ [Rechercher un résident...]           [RECHERCHER]     │
└─────────────────────────────────────────────────────────┘
```

- Tapez le nom d'un résident
- Appuyez sur **Entrée** ou cliquez sur **RECHERCHER**

#### 4. **Tableau Détaillé**

| Code  | Propriétaire  | Total Dû | Total Payé | Solde    | Mois Impayés | Statut |
|-------|---------------|----------|------------|----------|--------------|--------|
| A-101 | Ahmed Ali     | 2,400    | 2,000      | **-400** | 2            | 🔴     |
| A-102 | Fatma Trabelsi| 2,400    | 2,400      | **0**    | 0            | 🟢     |
| A-103 | Mohamed Ben   | 2,400    | 2,600      | **+200** | 0            | 🔵     |

**Codes Couleur** :
- 🟢 **Vert** : À jour (solde = 0)
- 🟠 **Orange** : Léger retard (1-2 mois)
- 🔴 **Rouge** : Retard important (3+ mois)
- 🔵 **Bleu** : En avance (solde positif)

---

## 🎨 Mode Clair/Sombre

### Activation
1. Dans la barre supérieure, cherchez l'icône **🌙** ou le toggle
2. Cliquez pour basculer entre mode clair et sombre

### Avantages
- **Mode Sombre** : Recommandé pour usage prolongé, réduit la fatigue oculaire
- **Mode Clair** : Meilleur pour environnements très lumineux

---

## 🎨 Nouveau Design System

### Boutons

**Primary (Bleu)** :
```
[ENREGISTRER]  [ACTUALISER]  [VALIDER]
```
Utilisé pour les actions principales

**Secondary (Outlined)** :
```
[ANNULER]  [EXPORTER]  [FERMER]
```
Utilisé pour les actions secondaires

**Danger (Rouge)** :
```
[SUPPRIMER]  [RÉINITIALISER]
```
Utilisé pour les actions destructives

### Cartes (Cards)

Toutes les cartes ont maintenant :
- ✅ Coins arrondis (8px)
- ✅ Ombre subtile (Elevation 2)
- ✅ Effet hover (Elevation 4)
- ✅ Espacement cohérent (16px padding)

### DataGrid

Améliorations :
- ✅ Lignes aérées (52px de hauteur)
- ✅ Alternance de couleurs
- ✅ Hover visible
- ✅ En-têtes en gras
- ✅ Padding généreux (12px)

---

## 📤 Exports de Données

### Excel (.xlsx)

**Avantages** :
- Formatage automatique
- Colonnes ajustées
- Prêt pour analyse
- Compatible toutes versions Excel

**Utilisation** :
1. Cliquez sur **[EXCEL]**
2. Choisissez le nom et l'emplacement
3. Le fichier s'ouvre automatiquement
4. Vous pouvez :
   - Trier les données
   - Créer des graphiques
   - Faire des calculs

### CSV (.csv)

**Avantages** :
- Léger et universel
- Compatible Excel français (séparateur `;`)
- UTF-8 avec BOM
- Importable partout

**Utilisation** :
1. Cliquez sur **[CSV]**
2. Choisissez le nom et l'emplacement
3. Ouvrez avec Excel ou tout tableur
4. Les données sont prêtes à l'emploi

---

## 🎯 Cas d'Usage Pratiques

### 1. Identifier les Mauvais Payeurs

```
1. Allez dans "Rapports Financiers"
2. Regardez la carte "En Retard" (orange)
3. Dans le tableau, cherchez les lignes rouges
4. Cliquez sur "EXCEL" pour exporter
5. Triez par "Solde" (du plus négatif au moins négatif)
6. Contactez les résidents en priorité
```

### 2. Préparer un Rapport Mensuel

```
1. Allez dans "Rapports Financiers"
2. Notez les 4 KPI en haut
3. Cliquez sur "EXCEL"
4. Ouvrez le fichier
5. Créez un graphique (Insertion > Graphique)
6. Envoyez le rapport au conseil syndical
```

### 3. Vérifier un Résident Spécifique

```
1. Allez dans "Rapports Financiers"
2. Tapez le nom dans la recherche
3. Appuyez sur Entrée
4. Consultez :
   - Total Dû
   - Total Payé
   - Solde
   - Mois Impayés
```

### 4. Exporter pour Comptable

```
1. Allez dans "Rapports Financiers"
2. Cliquez sur "CSV"
3. Enregistrez le fichier
4. Envoyez par email au comptable
5. Il peut l'importer dans son logiciel
```

---

## 💡 Astuces

### Raccourcis Clavier
- **Entrée** dans la recherche = Rechercher
- **Échap** = Fermer les dialogues
- **Tab** = Naviguer entre les champs

### Meilleures Pratiques

**Exports** :
- Exportez régulièrement (fin de mois)
- Nommez les fichiers avec la date (ex: `Rapport_2026-01-15.xlsx`)
- Archivez les exports mensuels

**Recherche** :
- Utilisez des mots-clés courts
- Pas besoin de taper le nom complet
- La recherche est insensible à la casse

**KPI** :
- Consultez-les quotidiennement
- Surveillez l'évolution des arriérés
- Agissez rapidement si augmentation

---

## 🆘 Dépannage

### "Aucune donnée affichée"
→ Cliquez sur **[ACTUALISER]**

### "Export échoue"
→ Vérifiez que vous avez les droits d'écriture dans le dossier

### "Recherche ne fonctionne pas"
→ Vérifiez l'orthographe, essayez un mot-clé différent

### "Mode sombre ne change pas"
→ Redémarrez l'application

---

## 📞 Support

Pour toute question :
1. Consultez la **FAQ.md**
2. Lisez le **GUIDE_PREMIERE_UTILISATION.md**
3. Contactez le support technique

---

**Profitez des nouvelles fonctionnalités ! 🎉**

*Guide créé le : 15 janvier 2026*  
*Version : 1.0*
