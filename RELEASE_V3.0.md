# 🌟 RELEASE V3.0.0 - CLOUD & STABILITÉ

**Version :** 3.0.0 (Majeure)
**Date :** 22 Janvier 2026

Cette version marque une refonte majeure de l'infrastructure de données.

## 🚀 Fonctionnalités Clés

### ☁️ Synchronisation Cloud Native (Supabase)
- L'application est connectée par défaut au Cloud.
- Données centralisées et sécurisées sur Internet.
- Multi-utilisateurs en temps réel.
- **Plus besoin de configuration manuelle** (installateur pré-configuré).

### 💾 Nouveau Système de Sauvegarde
- **Format ZIP Standard** : Les sauvegardes ne sont plus chiffrées propriétairement. Vous pouvez désormais ouvrir les fichiers ZIP de sauvegarde avec Windows, WinRAR, etc.
- Plus fiable et moins sujet à corruption.

## ⚠️ Notes Importantes
- Comme nous avons changé de système de base de données (SQLite -> PostgreSQL), les anciennes données locales ne sont pas visibles. Vous repartez sur une base saine et partagée.
- Les anciens backups (V2.1.0 et antérieurs) ne sont pas compatibles avec le nouvel outil de restauration V3.0 (car ils étaient chiffrés).
