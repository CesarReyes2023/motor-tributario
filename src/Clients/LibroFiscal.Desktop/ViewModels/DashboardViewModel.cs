using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LibroFiscal.Application.Taxes.Queries.GetDashboardMetrics;
using MediatR;
using System;
using System.Threading.Tasks;

namespace LibroFiscal.Desktop.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    private readonly IMediator _mediator;

    [ObservableProperty]
    private decimal _totalSales;

    [ObservableProperty]
    private decimal _totalSalesTaxes;

    [ObservableProperty]
    private decimal _totalPurchases;

    [ObservableProperty]
    private decimal _totalPurchaseTaxes;

    [ObservableProperty]
    private decimal _ivaBalance;

    [ObservableProperty]
    private int _pendingDtesCount;

    [ObservableProperty]
    private double _salesPercentage;

    [ObservableProperty]
    private double _purchasesPercentage;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Used for XAML Binding")]
    public string CurrentMonthName => DateTime.Now.ToString("MMMM yyyy", new System.Globalization.CultureInfo("es-ES")).ToUpper(new System.Globalization.CultureInfo("es-ES"));

    public DashboardViewModel(IMediator mediator)
    {
        _mediator = mediator;
    }

    [RelayCommand]
    public async Task LoadDashboardAsync()
    {
        if (IsLoading) return;
        
        IsLoading = true;
        ErrorMessage = string.Empty;

        try
        {
            var query = new GetDashboardMetricsQuery(DateTime.Now.Year, DateTime.Now.Month);
            var result = await _mediator.Send(query);

            if (result.IsSuccess)
            {
                TotalSales = result.Value.TotalSales;
                TotalSalesTaxes = result.Value.TotalSalesTaxes;
                TotalPurchases = result.Value.TotalPurchases;
                TotalPurchaseTaxes = result.Value.TotalPurchaseTaxes;
                IvaBalance = result.Value.IvaBalance;
                PendingDtesCount = result.Value.PendingDtesCount;
                SalesPercentage = result.Value.SalesPercentage;
                PurchasesPercentage = result.Value.PurchasesPercentage;
            }
            else
            {
                ErrorMessage = result.Error.Message;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = "Error de conexión: " + ex.Message;
        }
        finally
        {
            IsLoading = false;
        }
    }
}
