using LibroFiscal.Domain.Users.Enums;
using LibroFiscal.Domain.Users.Ids;
using LibroFiscal.SharedKernel.Primitives;
using System.Collections.Generic;

namespace LibroFiscal.Domain.Users.Entities;

public sealed class User : AggregateRoot<UserId>
{
    private User(UserId id, string username, string passwordHash, UserRole role)
    {
        Id = id;
        Username = username;
        PasswordHash = passwordHash;
        Role = role;
        IsActive = true;
    }

    private User() // For EF Core
    {
    }

    public string Username { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public UserRole Role { get; private set; } = null!;
    public string? ProfilePicturePath { get; private set; }
    public bool IsActive { get; private set; }

    private readonly List<UserCompanyAccess> _companyAccesses = new();
    public IReadOnlyCollection<UserCompanyAccess> CompanyAccesses => _companyAccesses.AsReadOnly();

    public static User Create(string username, string passwordHash, UserRole role)
    {
        return new User(UserId.New(), username, passwordHash, role);
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void UpdatePassword(string passwordHash)
    {
        PasswordHash = passwordHash;
    }

    public void UpdateProfilePicture(string? profilePicturePath)
    {
        ProfilePicturePath = profilePicturePath;
    }

    public void AssignCompany(LibroFiscal.Domain.Common.Ids.CompanyId companyId)
    {
        if (!_companyAccesses.Exists(c => c.CompanyId == companyId))
        {
            _companyAccesses.Add(UserCompanyAccess.Create(Id, companyId));
        }
    }

    public void RemoveCompany(LibroFiscal.Domain.Common.Ids.CompanyId companyId)
    {
        _companyAccesses.RemoveAll(c => c.CompanyId == companyId);
    }
}
