using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LibroFiscal.Application.Abstractions.Services;
using LibroFiscal.Application.OCR.Commands.ScanDte;
using MediatR;
using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LibroFiscal.Desktop.ViewModels;

public partial class OcrScannerViewModel : ObservableObject
{
    private readonly IMediator _mediator;
    private readonly IEmpresaActivaService _empresaActivaService;
    private readonly IDialogService _dialogService;

    [ObservableProperty]
    private string _imagePath = string.Empty;

    [ObservableProperty]
    private string _extractedText = string.Empty;

    [ObservableProperty]
    private string _nitDetectado = string.Empty;

    [ObservableProperty]
    private string _nrcDetectado = string.Empty;

    [ObservableProperty]
    private string _totalDetectado = string.Empty;

    [ObservableProperty]
    private string _ivaDetectado = string.Empty;

    [ObservableProperty]
    private string _fechaDetectada = string.Empty;

    [ObservableProperty]
    private string _numeroDocumentoDetectado = string.Empty;

    [ObservableProperty]
    private string _proveedorDetectado = string.Empty;

    [ObservableProperty]
    private bool _isScanning;

    [ObservableProperty]
    private bool _isPdfFile;

    [ObservableProperty]
    private bool _isImageFile;

    [ObservableProperty]
    private string _imagePreviewPath = string.Empty;

    private DateTimeOffset? _rawFechaDetectada;

    public OcrScannerViewModel(
        IMediator mediator, 
        IEmpresaActivaService empresaActivaService,
        IDialogService dialogService)
    {
        _mediator = mediator;
        _empresaActivaService = empresaActivaService;
        _dialogService = dialogService;
    }

    [RelayCommand]
    private void SelectImage()
    {
        var dialog = new OpenFileDialog
        {
            Filter = "Imágenes y PDFs (*.pdf;*.jpg;*.png;*.jpeg)|*.pdf;*.jpg;*.png;*.jpeg|All files (*.*)|*.*",
            Title = "Seleccionar Factura (Imagen o PDF)"
        };

        if (dialog.ShowDialog() == true)
        {
            ImagePath = dialog.FileName;
            IsPdfFile = Path.GetExtension(ImagePath).Equals(".pdf", StringComparison.OrdinalIgnoreCase);
            IsImageFile = !IsPdfFile;
            ImagePreviewPath = IsImageFile ? ImagePath : string.Empty;

            // Limpiar resultados anteriores
            ExtractedText = string.Empty;
            NitDetectado = string.Empty;
            NrcDetectado = string.Empty;
            TotalDetectado = string.Empty;
            IvaDetectado = string.Empty;
            FechaDetectada = string.Empty;
            NumeroDocumentoDetectado = string.Empty;
            ProveedorDetectado = string.Empty;
            _rawFechaDetectada = null;
        }
    }

    [RelayCommand]
    private async Task ScanImageAsync()
    {
        if (string.IsNullOrEmpty(ImagePath) || !File.Exists(ImagePath))
        {
            _dialogService.ShowWarning("Por favor, seleccione una imagen válida primero.");
            return;
        }

        IsScanning = true;

        try
        {
            var bytes = await File.ReadAllBytesAsync(ImagePath);
            var result = await _mediator.Send(new ScanDteCommand(bytes));

            if (result.IsSuccess)
            {
                var dto = result.Value;
                ExtractedText = dto.RawText;
                NitDetectado = dto.NitEncontrado ?? "No detectado";
                NrcDetectado = dto.NrcEncontrado ?? "No detectado";
                var culture = new System.Globalization.CultureInfo("en-US");
                TotalDetectado = dto.TotalEncontrado?.ToString("C", culture) ?? "No detectado";
                IvaDetectado = dto.IvaEncontrado?.ToString("C", culture) ?? "No detectado";
                _rawFechaDetectada = dto.FechaEncontrada;
                FechaDetectada = dto.FechaEncontrada?.ToString("dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture) ?? "No detectada";
                NumeroDocumentoDetectado = dto.NumeroDocumento ?? "No detectado";
                ProveedorDetectado = dto.NombreProveedor ?? "No detectado";

                _dialogService.ShowInfo("Análisis completado.", "Éxito");
            }
            else
            {
                _dialogService.ShowError($"Error al procesar: {result.Error.Message}", "Error OCR");
            }
        }
        catch (Exception ex)
        {
            _dialogService.ShowError($"Error inesperado: {ex.Message}");
        }
        finally
        {
            IsScanning = false;
        }
    }

    [RelayCommand]
    private async Task RegistrarCompraAsync()
    {
        if (string.IsNullOrWhiteSpace(NitDetectado) || NitDetectado == "No detectado")
        {
            _dialogService.ShowWarning("Por favor, asegúrese de que el NIT es válido antes de registrar la compra.");
            return;
        }

        IsScanning = true;

        try
        {
            // Parse amounts (removing formatting if any)
            decimal subTotal = 0;
            decimal taxAmount = 0;
            decimal totalAmount = 0;
            
            var style = System.Globalization.NumberStyles.Any;
            var culture = new System.Globalization.CultureInfo("en-US");

            decimal.TryParse(TotalDetectado?.Replace("$", ""), style, culture, out totalAmount);
            decimal.TryParse(IvaDetectado?.Replace("$", ""), style, culture, out taxAmount);
            
            if (totalAmount == 0)
            {
                _dialogService.ShowError("El monto total debe ser mayor a cero.");
                return;
            }

            subTotal = totalAmount - taxAmount;

            var companyId = _empresaActivaService.EmpresaActualId;

            if (companyId == null)
            {
                _dialogService.ShowError("No hay ninguna empresa configurada o activa para registrar la compra.");
                return;
            }

            var command = new LibroFiscal.Application.Purchases.Commands.RegisterPurchase.RegisterPurchaseCommand(
                CompanyId: companyId.Value,
                SupplierNit: NitDetectado,
                SupplierNrc: NrcDetectado == "No detectado" ? string.Empty : NrcDetectado,
                SupplierName: ProveedorDetectado == "No detectado" ? "Proveedor OCR" : ProveedorDetectado,
                IssueDate: _rawFechaDetectada ?? DateTimeOffset.UtcNow,
                DocumentNumber: NumeroDocumentoDetectado == "No detectado" ? $"DTE-OCR-{Guid.NewGuid().ToString().Substring(0,5).ToUpperInvariant()}" : NumeroDocumentoDetectado,
                SubTotal: subTotal,
                TaxAmount: taxAmount,
                TotalAmount: totalAmount
            );

            var result = await _mediator.Send(command);

            if (result.IsSuccess)
            {
                _dialogService.ShowInfo("¡Compra y partida doble registradas exitosamente!", "Éxito");
                // Limpiar vista
                ImagePath = string.Empty;
                ImagePreviewPath = string.Empty;
                ExtractedText = string.Empty;
                NitDetectado = string.Empty;
                NrcDetectado = string.Empty;
                TotalDetectado = string.Empty;
                IvaDetectado = string.Empty;
                FechaDetectada = string.Empty;
            }
            else
            {
                _dialogService.ShowError($"Error al registrar: {result.Error.Message}");
            }
        }
        catch (Exception ex)
        {
            _dialogService.ShowError($"Error inesperado: {ex.Message}");
        }
        finally
        {
            IsScanning = false;
        }
    }
}
