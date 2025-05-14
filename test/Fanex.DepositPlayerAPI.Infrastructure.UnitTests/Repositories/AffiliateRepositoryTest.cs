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
public class AffiliateRepositoryTest(DbFactoryFixture dbFactoryFixture) : RepositoryTestBase(dbFactoryFixture)
{
    private readonly IAffiliateRepository _affiliateRepository = new AffiliateRepository(dbFactoryFixture);

    [Fact]
    public async Task InsertPlayerAffiliateAsync_Should_SetParameterValueCorrect()
    {
        // Arrange
        var param = Fixture.Create<NewPlayerAffiliateParam>();

        // Action
        await _affiliateRepository.InsertPlayerAffiliateAsync(param);

        // Assert
        ObjectDb.Received(1).SetParameterValue("IbcCustID", param.CustId);
        ObjectDb.Received(1).SetParameterValue("AffID", param.AffiliateId);
        ObjectDb.Received(1).SetParameterValue(DbParam.Username, param.Username2);
        ObjectDb.Received(1).SetParameterValue("IBCUsername", param.Username);
        ObjectDb.Received(1).SetParameterValue("DepositCustID", param.DepositUserId);
        ObjectDb.Received(1).SetParameterValue(DbParam.Currency, param.Currency);
        ObjectDb.Received(1).SetParameterValue(DbParam.Recommend, param.Recommend);
        ObjectDb.Received(1).SetParameterValue("FirstName", "");
        ObjectDb.Received(1).SetParameterValue("LastName", "");
        ObjectDb.Received(1).SetParameterValue("Email", "");
    }

    [Theory]
    [InlineData(12, "Invalid Affiliate agent.")]
    [InlineData(911, "Create customer failed. Please try again.")]
    public async Task ValidateAffiliateAsync_Should_ReturnError_WhenDBReturnError(int errorCode, string errorMessage)
    {
        // Arrange
        var affiliateId = Fixture.Create<int>();
        ObjectDb.GetOutputParams().Returns(new Hashtable
        {
            { DbParam.ErrCode, errorCode },
            { DbParam.ErrMsg, errorMessage }
        });

        var dbInfo = DbUtils.GetDatabaseInfo(ObjectDb);

        // Action
        var result = await _affiliateRepository.ValidateAffiliateAsync(affiliateId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeEquivalentTo(ServiceError.GetFromDbError(errorCode, errorMessage));
        result.ExtraInfo.Should().BeEquivalentTo(dbInfo);
    }

    [Fact]
    public async Task ValidateAffiliateAsync_Should_ReturnSuccess()
    {
        // Arrange
        var affiliateId = Fixture.Create<int>();
        ObjectDb.GetOutputParams().Returns(new Hashtable
        {
            { DbParam.ErrCode, 0 },
            { DbParam.ErrMsg, "" }
        });
        var dbInfo = DbUtils.GetDatabaseInfo(ObjectDb);

        // Action
        var result = await _affiliateRepository.ValidateAffiliateAsync(affiliateId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Error.Should().BeEquivalentTo(ServiceError.None);
        result.ExtraInfo.Should().BeEquivalentTo(dbInfo);
    }

    [Fact]
    public async Task ValidateAffiliateAsync_Should_SetParameterValueCorrect()
    {
        // Arrange
        var affiliateId = Fixture.Create<int>();

        // Action
        await _affiliateRepository.ValidateAffiliateAsync(affiliateId);

        // Assert
        ObjectDb.Received(1).SetParameterValue(DbParam.AffiliateId, affiliateId);
    }
}