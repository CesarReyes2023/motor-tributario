using LibroFiscal.SharedKernel.Interfaces;

namespace LibroFiscal.Observability;

/// <summary>
/// Production implementation of IDateTimeProvider.
/// Returns actual UTC time. Tests can substitute with a controllable implementation.
/// </summary>
public sealed class SystemDateTimeProvider : IDateTimeProvider
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
