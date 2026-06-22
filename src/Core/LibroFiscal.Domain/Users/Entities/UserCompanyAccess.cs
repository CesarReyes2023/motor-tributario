#pragma warning disable CS8618 // EF Core constructor bindings

using LibroFiscal.Domain.Common.Ids;

namespace LibroFiscal.Domain.Users.Entities;

/// <summary>
/// Representa el acceso puente (Many-to-Many) entre un Usuario (Contador) y una Empresa (Cliente).
/// Los SuperAdmin no necesitan esta tabla porque tienen acceso a todas por defecto.
/// </summary>
public sealed class UserCompanyAccess
{
    public LibroFiscal.Domain.Users.Ids.UserId UserId { get; private set; }
    public CompanyId CompanyId { get; private set; }

    private UserCompanyAccess() { } // EF Core

    public static UserCompanyAccess Create(LibroFiscal.Domain.Users.Ids.UserId userId, CompanyId companyId)
    {
        return new UserCompanyAccess
        {
            UserId = userId,
            CompanyId = companyId
        };
    }
}
