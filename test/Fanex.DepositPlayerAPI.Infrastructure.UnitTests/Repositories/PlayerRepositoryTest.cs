namespace Fanex.DepositPlayerAPI.Infrastructure.UnitTests.Repositories;

using System.Collections;
using System.Data;
using AutoFixture;
using Constants;
using Data;
using Domain.Entities;
using Domain.Errors;
using Domain.Params;
using Domain.Repositories;
using FluentAssertions;
using Infrastructure.Repositories;
using Infrastructure.Utils;
using Microsoft.Data.SqlClient;
using NSubstitute;

[Collection("DbFactoryFixture")]
public class PlayerRepositoryTest : RepositoryTestBase
{
    private readonly IMultiResultReader _multiResultReader;
    private readonly IPlayerRepository _playerRepository;

    public PlayerRepositoryTest(DbFactoryFixture dbFactoryFixture) : base(dbFactoryFixture)
    {
        _multiResultReader = Substitute.For<IMultiResultReader>();
        _playerRepository = new PlayerRepository(dbFactoryFixture, ShardDbInfoAccessor);

        ObjectDb.QueryMultipleAsync().Returns(_multiResultReader);
    }

    #region VerifyUsernameExistence

    [Fact]
    public async Task VerifyUsernameExistence_Should_SetParameterValueCorrect()
    {
        // Arrange
        var username = Fixture.Create<string>();

        // Action
        await _playerRepository.VerifyUsernameExistence(username);

        // Assert
        ObjectDb.Received(1).SetParameterValue(DbParam.Username, username);
    }

    [Theory]
    [InlineData(911, "Create customer failed. Please try again.")]
    [InlineData(999, "System error.")]
    public async Task VerifyUsernameExistence_Should_ReturnError_WhenDBReturnError(int errorCode, string errorMessage)
    {
        // Arrange
        var username = Fixture.Create<string>();

        ObjectDb.GetOutputParams().Returns(new Hashtable
        {
            { DbParam.ErrCode, errorCode },
            { DbParam.ErrMsg, errorMessage }
        });

        var dbInfo = DbUtils.GetDatabaseInfo(ObjectDb);

        // Action
        var result = await _playerRepository.VerifyUsernameExistence(username);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeEquivalentTo(ServiceErrors.GetFromDbError(errorCode, errorMessage));
        result.ExtraInfo.Should().BeEquivalentTo(dbInfo);
    }

    [Fact]
    public async Task VerifyUsernameExistence_Should_ReturnSuccess()
    {
        // Arrange
        var username = Fixture.Create<string>();

        ObjectDb.GetOutputParams().Returns(new Hashtable
        {
            { DbParam.ErrCode, 0 },
            { DbParam.ErrMsg, "" }
        });

        var dbInfo = DbUtils.GetDatabaseInfo(ObjectDb);

        // Action
        var result = await _playerRepository.VerifyUsernameExistence(username);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Error.Should().BeEquivalentTo(ServiceErrors.None);
        result.ExtraInfo.Should().BeEquivalentTo(dbInfo);
    }

    #endregion

    #region InsertShadowAccountSettingAsync

    [Fact]
    public async Task InsertShadowAccountSettingAsync_Should_CreateCorrectShardSpKey()
    {
        // Arrange
        var param = Fixture.Create<InsertShadowAccountSettingParam>();
        var shardSpKey = Fixture.Create<string>();
        ShardDbInfoAccessor.ToShardCustDbSp(Arg.Any<string>()).Returns(shardSpKey);

        // Action
        await _playerRepository.InsertShadowAccountSettingAsync(param);

        // Assert
        ObjectDb.SettingKey.Should().Be(shardSpKey);
    }

