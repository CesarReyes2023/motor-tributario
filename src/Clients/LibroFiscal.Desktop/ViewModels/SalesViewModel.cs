using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LibroFiscal.Application.Sales.Commands.RegisterSale;
using LibroFiscal.Application.Invoices.Commands.GeneratePdf;
using LibroFiscal.Application.Sales.Queries.GetSales;
using MediatR;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using LibroFiscal.Desktop.Services;

namespace LibroFiscal.Desktop.ViewModels;

public partial class SalesViewModel : ObservableObject, IDisposable
{
    private readonly IMediator _mediator;
    private readonly LibroFiscal.Application.Abstractions.Services.IEmpresaActivaService _empresaActivaService;

    public SalesViewModel(
        IMediator mediator,
        LibroFiscal.Application.Abstractions.Services.IEmpresaActivaService empresaActivaService)
    {
        _mediator = mediator;
        _empresaActivaService = empresaActivaService;
        Sales = new ObservableCollection<SaleDto>();
        
        _empresaActivaService.EmpresaCambiadaEvent += OnEmpresaCambiada;
        IssueDate = DateTimeOffset.Now;
    }

    private void OnEmpresaCambiada(object? sender, Guid e) => _ = LoadSalesAsync();

    public void Dispose()
    {
        _empresaActivaService.EmpresaCambiadaEvent -= OnEmpresaCambiada;
        GC.SuppressFinalize(this);
    }

    [ObservableProperty]
    private ObservableCollection<SaleDto> _sales;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _successMessage = string.Empty;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    // Form fields
    [ObservableProperty] private string _customerName = string.Empty;
    [ObservableProperty] private string _customerNit = string.Empty;
    [ObservableProperty] private string _customerNrc = string.Empty;
    [ObservableProperty] private DateTimeOffset _issueDate;
    [ObservableProperty] private string _documentNumber = string.Empty;
    [ObservableProperty] private decimal _taxableAmount;
    [ObservableProperty] private decimal _exemptAmount;
    [ObservableProperty] private decimal _taxAmount;
    [ObservableProperty] private decimal _totalAmount;
    [ObservableProperty] private string _invoiceType = "Consumidor Final"; // Consumidor Final o CCF

    // Para simplificar la demo, asumo que la empresa tiene un ID de plantilla por defecto o uso la primera
    // En producción esto estaría vinculado a la configuración de la empresa.

    [RelayCommand]
    public async Task LoadSalesAsync()
    {
        if (IsLoading) return;
        
        var companyId = _empresaActivaService.EmpresaActualId;
        if (companyId == null) return;

        IsLoading = true;
        ErrorMessage = string.Empty;
        Sales.Clear();

        try
        {
            var result = await _mediator.Send(new GetSalesQuery(companyId.Value));

            if (result.IsSuccess)
            {
                foreach (var sale in result.Value)
                {
                    Sales.Add(sale);
                }
            }
            else
            {
                ErrorMessage = result.Error.Message;
            }
        }
        catch (System.Exception ex)
        {
            ErrorMessage = $"Error al cargar ventas: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public async Task RegisterSaleAsync()
    {
        var companyId = _empresaActivaService.EmpresaActualId;
        if (companyId == null)
        {
            ErrorMessage = "Seleccione una empresa primero.";
            return;
        }

        IsLoading = true;
        ErrorMessage = string.Empty;
        SuccessMessage = string.Empty;

        try
        {
            var cmd = new RegisterSaleCommand(
                companyId.Value,
                CustomerNit,
                CustomerNrc,
                CustomerName,
                IssueDate,
                DocumentNumber,
                TaxableAmount,
                ExemptAmount,
                TaxAmount,
                TotalAmount);

            var result = await _mediator.Send(cmd);

            if (result.IsSuccess)
            {
                SuccessMessage = "Venta registrada exitosamente.";
                ClearForm();
                await LoadSalesAsync();
            }
            else
            {
                ErrorMessage = result.Error.Message;
            }
        }
        catch (System.Exception ex)
        {
            ErrorMessage = $"Error inesperado: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public async Task PrintPdfAsync(SaleDto sale)
    {
        if (sale == null) return;
        
        // Obtener el ID de la primera plantilla para la demo. 
        // Idealmente, se debe almacenar el TemplateId en Company o usar una query.
        // Haremos un hack de Ponytail: le pasamos Guid.Empty y que el Handler decida o simplemente listamos la primera.
        // Espera, el GenerateInvoicePdfCommand requiere un TemplateId válido.
        // Vamos a modificar el backend momentáneamente si es necesario, pero por ahora...
        
        IsLoading = true;
        ErrorMessage = string.Empty;
        SuccessMessage = string.Empty;
        
        try
        {
            // OJO: En la vida real aquí obtienes el TemplateId de la BD. 
            // Para la demo, el usuario guardó una en Settings. Vamos a mandar un request a la API que devuelva la plantilla de la empresa.
            // Puesto que Ponytail dicta "menos código", vamos a cambiar GenerateInvoicePdfCommand en el próximo paso para que no exija TemplateId sino CompanyId y tome la primera.
            
            var pdfResult = await _mediator.Send(new GenerateInvoicePdfCommand(sale.Id, Guid.Empty)); // Placeholder
            
            if (pdfResult.IsSuccess)
            {
                var filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), $"Factura_{sale.DocumentNumber}.pdf");
                await File.WriteAllBytesAsync(filePath, pdfResult.Value);
                SuccessMessage = $"PDF Guardado en Escritorio: {filePath}";
            }
            else
            {
                ErrorMessage = pdfResult.Error.Message;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error al imprimir PDF: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void ClearForm()
    {
        CustomerName = string.Empty;
        CustomerNit = string.Empty;
        CustomerNrc = string.Empty;
        DocumentNumber = string.Empty;
        TaxableAmount = 0;
        ExemptAmount = 0;
        TaxAmount = 0;
        TotalAmount = 0;
    }

    [RelayCommand]
    public async Task ExportToCsvAsync()
    {
        if (Sales == null || Sales.Count == 0)
        {
            ErrorMessage = "No hay datos para exportar.";
            return;
        }

        try
        {
            var dialog = new SaveFileDialog
            {
                Filter = "Archivos CSV (*.csv)|*.csv",
                Title = "Exportar Ventas a CSV",
                FileName = $"Ventas_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
            };

            if (dialog.ShowDialog() == true)
            {
                IsLoading = true;
                
                // Ponytail: Evitamos bloquear el UI Thread en discos duros lentos
                await Task.Run(async () =>
                {
                    var csvContent = CsvExporter.GenerateCsv(Sales);
                    // BOM requerido para que Excel lea los tildes correctamente
                    await File.WriteAllTextAsync(dialog.FileName, csvContent, Encoding.UTF8);
                });

                SuccessMessage = $"Exportado exitosamente: {dialog.FileName}";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error al exportar CSV: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
}
