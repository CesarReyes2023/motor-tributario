using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LibroFiscal.Application.Abstractions.Services;
using LibroFiscal.Application.Companies.Queries.GetActiveCompanies;
using MediatR;

namespace LibroFiscal.Desktop.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly IMediator _mediator;
    private readonly IEmpresaActivaService _empresaActivaService;
    private readonly IErrorLogger _errorLogger;
    private readonly DashboardViewModel _dashboardViewModel;
    private readonly CompanyViewModel _companyViewModel;
    private readonly DteListViewModel _dteListViewModel;
    private readonly CreateDteViewModel _createDteViewModel;
    private readonly OcrScannerViewModel _ocrScannerViewModel;
    private readonly PurchasesViewModel _purchasesViewModel;
    private readonly VatBooksViewModel _vatBooksViewModel;
    private readonly IngestionViewModel _ingestionViewModel;
    private readonly SettingsViewModel _settingsViewModel;
    private readonly SalesViewModel _salesViewModel;
    private readonly UsersManagementViewModel _usersManagementViewModel;
    private readonly ICurrentUserService _currentUserService;

    [ObservableProperty]
    private ObservableObject _currentViewModel = null!;

    public string CurrentUserName => _currentUserService.Username ?? "Usuario";
    
    // Solo SuperAdmin (Admin) puede ver la gestión de usuarios
    public bool IsSuperAdmin => _currentUserService.Role == "Admin";
    
    [ObservableProperty]
    private string _profilePicturePath = string.Empty;

    [ObservableProperty]
    private ObservableCollection<CompanyDto> _companies = new();

    private CompanyDto? _selectedCompany;
    public CompanyDto? SelectedCompany
    {
        get => _selectedCompany;
        set
        {
            if (SetProperty(ref _selectedCompany, value) && value != null)
            {
                _empresaActivaService.CambiarEmpresa(value.Id);
            }
        }
    }

    public MainViewModel(
        IMediator mediator,
        IEmpresaActivaService empresaActivaService,
        IErrorLogger errorLogger,
        DashboardViewModel dashboardViewModel, 
        CompanyViewModel companyViewModel,
        DteListViewModel dteListViewModel,
        CreateDteViewModel createDteViewModel,
        OcrScannerViewModel ocrScannerViewModel,
        PurchasesViewModel purchasesViewModel,
        VatBooksViewModel vatBooksViewModel,
        IngestionViewModel ingestionViewModel,
        SettingsViewModel settingsViewModel,
        SalesViewModel salesViewModel,
        UsersManagementViewModel usersManagementViewModel,
        ICurrentUserService currentUserService)
    {
        _mediator = mediator;
        _empresaActivaService = empresaActivaService;
        _errorLogger = errorLogger;
        _dashboardViewModel = dashboardViewModel;
        _companyViewModel = companyViewModel;
        _dteListViewModel = dteListViewModel;
        _createDteViewModel = createDteViewModel;
        _ocrScannerViewModel = ocrScannerViewModel;
        _purchasesViewModel = purchasesViewModel;
        _vatBooksViewModel = vatBooksViewModel;
        _ingestionViewModel = ingestionViewModel;
        _settingsViewModel = settingsViewModel;
        _salesViewModel = salesViewModel;
        _usersManagementViewModel = usersManagementViewModel;
        _currentUserService = currentUserService;

        _profilePicturePath = _currentUserService.ProfilePicturePath ?? string.Empty;
        _currentUserService.ProfilePictureChanged += (s, e) =>
        {
            ProfilePicturePath = _currentUserService.ProfilePicturePath ?? string.Empty;
        };
        
        // Setup cross-VM navigation
        _dteListViewModel.NavigateToCreateDteAction = NavigateToCreateDte;
        
        // Start on Dashboard
        CurrentViewModel = _dashboardViewModel;

        // Load Companies
        _ = LoadCompaniesAsync();
    }

    private async Task LoadCompaniesAsync()
    {
        try
        {
            var result = await _mediator.Send(new GetActiveCompaniesQuery());
            if (result.IsSuccess)
            {
                Companies.Clear();
                foreach (var company in result.Value)
                {
                    Companies.Add(company);
                }

                if (Companies.Any())
                {
                    SelectedCompany = Companies.First();
                }
            }
        }
        catch (System.Exception ex)
        {
            _errorLogger.LogError("startup", $"Error cargando empresas al inicio: {ex}");
            System.Diagnostics.Debug.WriteLine($"[MainViewModel] Error loading companies: {ex.Message}");
        }
    }

    [RelayCommand]
    private void NavigateToDashboard()
    {
        CurrentViewModel = _dashboardViewModel;
        _ = _dashboardViewModel.LoadDashboardAsync();
    }

    [RelayCommand]
    private void NavigateToCompanies() => CurrentViewModel = _companyViewModel;

    [RelayCommand]
    private void NavigateToDtes()
    {
        CurrentViewModel = _dteListViewModel;
        // Load data every time we navigate here
        _ = _dteListViewModel.LoadDtesAsync();
    }

    [RelayCommand]
    private void NavigateToCreateDte() => CurrentViewModel = _createDteViewModel;

    [RelayCommand]
    private void NavigateToOcrScanner() => CurrentViewModel = _ocrScannerViewModel;
    
    [RelayCommand]
    private void NavigateToIngestion() => CurrentViewModel = _ingestionViewModel;

    [RelayCommand]
    private void NavigateToPurchases()
    {
        CurrentViewModel = _purchasesViewModel;
        _ = _purchasesViewModel.LoadPurchasesAsync();
    }

    [RelayCommand]
    private void NavigateToSales()
    {
        CurrentViewModel = _salesViewModel;
        _ = _salesViewModel.LoadSalesAsync();
    }

    [RelayCommand]
    private void NavigateToVatBooks()
    {
        CurrentViewModel = _vatBooksViewModel;
        _ = _vatBooksViewModel.GenerateReportAsync();
    }

    [RelayCommand]
    private void NavigateToSettings() => CurrentViewModel = _settingsViewModel;

    [RelayCommand]
    private void NavigateToUsersManagement()
    {
        if (IsSuperAdmin)
        {
            CurrentViewModel = _usersManagementViewModel;
        }
    }

#pragma warning disable CA1822
    [RelayCommand]
    private void ToggleTheme()
    {
        LibroFiscal.Desktop.Services.ThemeService.ToggleTheme();
    }
#pragma warning restore CA1822
}