    [Fact]
    public async Task InsertShadowAccountSettingAsync_Should_SetParameterValueCorrect()
    {
        // Arrange
        var param = Fixture.Create<InsertShadowAccountSettingParam>();

        // Action
        await _playerRepository.InsertShadowAccountSettingAsync(param);

        // Assert
        ObjectDb.Received(1).SetParameterValue(DbParam.CustId, param.CustId);
        ObjectDb.Received(1).SetParameterValue(DbParam.Username, param.Username);
        ObjectDb.Received(1).SetParameterValue(DbParam.IsTest, param.IsTest);
        ObjectDb.Received(1).SetParameterValue(DbParam.AgentId, param.AgentId);
    }

    [Theory]
    [InlineData(911, "Create customer failed. Please try again.")]
    [InlineData(999, "System error.")]
    public async Task InsertShadowAccountSettingAsync_Should_ReturnError_WhenDBReturnError(int errorCode, string errorMessage)
    {
        // Arrange
        var param = Fixture.Create<InsertShadowAccountSettingParam>();
        var data = Fixture.Create<LogData>();

        ObjectDb.GetOutputParams().Returns(new Hashtable
        {
            { DbParam.ErrCode, errorCode },
            { DbParam.ErrMsg, errorMessage }
        });

        ObjectDb.QueryEntityAsync<LogData>().Returns(data);

        var dbInfo = DbUtils.GetDatabaseInfo(ObjectDb, data);

        // Action
        var result = await _playerRepository.InsertShadowAccountSettingAsync(param);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeEquivalentTo(ServiceErrors.GetFromDbError(errorCode, errorMessage));
        result.ExtraInfo.Should().BeEquivalentTo(dbInfo);
        result.Result.Should().BeEquivalentTo(data);
    }

    [Fact]
    public async Task InsertShadowAccountSettingAsync_Should_ReturnSuccess()
    {
        // Arrange
        var param = Fixture.Create<InsertShadowAccountSettingParam>();
        var data = Fixture.Create<LogData>();

        ObjectDb.GetOutputParams().Returns(new Hashtable
        {
            { DbParam.ErrCode, 0 },
            { DbParam.ErrMsg, "" }
        });

        ObjectDb.QueryEntityAsync<LogData>().Returns(data);

        var dbInfo = DbUtils.GetDatabaseInfo(ObjectDb, data);

        // Action
        var result = await _playerRepository.InsertShadowAccountSettingAsync(param);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Error.Should().BeEquivalentTo(ServiceErrors.None);
        result.ExtraInfo.Should().BeEquivalentTo(dbInfo);
        result.Result.Should().BeEquivalentTo(data);
    }

    #endregion

    #region InsertShadowAccountInfoAsync

    [Fact]
    public async Task InsertShadowAccountInfoAsync_Should_SetParameterValueCorrect()
    {
        // Arrange
        var param = Fixture.Create<InsertShadowAccountInfoParam>();

        // Action
        await _playerRepository.InsertShadowAccountInfoAsync(param);

        // Assert
        ObjectDb.Received(1).SetParameterValue(DbParam.IsTest, param.IsTest);
        ObjectDb.Received(1).SetParameterValue(DbParam.AgentId, param.AgentId);
        ObjectDb.Received(1).SetParameterValue(DbParam.SiteDbGroupId, param.SiteDbGroupId);
    }

    [Theory]
    [InlineData(99, "Invalid member.")]
    [InlineData(10, "Invalid agent.")]
    [InlineData(11, "The username is not available.")]
    [InlineData(999, "System error.")]
    public async Task InsertShadowAccountInfoAsync_Should_ReturnError_WhenDBReturnError(int errorCode, string errorMessage)
    {
        // Arrange
        var param = Fixture.Create<InsertShadowAccountInfoParam>();
        var data = Fixture.Create<InsertShadowAccountInfoOutputParam>();

        ObjectDb.GetOutputParams().Returns(new Hashtable
        {
            { DbParam.ErrCode, errorCode },
            { DbParam.ErrMsg, errorMessage },
            { DbParam.Username, data.Username },
            { DbParam.Username2, data.Username2 },
            { DbParam.CustId, data.CustId }
        });

        var dbInfo = DbUtils.GetDatabaseInfo(ObjectDb, data);

        // Action
        var result = await _playerRepository.InsertShadowAccountInfoAsync(param);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeEquivalentTo(ServiceErrors.GetFromDbError(errorCode, errorMessage));
        result.ExtraInfo.Should().BeEquivalentTo(dbInfo);
        result.Result.Should().BeEquivalentTo(data);
    }

