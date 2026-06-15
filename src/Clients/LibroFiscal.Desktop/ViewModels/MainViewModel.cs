using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace LibroFiscal.Desktop.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly DashboardViewModel _dashboardViewModel;
    private readonly CompanyViewModel _companyViewModel;
    private readonly DteListViewModel _dteListViewModel;
    private readonly CreateDteViewModel _createDteViewModel;
    private readonly OcrScannerViewModel _ocrScannerViewModel;
    private readonly PurchasesViewModel _purchasesViewModel;
    private readonly VatBooksViewModel _vatBooksViewModel;
    private readonly IngestionViewModel _ingestionViewModel;

    [ObservableProperty]
    private ObservableObject _currentViewModel;

    public MainViewModel(
        DashboardViewModel dashboardViewModel, 
        CompanyViewModel companyViewModel,
        DteListViewModel dteListViewModel,
        CreateDteViewModel createDteViewModel,
        OcrScannerViewModel ocrScannerViewModel,
        PurchasesViewModel purchasesViewModel,
        VatBooksViewModel vatBooksViewModel,
        IngestionViewModel ingestionViewModel)
    {
        _dashboardViewModel = dashboardViewModel;
        _companyViewModel = companyViewModel;
        _dteListViewModel = dteListViewModel;
        _createDteViewModel = createDteViewModel;
        _ocrScannerViewModel = ocrScannerViewModel;
        _purchasesViewModel = purchasesViewModel;
        _vatBooksViewModel = vatBooksViewModel;
        _ingestionViewModel = ingestionViewModel;
        
        // Setup cross-VM navigation
        _dteListViewModel.NavigateToCreateDteAction = NavigateToCreateDte;
        
        // Start on Dashboard
        _currentViewModel = _dashboardViewModel;
    }

    [RelayCommand]
    private void NavigateToDashboard() => CurrentViewModel = _dashboardViewModel;

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
    private void NavigateToVatBooks()
    {
        CurrentViewModel = _vatBooksViewModel;
        _ = _vatBooksViewModel.GenerateReportAsync();
    }

#pragma warning disable CA1822
    [RelayCommand]
    private void ToggleTheme()
    {
        LibroFiscal.Desktop.Services.ThemeService.ToggleTheme();
    }
#pragma warning restore CA1822
}
