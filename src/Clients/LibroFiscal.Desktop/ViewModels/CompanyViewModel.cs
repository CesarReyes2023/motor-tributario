using System;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LibroFiscal.Application.Companies.Commands.CreateCompany;
using MediatR;
using System.Windows;

namespace LibroFiscal.Desktop.ViewModels;

public partial class CompanyViewModel : ObservableObject
{
    private readonly IMediator _mediator;

    [ObservableProperty]
    private string _nombreComercial = string.Empty;

    [ObservableProperty]
    private string _razonSocial = string.Empty;

    [ObservableProperty]
    private string _nit = string.Empty;

    [ObservableProperty]
    private string _nrc = string.Empty;

    [ObservableProperty]
    private string _actividadEconomica = string.Empty;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _phone = string.Empty;

    [ObservableProperty]
    private string _department = string.Empty;

    [ObservableProperty]
    private string _municipality = string.Empty;

    [ObservableProperty]
    private string _addressLine = string.Empty;

    [ObservableProperty]
    private string _codigoActividad = string.Empty;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private string _companyId = string.Empty;

    public CompanyViewModel(IMediator mediator)
    {
        _mediator = mediator;
    }

    [RelayCommand]
    public async Task LoadCompanyAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        ErrorMessage = string.Empty;

        try
        {
            var result = await _mediator.Send(new LibroFiscal.Application.Companies.Queries.GetCompanyProfile.GetCompanyProfileQuery());

            if (result.IsSuccess && result.Value != null)
            {
                var profile = result.Value;
                CompanyId = profile.Id.Value.ToString();
                RazonSocial = profile.LegalName;
                NombreComercial = profile.TradeName;
                Nit = profile.Nit;
                Nrc = profile.Nrc;
                CodigoActividad = profile.EconomicActivityCode;
                ActividadEconomica = profile.EconomicActivityDescription;
                Phone = profile.Phone;
                Email = profile.Email;
                Department = profile.Department;
                Municipality = profile.Municipality;
                AddressLine = profile.AddressLine;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = "Error al cargar la empresa: " + ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SaveCompanyAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        ErrorMessage = string.Empty;

        try
        {
            var command = new LibroFiscal.Application.Companies.Commands.UpdateCompanyProfile.UpdateCompanyProfileCommand(
                CompanyId,
                RazonSocial,
                NombreComercial,
                Nit,
                Nrc,
                CodigoActividad,
                ActividadEconomica,
                Phone,
                Email,
                Department,
                Municipality,
                AddressLine
            );

            var result = await _mediator.Send(command);

            if (result.IsSuccess)
            {
                MessageBox.Show("Perfil de empresa guardado exitosamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show($"Error al guardar: {result.Error.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                ErrorMessage = result.Error.Message;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ocurrió un error inesperado: {ex.Message}", "Error Crítico", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsBusy = false;
        }
    }
}
