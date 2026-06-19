namespace LibroFiscal.Application.Abstractions.Services;

/// <summary>
/// Centralized error logger — ViewModels must NOT write to the filesystem directly.
/// All error logs are written to %LOCALAPPDATA%/LibroFiscal/Logs/.
/// </summary>
public interface IErrorLogger
{
    void LogError(string context, string message);
    void LogError(string context, Exception exception);
}
