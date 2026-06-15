using FluentValidation;

namespace LibroFiscal.Application.Companies.Commands.CreateCompany;

public sealed class CreateCompanyCommandValidator : AbstractValidator<CreateCompanyCommand>
{
    public CreateCompanyCommandValidator()
    {
        RuleFor(x => x.RazonSocial).NotEmpty().MaximumLength(200);
        RuleFor(x => x.NombreComercial).MaximumLength(200);
        RuleFor(x => x.Nit).NotEmpty();
        RuleFor(x => x.Nrc).NotEmpty();
        RuleFor(x => x.CodigoActividad).NotEmpty().MaximumLength(10);
        RuleFor(x => x.DescripcionActividad).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Departamento).NotEmpty().Length(2);
        RuleFor(x => x.Municipio).NotEmpty().Length(2);
        RuleFor(x => x.ComplementoDireccion).NotEmpty().MaximumLength(200);
    }
}
