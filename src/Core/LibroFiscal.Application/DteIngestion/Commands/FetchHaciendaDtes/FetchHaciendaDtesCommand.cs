using System;
using LibroFiscal.Application.Abstractions.Messaging;
using LibroFiscal.Application.Abstractions.Services;
using System.Collections.Generic;

namespace LibroFiscal.Application.DteIngestion.Commands.FetchHaciendaDtes;

public sealed record FetchHaciendaDtesCommand(
    Guid CompanyId,
    DateTime StartDate,
    DateTime EndDate) : ICommand<List<DteDownloadDto>>;
