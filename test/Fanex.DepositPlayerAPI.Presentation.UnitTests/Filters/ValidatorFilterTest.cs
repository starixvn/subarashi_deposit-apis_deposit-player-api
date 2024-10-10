namespace Fanex.DepositPlayerAPI.Presentation.UnitTests.Filters;

using System.ComponentModel.DataAnnotations;
using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using Presentation.Filters;

public class ValidatorFilterTest
{
    private readonly EndpointFilterDelegate _filterDelegate = Substitute.For<EndpointFilterDelegate>();
    private readonly IFixture _fixture = new Fixture();
    private readonly EndpointFilterInvocationContext _invocationContext = Substitute.For<EndpointFilterInvocationContext>();
    private readonly IEndpointFilter _validatorFilter = new ValidatorFilter<TestEntity>();

    [Fact]
    public async Task ValidateRequestParameters_Should_ReturnBadRequestForNullValues()
    {
        // Arrange
        _invocationContext.Arguments.Returns(new object[] { null! });
        var expected = Results.BadRequest(new
        {
            code = 400,
            type = "INVALID_PARAMETER_VALUE",
            message = "Invalid value or required parameter list: Request parameters are null."
        });

        // Action
        var result = await _validatorFilter.InvokeAsync(_invocationContext, _filterDelegate);

        // Assert
        result.Should().BeEquivalentTo(expected, options => options.RespectingRuntimeTypes());
    }

    [Fact]
    public async Task ValidateRequestParameters_Should_ReturnBadRequestForInvalidValues()
    {
        // Arrange
        var entity = _fixture
            .Build<TestEntity>()
            .With(e => e.PropertyA, "")
            .With(e => e.PropertyB, -1)
            .Create();

        _invocationContext.Arguments.Returns(new object[] { entity });
        var expected = Results.BadRequest(new
        {
            code = 400,
            type = "INVALID_PARAMETER_VALUE",
            message = "Invalid value or required parameter list: PropertyA, PropertyB"
        });

        // Action
        var result = await _validatorFilter.InvokeAsync(_invocationContext, _filterDelegate);

        // Assert
        result.Should().BeEquivalentTo(expected, options => options.RespectingRuntimeTypes());
    }

    [Fact]
    public async Task ValidateRequestParameters_Should_ReturnBadRequestForMissingRequiredValues()
    {
        // Arrange
        var entity = new TestEntity { PropertyB = 1, PropertyC = "" };

        _invocationContext.Arguments.Returns(new object[] { entity });
        var expected = Results.BadRequest(new
        {
            code = 400,
            type = "INVALID_PARAMETER_VALUE",
            message = "Invalid value or required parameter list: PropertyA"
        });

        // Action
        var result = await _validatorFilter.InvokeAsync(_invocationContext, _filterDelegate);

        // Assert
        result.Should().BeEquivalentTo(expected, options => options.RespectingRuntimeTypes());
    }

    [Fact]
    public async Task ValidateRequestParameters_Should_CallNextDelegateForValidParams()
    {
        // Arrange
        var entity = _fixture.Create<TestEntity>();
        _invocationContext.Arguments.Returns(new object[] { entity });

        // Action
        await _validatorFilter.InvokeAsync(_invocationContext, _filterDelegate);

        // Assert
        await _filterDelegate.Received(1).Invoke(_invocationContext);
    }

    public class TestEntity
    {
        [Required(AllowEmptyStrings = false)]
        public string PropertyA { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int PropertyB { get; set; }

        public string PropertyC { get; set; }
    }
}