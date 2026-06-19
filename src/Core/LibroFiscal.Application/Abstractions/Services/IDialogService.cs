namespace LibroFiscal.Application.Abstractions.Services;

/// <summary>
/// Abstraction for UI dialogs — ViewModels must NOT reference System.Windows directly.
/// Enables unit testing and separation of concerns (MVVM).
/// </summary>
public interface IDialogService
{
    void ShowInfo(string message, string title = "Información");
    void ShowWarning(string message, string title = "Aviso");
    void ShowError(string message, string title = "Error");
    bool ShowConfirmation(string message, string title = "Confirmar");
}
