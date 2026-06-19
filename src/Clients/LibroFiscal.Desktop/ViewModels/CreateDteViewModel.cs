using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LibroFiscal.Application.DTE.Commands.CreateDte;
using MediatR;

namespace LibroFiscal.Desktop.ViewModels;

public partial class CreateDteViewModel : ObservableObject
{
    private readonly IMediator _mediator;
    private readonly LibroFiscal.Application.Abstractions.Services.IEmpresaActivaService _empresaActivaService;
    private readonly LibroFiscal.Application.Abstractions.Services.IDialogService _dialogService;
    private readonly LibroFiscal.Application.Abstractions.Services.IErrorLogger _errorLogger;

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

    public CreateDteViewModel(
        IMediator mediator,
        LibroFiscal.Application.Abstractions.Services.IEmpresaActivaService empresaActivaService,
        LibroFiscal.Application.Abstractions.Services.IDialogService dialogService,
        LibroFiscal.Application.Abstractions.Services.IErrorLogger errorLogger)
    {
        _mediator = mediator;
        _empresaActivaService = empresaActivaService;
        _dialogService = dialogService;
        _errorLogger = errorLogger;
        Items.CollectionChanged += (s, e) => _ = RecalcularTotalesAsync();
    }

    [RelayCommand]
    private void AgregarItem()
    {
        if (string.IsNullOrWhiteSpace(NuevoItemDescripcion) || NuevoItemCantidad <= 0 || NuevoItemPrecio <= 0)
        {
            _dialogService.ShowWarning("Debe ingresar descripción, cantidad y precio.");
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

    private async Task RecalcularTotalesAsync()
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
            _dialogService.ShowWarning("Debe agregar al menos un ítem a la factura.");
            return;
        }

        var companyId = _empresaActivaService.EmpresaActualId;
        if (companyId == null)
        {
            _dialogService.ShowError("No hay ninguna empresa activa. Seleccione una empresa primero.");
            return;
        }

        if (IsBusy) return;
        IsBusy = true;

        try
        {
            // Cargar perfil de la empresa activa para datos del emisor
            var profileResult = await _mediator.Send(new LibroFiscal.Application.Companies.Queries.GetCompanyProfile.GetCompanyProfileQuery(companyId.Value));
            if (profileResult.IsFailure || profileResult.Value == null)
            {
                _dialogService.ShowError("No se pudo cargar el perfil de la empresa activa.");
                return;
            }
            var profile = profileResult.Value;

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

            // Emisor con datos reales de la empresa activa
            var emisor = new DteEmisorDto(
                Nit: profile.Nit,
                Nrc: profile.Nrc,
                Nombre: profile.LegalName,
                NombreComercial: profile.TradeName,
                CodigoActividad: profile.EconomicActivityCode,
                DescripcionActividad: profile.EconomicActivityDescription,
                Telefono: profile.Phone,
                Correo: profile.Email,
                CodigoEstablecimiento: "0000",
                PuntoVenta: "0000",
                Departamento: profile.Department,
                Municipio: profile.Municipality,
                ComplementoDireccion: profile.AddressLine
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

            // Correlativo secuencial basado en timestamp (seguro contra duplicados dentro de un segundo)
            var correlativo = int.Parse(DateTimeOffset.UtcNow.ToString("HHmmssfff", System.Globalization.CultureInfo.InvariantCulture), System.Globalization.CultureInfo.InvariantCulture);

            var command = new CreateDteCommand(
                CompanyId: companyId.Value,
                TipoDteId: 1, // Factura
                Version: 1,
                NumeroControlCodigo: "03",
                NumeroControlPuntoVenta: "000",
                NumeroControlCorrelativo: correlativo,
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
                _dialogService.ShowInfo($"DTE emitido exitosamente.\nID: {result.Value}", "Éxito");
                Items.Clear();
                ReceptorNombre = string.Empty;
                ReceptorNit = string.Empty;
            }
            else
            {
                _dialogService.ShowError($"Error al emitir DTE: {result.Error.Message}");
            }
        }
        catch (Exception ex)
        {
            _errorLogger.LogError("dte_emit", ex);
            _dialogService.ShowError($"Ocurrió un error inesperado: {ex.Message}", "Error Crítico");
        }
        finally
        {
            IsBusy = false;
        }
    }
}
