using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LibroFiscal.Application.Abstractions.Services;
using MediatR;

namespace LibroFiscal.Desktop.ViewModels;

public partial class CompanyViewModel : ObservableObject, IDisposable
{
    private readonly IMediator _mediator;
    private readonly IEmpresaActivaService _empresaActivaService;
    private readonly IDialogService _dialogService;
    private readonly IErrorLogger _errorLogger;

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

    [ObservableProperty]
    private string _apiPassword = string.Empty;

    [ObservableProperty]
    private string _logoPath = string.Empty;

    public CompanyViewModel(
        IMediator mediator,
        IEmpresaActivaService empresaActivaService,
        IDialogService dialogService,
        IErrorLogger errorLogger)
    {
        _mediator = mediator;
        _empresaActivaService = empresaActivaService;
        _dialogService = dialogService;
        _errorLogger = errorLogger;
        
        _empresaActivaService.EmpresaCambiadaEvent += OnEmpresaCambiada;
    }

    private void OnEmpresaCambiada(object? sender, Guid e) => _ = LoadCompanyAsync();

    public void Dispose()
    {
        _empresaActivaService.EmpresaCambiadaEvent -= OnEmpresaCambiada;
        GC.SuppressFinalize(this);
    }

    [RelayCommand]
    public async Task LoadCompanyAsync()
    {
        if (IsBusy) return;
        
        var companyId = _empresaActivaService.EmpresaActualId;
        if (companyId == null) return;

        IsBusy = true;
        ErrorMessage = string.Empty;

        try
        {
            var result = await _mediator.Send(new LibroFiscal.Application.Companies.Queries.GetCompanyProfile.GetCompanyProfileQuery(companyId.Value));

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
                ApiPassword = profile.ApiPassword;
                LogoPath = profile.LogoPath ?? string.Empty;
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
        
        var activeCompanyId = _empresaActivaService.EmpresaActualId;
        if (activeCompanyId == null)
        {
            _dialogService.ShowError("No hay empresa activa.");
            return;
        }
        
        IsBusy = true;
        ErrorMessage = string.Empty;

        try
        {
            var command = new LibroFiscal.Application.Companies.Commands.UpdateCompanyProfile.UpdateCompanyProfileCommand(
                activeCompanyId.Value.ToString(),
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
                AddressLine,
                ApiPassword,
                string.IsNullOrEmpty(LogoPath) ? null : LogoPath
            );

            var result = await _mediator.Send(command);

            if (result.IsSuccess)
            {
                _dialogService.ShowInfo("Perfil de empresa guardado exitosamente.", "Éxito");
                _empresaActivaService.CambiarEmpresa(activeCompanyId.Value);
            }
            else
            {
                _errorLogger.LogError("company_save", result.Error.Message);
                _dialogService.ShowError($"Error al guardar: {result.Error.Message}");
                ErrorMessage = result.Error.Message;
            }
        }
        catch (Exception ex)
        {
            _errorLogger.LogError("company_save", ex);
            _dialogService.ShowError("Ocurrió un error inesperado. Detalle guardado en logs.", "Error Crítico");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void SelectLogo()
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "Imágenes (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg|Todos los archivos (*.*)|*.*",
            Title = "Seleccionar Logo de la Empresa"
        };

        if (dialog.ShowDialog() == true)
        {
            try
            {
                // Asegurar que exista el directorio de logos en AppData
                var appDataPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData);
                var libroFiscalPath = System.IO.Path.Combine(appDataPath, "LibroFiscal", "Logos");
                
                if (!System.IO.Directory.Exists(libroFiscalPath))
                {
                    System.IO.Directory.CreateDirectory(libroFiscalPath);
                }

                // Generar un nombre único para el archivo
                var extension = System.IO.Path.GetExtension(dialog.FileName);
                var newFileName = $"{System.Guid.NewGuid()}{extension}";
                var destinationPath = System.IO.Path.Combine(libroFiscalPath, newFileName);

                // Copiar archivo
                System.IO.File.Copy(dialog.FileName, destinationPath, true);

                // Actualizar propiedad
                LogoPath = destinationPath;
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Error al copiar la imagen: {ex.Message}");
            }
        }
    }
}
