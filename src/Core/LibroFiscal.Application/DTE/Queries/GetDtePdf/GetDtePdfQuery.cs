using System;
using System.Threading;
using System.Threading.Tasks;
using LibroFiscal.Application.Abstractions.Messaging;
using LibroFiscal.Application.Abstractions.Services;
using LibroFiscal.Domain.Common.Ids;
using LibroFiscal.Domain.DTE.Entities;
using LibroFiscal.SharedKernel.Interfaces;
using LibroFiscal.SharedKernel.Results;

namespace LibroFiscal.Application.DTE.Queries.GetDtePdf;

public record GetDtePdfQuery(Guid Id) : IQuery<byte[]>;

public sealed class GetDtePdfQueryHandler : IQueryHandler<GetDtePdfQuery, byte[]>
{
    private readonly IRepository<DteDocument, DteId> _repository;
    private readonly IDtePdfGenerator _pdfGenerator;

    public GetDtePdfQueryHandler(
        IRepository<DteDocument, DteId> repository,
        IDtePdfGenerator pdfGenerator)
    {
        _repository = repository;
        _pdfGenerator = pdfGenerator;
    }

    public async Task<Result<byte[]>> Handle(GetDtePdfQuery request, CancellationToken cancellationToken)
    {
        var dte = await _repository.GetByIdAsync(new DteId(request.Id), cancellationToken);

        if (dte is null)
        {
            return Result.Failure<byte[]>(Error.NotFound("DTE.NotFound", $"El DTE con ID {request.Id} no fue encontrado."));
        }

        var pdfResult = await _pdfGenerator.GeneratePdfAsync(dte, cancellationToken);
        
        if (pdfResult.IsFailure)
        {
            return Result.Failure<byte[]>(pdfResult.Error);
        }

        return pdfResult.Value;
    }
}
