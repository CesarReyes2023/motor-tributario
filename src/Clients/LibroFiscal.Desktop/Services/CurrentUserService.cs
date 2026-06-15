using LibroFiscal.Application.Abstractions.Services;

namespace LibroFiscal.Desktop.Services;

public sealed class CurrentUserService : ICurrentUserService
{
    public Guid? UserId { get; private set; }
    public string? Username { get; private set; }

    public void SetUser(Guid userId, string username)
    {
        UserId = userId;
        Username = username;
    }

    public void Clear()
    {
        UserId = null;
        Username = null;
    }
}
