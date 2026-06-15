using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LibroFiscal.Application.OCR.Commands.ScanDte;
using MediatR;
using Microsoft.Win32;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace LibroFiscal.Desktop.ViewModels;

public partial class OcrScannerViewModel : ObservableObject
{
    private readonly IMediator _mediator;

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
    private bool _isScanning;

    public OcrScannerViewModel(IMediator mediator)
    {
        _mediator = mediator;
    }

    [ObservableProperty]
    private bool _isPdfFile;

    [ObservableProperty]
    private bool _isImageFile;

    [ObservableProperty]
    private string _imagePreviewPath = string.Empty;

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
        }
    }

    [RelayCommand]
    private async Task ScanImageAsync()
    {
        if (string.IsNullOrEmpty(ImagePath) || !File.Exists(ImagePath))
        {
            MessageBox.Show("Por favor, seleccione una imagen válida primero.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                FechaDetectada = dto.FechaEncontrada?.ToString("dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture) ?? "No detectada";

                MessageBox.Show("Análisis completado.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show($"Error al procesar: {result.Error.Message}", "Error OCR", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error inesperado: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
            MessageBox.Show("Por favor, asegúrese de que el NIT es válido antes de registrar la compra.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                MessageBox.Show("El monto total debe ser mayor a cero.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            subTotal = totalAmount - taxAmount;

            var command = new LibroFiscal.Application.Purchases.Commands.RegisterPurchase.RegisterPurchaseCommand(
                CompanyId: Guid.Parse("00000000-0000-0000-0000-000000000001"), // MVP Hardcoded for now
                SupplierNit: NitDetectado,
                SupplierNrc: NrcDetectado == "No detectado" ? string.Empty : NrcDetectado,
                SupplierName: "Proveedor OCR", // Podría ser extraído en el futuro
                IssueDate: DateTimeOffset.UtcNow, // Debería extraerse la fecha real en el futuro
                DocumentNumber: "DTE-XXX",
                SubTotal: subTotal,
                TaxAmount: taxAmount,
                TotalAmount: totalAmount
            );

            var result = await _mediator.Send(command);

            if (result.IsSuccess)
            {
                MessageBox.Show("¡Compra y partida doble registradas exitosamente!", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
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
                MessageBox.Show($"Error al registrar: {result.Error.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error inesperado: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsScanning = false;
        }
    }
}
