using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LibroFiscal.Application.Abstractions.Services;
using LibroFiscal.Application.Users.Queries.AuthenticateUser;
using MediatR;
using System.Windows;

namespace LibroFiscal.Desktop.ViewModels;

public sealed partial class LoginViewModel : ObservableObject
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;

    [ObservableProperty]
    private string _username = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _isBusy;

    public event EventHandler? LoginSuccessful;

    public LoginViewModel(IMediator mediator, ICurrentUserService currentUserService)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
    }

    [RelayCommand]
    private async Task LoginAsync()
    {
        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Por favor ingrese usuario y contraseña.";
            return;
        }

        IsBusy = true;
        ErrorMessage = string.Empty;

        try
        {
            var query = new AuthenticateUserQuery(Username, Password);
            var result = await _mediator.Send(query);

            if (result.IsSuccess)
            {
                _currentUserService.SetUser(result.Value.UserId, result.Value.Username, result.Value.ProfilePicturePath);
                LoginSuccessful?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                ErrorMessage = result.Error.Message;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error: {ex.Message} \n\n {ex.InnerException?.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
