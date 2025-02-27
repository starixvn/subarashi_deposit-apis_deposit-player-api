namespace Fanex.DepositPlayerAPI.Domain.UnitTests.Converters;

using Domain.Converters;
using FluentAssertions;
using Newtonsoft.Json;
using NSubstitute;

public class RawJsonConverterTest
{
    private readonly RawJsonConverter _converter = new();

    [Fact]
    public void CanConvert_Should_ReturnTrue_ForStringType()
    {
        // Act
        var result = _converter.CanConvert(typeof(string));

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void CanConvert_Should_ReturnFalse_ForNonStringType()
    {
        // Act
        var result = _converter.CanConvert(typeof(int));

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void WriteJson_Should_WriteRawJsonString()
    {
        // Arrange
        var writer = Substitute.For<JsonWriter>();
        var value = "{\"key\":\"value\"}";

        // Act
        _converter.WriteJson(writer, value, new JsonSerializer());

        // Assert
        writer.Received(1).WriteRawValue(value);
    }

    [Fact]
    public void ReadJson_Should_ReturnJsonString()
    {
        // Arrange
        var json = "{\"key\":\"value\"}";
        var reader = new JsonTextReader(new StringReader(json));

        // Act
        var result = _converter.ReadJson(reader, typeof(string), null, new JsonSerializer());

        // Assert
        result.Should().Be(json);
    }
}