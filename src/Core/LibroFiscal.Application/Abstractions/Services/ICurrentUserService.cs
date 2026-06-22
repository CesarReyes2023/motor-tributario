namespace LibroFiscal.Application.Abstractions.Services;

public interface ICurrentUserService
{
    Guid? UserId { get; }
    string? Username { get; }
    string? ProfilePicturePath { get; }
    string? Role { get; }
    
    event EventHandler? ProfilePictureChanged;

    void SetUser(Guid userId, string username, string? profilePicturePath, string role);
    void UpdateProfilePicturePath(string? path);
    void Clear();
}
