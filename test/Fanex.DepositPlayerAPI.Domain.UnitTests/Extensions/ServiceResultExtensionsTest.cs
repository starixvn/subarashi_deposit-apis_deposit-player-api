namespace Fanex.DepositPlayerAPI.Domain.UnitTests.Extensions;

using Domain.Extensions;
using Errors;
using NSubstitute;
using Results;

public class ServiceResultExtensionsTest
{
    [Fact]
    public void Match_WithSuccessResult_ShouldCallOnSuccess()
    {
        // Arrange
        var serviceResult = new ServiceResult<string>(true, ServiceError.None, null, "Success Info");
        var onSuccess = Substitute.For<Func<string, string>>();
        var onFailure = Substitute.For<Func<ServiceError, string, string>>();

        // Act
        var result = serviceResult.Match(onSuccess, onFailure);

        // Assert
        onSuccess.Received(1).Invoke("Success Info");
        onFailure.DidNotReceive().Invoke(Arg.Any<ServiceError>(), Arg.Any<string>());
    }

    [Fact]
    public void Match_WithFailureResult_ShouldCallOnFailure()
    {
        // Arrange
        var serviceError = ServiceError.SystemError;
        var serviceResult = new ServiceResult<string>(false, serviceError, null, "Failure Info");
        var onSuccess = Substitute.For<Func<string, string>>();
        var onFailure = Substitute.For<Func<ServiceError, string, string>>();

        // Act
        var result = serviceResult.Match(onSuccess, onFailure);

        // Assert
        onFailure.Received(1).Invoke(serviceError, "Failure Info");
        onSuccess.DidNotReceive().Invoke(Arg.Any<string>());
    }

    [Fact]
    public void Match_Generic_WithSuccessResult_ShouldCallOnSuccess()
    {
        // Arrange
        var serviceResult = new ServiceResult<string>(true, ServiceError.None, null, "Success Info");
        var onSuccess = Substitute.For<Func<string, string>>();
        var onFailure = Substitute.For<Func<ServiceError, string, string>>();

        // Act
        var result = serviceResult.Match(onSuccess, onFailure);

        // Assert
        onSuccess.Received(1).Invoke("Success Info");
        onFailure.DidNotReceive().Invoke(Arg.Any<ServiceError>(), Arg.Any<string>());
    }

    [Fact]
    public void Match_Generic_WithFailureResult_ShouldCallOnFailure()
    {
        // Arrange
        var serviceError = ServiceError.SystemError;
        var serviceResult = new ServiceResult<string>(false, serviceError, null, "Failure Info");
        var onSuccess = Substitute.For<Func<string, string>>();
        var onFailure = Substitute.For<Func<ServiceError, string, string>>();

        // Act
        var result = serviceResult.Match(onSuccess, onFailure);

        // Assert
        onFailure.Received(1).Invoke(serviceError, "Failure Info");
        onSuccess.DidNotReceive().Invoke(Arg.Any<string>());
    }
}