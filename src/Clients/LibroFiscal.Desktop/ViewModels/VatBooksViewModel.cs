using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LibroFiscal.Application.Taxes.Queries.GetVatPurchasesBook;
using LibroFiscal.Application.Taxes.Queries.GetVatSalesTaxpayerBook;
using LibroFiscal.Application.Taxes.Queries.GetVatSalesConsumerBook;
using MediatR;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Win32;
using LibroFiscal.Desktop.Services;

using LibroFiscal.Application.Abstractions.Services;

namespace LibroFiscal.Desktop.ViewModels;

public partial class VatBooksViewModel : ObservableObject, IDisposable
{
    private readonly IMediator _mediator;
    private readonly CsvExportService _csvExportService;
    private readonly IHaciendaF930ExportService _f930ExportService;
    private readonly IEmpresaActivaService _empresaActivaService;

    public VatBooksViewModel(IMediator mediator, CsvExportService csvExportService, IHaciendaF930ExportService f930ExportService, IEmpresaActivaService empresaActivaService)
    {
        _mediator = mediator;
        _csvExportService = csvExportService;
        _f930ExportService = f930ExportService;
        _empresaActivaService = empresaActivaService;
        
        _empresaActivaService.EmpresaCambiadaEvent += OnEmpresaCambiada;
        
        // Populate months
        for (int i = 1; i <= 12; i++)
        {
            Months.Add(new MonthItem(i, new DateTime(2000, i, 1).ToString("MMMM", System.Globalization.CultureInfo.CurrentCulture).ToUpper(System.Globalization.CultureInfo.CurrentCulture)));
        }

        // Populate years
        int currentYear = DateTime.Now.Year;
        for (int i = currentYear - 5; i <= currentYear; i++)
        {
            Years.Add(i);
        }

        SelectedMonth = Months.FirstOrDefault(m => m.Number == DateTime.Now.Month);
        SelectedYear = currentYear;
        SelectedBookType = BookTypes.First();
    }

    private void OnEmpresaCambiada(object? sender, Guid e)
    {
        _ = GenerateReportAsync();
    }

    public void Dispose()
    {
        if (_empresaActivaService != null)
        {
            _empresaActivaService.EmpresaCambiadaEvent -= OnEmpresaCambiada;
        }
        GC.SuppressFinalize(this);
    }

    public ObservableCollection<MonthItem> Months { get; } = new();
    public ObservableCollection<int> Years { get; } = new();
    public ObservableCollection<string> BookTypes { get; } = new() 
    { 
        "Libro de Compras", 
        "Libro de Ventas (Contribuyentes)", 
        "Libro de Ventas (Consumidor Final)" 
    };

    [ObservableProperty]
    private string _selectedBookType;

    [ObservableProperty]
    private MonthItem? _selectedMonth;

    [ObservableProperty]
    private int _selectedYear;

    [ObservableProperty]
    private ObservableCollection<VatPurchaseDto> _vatPurchases = new();

    [ObservableProperty]
    private ObservableCollection<VatSalesTaxpayerDto> _vatSalesTaxpayer = new();

    [ObservableProperty]
    private ObservableCollection<VatSalesConsumerDto> _vatSalesConsumer = new();

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    // Totals
    [ObservableProperty]
    private decimal _totalBase;

    [ObservableProperty]
    private decimal _totalTaxes;

