namespace Fanex.DepositPlayerAPI.Infrastructure.UnitTests.Utils;

using System.Globalization;
using FluentAssertions;
using Infrastructure.Utils;

public class ConverterTest
{
    [Theory]
    [InlineData("123.45", 123.45)]
    [InlineData("invalid", 0)]
    [InlineData(null, 0)]
    public void ToDecimal_Should_ReturnExpectedResult(object value, decimal expectedResult)
    {
        // Act
        var result = Converter.ToDecimal(value);

        // Assert
        result.Should().Be(expectedResult);
    }

    [Theory]
    [InlineData("true", true)]
    [InlineData("false", false)]
    [InlineData("invalid", false)]
    [InlineData(null, false)]
    public void ToBoolean_Should_ReturnExpectedResult(object value, bool expectedResult)
    {
        // Act
        var result = Converter.ToBoolean(value);

        // Assert
        result.Should().Be(expectedResult);
    }

    [Theory]
    [InlineData("123", (byte)123)]
    [InlineData("invalid", (byte)0)]
    [InlineData(null, (byte)0)]
    public void ToByte_Should_ReturnExpectedResult(object value, byte expectedResult)
    {
        // Act
        var result = Converter.ToByte(value);

        // Assert
        result.Should().Be(expectedResult);
    }

    [Theory]
    [InlineData("123", 123)]
    [InlineData("invalid", 0)]
    [InlineData(null, 0)]
    public void ToInt_Should_ReturnExpectedResult(object value, int expectedResult)
    {
        // Act
        var result = Converter.ToInt(value);

        // Assert
        result.Should().Be(expectedResult);
    }

    [Theory]
    [InlineData("123", 123)]
    [InlineData("invalid", null)]
    [InlineData(null, null)]
    public void ToNullableInt_Should_ReturnExpectedResult(object value, int? expectedResult)
    {
        // Act
        var result = Converter.ToNullableInt(value);

        // Assert
        result.Should().Be(expectedResult);
    }

    [Theory]
    [InlineData("1234567890123", 1234567890123L)]
    [InlineData("invalid", 0L)]
    [InlineData(null, 0L)]
    public void ToLong_Should_ReturnExpectedResult(object value, long expectedResult)
    {
        // Act
        var result = Converter.ToLong(value);

        // Assert
        result.Should().Be(expectedResult);
    }

    [Theory]
    [InlineData("1234567890123", 1234567890123L)]
    [InlineData("invalid", null)]
    [InlineData(null, null)]
    public void ToNullableLong_Should_ReturnExpectedResult(object value, long? expectedResult)
    {
        // Act
        var result = Converter.ToNullableLong(value);

        // Assert
        result.Should().Be(expectedResult);
    }

    [Theory]
    [InlineData("test", "test")]
    [InlineData(null, "")]
    public void ToString_Should_ReturnExpectedResult(object value, string expectedResult)
    {
        // Act
        var result = Converter.ToString(value);

        // Assert
        result.Should().Be(expectedResult);
    }

    [Theory]
    [InlineData("2025-02-27", "2025-02-27")]
    [InlineData("invalid", "0001-01-01")]
    [InlineData(null, "0001-01-01")]
    public void ToDateTime_Should_ReturnExpectedResult(object value, string expectedResult)
    {
        // Act
        var result = Converter.ToDateTime(value);

        // Assert
        result.Should().Be(DateTime.Parse(expectedResult, CultureInfo.InvariantCulture));
    }
}