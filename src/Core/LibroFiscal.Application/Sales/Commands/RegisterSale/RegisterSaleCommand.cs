using LibroFiscal.Domain.Common.Ids;
using LibroFiscal.Domain.Sales.Entities;
using LibroFiscal.SharedKernel.Interfaces;
using LibroFiscal.SharedKernel.Results;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LibroFiscal.Application.Sales.Commands.RegisterSale;

public sealed record RegisterSaleCommand(
    Guid CompanyId,
    string CustomerNit,
    string CustomerNrc,
    string CustomerName,
    DateTimeOffset IssueDate,
    string DocumentNumber,
    decimal TaxableAmount,
    decimal ExemptAmount,
    decimal TaxAmount,
    decimal TotalAmount) : IRequest<Result<Guid>>;

internal sealed class RegisterSaleCommandHandler : IRequestHandler<RegisterSaleCommand, Result<Guid>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRepository<Sale, SaleId> _saleRepository;

    public RegisterSaleCommandHandler(IUnitOfWork unitOfWork, IRepository<Sale, SaleId> saleRepository)
    {
        _unitOfWork = unitOfWork;
        _saleRepository = saleRepository;
    }

    public async Task<Result<Guid>> Handle(RegisterSaleCommand request, CancellationToken cancellationToken)
    {
        var companyId = CompanyId.From(request.CompanyId);

        var saleResult = Sale.Create(
            companyId,
            request.CustomerNit,
            request.CustomerNrc,
            request.CustomerName,
            request.IssueDate,
            request.DocumentNumber,
            request.TaxableAmount,
            request.ExemptAmount,
            request.TaxAmount,
            request.TotalAmount);

        if (saleResult.IsFailure)
            return saleResult.Error;

        _saleRepository.Add(saleResult.Value);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return saleResult.Value.Id.Value;
    }
}
