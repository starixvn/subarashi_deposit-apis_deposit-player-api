namespace Fanex.DepositPlayerAPI.Infrastructure.UnitTests.Repositories;

using System.Collections;
using AutoFixture;
using Constants;
using Domain.Errors;
using Domain.Params;
using Domain.Repositories;
using FluentAssertions;
using Infrastructure.Repositories;
using Infrastructure.Utils;
using NSubstitute;

[Collection("DbFactoryFixture")]
public class CommissionRepositoryTest : RepositoryTestBase
{
    private readonly ICommissionRepository _commissionRepository;

    public CommissionRepositoryTest(DbFactoryFixture dbFactoryFixture) : base(dbFactoryFixture)
    {
        _commissionRepository = new CommissionRepository(dbFactoryFixture, ShardDbInfoAccessor);
    }

    #region InsertDefaultGroupCommissionAsync

    [Fact]
    public async Task InsertDefaultGroupCommissionAsync_Should_SetParameterValueCorrect()
    {
        // Arrange
        var param = Fixture.Create<InsertDefaultGroupCommissionParam>();

        // Action
        await _commissionRepository.InsertDefaultGroupCommissionAsync(param);

        // Assert
        ObjectDb.Received(1).SetParameterValue(DbParam.Site, param.Site);
        ObjectDb.Received(1).SetParameterValue(DbParam.CommissionProducts, param.CommissionProducts);
    }

    [Theory]
    [InlineData(999, "System error.")]
    [InlineData(911, "Create customer failed. Please try again.")]
    public async Task InsertDefaultGroupCommissionAsync_Should_ReturnError_WhenDBReturnError(int errorCode, string errorMessage)
    {
        // Arrange
        var param = Fixture.Create<InsertDefaultGroupCommissionParam>();
        var data = Fixture.Create<InsertDefaultGroupCommissionOutputParam>();

        ObjectDb.GetOutputParams().Returns(new Hashtable
        {
            { DbParam.ErrCode, errorCode },
            { DbParam.ErrMsg, errorMessage },
            { DbParam.DefaultCommissions, data.DefaultCommissions },
            { DbParam.GroupDiscountId, data.GroupDiscountId }
        });
        var dbInfo = DbUtils.GetDatabaseInfo(ObjectDb, data);

        // Action
        var result = await _commissionRepository.InsertDefaultGroupCommissionAsync(param);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeEquivalentTo(ServiceErrors.GetFromDbError(errorCode, errorMessage));
        result.ExtraInfo.Should().BeEquivalentTo(dbInfo);
        result.Result.Should().BeEquivalentTo(data);
    }

    [Fact]
    public async Task InsertDefaultGroupCommissionAsync_Should_ReturnSuccess()
    {
        // Arrange
        var param = Fixture.Create<InsertDefaultGroupCommissionParam>();
        var data = Fixture.Create<InsertDefaultGroupCommissionOutputParam>();

        ObjectDb.GetOutputParams().Returns(new Hashtable
        {
            { DbParam.ErrCode, 0 },
            { DbParam.ErrMsg, "" },
            { DbParam.DefaultCommissions, data.DefaultCommissions },
            { DbParam.GroupDiscountId, data.GroupDiscountId }
        });
        var dbInfo = DbUtils.GetDatabaseInfo(ObjectDb, data);

        // Action
        var result = await _commissionRepository.InsertDefaultGroupCommissionAsync(param);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Error.Should().BeEquivalentTo(ServiceErrors.None);
        result.ExtraInfo.Should().BeEquivalentTo(dbInfo);
        result.Result.Should().BeEquivalentTo(data);
    }

    #endregion

    #region InsertDefaultCommAsync

    [Fact]
    public async Task InsertDefaultCommAsync_Should_SetParameterValueCorrect()
    {
        // Arrange
        var param = Fixture.Create<InsertDefaultCommissionParam>();

        // Action
        await _commissionRepository.InsertDefaultCommAsync(param);

        // Assert
        ObjectDb.Received(1).SetParameterValue(DbParam.Site, param.Site);
        ObjectDb.Received(1).SetParameterValue(DbParam.CustId, param.CustId);
        ObjectDb.Received(1).SetParameterValue(DbParam.GroupDiscountId, param.GroupDiscountId);
    }

    [Theory]
    [InlineData(999, "System error.")]
    [InlineData(911, "Create customer failed. Please try again.")]
    public async Task InsertDefaultCommAsync_Should_ReturnError_WhenDBReturnError(int errorCode, string errorMessage)
    {
        // Arrange
        var param = Fixture.Create<InsertDefaultCommissionParam>();

        ObjectDb.GetOutputParams().Returns(new Hashtable
        {
            { DbParam.ErrCode, errorCode },
            { DbParam.ErrMsg, errorMessage }
        });

        var dbInfo = DbUtils.GetDatabaseInfo(ObjectDb);

        // Action
        var result = await _commissionRepository.InsertDefaultCommAsync(param);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeEquivalentTo(ServiceErrors.GetFromDbError(errorCode, errorMessage));
        result.ExtraInfo.Should().BeEquivalentTo(dbInfo);
    }

    [Fact]
    public async Task InsertDefaultCommAsync_Should_ReturnSuccess()
    {
        // Arrange
        var param = Fixture.Create<InsertDefaultCommissionParam>();

        ObjectDb.GetOutputParams().Returns(new Hashtable
        {
            { DbParam.ErrCode, 0 },
            { DbParam.ErrMsg, "" }
        });

        var dbInfo = DbUtils.GetDatabaseInfo(ObjectDb);

        // Action
        var result = await _commissionRepository.InsertDefaultCommAsync(param);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Error.Should().BeEquivalentTo(ServiceErrors.None);
        result.ExtraInfo.Should().BeEquivalentTo(dbInfo);
    }

    #endregion

    #region GetPlayerCommProductsAsync

    [Fact]
    public async Task GetPlayerCommProductsAsync_Should_CreateCorrectShardSpKey()
    {
        // Arrange
        var shardSpKey = Fixture.Create<string>();
        ShardDbInfoAccessor.ToShardCustDbSp(Arg.Any<string>()).Returns(shardSpKey);

        // Action
        await _commissionRepository.GetPlayerCommProductsAsync();

        // Assert
        ObjectDb.SettingKey.Should().Be(shardSpKey);
    }

    [Fact]
    public async Task GetPlayerCommProductsAsync_Should_ReturnData_WhenDBReturnData()
    {
        // Arrange
        var data = Fixture.Create<string>();
        ObjectDb.GetOutputParams().Returns(new Hashtable
        {
            { DbParam.CommissionProducts, data }
        });

        // Action
        var result = await _commissionRepository.GetPlayerCommProductsAsync();

        // Assert
        result.Should().Be(data);
    }

    [Fact]
    public async Task GetPlayerCommProductsAsync_Should_ReturnEmpty_WhenDBDoNotReturnData()
    {
        // Arrange

        // Action
        var result = await _commissionRepository.GetPlayerCommProductsAsync();

        // Assert
        result.Should().BeEmpty();
    }

    #endregion
}