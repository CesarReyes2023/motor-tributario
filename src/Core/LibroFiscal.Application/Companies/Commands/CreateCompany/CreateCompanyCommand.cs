using LibroFiscal.Application.Abstractions.Messaging;

namespace LibroFiscal.Application.Companies.Commands.CreateCompany;

public sealed record CreateCompanyCommand(
    string RazonSocial,
    string NombreComercial,
    string Nit,
    string Nrc,
    string CodigoActividad,
    string DescripcionActividad,
    string Telefono,
    string Correo,
    string Departamento,
    string Municipio,
    string ComplementoDireccion,
    int AmbienteHaciendaId) : ICommand<Guid>;
