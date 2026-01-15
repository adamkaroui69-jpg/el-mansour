using ElMansourSyndicManager.Core.Domain.Interfaces.Services;
using MaterialDesignThemes.Wpf;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ElMansourSyndicManager.Services
{
    public class DialogService : IDialogService
    {
        private readonly ISnackbarMessageQueue _snackbarMessageQueue;
        private const string DialogIdentifier = "RootDialog";

        public DialogService(ISnackbarMessageQueue snackbarMessageQueue)
        {
            _snackbarMessageQueue = snackbarMessageQueue;
        }

        public void ShowMessage(string message, string? actionLabel = null, Action? action = null)
        {
            if (action != null && !string.IsNullOrEmpty(actionLabel))
            {
                _snackbarMessageQueue.Enqueue(message, actionLabel, action);
            }
            else
            {
                _snackbarMessageQueue.Enqueue(message);
            }
        }

        public async Task<bool> ShowConfirmationAsync(string message, string title = "Confirmation", string confirmText = "Confirmer", string cancelText = "Annuler")
        {
            var content = new StackPanel { Margin = new Thickness(16) };
            
            content.Children.Add(new TextBlock 
            { 
                Text = title, 
                Style = (Style)Application.Current.FindResource("MaterialDesignHeadline6TextBlock"),
                Margin = new Thickness(0, 0, 0, 16)
            });

            content.Children.Add(new TextBlock 
            { 
                Text = message, 
                TextWrapping = TextWrapping.Wrap,
                Style = (Style)Application.Current.FindResource("MaterialDesignBody1TextBlock"),
                Margin = new Thickness(0, 0, 0, 24)
            });

            var buttonsPanel = new StackPanel 
            { 
                Orientation = Orientation.Horizontal, 
                HorizontalAlignment = HorizontalAlignment.Right 
            };

            var cancelButton = new Button 
            { 
                Content = cancelText, 
                Command = DialogHost.CloseDialogCommand, 
                CommandParameter = false,
                Style = (Style)Application.Current.FindResource("MaterialDesignFlatButton"),
                Margin = new Thickness(0, 0, 8, 0)
            };

            var confirmButton = new Button 
            { 
                Content = confirmText, 
                Command = DialogHost.CloseDialogCommand, 
                CommandParameter = true,
                Style = (Style)Application.Current.FindResource("MaterialDesignRaisedButton")
            };

            buttonsPanel.Children.Add(cancelButton);
            buttonsPanel.Children.Add(confirmButton);
            content.Children.Add(buttonsPanel);

            var result = await DialogHost.Show(content, DialogIdentifier);

            return result is bool b && b;
        }

        public async Task ShowAlertAsync(string message, string title = "Attention")
        {
            var content = new StackPanel { Margin = new Thickness(16) };
            
            content.Children.Add(new TextBlock 
            { 
                Text = title, 
                Style = (Style)Application.Current.FindResource("MaterialDesignHeadline6TextBlock"),
                Margin = new Thickness(0, 0, 0, 16)
            });

            content.Children.Add(new TextBlock 
            { 
                Text = message, 
                TextWrapping = TextWrapping.Wrap,
                Style = (Style)Application.Current.FindResource("MaterialDesignBody1TextBlock"),
                Margin = new Thickness(0, 0, 0, 24)
            });

            var buttonsPanel = new StackPanel 
            { 
                Orientation = Orientation.Horizontal, 
                HorizontalAlignment = HorizontalAlignment.Right 
            };

            var okButton = new Button 
            { 
                Content = "OK", 
                Command = DialogHost.CloseDialogCommand, 
                CommandParameter = true,
                Style = (Style)Application.Current.FindResource("MaterialDesignRaisedButton")
            };

            buttonsPanel.Children.Add(okButton);
            content.Children.Add(buttonsPanel);

            await DialogHost.Show(content, DialogIdentifier);
        }
    }
}
