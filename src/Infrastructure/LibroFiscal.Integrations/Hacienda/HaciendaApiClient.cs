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

    public async Task<Result<IEnumerable<HaciendaReceivedDte>>> QueryReceivedDtesAsync(
        string authToken,
        string nitReceptor,
        DateTimeOffset startDate,
        DateTimeOffset endDate,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Consultando metadatos DTEs recibidos para NIT {Nit} desde {Start} hasta {End}", nitReceptor, startDate, endDate);

        try
        {
            await Task.Delay(1500, cancellationToken); // Simulación MH

            var fakeDte1 = new HaciendaReceivedDte(
                "12345678-1234-1234-1234-123456789012",
                "DTE-03-M001-000000000000001",
                $"SELLO-{Guid.NewGuid():N}".Substring(0, 15).ToUpperInvariant(),
                DateTimeOffset.UtcNow.AddDays(-2),
                "03",
                "PROVEEDOR SIMULADO S.A. DE C.V.",
                "06140101891011",
                113.00m
            );

            var fakeDte2 = new HaciendaReceivedDte(
                "87654321-4321-4321-4321-210987654321",
                "DTE-03-M001-000000000000002",
                $"SELLO-{Guid.NewGuid():N}".Substring(0, 15).ToUpperInvariant(),
                DateTimeOffset.UtcNow.AddDays(-1),
                "03",
                "OTRO PROVEEDOR S.A.",
                "06141212121011",
                226.00m
            );

            var results = new List<HaciendaReceivedDte> { fakeDte1, fakeDte2 };
            
            _logger.LogInformation("Consulta exitosa. Se obtuvieron metadatos de {Count} DTEs", results.Count);
            return Result.Success<IEnumerable<HaciendaReceivedDte>>(results);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al consultar metadatos DTEs en Hacienda");
            return Result.Failure<IEnumerable<HaciendaReceivedDte>>(Error.External("Hacienda.SyncFailed", "Falló la consulta de documentos al Ministerio de Hacienda."));
        }
    }

    public async Task<Result<string>> DownloadDteJsonAsync(
        string authToken,
        string codigoGeneracion,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Descargando JSON para DTE {CodigoGeneracion}", codigoGeneracion);

        try
        {
            await Task.Delay(500, cancellationToken); // Simulación descarga

            // Simular JSON del MH basado en el código
            var monto = codigoGeneracion.StartsWith("1234", StringComparison.Ordinal) ? 113.00m : 226.00m;
            var numControl = codigoGeneracion.StartsWith("1234", StringComparison.Ordinal) ? "DTE-03-M001-000000000000001" : "DTE-03-M001-000000000000002";
            
            var fakeJson = $$"""
            {
                "identificacion": { "version": 1, "ambiente": "00", "tipoDte": "03", "numeroControl": "{{numControl}}", "codigoGeneracion": "{{codigoGeneracion}}" },
                "emisor": { "nit": "06140101891011", "nrc": "123456-7", "nombre": "PROVEEDOR SIMULADO S.A. DE C.V." },
                "receptor": { "nit": "00000000000000", "nrc": "000000-0", "nombre": "NUESTRA EMPRESA S.A. DE C.V." },
                "resumen": { "totalSujetoRetencion": 0.0, "totalIVARetenido": 0.0, "totalPagar": {{monto.ToString("F2", System.Globalization.CultureInfo.InvariantCulture)}}, "totalGravada": {{(monto / 1.13m).ToString("F2", System.Globalization.CultureInfo.InvariantCulture)}}, "tributos": [{"codigo": "20", "descripcion": "IVA", "valor": {{(monto - (monto / 1.13m)).ToString("F2", System.Globalization.CultureInfo.InvariantCulture)}}}] }
            }
            """;
            
            return Result.Success(fakeJson);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al descargar JSON de DTE");
            return Result.Failure<string>(Error.External("Hacienda.DownloadFailed", "Falló la descarga del DTE."));
        }
    }
}
