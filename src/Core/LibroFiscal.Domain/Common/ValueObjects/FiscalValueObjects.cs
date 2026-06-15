#pragma warning disable CS8618 // EF Core constructor bindings

using System.Text.RegularExpressions;
using LibroFiscal.SharedKernel.Primitives;
using LibroFiscal.SharedKernel.Results;

#pragma warning disable CA1036 // Override comparison operators — ValueObject base handles == and != via structural equality; IComparable is for ordering only

namespace LibroFiscal.Domain.Common.ValueObjects;

/// <summary>
/// NIT (Número de Identificación Tributaria) de El Salvador.
/// Format: XXXX-XXXXXX-XXX-X (14 digits with dashes) or 14 digits without dashes.
/// Immutable value object with format validation.
/// </summary>
public sealed class Nit : ValueObject
{
    /// <summary>NIT value normalized without dashes (14 digits).</summary>
    public string Value { get; }

    /// <summary>NIT formatted with dashes: XXXX-XXXXXX-XXX-X.</summary>
    public string Formatted =>
        $"{Value[..4]}-{Value[4..10]}-{Value[10..13]}-{Value[13]}";

    private Nit() { } // EF Core

    private Nit(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a NIT from a raw string, validating format.
    /// </summary>
    public static Result<Nit> Create(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Error.Validation("Nit.Empty", "El NIT no puede estar vacío.");

        string normalized = value.Replace("-", "");

        if (normalized.Length != 14 || !normalized.All(char.IsDigit))
            return Error.Validation("Nit.InvalidFormat", $"El NIT '{value}' debe contener exactamente 14 dígitos.");

        return new Nit(normalized);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Formatted;
}

/// <summary>
/// NRC (Número de Registro de Contribuyente) de El Salvador.
/// Format: XXXXXX-X (6 digits, dash, 1 digit).
/// </summary>
public sealed class Nrc : ValueObject
{
    public string Value { get; }

    public string Formatted =>
        Value.Length == 7 ? $"{Value[..6]}-{Value[6]}" : Value;

    private Nrc() { } // EF Core

    private Nrc(string value)
    {
        Value = value;
    }

    public static Result<Nrc> Create(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Error.Validation("Nrc.Empty", "El NRC no puede estar vacío.");

        string normalized = value.Replace("-", "");

        if (normalized.Length < 2 || !normalized.All(char.IsDigit))
            return Error.Validation("Nrc.InvalidFormat", $"El NRC '{value}' tiene un formato inválido.");

        return new Nrc(normalized);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Formatted;
}

/// <summary>
/// Período fiscal representado por Año y Mes.
/// Inmutable. Utilizado para libros IVA, declaraciones y reportes periódicos.
/// </summary>
public sealed class FiscalPeriod : ValueObject, IComparable<FiscalPeriod>
{
    public int Year { get; }
    public int Month { get; }

    private FiscalPeriod() { } // EF Core

    private FiscalPeriod(int year, int month)
    {
        Year = year;
        Month = month;
    }

    public static Result<FiscalPeriod> Create(int year, int month)
    {
        if (year < 2000 || year > 2100)
            return Error.Validation("FiscalPeriod.InvalidYear", $"El año {year} está fuera de rango válido.");

        if (month < 1 || month > 12)
            return Error.Validation("FiscalPeriod.InvalidMonth", $"El mes {month} debe estar entre 1 y 12.");

        return new FiscalPeriod(year, month);
    }

    /// <summary>
    /// Creates a FiscalPeriod from a date, extracting year and month.
    /// </summary>
    public static FiscalPeriod FromDate(DateTimeOffset date) =>
        new(date.Year, date.Month);

    /// <summary>
    /// Returns the first day of this fiscal period.
    /// </summary>
    public DateTimeOffset StartDate =>
        new(new DateTime(Year, Month, 1, 0, 0, 0, DateTimeKind.Utc));

    /// <summary>
    /// Returns the last day of this fiscal period.
    /// </summary>
    public DateTimeOffset EndDate =>
        new(new DateTime(Year, Month, DateTime.DaysInMonth(Year, Month), 23, 59, 59, DateTimeKind.Utc));

    /// <summary>
    /// Returns the next fiscal period.
    /// </summary>
    public FiscalPeriod Next()
    {
        if (Month == 12)
            return new FiscalPeriod(Year + 1, 1);
        return new FiscalPeriod(Year, Month + 1);
    }

    /// <summary>
    /// Returns the previous fiscal period.
    /// </summary>
    public FiscalPeriod Previous()
    {
        if (Month == 1)
            return new FiscalPeriod(Year - 1, 12);
        return new FiscalPeriod(Year, Month - 1);
    }

    public int CompareTo(FiscalPeriod? other)
    {
        if (other is null) return 1;
        int yearComparison = Year.CompareTo(other.Year);
        return yearComparison != 0 ? yearComparison : Month.CompareTo(other.Month);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Year;
        yield return Month;
    }

    public override string ToString() => $"{Year:D4}-{Month:D2}";
}

/// <summary>
/// Represents a monetary amount with its currency.
/// Ensures decimal precision for fiscal calculations.
/// </summary>
public sealed class Money : ValueObject, IComparable<Money>
{
    public decimal Amount { get; }
    public string Currency { get; }

    private Money() { } // EF Core

    private Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    public static Money Usd(decimal amount) => new(Math.Round(amount, 2), "USD");

    public static Result<Money> Create(decimal amount, string currency)
    {
        if (string.IsNullOrWhiteSpace(currency))
            return Error.Validation("Money.InvalidCurrency", "La moneda no puede estar vacía.");

        return new Money(Math.Round(amount, 2), currency.ToUpperInvariant());
    }

    public static Money Zero(string currency = "USD") => new(0m, currency);

    public Money Add(Money other)
    {
        EnsureSameCurrency(other);
        return new Money(Amount + other.Amount, Currency);
    }

    public Money Subtract(Money other)
    {
        EnsureSameCurrency(other);
        return new Money(Amount - other.Amount, Currency);
    }

    public Money Multiply(decimal factor) =>
        new(Math.Round(Amount * factor, 2), Currency);

    public int CompareTo(Money? other)
    {
        if (other is null) return 1;
        EnsureSameCurrency(other);
        return Amount.CompareTo(other.Amount);
    }

    public static Money operator +(Money left, Money right) => left.Add(right);
    public static Money operator -(Money left, Money right) => left.Subtract(right);

    private void EnsureSameCurrency(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException(
                $"Cannot operate on Money with different currencies: {Currency} vs {other.Currency}.");
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }

    public override string ToString() => $"{Currency} {Amount:F2}";
}

/// <summary>
/// Dirección fiscal de El Salvador.
/// </summary>
public sealed class DireccionFiscal : ValueObject
{
    public string Departamento { get; private set; }
    public string Municipio { get; private set; }
    public string Complemento { get; private set; }

    private DireccionFiscal() { } // EF Core

    private DireccionFiscal(string departamento, string municipio, string complemento)
    {
        Departamento = departamento;
        Municipio = municipio;
        Complemento = complemento;
    }

    public static Result<DireccionFiscal> Create(string departamento, string municipio, string complemento)
    {
        if (string.IsNullOrWhiteSpace(departamento))
            return Error.Validation("Direccion.DepartamentoVacio", "El departamento es obligatorio.");
        if (string.IsNullOrWhiteSpace(municipio))
            return Error.Validation("Direccion.MunicipioVacio", "El municipio es obligatorio.");
        if (string.IsNullOrWhiteSpace(complemento))
            return Error.Validation("Direccion.ComplementoVacio", "El complemento de dirección es obligatorio.");

        return new DireccionFiscal(departamento, municipio, complemento);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Departamento;
        yield return Municipio;
        yield return Complemento;
    }

    public override string ToString() => $"{Complemento}, {Municipio}, {Departamento}";
}

/// <summary>
/// Número de Control del DTE. Formato: DTE-{TipoDte}-{CodigoEstablecimiento}-{PuntoVenta}-{Correlativo}
/// Ejemplo: DTE-01-0001-000-000000000000001
/// </summary>
public sealed class NumeroControl : ValueObject
{
    public string Value { get; }

    private NumeroControl() { } // EF Core

    private NumeroControl(string value)
    {
        Value = value;
    }

    public static Result<NumeroControl> Create(string tipoDteCodigo, string codigoEstablecimiento, string puntoVenta, long correlativo)
    {
        if (string.IsNullOrWhiteSpace(tipoDteCodigo))
            return Error.Validation("NumeroControl.TipoDteVacio", "El código de tipo DTE es obligatorio.");

        string value = $"DTE-{tipoDteCodigo}-{codigoEstablecimiento.PadLeft(4, '0')}-{puntoVenta.PadLeft(3, '0')}-{correlativo:D15}";
        return new NumeroControl(value);
    }

    public static Result<NumeroControl> FromRaw(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Error.Validation("NumeroControl.Vacio", "El número de control no puede estar vacío.");
        return new NumeroControl(value);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
