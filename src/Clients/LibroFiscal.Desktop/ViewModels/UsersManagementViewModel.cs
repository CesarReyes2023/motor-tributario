using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LibroFiscal.Application.Users.Queries.GetUsers;
using LibroFiscal.Application.Users.Commands.CreateUser;
using LibroFiscal.Application.Users.Commands.AssignCompany;
using LibroFiscal.Domain.Users.Enums;
using LibroFiscal.Domain.Users.Ids;
using LibroFiscal.Application.Companies.Queries.GetActiveCompanies;
using MediatR;
using System.Linq;

namespace LibroFiscal.Desktop.ViewModels;

public partial class UsersManagementViewModel : ObservableObject
{
    private readonly IMediator _mediator;

    public UsersManagementViewModel(IMediator mediator)
    {
        _mediator = mediator;
        Users = new ObservableCollection<UserDto>();
        Companies = new ObservableCollection<CompanyDto>();
    }

    public ObservableCollection<UserDto> Users { get; }
    public ObservableCollection<CompanyDto> Companies { get; }

    [ObservableProperty]
    private UserDto? _selectedUser;

    [ObservableProperty]
    private string _newUsername = string.Empty;

    [ObservableProperty]
    private string _newPassword = string.Empty;

    [ObservableProperty]
    private bool _isLoading;

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        IsLoading = true;
        try
        {
            Users.Clear();
            var usersResult = await _mediator.Send(new GetUsersQuery());
            if (usersResult.IsSuccess)
            {
                foreach (var user in usersResult.Value)
                {
                    Users.Add(user);
                }
            }

            Companies.Clear();
            var companiesResult = await _mediator.Send(new GetActiveCompaniesQuery());
            if (companiesResult.IsSuccess)
            {
                foreach (var company in companiesResult.Value)
                {
                    Companies.Add(company);
                }
            }
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task CreateUserAsync()
    {
        if (string.IsNullOrWhiteSpace(NewUsername) || string.IsNullOrWhiteSpace(NewPassword))
        {
            MessageBox.Show("Debe ingresar un usuario y contraseña válidos.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        IsLoading = true;
        try
        {
            var command = new CreateUserCommand(NewUsername, NewPassword, UserRole.Operador); // Default to Operador for created users
            var result = await _mediator.Send(command);

            if (result.IsSuccess)
            {
                MessageBox.Show("Usuario creado con éxito.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                NewUsername = string.Empty;
                NewPassword = string.Empty;
                await LoadDataAsync();
            }
            else
            {
                MessageBox.Show($"Error al crear usuario: {result.Error.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        finally
        {
            IsLoading = false;
        }
    }

    // A real implementation would have a dialog or checklist per user. 
    // Here we'll add a simplified command for demonstration that takes a CompanyId.
    [RelayCommand]
    private async Task AssignCompanyAsync(LibroFiscal.Domain.Common.Ids.CompanyId companyId)
    {
        if (SelectedUser == null) return;

        var command = new AssignUserToCompanyCommand(SelectedUser.Id, companyId);
        var result = await _mediator.Send(command);

        if (result.IsSuccess)
        {
            MessageBox.Show("Empresa asignada correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            await LoadDataAsync(); // Refresh count
        }
        else
        {
            MessageBox.Show($"Error al asignar: {result.Error.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
