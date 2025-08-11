namespace Fanex.DepositPlayerAPI.Domain.UnitTests.Results;

using AutoFixture;
using Domain.Results;
using Errors;
using FluentAssertions;

public class ServiceResultTest
{
    private readonly IFixture _fixture = new Fixture();

    [Fact]
    public void Success_Should_CreateSuccessfulResult()
    {
        // Arrange
        var extraInfo = _fixture.Create<string>();

        // Act
        var result = ServiceResult.Success(extraInfo);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Error.Should().Be(ServiceError.None);
        result.ExtraInfo.Should().Be(extraInfo);
    }

    [Fact]
    public void Success_WithoutExtraInfo_Should_CreateSuccessfulResult()
    {
        // Act
        var result = ServiceResult.Success();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Error.Should().Be(ServiceError.None);
        result.ExtraInfo.Should().BeNull();
    }

    [Fact]
    public void Failure_Should_CreateFailureResult()
    {
        // Arrange
        var error = ServiceError.SystemError;
        var extraInfo = _fixture.Create<string>();

        // Act
        var result = ServiceResult.Failure(error, extraInfo);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
        result.ExtraInfo.Should().Be(extraInfo);
    }

    [Fact]
    public void Failure_WithoutExtraInfo_Should_CreateFailureResult()
    {
        // Arrange
        var error = ServiceError.SystemError;

        // Act
        var result = ServiceResult.Failure(error);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
        result.ExtraInfo.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithSuccessTrue_AndErrorNone_Should_Succeed()
    {
        // Arrange
        var extraInfo = _fixture.Create<string>();

        // Act
        var result = ServiceResult.Success(extraInfo);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Error.Should().Be(ServiceError.None);
    }

    [Fact]
    public void Constructor_WithSuccessFalse_AndValidError_Should_Succeed()
    {
        // Arrange
        var error = ServiceError.SystemError;
        var extraInfo = _fixture.Create<string>();

        // Act
        var result = ServiceResult.Failure(error, extraInfo);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(error);
    }

    [Fact]
    public void ExtraInfo_Should_BeSettable()
    {
        // Arrange
        var result = ServiceResult.Success();
        var newExtraInfo = _fixture.Create<string>();

        // Act
        result.ExtraInfo = newExtraInfo;

        // Assert
        result.ExtraInfo.Should().Be(newExtraInfo);
    }
}

public class ServiceResultGenericTest
{
    private readonly IFixture _fixture = new Fixture();

    [Fact]
    public void Success_Should_CreateSuccessfulResultWithData()
    {
        // Arrange
        var data = _fixture.Create<string>();
        var extraInfo = _fixture.Create<string>();

        // Act
        var result = ServiceResult<string>.Success(data, extraInfo);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Error.Should().Be(ServiceError.None);
        result.Result.Should().Be(data);
        result.ExtraInfo.Should().Be(extraInfo);
    }

    [Fact]
    public void Success_WithoutExtraInfo_Should_CreateSuccessfulResultWithData()
    {
        // Arrange
        var data = _fixture.Create<string>();

        // Act
        var result = ServiceResult<string>.Success(data);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Error.Should().Be(ServiceError.None);
        result.Result.Should().Be(data);
        result.ExtraInfo.Should().BeNull();
    }

    [Fact]
    public void Failure_Should_CreateFailureResultWithoutData()
    {
        // Arrange
        var error = ServiceError.SystemError;
        var extraInfo = _fixture.Create<string>();

        // Act
        var result = ServiceResult<string>.Failure(error, extraInfo);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
        result.Result.Should().BeNull();
        result.ExtraInfo.Should().Be(extraInfo);
    }

    [Fact]
    public void Failure_WithoutExtraInfo_Should_CreateFailureResultWithoutData()
    {
        // Arrange
        var error = ServiceError.SystemError;

        // Act
        var result = ServiceResult<string>.Failure(error);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
        result.Result.Should().BeNull();
        result.ExtraInfo.Should().BeNull();
    }

    [Fact]
    public void FailureWithData_Should_CreateFailureResultWithData()
    {
        // Arrange
        var error = ServiceError.SystemError;
        var data = _fixture.Create<string>();
        var extraInfo = _fixture.Create<string>();

        // Act
        var result = ServiceResult<string>.FailureWithData(error, data, extraInfo);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
        result.Result.Should().Be(data);
        result.ExtraInfo.Should().Be(extraInfo);
    }

    [Fact]
    public void FailureWithData_WithoutExtraInfo_Should_CreateFailureResultWithData()
    {
        // Arrange
        var error = ServiceError.SystemError;
        var data = _fixture.Create<string>();

        // Act
        var result = ServiceResult<string>.FailureWithData(error, data);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
        result.Result.Should().Be(data);
        result.ExtraInfo.Should().BeNull();
    }

    [Fact]
    public void Result_Should_BeReadOnly()
    {
        // Arrange
        var data = _fixture.Create<string>();

        // Act
        var result = ServiceResult<string>.Success(data);

        // Assert
        result.Result.Should().Be(data);
        typeof(ServiceResult<string>).GetProperty(nameof(ServiceResult<string>.Result))?.CanWrite.Should().BeFalse();
    }

    [Fact]
    public void GenericServiceResult_Should_InheritFromBaseServiceResult()
    {
        // Arrange
        var data = _fixture.Create<string>();

        // Act
        var result = ServiceResult<string>.Success(data);

        // Assert
        result.Should().BeAssignableTo<ServiceResult>();
    }
}