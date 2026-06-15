#pragma warning disable CA1848 // Logger warnings disable for MVP

using System;
using System.Threading;
using System.Threading.Tasks;
using LibroFiscal.Application.Abstractions.Services;
using LibroFiscal.SharedKernel.Results;
using Microsoft.Extensions.Logging;

namespace LibroFiscal.Integrations.Signing;

public sealed class FirmadorService : ISigningService
{
    private readonly ILogger<FirmadorService> _logger;

    public FirmadorService(ILogger<FirmadorService> logger)
    {
        _logger = logger;
    }

    public async Task<Result<SigningResult>> SignDocumentAsync(string jsonDocument, Guid companyId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Firmando documento JSON para la empresa {CompanyId}", companyId);

        try
        {
            // Simular proceso del Firmador del Ministerio de Hacienda (JWS)
            await Task.Delay(300, cancellationToken);

            var base64Doc = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(jsonDocument));
            var truncatedBase64 = base64Doc.Length > 50 ? base64Doc.Substring(0, 50) : base64Doc;
            var fakeJws = string.Concat("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.", truncatedBase64, ".fake_signature_hash");

            var result = new SigningResult(
                JwsSignature: fakeJws,
                SignedPayload: jsonDocument,
                SignedAt: DateTimeOffset.UtcNow
            );

            _logger.LogInformation("Documento firmado exitosamente con JWS.");
            return Result.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al firmar documento");
            return Result.Failure<SigningResult>(Error.External("Signing.Failed", "No se pudo firmar el documento JSON con el Firmador."));
        }
    }

    public Task<Result<bool>> VerifySignatureAsync(string jwsSignature, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
