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
public class BalanceRepositoryTest : RepositoryBaseTest
{
    private readonly IBalanceRepository _balanceRepository;

    public BalanceRepositoryTest(DbFactoryFixture dbFactoryFixture) : base(dbFactoryFixture)
    {
        _balanceRepository = new BalanceRepository(dbFactoryFixture, ShardDbInfoAccessor);
    }

    #region InitializeReactivatePlayerBalanceAsync

    [Fact]
    public async Task InitializeReactivatePlayerBalanceAsync_Should_CreateCorrectShardSpKey()
    {
        // Arrange
        var param = Fixture.Create<InitializeReactivatePlayerBalanceParam>();
        var shardSpKey = Fixture.Create<string>();
        ShardDbInfoAccessor.ToShardSiteDbSp(Arg.Any<string>()).Returns(shardSpKey);

        // Action
        await _balanceRepository.InitializeReactivatePlayerBalanceAsync(param);

        // Assert
        ObjectDb.SettingKey.Should().Be(shardSpKey);
    }

    [Fact]
    public async Task InitializeReactivatePlayerBalanceAsync_Should_SetParameterValueCorrect()
    {
        // Arrange
        var param = Fixture.Create<InitializeReactivatePlayerBalanceParam>();

        // Action
        await _balanceRepository.InitializeReactivatePlayerBalanceAsync(param);

        // Assert
        ObjectDb.Received(1).SetParameterValue(DbParam.CustId, param.CustId);
        ObjectDb.Received(1).SetParameterValue(DbParam.CustIdBalanceDummy, param.CustIdBalanceDummy);
        ObjectDb.Received(1).SetParameterValue(DbParam.ClosingBalance, param.ClosingBalance);
    }

    [Theory]
    [InlineData(999, "System error.")]
    [InlineData(911, "Create customer failed. Please try again.")]
    public async Task InitializeReactivatePlayerBalanceAsync_Should_ReturnError_WhenDBReturnError(int errorCode, string errorMessage)
    {
        // Arrange
        var param = Fixture.Create<InitializeReactivatePlayerBalanceParam>();
        var logTacking = Fixture.Create<LogTracking>();
        ObjectDb.GetOutputParams().Returns(new Hashtable
        {
            { DbParam.ErrCode, errorCode },
            { DbParam.ErrMsg, errorMessage }
        });
        var dbInfo = DbUtils.GetDatabaseInfo(ObjectDb, logTacking);
        ObjectDb.QueryEntityAsync<LogTracking>().Returns(logTacking);

        // Action
        var result = await _balanceRepository.InitializeReactivatePlayerBalanceAsync(param);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeEquivalentTo(ServiceErrors.GetFromDbError(errorCode, errorMessage));
        result.ExtraInfo.Should().BeEquivalentTo(dbInfo);
        result.Result.Should().BeEquivalentTo(logTacking);
    }

    [Fact]
    public async Task InitializeReactivatePlayerBalanceAsync_Should_ReturnSuccess()
    {
        // Arrange
        var param = Fixture.Create<InitializeReactivatePlayerBalanceParam>();
        var logTacking = Fixture.Create<LogTracking>();
        ObjectDb.GetOutputParams().Returns(new Hashtable
        {
            { DbParam.ErrCode, 0 },
            { DbParam.ErrMsg, "" }
        });
        var dbInfo = DbUtils.GetDatabaseInfo(ObjectDb, logTacking);
        ObjectDb.QueryEntityAsync<LogTracking>().Returns(logTacking);

        // Action
        var result = await _balanceRepository.InitializeReactivatePlayerBalanceAsync(param);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Error.Should().BeEquivalentTo(ServiceErrors.None);
        result.ExtraInfo.Should().BeEquivalentTo(dbInfo);
        result.Result.Should().BeEquivalentTo(logTacking);
    }

    #endregion

    #region InitializeNewPlayerDebitBalanceAsync

    [Fact]
    public async Task InitializeNewPlayerDebitBalanceAsync_Should_CreateCorrectShardSpKey()
    {
        // Arrange
        var custId = Fixture.Create<int>();
        var shardSpKey = Fixture.Create<string>();
        ShardDbInfoAccessor.ToShardSiteDbSp(Arg.Any<string>()).Returns(shardSpKey);

        // Action
        await _balanceRepository.InitializeNewPlayerDebitBalanceAsync(custId);

        // Assert
        ObjectDb.SettingKey.Should().Be(shardSpKey);
    }