    [RelayCommand]
    public async Task GenerateReportAsync()
    {
        if (IsLoading || SelectedMonth == null) return;

        var companyId = _empresaActivaService.EmpresaActualId;
        if (companyId == null) return;

        IsLoading = true;
        ErrorMessage = string.Empty;
        
        VatPurchases.Clear();
        VatSalesTaxpayer.Clear();
        VatSalesConsumer.Clear();

        TotalBase = 0;
        TotalTaxes = 0;

        try
        {
            if (SelectedBookType == "Libro de Compras")
            {
                var result = await _mediator.Send(new GetVatPurchasesBookQuery(companyId.Value, SelectedYear, SelectedMonth.Number));
                if (result.IsSuccess)
                {
                    foreach (var p in result.Value) VatPurchases.Add(p);
                    TotalBase = VatPurchases.Sum(p => p.TotalPurchases);
                    TotalTaxes = VatPurchases.Sum(p => p.TaxCredit);
                }
                else ErrorMessage = result.Error.Message;
            }
            else if (SelectedBookType == "Libro de Ventas (Contribuyentes)")
            {
                var result = await _mediator.Send(new GetVatSalesTaxpayerBookQuery(companyId.Value, SelectedYear, SelectedMonth.Number));
                if (result.IsSuccess)
                {
                    foreach (var s in result.Value) VatSalesTaxpayer.Add(s);
                    TotalBase = VatSalesTaxpayer.Sum(s => s.LocalGravadaSales + s.ExemptSales);
                    TotalTaxes = VatSalesTaxpayer.Sum(s => s.FiscalDebit);
                }
                else ErrorMessage = result.Error.Message;
            }
            else if (SelectedBookType == "Libro de Ventas (Consumidor Final)")
            {
                var result = await _mediator.Send(new GetVatSalesConsumerBookQuery(companyId.Value, SelectedYear, SelectedMonth.Number));
                if (result.IsSuccess)
                {
                    foreach (var s in result.Value) VatSalesConsumer.Add(s);
                    TotalBase = VatSalesConsumer.Sum(s => s.LocalGravadaSales + s.ExemptSales + s.ExportSales);
                    TotalTaxes = VatSalesConsumer.Sum(s => s.RetainedIva); // Usually Consumidor Final doesn't show debito fiscal detached in total, but total sales include IVA. We can just sum Retained if any.
                }
                else ErrorMessage = result.Error.Message;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error al generar el reporte: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public async Task ExportToCsvAsync()
    {
        if (IsLoading || SelectedMonth == null) return;

        var saveFileDialog = new SaveFileDialog
        {
            Filter = "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*",
            DefaultExt = "csv",
            FileName = $"{SelectedBookType} - {SelectedMonth.Name} {SelectedYear}.csv"
        };

        if (saveFileDialog.ShowDialog() == true)
        {
            try
            {
                IsLoading = true;
                if (SelectedBookType == "Libro de Compras")
                {
                    await _csvExportService.ExportPurchasesAsync(saveFileDialog.FileName, VatPurchases);
                }
                else if (SelectedBookType == "Libro de Ventas (Contribuyentes)")
                {
                    await _csvExportService.ExportSalesTaxpayerAsync(saveFileDialog.FileName, VatSalesTaxpayer);
                }
                else if (SelectedBookType == "Libro de Ventas (Consumidor Final)")
                {
                    await _csvExportService.ExportSalesConsumerAsync(saveFileDialog.FileName, VatSalesConsumer);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error al exportar a CSV: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
    [RelayCommand]
    public async Task ExportToF930Async()
    {
        if (IsLoading || SelectedMonth == null) return;

        var saveFileDialog = new SaveFileDialog
        {
            Filter = "CSV F930 (*.csv)|*.csv",
            DefaultExt = "csv",
            FileName = $"AnexoF930_{SelectedBookType}_{SelectedMonth.Name}_{SelectedYear}.csv"
        };

        if (saveFileDialog.ShowDialog() == true)
        {
            try
            {
                IsLoading = true;
                if (SelectedBookType == "Libro de Compras")
                {
                    await _f930ExportService.ExportPurchasesAsync(saveFileDialog.FileName, VatPurchases);
                }
                else if (SelectedBookType == "Libro de Ventas (Contribuyentes)")
                {
                    await _f930ExportService.ExportSalesTaxpayerAsync(saveFileDialog.FileName, VatSalesTaxpayer);
                }
                else if (SelectedBookType == "Libro de Ventas (Consumidor Final)")
                {
                    await _f930ExportService.ExportSalesConsumerAsync(saveFileDialog.FileName, VatSalesConsumer);
                }
                ErrorMessage = "Anexo F930 exportado correctamente (CSV plano sin encabezados).";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error al exportar F930: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}

public record MonthItem(int Number, string Name);
