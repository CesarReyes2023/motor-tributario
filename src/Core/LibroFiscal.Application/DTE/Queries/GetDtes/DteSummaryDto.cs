using System;

namespace LibroFiscal.Application.DTE.Queries.GetDtes;

public record DteSummaryDto(
    Guid Id,
    string NumeroControl,
    string Tipo,
    string Cliente,
    decimal Total,
    string Estado,
    string? SelloRecepcion,
    DateTimeOffset FechaEmision
);
