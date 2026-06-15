using LibroFiscal.Application.Abstractions.Messaging;
using LibroFiscal.Domain.Common.Enumerations;
using LibroFiscal.Domain.Common.Ids;
using LibroFiscal.Domain.Common.ValueObjects;
using LibroFiscal.Domain.Companies.Entities;
using LibroFiscal.SharedKernel.Interfaces;
using LibroFiscal.SharedKernel.Primitives;
using LibroFiscal.SharedKernel.Results;

namespace LibroFiscal.Application.Companies.Commands.CreateCompany;

public sealed class CreateCompanyCommandHandler : ICommandHandler<CreateCompanyCommand, Guid>
{
    private readonly IRepository<Company, CompanyId> _companyRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateCompanyCommandHandler(
        IRepository<Company, CompanyId> companyRepository,
        IUnitOfWork unitOfWork)
    {
        _companyRepository = companyRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(CreateCompanyCommand request, CancellationToken cancellationToken)
    {
        var nitResult = Nit.Create(request.Nit);
        var nrcResult = Nrc.Create(request.Nrc);
        var direccionResult = DireccionFiscal.Create(request.Departamento, request.Municipio, request.ComplementoDireccion);
        var ambiente = Enumeration.TryFromId<AmbienteHacienda>(request.AmbienteHaciendaId);

        if (nitResult.IsFailure) return Result.Failure<Guid>(nitResult.Error);
        if (nrcResult.IsFailure) return Result.Failure<Guid>(nrcResult.Error);
        if (direccionResult.IsFailure) return Result.Failure<Guid>(direccionResult.Error);
        if (ambiente is null) return Result.Failure<Guid>(Error.Validation("Company.InvalidAmbiente", "Ambiente de Hacienda inválido."));

        var companyResult = Company.Create(
            request.RazonSocial,
            request.NombreComercial,
            nitResult.Value,
            nrcResult.Value,
            request.CodigoActividad,
            request.DescripcionActividad,
            request.Telefono,
            request.Correo,
            direccionResult.Value,
            ambiente);

        if (companyResult.IsFailure)
        {
            return Result.Failure<Guid>(companyResult.Error);
        }

        _companyRepository.Add(companyResult.Value);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return companyResult.Value.Id.Value;
    }
}
