using FluentAssertions;
using LibroFiscal.Domain.Common.Enumerations;
using LibroFiscal.Domain.Common.Ids;
using LibroFiscal.Domain.Common.ValueObjects;
using LibroFiscal.Domain.DTE.Entities;
using Xunit;

namespace LibroFiscal.Tests.Unit.Domain;

/// <summary>
/// Tests for the DTE Document aggregate root lifecycle and state machine.
/// Verifies that all state transition invariants are enforced.
/// </summary>
public class DteDocumentTests
{
    [Fact]
    public void Create_WithValidData_ShouldReturnSuccessInBorradorState()
    {
        // Arrange & Act
        var result = CreateValidDte();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Estado.Should().Be(EstadoDte.Borrador);
        result.Value.CuerpoDocumento.Should().HaveCount(1);
        result.Value.DomainEvents.Should().HaveCount(1);
        result.Value.HistorialEstados.Should().HaveCount(1);
    }

    [Fact]
    public void Create_WithEmptyCuerpo_ShouldReturnValidationError()
    {
        // Arrange
        var emisor = CreateEmisor();
        var resumen = CreateResumen();
        var numeroControl = NumeroControl.Create("01", "0001", "001", 1).Value;

        // Act
        var result = DteDocument.Create(
            CompanyId.New(),
            TipoDte.Factura,
            version: 3,
            numeroControl,
            DateTimeOffset.UtcNow,
            emisor,
            receptor: null,
            cuerpoDocumento: [],
            resumen,
            AmbienteHacienda.Pruebas,
            ModeloFacturacion.Normal,
            TipoTransmision.Normal);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("DTE.CuerpoVacio");
    }

    [Fact]
    public void MarkAsValidated_FromBorrador_ShouldSucceed()
    {
        var dte = CreateValidDte().Value;
        var result = dte.MarkAsValidated();

        result.IsSuccess.Should().BeTrue();
        dte.Estado.Should().Be(EstadoDte.Validado);
    }

    [Fact]
    public void MarkAsValidated_FromSellado_ShouldFail()
    {
        var dte = CreateSealedDte();
        var result = dte.MarkAsValidated();

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("DTE.InvalidTransition");
    }

    [Fact]
    public void MarkAsSigned_FromValidado_ShouldSucceed()
    {
        var dte = CreateValidDte().Value;
        dte.MarkAsValidated();

        var result = dte.MarkAsSigned("jws-signature", "{}", "signed-payload");

        result.IsSuccess.Should().BeTrue();
        dte.Estado.Should().Be(EstadoDte.Firmado);
        dte.FirmaElectronica.Should().Be("jws-signature");
    }

    [Fact]
    public void FullLifecycle_BorradorToSellado_ShouldTrackAllStates()
    {
        // Arrange
        var dte = CreateValidDte().Value;

        // Act — full happy path
        dte.MarkAsValidated();
        dte.MarkAsSigned("jws-sig", "{}", "signed");
        dte.EnqueueForTransmission();
        dte.MarkAsTransmitting();
        dte.MarkAsSealed("sello-hacienda-123", DateTimeOffset.UtcNow);

        // Assert
        dte.Estado.Should().Be(EstadoDte.Sellado);
        dte.SelloRecepcion.Should().Be("sello-hacienda-123");
        dte.HasFiscalValidity.Should().BeTrue();
        dte.HistorialEstados.Should().HaveCount(6); // Borrador + 5 transitions
        dte.IntentosTransmision.Should().Be(1);
    }

    [Fact]
    public void Anular_FromSellado_ShouldSucceed()
    {
        var dte = CreateSealedDte();
        var result = dte.Anular("Error en datos del receptor");

        result.IsSuccess.Should().BeTrue();
        dte.Estado.Should().Be(EstadoDte.Anulado);
        dte.FechaAnulacion.Should().NotBeNull();
    }

    [Fact]
    public void Anular_FromBorrador_ShouldFail()
    {
        var dte = CreateValidDte().Value;
        var result = dte.Anular("Motivo");

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("DTE.InvalidTransition");
    }

    // ── Helpers ────────────────────────────────────────────────────

    private static SharedKernel.Results.Result<DteDocument> CreateValidDte()
    {
        var emisor = CreateEmisor();
        var resumen = CreateResumen();
        var numeroControl = NumeroControl.Create("01", "0001", "001", 1).Value;
        var lineItem = new DteLineItem(1, "Servicio de consultoría", 1, 100m, 0m, TipoImpuesto.Iva);

        return DteDocument.Create(
            CompanyId.New(),
            TipoDte.Factura,
            version: 3,
            numeroControl,
            DateTimeOffset.UtcNow,
            emisor,
            receptor: null,
            cuerpoDocumento: [lineItem],
            resumen,
            AmbienteHacienda.Pruebas,
            ModeloFacturacion.Normal,
            TipoTransmision.Normal);
    }

    private static DteDocument CreateSealedDte()
    {
        var dte = CreateValidDte().Value;
        dte.MarkAsValidated();
        dte.MarkAsSigned("jws", "{}", "signed");
        dte.EnqueueForTransmission();
        dte.MarkAsTransmitting();
        dte.MarkAsSealed("sello-123", DateTimeOffset.UtcNow);
        return dte;
    }

    private static DteEmisor CreateEmisor()
    {
        var direccion = DireccionFiscal.Create("San Salvador", "San Salvador", "Col. Escalón, Calle El Mirador #123").Value;
        return new DteEmisor(
            "06141234567890", "123456-7", "Empresa Test S.A. de C.V.",
            "Empresa Test", "46510", "Actividades de programación informática",
            "2222-3333", "test@empresa.com", "0001", "001", direccion);
    }

    private static DteResumen CreateResumen()
    {
        return new DteResumen(
            totalGravada: 100m,
            totalExenta: 0m,
            totalNoSujeta: 0m,
            subTotal: 100m,
            totalDescuento: 0m,
            totalIva: 13m,
            montoTotalOperacion: 113m,
            totalPagar: 113m,
            condicionOperacion: CondicionOperacion.Contado);
    }
}
