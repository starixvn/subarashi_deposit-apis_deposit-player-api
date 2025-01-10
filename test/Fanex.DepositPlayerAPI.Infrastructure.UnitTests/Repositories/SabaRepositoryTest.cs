namespace Fanex.DepositPlayerAPI.Infrastructure.UnitTests.Repositories;

using System.Collections;
using AutoFixture;
using Constants;
using Domain.Entities;
using Domain.Errors;
using Domain.Params;
using Domain.Repositories;
using FluentAssertions;
using Infrastructure.Repositories;
using Infrastructure.Utils;
using NSubstitute;

[Collection("DbFactoryFixture")]
public class SabaRepositoryTest : RepositoryTestBase
{
    private readonly ISabaRepository _sabaRepository;

    public SabaRepositoryTest(DbFactoryFixture dbFactoryFixture) : base(dbFactoryFixture)
    {
        _sabaRepository = new SabaRepository(dbFactoryFixture, ShardDbInfoAccessor);
    }

    #region ValidateSabaInfoAsync

    [Fact]
    public async Task ValidateSabaInfoAsync_Should_SetParameterValueCorrect()
    {
        // Arrange
        var param = Fixture.Create<ValidateSabaInfoParam>();

        // Action
        await _sabaRepository.ValidateSabaInfoAsync(param);

        // Assert
        ObjectDb.Received(1).SetParameterValue(DbParam.CustId, param.CustId);
        ObjectDb.Received(1).SetParameterValue(DbParam.SabaId, param.SabaId);
        ObjectDb.Received(1).SetParameterValue(DbParam.CustType, param.CustType);
    }

    [Theory]
    [InlineData(911, "Create customer failed. Please try again.")]
    [InlineData(999, "System error.")]
    public async Task ValidateSabaInfoAsync_Should_ReturnError_WhenDBReturnError(int errorCode, string errorMessage)
    {
        // Arrange
        var param = Fixture.Create<ValidateSabaInfoParam>();
        var data = Fixture.Create<ValidateSabaInfoOutputParam>();

        ObjectDb.GetOutputParams().Returns(new Hashtable
        {
            { DbParam.ErrCode, errorCode },
            { DbParam.ErrMsg, errorMessage },
            { DbParam.IsTest, data.IsTest }
        });

        var dbInfo = DbUtils.GetDatabaseInfo(ObjectDb, data);

        // Action
        var result = await _sabaRepository.ValidateSabaInfoAsync(param);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeEquivalentTo(ServiceErrors.GetFromDbError(errorCode, errorMessage));
        result.ExtraInfo.Should().BeEquivalentTo(dbInfo);
        result.Result.Should().BeEquivalentTo(data);
    }

    [Fact]
    public async Task ValidateSabaInfoAsync_Should_ReturnSuccess()
    {
        // Arrange
        var param = Fixture.Create<ValidateSabaInfoParam>();
        var data = Fixture.Create<ValidateSabaInfoOutputParam>();

        ObjectDb.GetOutputParams().Returns(new Hashtable
        {
            { DbParam.ErrCode, 0 },
            { DbParam.ErrMsg, "" },
            { DbParam.IsTest, data.IsTest }
        });

        var dbInfo = DbUtils.GetDatabaseInfo(ObjectDb, data);

        // Action
        var result = await _sabaRepository.ValidateSabaInfoAsync(param);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Error.Should().BeEquivalentTo(ServiceErrors.None);
        result.ExtraInfo.Should().BeEquivalentTo(dbInfo);
        result.Result.Should().BeEquivalentTo(data);
    }

    #endregion

    #region MapSabaInfoAsync

    [Fact]
    public async Task MapSabaInfoAsync_Should_CreateCorrectShardSpKey()
    {
        // Arrange
        var param = Fixture.Create<MapSabaInfoParam>();
        var shardSpKey = Fixture.Create<string>();
        ShardDbInfoAccessor.ToShardCustDbSp(Arg.Any<string>()).Returns(shardSpKey);

        // Action
        await _sabaRepository.MapSabaInfoAsync(param);

        // Assert
        ObjectDb.SettingKey.Should().Be(shardSpKey);
    }

    [Fact]
    public async Task MapSabaInfoAsync_Should_SetParameterValueCorrect()
    {
        // Arrange
        var param = Fixture.Create<MapSabaInfoParam>();

        // Action
        await _sabaRepository.MapSabaInfoAsync(param);

        // Assert
        ObjectDb.Received(1).SetParameterValue(DbParam.CustId, param.CustId);
        ObjectDb.Received(1).SetParameterValue(DbParam.SabaId, param.SabaId);
        ObjectDb.Received(1).SetParameterValue(DbParam.CustType, param.CustType);
        ObjectDb.Received(1).SetParameterValue(DbParam.IsTest, param.IsTest);
    }

    [Theory]
    [InlineData(911, "Create customer failed. Please try again.")]
    [InlineData(999, "System error.")]
    public async Task MapSabaInfoAsync_Should_ReturnError_WhenDBReturnError(int errorCode, string errorMessage)
    {
        // Arrange
        var param = Fixture.Create<MapSabaInfoParam>();
        var data = Fixture.Create<LogData>();

        ObjectDb.GetOutputParams().Returns(new Hashtable
        {
            { DbParam.ErrCode, errorCode },
            { DbParam.ErrMsg, errorMessage }
        });

        ObjectDb.QueryEntityAsync<LogData>().Returns(data);

        var dbInfo = DbUtils.GetDatabaseInfo(ObjectDb, data);

        // Action
        var result = await _sabaRepository.MapSabaInfoAsync(param);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeEquivalentTo(ServiceErrors.GetFromDbError(errorCode, errorMessage));
        result.ExtraInfo.Should().BeEquivalentTo(dbInfo);
        result.Result.Should().BeEquivalentTo(data);
    }

    [Fact]
    public async Task MapSabaInfoAsync_Should_ReturnSuccess()
    {
        // Arrange
        var param = Fixture.Create<MapSabaInfoParam>();
        var data = Fixture.Create<LogData>();

        ObjectDb.GetOutputParams().Returns(new Hashtable
        {
            { DbParam.ErrCode, 0 },
            { DbParam.ErrMsg, "" }
        });

        ObjectDb.QueryEntityAsync<LogData>().Returns(data);

        var dbInfo = DbUtils.GetDatabaseInfo(ObjectDb, data);

        // Action
        var result = await _sabaRepository.MapSabaInfoAsync(param);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Error.Should().BeEquivalentTo(ServiceErrors.None);
        result.ExtraInfo.Should().BeEquivalentTo(dbInfo);
        result.Result.Should().BeEquivalentTo(data);
    }

    #endregion
}