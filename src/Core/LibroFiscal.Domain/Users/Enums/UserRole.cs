using LibroFiscal.SharedKernel.Primitives;

namespace LibroFiscal.Domain.Users.Enums;

public sealed class UserRole : Enumeration
{
    public static readonly UserRole Admin = new(1, "Administrador");
    public static readonly UserRole Operador = new(2, "Operador");
    public static readonly UserRole Auditor = new(3, "Auditor");

    private UserRole(int id, string name) : base(id, name)
    {
    }
}
