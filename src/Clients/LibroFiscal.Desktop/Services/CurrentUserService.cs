using LibroFiscal.Application.Abstractions.Services;

namespace LibroFiscal.Desktop.Services;

public sealed class CurrentUserService : ICurrentUserService
{
    public Guid? UserId { get; private set; }
    public string? Username { get; private set; }
    public string? ProfilePicturePath { get; private set; }
    public string? Role { get; private set; }

    public event EventHandler? ProfilePictureChanged;

    public void SetUser(Guid userId, string username, string? profilePicturePath, string role)
    {
        UserId = userId;
        Username = username;
        ProfilePicturePath = profilePicturePath;
        Role = role;
        ProfilePictureChanged?.Invoke(this, EventArgs.Empty);
    }

    public void UpdateProfilePicturePath(string? path)
    {
        ProfilePicturePath = path;
        ProfilePictureChanged?.Invoke(this, EventArgs.Empty);
    }

    public void Clear()
    {
        UserId = null;
        Username = null;
        ProfilePicturePath = null;
        Role = null;
        ProfilePictureChanged?.Invoke(this, EventArgs.Empty);
    }
}