    [Fact]
    public async Task InsertShadowAccountInfoAsync_Should_ReturnSuccess()
    {
        // Arrange
        var param = Fixture.Create<InsertShadowAccountInfoParam>();
        var data = Fixture.Create<InsertShadowAccountInfoOutputParam>();

        ObjectDb.GetOutputParams().Returns(new Hashtable
        {
            { DbParam.ErrCode, 0 },
            { DbParam.ErrMsg, "" },
            { DbParam.Username, data.Username },
            { DbParam.Username2, data.Username2 },
            { DbParam.CustId, data.CustId }
        });

        var dbInfo = DbUtils.GetDatabaseInfo(ObjectDb, data);

        // Action
        var result = await _playerRepository.InsertShadowAccountInfoAsync(param);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Error.Should().BeEquivalentTo(ServiceErrors.None);
        result.ExtraInfo.Should().BeEquivalentTo(dbInfo);
        result.Result.Should().BeEquivalentTo(data);
    }

    #endregion

    #region GetArchivedInfoAsync

    [Fact]
    public async Task GetArchivedInfoAsync_Should_SetParameterValueCorrect()
    {
        // Arrange
        var custId = Fixture.Create<int>();

        // Action
        await _playerRepository.GetArchivedInfoAsync(custId);

        // Assert
        ObjectDb.Received(1).SetParameterValue(DbParam.CustId, custId);
    }

