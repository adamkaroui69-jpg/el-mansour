# Guide de Gestion des Logs et Erreurs

Ce document explique la stratégie de logging et de gestion des erreurs mise en place dans l'application **El Mansour Syndic Manager**.

## 1. Stratégie de Logging (Serilog)

Nous utilisons **Serilog** pour sa robustesse, sa performance et sa facilité de configuration.

### Emplacement des Logs
Les fichiers de logs sont stockés dans le dossier portable de l'application :
- **Chemin** : `data/logs/`
- **Format** : `log-YYYYMMDD.txt` (Rotation journalière)
- **Rétention** : 30 jours (les vieux logs sont supprimés automatiquement)

### Niveaux de Logs
| Niveau | Description | Exemple |
| :--- | :--- | :--- |
| **Information** | Événements normaux du cycle de vie (Démarrage, Arrêt, Sauvegarde réussie). | `Application Démarrée. Version: 2.0.0` |
| **Warning** | Situations anormales mais non bloquantes (Fichier manquant, Configuration par défaut utilisée). | `Fichier non trouvé, création d'un nouveau...` |
| **Error** | Erreurs gérées (Échec de sauvegarde, Erreur de connexion BDD). L'utilisateur peut souvent continuer. | `Erreur lors de la sauvegarde automatique` |
| **Fatal** | Erreurs critiques (Crash, Exception non gérée). L'application doit souvent s'arrêter. | `Une erreur critique est survenue: Erreur UI Non Gérée` |

## 2. Gestion Globale des Erreurs

L'application intercepte toutes les erreurs non gérées pour éviter un crash silencieux et informer l'utilisateur.

### Points d'Interception
1.  **UI Thread (`DispatcherUnhandledException`)** : Erreurs survenant dans l'interface graphique (boutons, affichage).
2.  **AppDomain (`UnhandledException`)** : Erreurs critiques hors UI (Threads background).
3.  **TaskScheduler (`UnobservedTaskException`)** : Erreurs dans des tâches asynchrones (`Task.Run`) non attendues.

### Comportement
1.  L'erreur est **loguée** avec la stack trace complète dans `data/logs/`.
2.  Une **fenêtre d'erreur conviviale** s'affiche à l'utilisateur, expliquant qu'un rapport a été généré.
3.  Si possible, l'application tente de ne pas se fermer (pour les erreurs UI simples), sinon elle s'arrête proprement.

## 3. Exemple d'Utilisation dans le Code

Pour logger dans vos services ou ViewModels, demandez simplement `ILogger<T>` dans le constructeur.

```csharp
using Microsoft.Extensions.Logging;

public class MaClasse
{
    private readonly ILogger<MaClasse> _logger;

    public MaClasse(ILogger<MaClasse> logger)
    {
        _logger = logger;
    }

    public void FaireQuelqueChose()
    {
        _logger.LogInformation("Début du traitement...");

        try
        {
            // Code risqué
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Une erreur est survenue pendant le traitement");
        }
    }
}
```

## 4. Dépannage
En cas de bug rapporté par un utilisateur :
1.  Demandez-lui d'aller dans le dossier `data/logs`.
2.  Récupérez le fichier du jour (ex: `log-20260115.txt`).
3.  Cherchez les lignes contenant `[ERR]` ou `[FAT]`.
