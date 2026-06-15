using FluentAssertions;
using LibroFiscal.Domain.Common.ValueObjects;
using LibroFiscal.SharedKernel.Results;
using Xunit;

namespace LibroFiscal.Tests.Unit.Domain;

/// <summary>
/// Tests for fiscal value objects: NIT, NRC, FiscalPeriod, Money, NumeroControl.
/// </summary>
public class FiscalValueObjectTests
{
    [Theory]
    [InlineData("06141234567890")]
    [InlineData("0614-123456-789-0")]
    public void Nit_Create_WithValidFormats_ShouldSucceed(string rawNit)
    {
        var result = Nit.Create(rawNit);

        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().HaveLength(14);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("12345")]
    [InlineData("abcdefghijklmn")]
    public void Nit_Create_WithInvalidFormats_ShouldFail(string? rawNit)
    {
        var result = Nit.Create(rawNit);
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Nit_Formatted_ShouldReturnWithDashes()
    {
        var nit = Nit.Create("06141234567890").Value;
        nit.Formatted.Should().Be("0614-123456-789-0");
    }

    [Fact]
    public void FiscalPeriod_Create_WithValidData_ShouldSucceed()
    {
        var result = FiscalPeriod.Create(2026, 6);

        result.IsSuccess.Should().BeTrue();
        result.Value.Year.Should().Be(2026);
        result.Value.Month.Should().Be(6);
    }

    [Theory]
    [InlineData(1999, 6)]
    [InlineData(2026, 0)]
    [InlineData(2026, 13)]
    public void FiscalPeriod_Create_WithInvalidData_ShouldFail(int year, int month)
    {
        var result = FiscalPeriod.Create(year, month);
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void FiscalPeriod_Next_ShouldReturnNextMonth()
    {
        var period = FiscalPeriod.Create(2026, 12).Value;
        var next = period.Next();

        next.Year.Should().Be(2027);
        next.Month.Should().Be(1);
    }

    [Fact]
    public void Money_Usd_ShouldCreateWithTwoDecimalPlaces()
    {
        var money = Money.Usd(100.555m);
        money.Amount.Should().Be(100.56m); // Rounded
        money.Currency.Should().Be("USD");
    }

    [Fact]
    public void Money_Add_WithSameCurrency_ShouldSucceed()
    {
        var a = Money.Usd(100m);
        var b = Money.Usd(50.25m);

        var result = a + b;

        result.Amount.Should().Be(150.25m);
    }

    [Fact]
    public void NumeroControl_Create_ShouldFormatCorrectly()
    {
        var result = NumeroControl.Create("01", "0001", "001", 1);

        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be("DTE-01-0001-001-000000000000001");
    }

    [Fact]
    public void ValueObject_Equality_SameValues_ShouldBeEqual()
    {
        var nit1 = Nit.Create("06141234567890").Value;
        var nit2 = Nit.Create("0614-123456-789-0").Value;

        nit1.Should().Be(nit2); // Structural equality after normalization
    }

    [Fact]
    public void Result_Map_OnSuccess_ShouldTransformValue()
    {
        Result<int> result = 42;
        var mapped = result.Map(x => x.ToString());

        mapped.IsSuccess.Should().BeTrue();
        mapped.Value.Should().Be("42");
    }

    [Fact]
    public void Result_Map_OnFailure_ShouldPropagateError()
    {
        var error = Error.Validation("test", "test error");
        Result<int> result = error;
        var mapped = result.Map(x => x.ToString());

        mapped.IsFailure.Should().BeTrue();
        mapped.Error.Should().Be(error);
    }

    [Fact]
    public void Result_Bind_ShouldChainOperations()
    {
        var result = Nit.Create("06141234567890")
            .Bind(nit => FiscalPeriod.Create(2026, 6)
                .Map(period => $"{nit.Formatted} - {period}"));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("0614-123456-789-0 - 2026-06");
    }
}
