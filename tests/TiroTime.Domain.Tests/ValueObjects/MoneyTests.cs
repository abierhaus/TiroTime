using TiroTime.Domain.Exceptions;
using TiroTime.Domain.ValueObjects;

namespace TiroTime.Domain.Tests.ValueObjects;

public class MoneyTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateMoney()
    {
        // Act
        var money = Money.Create(100m, "EUR");

        // Assert
        Assert.NotNull(money);
        Assert.Equal(100m, money.Amount);
        Assert.Equal("EUR", money.Currency);
    }

    [Fact]
    public void Create_WithoutCurrency_ShouldDefaultToEUR()
    {
        // Act
        var money = Money.Create(100m);

        // Assert
        Assert.Equal("EUR", money.Currency);
    }

    [Fact]
    public void Create_WithLowercaseCurrency_ShouldConvertToUppercase()
    {
        // Act
        var money = Money.Create(100m, "usd");

        // Assert
        Assert.Equal("USD", money.Currency);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithEmptyCurrency_ShouldThrowDomainException(string invalidCurrency)
    {
        // Act & Assert
        var exception = Assert.Throws<DomainException>(() =>
            Money.Create(100m, invalidCurrency));

        Assert.Contains("Currency cannot be empty", exception.Message);
    }

    [Theory]
    [InlineData("EU")]
    [InlineData("EURO")]
    public void Create_WithInvalidCurrencyLength_ShouldThrowDomainException(string invalidCurrency)
    {
        // Act & Assert
        var exception = Assert.Throws<DomainException>(() =>
            Money.Create(100m, invalidCurrency));

        Assert.Contains("Currency must be a 3-letter ISO code", exception.Message);
    }

    [Fact]
    public void Zero_ShouldReturnZeroEuro()
    {
        // Act
        var money = Money.Zero;

        // Assert
        Assert.Equal(0m, money.Amount);
        Assert.Equal("EUR", money.Currency);
    }

    [Fact]
    public void FromEuro_ShouldCreateEuroMoney()
    {
        // Act
        var money = Money.FromEuro(50m);

        // Assert
        Assert.Equal(50m, money.Amount);
        Assert.Equal("EUR", money.Currency);
    }

    [Fact]
    public void Add_WithSameCurrency_ShouldAddAmounts()
    {
        // Arrange
        var money1 = Money.FromEuro(100m);
        var money2 = Money.FromEuro(50m);

        // Act
        var result = money1.Add(money2);

        // Assert
        Assert.Equal(150m, result.Amount);
        Assert.Equal("EUR", result.Currency);
    }

    [Fact]
    public void Add_WithDifferentCurrencies_ShouldThrowDomainException()
    {
        // Arrange
        var euro = Money.Create(100m, "EUR");
        var dollar = Money.Create(100m, "USD");

        // Act & Assert
        var exception = Assert.Throws<DomainException>(() =>
            euro.Add(dollar));

        Assert.Contains("Cannot add money with different currencies", exception.Message);
    }

    [Fact]
    public void Subtract_WithSameCurrency_ShouldSubtractAmounts()
    {
        // Arrange
        var money1 = Money.FromEuro(100m);
        var money2 = Money.FromEuro(30m);

        // Act
        var result = money1.Subtract(money2);

        // Assert
        Assert.Equal(70m, result.Amount);
        Assert.Equal("EUR", result.Currency);
    }

    [Fact]
    public void Subtract_WithDifferentCurrencies_ShouldThrowDomainException()
    {
        // Arrange
        var euro = Money.Create(100m, "EUR");
        var dollar = Money.Create(50m, "USD");

        // Act & Assert
        var exception = Assert.Throws<DomainException>(() =>
            euro.Subtract(dollar));

        Assert.Contains("Cannot subtract money with different currencies", exception.Message);
    }

    [Fact]
    public void Multiply_ShouldMultiplyAmount()
    {
        // Arrange
        var money = Money.FromEuro(100m);

        // Act
        var result = money.Multiply(1.5m);

        // Assert
        Assert.Equal(150m, result.Amount);
        Assert.Equal("EUR", result.Currency);
    }

    [Fact]
    public void OperatorPlus_ShouldAddMoney()
    {
        // Arrange
        var money1 = Money.FromEuro(100m);
        var money2 = Money.FromEuro(50m);

        // Act
        var result = money1 + money2;

        // Assert
        Assert.Equal(150m, result.Amount);
    }

    [Fact]
    public void OperatorMinus_ShouldSubtractMoney()
    {
        // Arrange
        var money1 = Money.FromEuro(100m);
        var money2 = Money.FromEuro(30m);

        // Act
        var result = money1 - money2;

        // Assert
        Assert.Equal(70m, result.Amount);
    }

    [Fact]
    public void OperatorMultiply_ShouldMultiplyMoney()
    {
        // Arrange
        var money = Money.FromEuro(100m);

        // Act
        var result = money * 2m;

        // Assert
        Assert.Equal(200m, result.Amount);
    }

    [Fact]
    public void OperatorMultiply_Reversed_ShouldMultiplyMoney()
    {
        // Arrange
        var money = Money.FromEuro(100m);

        // Act
        var result = 2m * money;

        // Assert
        Assert.Equal(200m, result.Amount);
    }

    [Fact]
    public void Equals_WithSameValues_ShouldReturnTrue()
    {
        // Arrange
        var money1 = Money.FromEuro(100m);
        var money2 = Money.FromEuro(100m);

        // Act & Assert
        Assert.Equal(money1, money2);
        Assert.True(money1 == money2);
    }

    [Fact]
    public void Equals_WithDifferentAmounts_ShouldReturnFalse()
    {
        // Arrange
        var money1 = Money.FromEuro(100m);
        var money2 = Money.FromEuro(50m);

        // Act & Assert
        Assert.NotEqual(money1, money2);
        Assert.False(money1 == money2);
    }

    [Fact]
    public void Equals_WithDifferentCurrencies_ShouldReturnFalse()
    {
        // Arrange
        var euro = Money.Create(100m, "EUR");
        var dollar = Money.Create(100m, "USD");

        // Act & Assert
        Assert.NotEqual(euro, dollar);
    }

    [Fact]
    public void ToString_ShouldFormatCorrectly()
    {
        // Arrange
        var money = Money.Create(1234.56m, "EUR");

        // Act
        var result = money.ToString();

        // Assert
        // Check that it contains the currency and decimal parts
        // Format can be culture-specific (1,234.56 or 1.234,56)
        Assert.Contains("EUR", result);
        Assert.Contains("56", result); // decimal part should be present
        Assert.True(result.Contains("234") || result.Contains("1234")); // Either with or without thousand separator
    }
}
