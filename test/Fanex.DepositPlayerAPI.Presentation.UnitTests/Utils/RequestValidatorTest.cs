namespace Fanex.DepositPlayerAPI.Presentation.UnitTests.Utils;

using System.ComponentModel.DataAnnotations;
using AutoFixture;
using FluentAssertions;
using Presentation.Utils;

public class RequestValidatorTest
{
    private readonly Fixture _fixture = new();

    [Fact]
    public void IsValidRequest_WithValidRequestObject_ReturnsTrue()
    {
        // Arrange
        var validRequest = _fixture.Build<RequestObject>()
            .With(x => x.Property1, "valid_value1")
            .With(x => x.Property2, "valid_value2")
            .Create();

        // Action
        var (isValid, _) = RequestValidator.IsValidRequest(validRequest);

        // Assert
        isValid.Should().BeTrue();
    }

    [Fact]
    public void IsValidRequest_WithInvalidRequestObject_ReturnsFalse()
    {
        // Arrange
        var invalidRequest = new RequestObject
        {
            Property1 = "valid_value1",
            Property2 = null!
        };

        // Action
        var (isValid, _) = RequestValidator.IsValidRequest(invalidRequest);

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void IsValidRequest_WithInvalidRequestObject_ReturnsValidationResults()
    {
        // Arrange
        var invalidRequest = new RequestObject
        {
            Property1 = "valid_value1",
            Property2 = null!
        };

        // Action
        var (_, validationResults) = RequestValidator.IsValidRequest(invalidRequest);

        // Assert
        validationResults.Should().NotBeEmpty();
        validationResults.SelectMany(x => x.MemberNames).Should().Contain("Property2");
    }

    [Fact]
    public void IsValidRequest_WithValidRequestObject_ReturnsEmptyValidationResults()
    {
        // Arrange
        var validRequest = new RequestObject
        {
            Property1 = "valid_value1",
            Property2 = "valid_value2"
        };

        // Action
        var (_, validationResults) = RequestValidator.IsValidRequest(validRequest);

        // Assert
        validationResults.Should().BeEmpty();
    }

    [Fact]
    public void IsValidRequest_WithNullInput_ThrowError()
    {
        // Arrange
        object nullRequest = null!;

        // Action
        Action action = () => RequestValidator.IsValidRequest(nullRequest);

        // Assert
        action.Should().Throw<Exception>();
    }

    [Fact]
    public void IsValidRequest_WithNestedObjects_ValidatesAllProperties()
    {
        // Arrange
        var nestedRequest = new RequestObject
        {
            Property1 = "valid_value1",
            Property2 = "valid_value2",
            NestedObject = new NestedObject
            {
                NestedProperty = "valid_nested_value"
            }
        };

        // Action
        var (isValid, _) = RequestValidator.IsValidRequest(nestedRequest);

        // Assert
        isValid.Should().BeTrue();
    }

    public class NestedObject
    {
        [Required]
        public string NestedProperty { get; set; }
    }

    public class RequestObject
    {
        [Required]
        public string Property1 { get; set; }

        [Required]
        public string Property2 { get; set; }

        public NestedObject NestedObject { get; set; }
    }
}