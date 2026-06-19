using System;
using System.IO;
using LibroFiscal.Application.Abstractions.Services;

namespace LibroFiscal.Desktop.Services;

/// <summary>
/// Centralized error logger that writes all errors to %LOCALAPPDATA%/LibroFiscal/Logs/.
/// Thread-safe via lock. This is the ONLY place in the app that writes error files.
/// </summary>
public sealed class ErrorLogger : IErrorLogger
{
    private static readonly string LogsDirectory = System.IO.Path.Combine(
        System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData),
        "LibroFiscal", "Logs");

    private static readonly object _lock = new object();

    public void LogError(string context, string message)
    {
        WriteLog(context, message);
    }

    public void LogError(string context, Exception exception)
    {
        var text = $"{exception.Message}\nStack Trace: {exception.StackTrace}";
        if (exception.InnerException != null)
            text += $"\nInner Exception: {exception.InnerException.Message}";
        WriteLog(context, text);
    }

    private static void WriteLog(string context, string content)
    {
        try
        {
            lock (_lock)
            {
                System.IO.Directory.CreateDirectory(LogsDirectory);
                var fileName = $"error_{context}_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
                var filePath = System.IO.Path.Combine(LogsDirectory, fileName);
                System.IO.File.WriteAllText(filePath, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Context: {context}\n{content}");
            }
        }
        catch
        {
            // Last resort — don't let logging crash the app
            System.Diagnostics.Debug.WriteLine($"[ErrorLogger] Failed to write log for context: {context}");
        }
    }
}
