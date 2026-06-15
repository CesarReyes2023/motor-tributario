using LibroFiscal.Application.Abstractions.Messaging;
using LibroFiscal.Application.Abstractions.Services;
using LibroFiscal.Domain.Common.Enumerations;
using LibroFiscal.Domain.Common.Ids;
using LibroFiscal.Domain.Common.ValueObjects;
using LibroFiscal.Domain.DTE.Entities;
using LibroFiscal.SharedKernel.Interfaces;
using LibroFiscal.SharedKernel.Primitives;
using LibroFiscal.SharedKernel.Results;

namespace LibroFiscal.Application.DTE.Commands.CreateDte;

public sealed class CreateDteCommandHandler : ICommandHandler<CreateDteCommand, Guid>
{
    private readonly IRepository<DteDocument, DteId> _dteRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISigningService _signingService;
    private readonly IHaciendaService _haciendaService;

    public CreateDteCommandHandler(
        IRepository<DteDocument, DteId> dteRepository,
        IUnitOfWork unitOfWork,
        ISigningService signingService,
        IHaciendaService haciendaService)
    {
        _dteRepository = dteRepository;
        _unitOfWork = unitOfWork;
        _signingService = signingService;
        _haciendaService = haciendaService;
    }

    public async Task<Result<Guid>> Handle(CreateDteCommand request, CancellationToken cancellationToken)
    {
        // En un caso real, validaríamos que la Company exista, etc.

        var numeroControlResult = NumeroControl.Create(
            request.NumeroControlCodigo, 
            request.Emisor.CodigoEstablecimiento, 
            request.NumeroControlPuntoVenta, 
            request.NumeroControlCorrelativo);

        if (numeroControlResult.IsFailure) return Result.Failure<Guid>(numeroControlResult.Error);

        var direccionEmisorResult = DireccionFiscal.Create(
            request.Emisor.Departamento, request.Emisor.Municipio, request.Emisor.ComplementoDireccion);

        if (direccionEmisorResult.IsFailure) return Result.Failure<Guid>(direccionEmisorResult.Error);

        var emisor = new DteEmisor(
            request.Emisor.Nit, request.Emisor.Nrc, request.Emisor.Nombre, request.Emisor.NombreComercial,
            request.Emisor.CodigoActividad, request.Emisor.DescripcionActividad,
            request.Emisor.Telefono, request.Emisor.Correo,
            request.Emisor.CodigoEstablecimiento, request.Emisor.PuntoVenta,
            direccionEmisorResult.Value);

        DteReceptor? receptor = null;
        if (request.Receptor is not null)
        {
            DireccionFiscal? dirReceptor = null;
            if (request.Receptor.Departamento is not null && request.Receptor.Municipio is not null && request.Receptor.ComplementoDireccion is not null)
            {
                var dirResult = DireccionFiscal.Create(request.Receptor.Departamento, request.Receptor.Municipio, request.Receptor.ComplementoDireccion);
                if (dirResult.IsFailure) return Result.Failure<Guid>(dirResult.Error);
                dirReceptor = dirResult.Value;
            }

            receptor = new DteReceptor(
                request.Receptor.Nombre, request.Receptor.Nit, request.Receptor.Nrc,
                request.Receptor.NombreComercial, request.Receptor.CodigoActividad,
                request.Receptor.Telefono, request.Receptor.Correo,
                dirReceptor, request.Receptor.NumeroDocumento, request.Receptor.TipoDocumento);
        }

        var lines = request.CuerpoDocumento.Select(l => new DteLineItem(
            l.NumeroLinea, l.Descripcion, l.Cantidad, l.PrecioUnitario, l.Descuento,
            Enumeration.FromId<TipoImpuesto>(l.TipoImpuestoId),
            l.CodigoProducto, l.UnidadMedida)).ToList();

        var resumen = new DteResumen(
            request.Resumen.TotalGravada, request.Resumen.TotalExenta, request.Resumen.TotalNoSujeta,
            request.Resumen.SubTotal, request.Resumen.TotalDescuento, request.Resumen.TotalIva,
            request.Resumen.MontoTotalOperacion, request.Resumen.TotalPagar,
            Enumeration.FromId<CondicionOperacion>(request.Resumen.CondicionOperacionId));

        var dteResult = DteDocument.Create(
            new CompanyId(request.CompanyId),
            Enumeration.FromId<TipoDte>(request.TipoDteId),
            request.Version,
            numeroControlResult.Value,
            request.FechaEmision,
            emisor,
            receptor,
            lines,
            resumen,
            Enumeration.FromId<AmbienteHacienda>(request.AmbienteHaciendaId),
            Enumeration.FromId<ModeloFacturacion>(request.ModeloFacturacionId),
            Enumeration.FromId<TipoTransmision>(request.TipoTransmisionId));

        if (dteResult.IsFailure) return Result.Failure<Guid>(dteResult.Error);

        var dte = dteResult.Value;
        
        // --- 1. VALIDACIÓN INTERNA ---
        var valResult = dte.MarkAsValidated();
        if (valResult.IsFailure) return Result.Failure<Guid>(valResult.Error);

        // --- 2. FIRMA ELECTRÓNICA ---
        // TODO: Mapear `dte` a DteJsonDto según la normativa de Hacienda usando System.Text.Json
        string jsonPayload = "{\"mock\": \"payload\"}"; 
        
        var signResult = await _signingService.SignDocumentAsync(jsonPayload, request.CompanyId, cancellationToken);
        if (signResult.IsFailure) return Result.Failure<Guid>(signResult.Error);

        dte.MarkAsSigned(signResult.Value.JwsSignature, jsonPayload, signResult.Value.SignedPayload);
        
        // --- 3. TRANSMISIÓN A HACIENDA ---
        dte.EnqueueForTransmission();
        dte.MarkAsTransmitting();

        var authResult = await _haciendaService.AuthenticateAsync(request.CompanyId, cancellationToken);
        if (authResult.IsFailure) 
        {
            dte.MarkAsRejected(authResult.Error.Message);
            _dteRepository.Add(dte);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Failure<Guid>(authResult.Error);
        }

        var transmitResult = await _haciendaService.TransmitDteAsync(signResult.Value.JwsSignature, authResult.Value.Token, cancellationToken);
        if (transmitResult.IsFailure)
        {
            dte.MarkAsRejected(transmitResult.Error.Message);
            _dteRepository.Add(dte);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Failure<Guid>(transmitResult.Error);
        }

        // --- 4. ÉXITO (SELLO) ---
        dte.MarkAsSealed(transmitResult.Value.SelloRecepcion, transmitResult.Value.FechaRecepcion);

        _dteRepository.Add(dte);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return dte.Id.Value;
    }
}
