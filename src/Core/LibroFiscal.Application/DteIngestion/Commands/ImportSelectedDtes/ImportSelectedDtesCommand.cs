using System;
using System.Collections.Generic;
using LibroFiscal.Application.Abstractions.Messaging;
using LibroFiscal.Application.Abstractions.Services;

namespace LibroFiscal.Application.DteIngestion.Commands.ImportSelectedDtes;

public sealed record ImportSelectedDtesCommand(
    Guid CompanyId,
    List<DteDownloadDto> SelectedDtes) : ICommand<IngestDtes.IngestionResultDto>;
