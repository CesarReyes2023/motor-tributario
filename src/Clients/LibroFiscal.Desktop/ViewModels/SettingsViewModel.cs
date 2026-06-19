using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LibroFiscal.Application.Abstractions.Services;
using LibroFiscal.Application.Users.Commands.ChangePassword;
using LibroFiscal.Application.Users.Commands.UpdateProfilePicture;
using MediatR;
using Microsoft.Win32;
using System.IO;

namespace LibroFiscal.Desktop.ViewModels;

public sealed partial class SettingsViewModel : ObservableObject
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;

    [ObservableProperty]
    private string _currentPassword = string.Empty;

    [ObservableProperty]
    private string _newPassword = string.Empty;

    [ObservableProperty]
    private string _confirmNewPassword = string.Empty;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private string _successMessage = string.Empty;

    [ObservableProperty]
    private string _profilePicturePath = string.Empty;

    public string Username => _currentUserService.Username ?? "Usuario";

    public SettingsViewModel(IMediator mediator, ICurrentUserService currentUserService)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
        _profilePicturePath = _currentUserService.ProfilePicturePath ?? string.Empty;
        
        _currentUserService.ProfilePictureChanged += (s, e) => 
        {
            ProfilePicturePath = _currentUserService.ProfilePicturePath ?? string.Empty;
        };
    }

    [RelayCommand]
    private async Task ChangePasswordAsync()
    {
        ErrorMessage = string.Empty;
        SuccessMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(CurrentPassword) || 
            string.IsNullOrWhiteSpace(NewPassword) || 
            string.IsNullOrWhiteSpace(ConfirmNewPassword))
        {
            ErrorMessage = "Todos los campos son obligatorios.";
            return;
        }

        if (NewPassword != ConfirmNewPassword)
        {
            ErrorMessage = "La nueva contraseña y la confirmación no coinciden.";
            return;
        }

        if (_currentUserService.UserId is null)
        {
            ErrorMessage = "Error de sesión: Usuario no identificado.";
            return;
        }

        var command = new ChangePasswordCommand(
            _currentUserService.UserId.Value,
            CurrentPassword,
            NewPassword);

        var result = await _mediator.Send(command);

        if (result.IsSuccess)
        {
            SuccessMessage = "Contraseña actualizada exitosamente.";
            CurrentPassword = string.Empty;
            NewPassword = string.Empty;
            ConfirmNewPassword = string.Empty;
        }
        else
        {
            ErrorMessage = result.Error.Message;
        }
    }

    [RelayCommand]
    private async Task ChangeProfilePictureAsync()
    {
        ErrorMessage = string.Empty;
        SuccessMessage = string.Empty;

        var openFileDialog = new OpenFileDialog
        {
            Title = "Seleccionar Foto de Perfil",
            Filter = "Imágenes (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg"
        };

        if (openFileDialog.ShowDialog() == true)
        {
            try
            {
                var appDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "LibroFiscal", "Avatars");
                Directory.CreateDirectory(appDataFolder);

                var ext = Path.GetExtension(openFileDialog.FileName);
                var newFileName = $"{_currentUserService.UserId}{ext}";
                var destinationPath = Path.Combine(appDataFolder, newFileName);

                File.Copy(openFileDialog.FileName, destinationPath, overwrite: true);

                var command = new UpdateProfilePictureCommand(_currentUserService.UserId!.Value, destinationPath);
                var result = await _mediator.Send(command);

                if (result.IsSuccess)
                {
                    _currentUserService.UpdateProfilePicturePath(destinationPath);
                    SuccessMessage = "Foto de perfil actualizada correctamente.";
                }
                else
                {
                    ErrorMessage = "No se pudo actualizar la foto de perfil en la base de datos.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error al copiar la imagen: {ex.Message}";
            }
        }
    }

    [RelayCommand]
    private void Logout()
    {
        ErrorMessage = string.Empty;
        var app = System.Windows.Application.Current as App;
        app?.Logout();
    }
}