    [Theory]
    [InlineData(7, "This account is not a dormant member.")]
    [InlineData(999, "System error.")]
    public async Task GetArchivedInfoAsync_Should_ReturnError_WhenDBReturnError(int errorCode, string errorMessage)
    {
        // Arrange
        var custId = Fixture.Create<int>();
        var data = Fixture.Create<GetArchivedPlayerInfoOutputParam>();

        ObjectDb.GetOutputParams().Returns(new Hashtable
        {
            { DbParam.ErrCode, errorCode },
            { DbParam.ErrMsg, errorMessage },
            { DbParam.CustomerInfo, data.CustomerInfo },
            { DbParam.Recommend, data.Recommend }
        });
        ObjectDb.QueryEntityAsync<LogTracking>().Returns(data.LogTracking);

        var dbInfo = DbUtils.GetDatabaseInfo(ObjectDb, data);

        // Action
        var result = await _playerRepository.GetArchivedInfoAsync(custId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeEquivalentTo(ServiceErrors.GetFromDbError(errorCode, errorMessage));
        result.ExtraInfo.Should().BeEquivalentTo(dbInfo);
        result.Result.Should().BeEquivalentTo(data);
    }

    [Fact]
    public async Task GetArchivedInfoAsync_Should_ReturnSuccess()
    {
        // Arrange
        var custId = Fixture.Create<int>();
        var data = Fixture.Create<GetArchivedPlayerInfoOutputParam>();

        ObjectDb.GetOutputParams().Returns(new Hashtable
        {
            { DbParam.ErrCode, 0 },
            { DbParam.ErrMsg, "" },
            { DbParam.CustomerInfo, data.CustomerInfo },
            { DbParam.Recommend, data.Recommend }
        });
        ObjectDb.QueryEntityAsync<LogTracking>().Returns(data.LogTracking);

        var dbInfo = DbUtils.GetDatabaseInfo(ObjectDb, data);

        // Action
        var result = await _playerRepository.GetArchivedInfoAsync(custId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Error.Should().BeEquivalentTo(ServiceErrors.None);
        result.ExtraInfo.Should().BeEquivalentTo(dbInfo);
        result.Result.Should().BeEquivalentTo(data);
    }

    #endregion

    #region InsertReactivePlayerSettingAsync

    [Fact]
    public async Task InsertReactivePlayerSettingAsync_Should_CreateCorrectShardSpKey()
    {
        // Arrange
        var param = Fixture.Create<InsertReactivePlayerSettingParam>();
        var shardSpKey = Fixture.Create<string>();
        ShardDbInfoAccessor.ToShardCustDbSp(Arg.Any<string>()).Returns(shardSpKey);

        // Action
        await _playerRepository.InsertReactivePlayerSettingAsync(param);

        // Assert
        ObjectDb.SettingKey.Should().Be(shardSpKey);
    }

    [Fact]
    public async Task InsertReactivePlayerSettingAsync_Should_SetParameterValueCorrect()
    {
        // Arrange
        var param = Fixture.Create<InsertReactivePlayerSettingParam>();

        // Action
        await _playerRepository.InsertReactivePlayerSettingAsync(param);

        // Assert
        ObjectDb.Received(1).SetParameterValue(DbParam.CustId, param.CustId);
        ObjectDb.Received(1).SetParameterValue(DbParam.CustomerInfo, param.CustomerInfo);
        ObjectDb.Received(1).SetParameterValue(DbParam.SubsidiaryId, param.SubsidiaryId);
        ObjectDb.Received(1).SetParameterValue(DbParam.DefaultCommissions, param.DefaultCommissions);
        ObjectDb.Received(1).SetParameterValue(DbParam.GroupDiscountId, param.GroupDiscountId);
    }

    [Theory]
    [InlineData(111, "Create customer failed. Please try again.")]
    [InlineData(112, "Create customer failed. Please try again.")]
    [InlineData(911, "Create customer failed. Please try again.")]
    [InlineData(999, "System error.")]
    public async Task InsertReactivePlayerSettingAsync_Should_ReturnError_WhenDBReturnError(int errorCode, string errorMessage)
    {
        // Arrange
        var param = Fixture.Create<InsertReactivePlayerSettingParam>();
        var data = Fixture.Create<InsertReactivePlayerSettingOutputParam>();
        _multiResultReader.ReadAsync<LogTracking>().Returns(new List<LogTracking> { data.LogTracking });
        _multiResultReader.ReadAsync<LogData>().Returns(new List<LogData> { data.LogData });

        ObjectDb.GetOutputParams().Returns(new Hashtable
        {
            { DbParam.ErrCode, errorCode },
            { DbParam.ErrMsg, errorMessage },
            { DbParam.CustIdBalanceDummy, data.CustIdBalanceDummy },
            { DbParam.ClosingBalance, data.ClosingBalance }
        });

        var dbInfo = DbUtils.GetDatabaseInfo(ObjectDb, data);

        // Action
        var result = await _playerRepository.InsertReactivePlayerSettingAsync(param);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeEquivalentTo(ServiceErrors.GetFromDbError(errorCode, errorMessage));
        result.ExtraInfo.Should().BeEquivalentTo(dbInfo);
        result.Result.Should().BeEquivalentTo(data);
    }

    [Fact]
    public async Task InsertReactivePlayerSettingAsync_Should_ReturnSuccess()
    {
        // Arrange
        var param = Fixture.Create<InsertReactivePlayerSettingParam>();
        var data = Fixture.Create<InsertReactivePlayerSettingOutputParam>();
        _multiResultReader.ReadAsync<LogTracking>().Returns(new List<LogTracking> { data.LogTracking });
        _multiResultReader.ReadAsync<LogData>().Returns(new List<LogData> { data.LogData });

        ObjectDb.GetOutputParams().Returns(new Hashtable
        {
            { DbParam.ErrCode, 0 },
            { DbParam.ErrMsg, "" },
            { DbParam.CustIdBalanceDummy, data.CustIdBalanceDummy },
            { DbParam.ClosingBalance, data.ClosingBalance }
        });

        var dbInfo = DbUtils.GetDatabaseInfo(ObjectDb, data);

        // Action
        var result = await _playerRepository.InsertReactivePlayerSettingAsync(param);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Error.Should().BeEquivalentTo(ServiceErrors.None);
        result.ExtraInfo.Should().BeEquivalentTo(dbInfo);
        result.Result.Should().BeEquivalentTo(data);
    }

    #endregion

    #region InsertNewPlayerSettingAsync

    [Fact]
    public async Task InsertNewPlayerSettingAsync_Should_CreateCorrectShardSpKey()
    {
        // Arrange
        var param = Fixture.Create<InsertNewPlayerSettingParam>();
        var shardSpKey = Fixture.Create<string>();
        ShardDbInfoAccessor.ToShardCustDbSp(Arg.Any<string>()).Returns(shardSpKey);

        // Action
        await _playerRepository.InsertNewPlayerSettingAsync(param);

        // Assert
        ObjectDb.SettingKey.Should().Be(shardSpKey);
    }

    [Fact]
    public async Task InsertNewPlayerSettingAsync_Should_SetParameterValueCorrect()
    {
        // Arrange
        var param = Fixture.Create<InsertNewPlayerSettingParam>();

        // Action
        await _playerRepository.InsertNewPlayerSettingAsync(param);

        // Assert
        ObjectDb.Received(1).SetParameterValue(DbParam.CustId, param.CustId);
        ObjectDb.Received(1).SetParameterValue(DbParam.Username, param.Username);
        ObjectDb.Received(1).SetParameterValue(DbParam.Username2, param.Username2);
        ObjectDb.Received(1).SetParameterValue("DepositUserId", param.DepositUserId);
        ObjectDb.Received(1).SetParameterValue(DbParam.Recommend, param.Recommend);
        ObjectDb.Received(1).SetParameterValue("Password", param.Password);
        ObjectDb.Received(1).SetParameterValue(DbParam.OddsType, param.OddsType);
        ObjectDb.Received(1).SetParameterValue("Country", param.Country);
        ObjectDb.Received(1).SetParameterValue(DbParam.SubsidiaryId, param.SubsidiaryId);
        ObjectDb.Received(1).SetParameterValue(DbParam.AffiliateId, param.AffiliateId);
        ObjectDb.Received(1).SetParameterValue(DbParam.DefaultCommissions, param.DefaultCommissions);
        ObjectDb.Received(1).SetParameterValue(DbParam.GroupDiscountId, param.GroupDiscountId);
        ObjectDb.Received(1).SetParameterValue(DbParam.Site, param.Site);
    }

    [Theory]
    [InlineData(999, "System error.")]
    [InlineData(911, "Create customer failed. Please try again.")]
    public async Task InsertNewPlayerSettingAsync_Should_ReturnError_WhenDBReturnError(int errorCode, string errorMessage)
    {
        // Arrange
        var param = Fixture.Create<InsertNewPlayerSettingParam>();
        var data = Fixture.Create<InsertNewPlayerSettingOutputParam>();

        ObjectDb.GetOutputParams().Returns(new Hashtable
        {
            { DbParam.ErrCode, errorCode },
            { DbParam.ErrMsg, errorMessage },
            { DbParam.Currency, data.Currency }
        });
        ObjectDb.QueryEntityAsync<LogData>().Returns(data.LogData);

        var dbInfo = DbUtils.GetDatabaseInfo(ObjectDb, data);

        // Action
        var result = await _playerRepository.InsertNewPlayerSettingAsync(param);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeEquivalentTo(ServiceErrors.GetFromDbError(errorCode, errorMessage));
        result.ExtraInfo.Should().BeEquivalentTo(dbInfo);
        result.Result.Should().BeEquivalentTo(data);
    }

    [Fact]
    public async Task InsertNewPlayerSettingAsync_Should_ReturnSuccess()
    {
        // Arrange
        var param = Fixture.Create<InsertNewPlayerSettingParam>();
        var data = Fixture.Create<InsertNewPlayerSettingOutputParam>();

        ObjectDb.GetOutputParams().Returns(new Hashtable
        {
            { DbParam.ErrCode, 0 },
            { DbParam.ErrMsg, "" },
            { DbParam.Currency, data.Currency }
        });
        ObjectDb.QueryEntityAsync<LogData>().Returns(data.LogData);

        var dbInfo = DbUtils.GetDatabaseInfo(ObjectDb, data);

        // Action
        var result = await _playerRepository.InsertNewPlayerSettingAsync(param);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Error.Should().BeEquivalentTo(ServiceErrors.None);
        result.ExtraInfo.Should().BeEquivalentTo(dbInfo);
        result.Result.Should().BeEquivalentTo(data);
    }

    #endregion

    #region InsertPlayerInfoAsync

    [Fact]
    public async Task InsertPlayerInfoAsync_Should_SetParameterValueCorrect()
    {
        // Arrange
        var param = Fixture.Create<InsertPlayerInfoParam>();

        // Action
        await _playerRepository.InsertPlayerInfoAsync(param);

        // Assert
        ObjectDb.Received(1).SetParameterValue(DbParam.Username2, param.Username2);
        ObjectDb.Received(1).SetParameterValue(DbParam.Recommend, param.Recommend);
        ObjectDb.Received(1).SetParameterValue(DbParam.SiteDbGroupId, param.SiteDbGroupId);
        ObjectDb.Received(1).SetParameterValue(DbParam.Site, param.Site);
        ObjectDb.Received(1).SetParameterValue(DbParam.CurrencyName, param.CurrencyName);
        ObjectDb.Received(1).SetParameterValue(DbParam.RetryNumber, param.RetryNumber);
        ObjectDb.Received(1).SetParameterValue(DbParam.CustId, param.CustId);
    }

    [Theory]
    [InlineData(11, "The username is not available.")]
    [InlineData(11, "Can not generate username.")]
    [InlineData(999, "System error.")]
    [InlineData(911, "Create customer failed. Please try again.")]
    public async Task InsertPlayerInfoAsync_Should_ReturnError_WhenDBReturnError(int errorCode, string errorMessage)
    {
        // Arrange
        var param = Fixture.Create<InsertPlayerInfoParam>();
        var data = Fixture.Create<InsertPlayerInfoOutputParam>();

        ObjectDb.GetOutputParams().Returns(new Hashtable
        {
            { DbParam.ErrCode, errorCode },
            { DbParam.ErrMsg, errorMessage },
            { DbParam.CustId, data.CustId },
            { DbParam.Username, data.Username }
        });

        ObjectDb.GetAddedParameters().Returns([
            new SqlParameter
            {
                Direction = ParameterDirection.Input,
                ParameterName = DbParam.CustId,
                Value = param.CustId
            }
        ]);

        var capturedParams = DbUtils.CaptureInputOutputParams(ObjectDb, [DbParam.CustId]);
        var dbInfo = DbUtils.GetDatabaseInfo(ObjectDb, data, capturedParams);

        // Action
        var result = await _playerRepository.InsertPlayerInfoAsync(param);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeEquivalentTo(ServiceErrors.GetFromDbError(errorCode, errorMessage));
        result.ExtraInfo.Should().BeEquivalentTo(dbInfo);
        result.Result.Should().BeEquivalentTo(data);
    }

    [Fact]
    public async Task InsertPlayerInfoAsync_Should_ReturnSuccess()
    {
        // Arrange
        var param = Fixture.Create<InsertPlayerInfoParam>();
        var data = Fixture.Create<InsertPlayerInfoOutputParam>();

        ObjectDb.GetOutputParams().Returns(new Hashtable
        {
            { DbParam.ErrCode, 0 },
            { DbParam.ErrMsg, "" },
            { DbParam.CustId, data.CustId },
            { DbParam.Username, data.Username }
        });

        ObjectDb.GetAddedParameters().Returns([
            new SqlParameter
            {
                Direction = ParameterDirection.Input,
                ParameterName = DbParam.CustId,
                Value = param.CustId
            }
        ]);

        var capturedParams = DbUtils.CaptureInputOutputParams(ObjectDb, [DbParam.CustId]);
        var dbInfo = DbUtils.GetDatabaseInfo(ObjectDb, data, capturedParams);

        // Action
        var result = await _playerRepository.InsertPlayerInfoAsync(param);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Error.Should().BeEquivalentTo(ServiceErrors.None);
        result.ExtraInfo.Should().BeEquivalentTo(dbInfo);
        result.Result.Should().BeEquivalentTo(data);
    }

    #endregion

    #region ValidatePlayerInfoAsync

    [Fact]
    public async Task ValidatePlayerInfoAsync_Should_CreateCorrectShardSpKey()
    {
        // Arrange
        var param = Fixture.Create<ValidatePlayerInfoParam>();
        var shardSpKey = Fixture.Create<string>();
        ShardDbInfoAccessor.ToShardCustDbSp(Arg.Any<string>()).Returns(shardSpKey);

        // Action
        await _playerRepository.ValidatePlayerInfoAsync(param);

        // Assert
        ObjectDb.SettingKey.Should().Be(shardSpKey);
    }

    [Fact]
    public async Task ValidatePlayerInfoAsync_Should_SetParameterValueCorrect()
    {
        // Arrange
        var param = Fixture.Create<ValidatePlayerInfoParam>();

        // Action
        await _playerRepository.ValidatePlayerInfoAsync(param);

        // Assert
        ObjectDb.Received(1).SetParameterValue("DepositUserName", param.DepositUserName);
        ObjectDb.Received(1).SetParameterValue(DbParam.AgentId, param.AgentId);
    }

    [Theory]
    [InlineData(10, "Invalid agent.")]
    [InlineData(999, "System error.")]
    [InlineData(911, "Create customer failed. Please try again.")]
    public async Task ValidatePlayerInfoAsync_Should_ReturnError_WhenDBReturnError(int errorCode, string errorMessage)
    {
        // Arrange
        var param = Fixture.Create<ValidatePlayerInfoParam>();
        var data = Fixture.Create<ValidatePlayerInfoOutputParam>();

        ObjectDb.GetOutputParams().Returns(new Hashtable
        {
            { DbParam.ErrCode, errorCode },
            { DbParam.ErrMsg, errorMessage },
            { DbParam.CurrencyName, data.CurrencyName },
            { DbParam.Site, data.Site },
            { DbParam.Username2, data.Username2 },
            { DbParam.Recommend, data.Recommend }
        });
        var dbInfo = DbUtils.GetDatabaseInfo(ObjectDb, data);

        // Action
        var result = await _playerRepository.ValidatePlayerInfoAsync(param);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeEquivalentTo(ServiceErrors.GetFromDbError(errorCode, errorMessage));
        result.ExtraInfo.Should().BeEquivalentTo(dbInfo);
        result.Result.Should().BeEquivalentTo(data);
    }

    [Fact]
    public async Task ValidatePlayerInfoAsync_Should_ReturnSuccess()
    {
        // Arrange
        var param = Fixture.Create<ValidatePlayerInfoParam>();
        var data = Fixture.Create<ValidatePlayerInfoOutputParam>();

        ObjectDb.GetOutputParams().Returns(new Hashtable
        {
            { DbParam.ErrCode, 0 },
            { DbParam.ErrMsg, "" },
            { DbParam.CurrencyName, data.CurrencyName },
            { DbParam.Site, data.Site },
            { DbParam.Username2, data.Username2 },
            { DbParam.Recommend, data.Recommend }
        });
        var dbInfo = DbUtils.GetDatabaseInfo(ObjectDb, data);

        // Action
        var result = await _playerRepository.ValidatePlayerInfoAsync(param);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Error.Should().BeEquivalentTo(ServiceErrors.None);
        result.ExtraInfo.Should().BeEquivalentTo(dbInfo);
        result.Result.Should().BeEquivalentTo(data);
    }

    #endregion
}