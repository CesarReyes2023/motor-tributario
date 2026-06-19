using System.Windows;
using LibroFiscal.Application.Abstractions.Services;

namespace LibroFiscal.Desktop.Services;

/// <summary>
/// WPF implementation of IDialogService using MessageBox.
/// This is the ONLY place in the entire app that references MessageBox.
/// </summary>
public sealed class DialogService : IDialogService
{
    public void ShowInfo(string message, string title = "Información")
        => MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);

    public void ShowWarning(string message, string title = "Aviso")
        => MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Warning);

    public void ShowError(string message, string title = "Error")
        => MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);

    public bool ShowConfirmation(string message, string title = "Confirmar")
        => MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
}
