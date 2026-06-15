using LibroFiscal.SharedKernel.Results;

namespace LibroFiscal.Application.Abstractions.Services;

/// <summary>
/// Digital signing service interface.
/// Signs DTE JSON documents using JWS (JSON Web Signature) standard.
/// Defined in Application, implemented in Infrastructure.Integrations.Signing.
/// </summary>
public interface ISigningService
{
    /// <summary>
    /// Signs a DTE JSON document with the company's digital certificate.
    /// Returns the JWS signature string.
    /// </summary>
    Task<Result<SigningResult>> SignDocumentAsync(
        string jsonDocument,
        Guid companyId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifies a JWS signature against the original document.
    /// </summary>
    Task<Result<bool>> VerifySignatureAsync(
        string jwsSignature,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Result of a signing operation.
/// </summary>
public sealed record SigningResult(
    string JwsSignature,
    string SignedPayload,
    DateTimeOffset SignedAt);
