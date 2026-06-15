namespace LibroFiscal.SharedKernel.Interfaces;

/// <summary>
/// Provides the current date/time. Abstracted for testability.
/// In production: returns DateTimeOffset.UtcNow.
/// In tests: returns a fixed or controllable time.
/// 
/// Critical for fiscal operations where dates determine:
/// - Tax rule applicability (vigencia periods)
/// - Catalog version selection
/// - Document emission timestamps
/// - Fiscal period calculations
/// </summary>
public interface IDateTimeProvider
{
    /// <summary>
    /// Current UTC date and time.
    /// </summary>
    DateTimeOffset UtcNow { get; }
}
