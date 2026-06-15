namespace LibroFiscal.Application.Abstractions.Services;

public interface ICurrentUserService
{
    Guid? UserId { get; }
    string? Username { get; }
    void SetUser(Guid userId, string username);
    void Clear();
}
