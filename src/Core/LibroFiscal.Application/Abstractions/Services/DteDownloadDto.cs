using System;

namespace LibroFiscal.Application.Abstractions.Services;

public sealed record DteDownloadDto(
    string CodigoGeneracion,
    string NumeroControl,
    DateTime FechaEmision,
    string TipoDte,
    string ReceptorNombre,
    decimal TotalPagar,
    string SelloRecepcion);
