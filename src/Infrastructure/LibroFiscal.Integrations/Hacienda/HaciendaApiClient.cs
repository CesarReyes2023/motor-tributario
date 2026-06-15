#pragma warning disable CA1848 // Logger warnings disable for MVP

using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using LibroFiscal.Application.Abstractions.Services;
using LibroFiscal.SharedKernel.Results;
using Microsoft.Extensions.Logging;

namespace LibroFiscal.Integrations.Hacienda;

public sealed class HaciendaApiClient : IHaciendaService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<HaciendaApiClient> _logger;

    public HaciendaApiClient(HttpClient httpClient, ILogger<HaciendaApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _httpClient.BaseAddress = new Uri("https://apitest.dtes.mh.gob.sv/");
    }

    public async Task<Result<HaciendaAuthToken>> AuthenticateAsync(Guid companyId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Autenticando con API de Hacienda para la empresa {CompanyId}", companyId);
        
        try
        {
            // Simulación de llamada HTTP real para el MVP
            await Task.Delay(500, cancellationToken);
            
            var fakeToken = new HaciendaAuthToken(
                Token: $"mh_token_{Guid.NewGuid():N}",
                ExpiresAt: DateTimeOffset.UtcNow.AddHours(24)
            );
            
            _logger.LogInformation("Autenticación exitosa. Token obtenido.");
            return Result.Success(fakeToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al autenticar con Hacienda");
            return Result.Failure<HaciendaAuthToken>(Error.External("Hacienda.AuthFailed", "Falló la autenticación con el Ministerio de Hacienda."));
        }
    }

    public async Task<Result<HaciendaTransmissionResult>> TransmitDteAsync(string signedDocument, string authToken, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Transmitiendo DTE a Hacienda con Token: {TokenPrefix}...", authToken.Substring(0, Math.Min(8, authToken.Length)));

        try
        {
            // Simulación de transmisión
            await Task.Delay(1000, cancellationToken);
            
            var selloBase = Guid.NewGuid().ToString().Substring(0, 15).ToUpperInvariant();
            var fakeResult = new HaciendaTransmissionResult(
                SelloRecepcion: $"SELLO-{selloBase}",
                Estado: "PROCESADO",
                FechaRecepcion: DateTimeOffset.UtcNow
            );

            _logger.LogInformation("DTE Transmitido. Sello de recepción: {Sello}", fakeResult.SelloRecepcion);
            return Result.Success(fakeResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al transmitir DTE");
            return Result.Failure<HaciendaTransmissionResult>(Error.External("Hacienda.TransmitFailed", "Falló la transmisión del DTE al servidor de Hacienda."));
        }
    }

    public Task<Result<HaciendaDteStatus>> CheckDteStatusAsync(string codigoGeneracion, string authToken, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(true);
    }
}
