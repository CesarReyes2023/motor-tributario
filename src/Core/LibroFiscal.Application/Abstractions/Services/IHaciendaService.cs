using LibroFiscal.SharedKernel.Results;

namespace LibroFiscal.Application.Abstractions.Services;

/// <summary>
/// Hacienda integration service interface (Anti-Corruption Layer boundary).
/// Isolates the domain from Hacienda API instability.
/// Defined in Application, implemented in Infrastructure.Integrations.Hacienda.
/// </summary>
public interface IHaciendaService
{
    /// <summary>
    /// Authenticates with Hacienda API and obtains a token.
    /// </summary>
    Task<Result<HaciendaAuthToken>> AuthenticateAsync(
        Guid companyId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Transmits a signed DTE document to Hacienda.
    /// Returns the reception seal on success.
    /// </summary>
    Task<Result<HaciendaTransmissionResult>> TransmitDteAsync(
        string signedDocument,
        string authToken,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks the status of a previously transmitted DTE.
    /// </summary>
    Task<Result<HaciendaDteStatus>> CheckDteStatusAsync(
        string codigoGeneracion,
        string authToken,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if Hacienda API is currently available.
    /// </summary>
    Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Queries the API for all DTEs received within a date range (Metadata).
    /// </summary>
    Task<Result<IEnumerable<HaciendaReceivedDte>>> QueryReceivedDtesAsync(
        string authToken,
        string nitReceptor,
        DateTimeOffset startDate,
        DateTimeOffset endDate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Downloads the specific JSON of a received DTE.
    /// </summary>
    Task<Result<string>> DownloadDteJsonAsync(
        string authToken,
        string codigoGeneracion,
        CancellationToken cancellationToken = default);
}

public sealed record HaciendaAuthToken(string Token, DateTimeOffset ExpiresAt);

public sealed record HaciendaTransmissionResult(
    string SelloRecepcion,
    string Estado,
    DateTimeOffset FechaRecepcion);

public sealed record HaciendaDteStatus(
    string CodigoGeneracion,
    string Estado,
    string? SelloRecepcion,
    string? MotivoRechazo);

public sealed record HaciendaReceivedDte(
    string CodigoGeneracion,
    string NumeroControl,
    string SelloRecepcion,
    DateTimeOffset FechaEmision,
    string TipoDte,
    string NombreEmisor,
    string NitEmisor,
    decimal MontoTotal);