    [Fact]
    public async Task InitializeNewPlayerDebitBalanceAsync_Should_SetParameterValueCorrect()
    {
        // Arrange
        var custId = Fixture.Create<int>();

        // Action
        await _balanceRepository.InitializeNewPlayerDebitBalanceAsync(custId);

        // Assert
        ObjectDb.Received(1).SetParameterValue(DbParam.CustId, custId);
    }

    [Theory]
    [InlineData(999, "System error.")]
    [InlineData(911, "Create customer failed. Please try again.")]
    public async Task InitializeNewPlayerDebitBalanceAsync_Should_ReturnError_WhenDBReturnError(int errorCode, string errorMessage)
    {
        // Arrange
        var custId = Fixture.Create<int>();
        ObjectDb.GetOutputParams().Returns(new Hashtable
        {
            { DbParam.ErrCode, errorCode },
            { DbParam.ErrMsg, errorMessage }
        });
        var dbInfo = DbUtils.GetDatabaseInfo(ObjectDb);

        // Action
        var result = await _balanceRepository.InitializeNewPlayerDebitBalanceAsync(custId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeEquivalentTo(ServiceErrors.GetFromDbError(errorCode, errorMessage));
        result.ExtraInfo.Should().BeEquivalentTo(dbInfo);
    }

    [Fact]
    public async Task InitializeNewPlayerDebitBalanceAsync_Should_ReturnSuccess()
    {
        // Arrange
        var custId = Fixture.Create<int>();
        ObjectDb.GetOutputParams().Returns(new Hashtable
        {
            { DbParam.ErrCode, 0 },
            { DbParam.ErrMsg, "" }
        });
        var dbInfo = DbUtils.GetDatabaseInfo(ObjectDb);

        // Action
        var result = await _balanceRepository.InitializeNewPlayerDebitBalanceAsync(custId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Error.Should().BeEquivalentTo(ServiceErrors.None);
        result.ExtraInfo.Should().BeEquivalentTo(dbInfo);
    }

    #endregion

    #region InitializeNewPlayerCreditBalanceAsync

    [Fact]
    public async Task InitializeNewPlayerCreditBalanceAsync_Should_CreateCorrectShardSpKey()
    {
        // Arrange
        var custId = Fixture.Create<int>();
        var shardSpKey = Fixture.Create<string>();
        ShardDbInfoAccessor.ToShardSiteDbSp(Arg.Any<string>()).Returns(shardSpKey);

        // Action
        await _balanceRepository.InitializeNewPlayerCreditBalanceAsync(custId);

        // Assert
        ObjectDb.SettingKey.Should().Be(shardSpKey);
    }


    [Fact]
    public async Task InitializeNewPlayerCreditBalanceAsync_Should_SetParameterValueCorrect()
    {
        // Arrange
        var custId = Fixture.Create<int>();

        // Action
        await _balanceRepository.InitializeNewPlayerCreditBalanceAsync(custId);

        // Assert
        ObjectDb.Received(1).SetParameterValue(DbParam.CustId, custId);
    }

    [Theory]
    [InlineData(999, "System error.")]
    [InlineData(911, "Create customer failed. Please try again.")]
    public async Task InitializeNewPlayerCreditBalanceAsync_Should_ReturnError_WhenDBReturnError(int errorCode, string errorMessage)
    {
        // Arrange
        var custId = Fixture.Create<int>();
        ObjectDb.GetOutputParams().Returns(new Hashtable
        {
            { DbParam.ErrCode, errorCode },
            { DbParam.ErrMsg, errorMessage }
        });
        var dbInfo = DbUtils.GetDatabaseInfo(ObjectDb);

        // Action
        var result = await _balanceRepository.InitializeNewPlayerCreditBalanceAsync(custId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeEquivalentTo(ServiceErrors.GetFromDbError(errorCode, errorMessage));
        result.ExtraInfo.Should().BeEquivalentTo(dbInfo);
    }

    [Fact]
    public async Task InitializeNewPlayerCreditBalanceAsync_Should_ReturnSuccess()
    {
        // Arrange
        var custId = Fixture.Create<int>();
        ObjectDb.GetOutputParams().Returns(new Hashtable
        {
            { DbParam.ErrCode, 0 },
            { DbParam.ErrMsg, "" }
        });
        var dbInfo = DbUtils.GetDatabaseInfo(ObjectDb);

        // Action
        var result = await _balanceRepository.InitializeNewPlayerCreditBalanceAsync(custId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Error.Should().BeEquivalentTo(ServiceErrors.None);
        result.ExtraInfo.Should().BeEquivalentTo(dbInfo);
    }

    #endregion
}