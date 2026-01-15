using System;
using System.Threading.Tasks;

namespace ElMansourSyndicManager.Core.Domain.Interfaces.Services
{
    public interface IDialogService
    {
        /// <summary>
        /// Affiche un message d'information ou d'erreur à l'utilisateur (Toast/Snackbar).
        /// </summary>
        void ShowMessage(string message, string? actionLabel = null, Action? action = null);

        /// <summary>
        /// Affiche une boîte de confirmation modale.
        /// </summary>
        /// <returns>True si confirmé, False sinon.</returns>
        Task<bool> ShowConfirmationAsync(string message, string title = "Confirmation", string confirmText = "Confirmer", string cancelText = "Annuler");

        /// <summary>
        /// Affiche une alerte modale.
        /// </summary>
        Task ShowAlertAsync(string message, string title = "Attention");
    }
}
