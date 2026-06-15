using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LibroFiscal.Application.DTE.Commands.CreateDte;
using MediatR;

namespace LibroFiscal.Desktop.ViewModels;

public partial class CreateDteViewModel : ObservableObject
{
    private readonly IMediator _mediator;

    [ObservableProperty]
    private string _receptorNombre = string.Empty;

    [ObservableProperty]
    private string _receptorNit = string.Empty;

    [ObservableProperty]
    private string _nuevoItemDescripcion = string.Empty;

    [ObservableProperty]
    private decimal _nuevoItemCantidad = 1;

    [ObservableProperty]
    private decimal _nuevoItemPrecio = 0;

    [ObservableProperty]
    private ObservableCollection<DteLineItemDto> _items = new();

    [ObservableProperty]
    private decimal _subTotal;

    [ObservableProperty]
    private decimal _totalIva;

    [ObservableProperty]
    private decimal _totalRetencion;

    [ObservableProperty]
    private decimal _totalPagar;

    [ObservableProperty]
    private bool _isBusy;

    public CreateDteViewModel(IMediator mediator)
    {
        _mediator = mediator;
        Items.CollectionChanged += (s, e) => RecalcularTotales();
    }

    [RelayCommand]
    private void AgregarItem()
    {
        if (string.IsNullOrWhiteSpace(NuevoItemDescripcion) || NuevoItemCantidad <= 0 || NuevoItemPrecio <= 0)
        {
            MessageBox.Show("Debe ingresar descripción, cantidad y precio.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var item = new DteLineItemDto(
            NumeroLinea: Items.Count + 1,
            Descripcion: NuevoItemDescripcion,
            Cantidad: NuevoItemCantidad,
            PrecioUnitario: NuevoItemPrecio,
            Descuento: 0,
            TipoImpuestoId: 1, // IVA 13%
            CodigoProducto: null,
            UnidadMedida: "59" // Unidad estándar
        );

        Items.Add(item);

        // Reset fields
        NuevoItemDescripcion = string.Empty;
        NuevoItemCantidad = 1;
        NuevoItemPrecio = 0;
    }

    private async void RecalcularTotales()
    {
        SubTotal = Items.Sum(i => i.Cantidad * i.PrecioUnitario);
        
        try
        {
            var query = new LibroFiscal.Application.Taxes.Queries.CalculateDteTaxes.CalculateDteTaxesQuery(SubTotal);
            var result = await _mediator.Send(query);

            if (result.IsSuccess)
            {
                TotalIva = result.Value.TotalTaxes;
                TotalRetencion = result.Value.TotalDeductions;
                TotalPagar = result.Value.GrandTotal;
            }
        }
        catch (Exception)
        {
            // Fallback en caso de que el mediador falle durante la escritura o DB offline
            TotalIva = SubTotal * 0.13m;
            TotalRetencion = 0;
            TotalPagar = SubTotal + TotalIva;
        }
    }

    [RelayCommand]
    private async Task EmitirDteAsync()
    {
        if (Items.Count == 0)
        {
            MessageBox.Show("Debe agregar al menos un ítem a la factura.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (IsBusy) return;
        IsBusy = true;

        try
        {
            // Construir Resumen
            var resumen = new DteResumenDto(
                TotalGravada: SubTotal,
                TotalExenta: 0,
                TotalNoSujeta: 0,
                SubTotal: SubTotal,
                TotalDescuento: 0,
                TotalIva: TotalIva,
                MontoTotalOperacion: SubTotal,
                TotalPagar: TotalPagar,
                CondicionOperacionId: 1 // Contado
            );

            // Mock Emisor para el MVP
            var emisor = new DteEmisorDto(
                Nit: "0614-010101-101-1",
                Nrc: "123456-7",
                Nombre: "Cesar Reyes",
                NombreComercial: "Cesar Reyes",
                CodigoActividad: "62011",
                DescripcionActividad: "Desarrollo de software",
                Telefono: "2222-2222",
                Correo: "contacto@cesarreyes.com",
                CodigoEstablecimiento: "0000",
                PuntoVenta: "0000",
                Departamento: "San Salvador",
                Municipio: "San Salvador",
                ComplementoDireccion: "Colonia Escalón"
            );

            var receptor = new DteReceptorDto(
                Nombre: ReceptorNombre,
                Nit: string.IsNullOrWhiteSpace(ReceptorNit) ? null : ReceptorNit,
                Nrc: null,
                NombreComercial: null,
                CodigoActividad: null,
                Telefono: null,
                Correo: null,
                Departamento: null,
                Municipio: null,
                ComplementoDireccion: null,
                NumeroDocumento: null,
                TipoDocumento: null
            );

            var command = new CreateDteCommand(
                CompanyId: Guid.NewGuid(), // En un caso real vendría del Auth o del CompanyService
                TipoDteId: 1, // Factura
                Version: 1,
                NumeroControlCodigo: "03",
                NumeroControlPuntoVenta: "000",
                NumeroControlCorrelativo: new Random().Next(1000, 9999),
                FechaEmision: DateTimeOffset.UtcNow,
                Emisor: emisor,
                Receptor: receptor,
                CuerpoDocumento: Items.ToList(),
                Resumen: resumen,
                AmbienteHaciendaId: 0, // Pruebas (0 = Pruebas, 1 = Produccion)
                ModeloFacturacionId: 1,
                TipoTransmisionId: 1
            );

            var result = await _mediator.Send(command);

            if (result.IsSuccess)
            {
                MessageBox.Show($"DTE emitido exitosamente.\nID: {result.Value}", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                Items.Clear();
                ReceptorNombre = string.Empty;
                ReceptorNit = string.Empty;
            }
            else
            {
                MessageBox.Show($"Error al emitir DTE: {result.Error.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            System.IO.File.WriteAllText("error_dte.txt", ex.ToString());
            MessageBox.Show($"Ocurrió un error inesperado (revisa error_dte.txt para detalles): {ex.Message}", "Error Crítico", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsBusy = false;
        }
    }
}
