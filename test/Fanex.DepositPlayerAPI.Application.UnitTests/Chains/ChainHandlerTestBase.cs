namespace Fanex.DepositPlayerAPI.Application.UnitTests.Chains;

using AutoFixture;
using Domain.Entities;
using Logging;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using NSubstitute.ClearExtensions;
using Services;

public abstract class ChainHandlerTestBase : IDisposable
{
    protected readonly IFixture Fixture = new Fixture();
    protected readonly ILogAggregate LogAggregate = Substitute.For<ILogAggregate>();
    protected readonly ILogger Logger = Substitute.For<ILogger>();

    public virtual void Dispose()
    {
        Logger.ClearSubstitute();
        LogAggregate.ClearSubstitute();
    }

    protected void AssertReceivedHttpContextAccessor(ServerDbConfig config, IHttpContextAccessor httpContextAccessorStub)
    {
        if (httpContextAccessorStub.HttpContext is null)
        {
            return;
        }

        httpContextAccessorStub.HttpContext.Items.Received(1)["CustDbGroupIdKey"] = config.CustDbGroupId;
        httpContextAccessorStub.HttpContext.Items.Received(1)["SiteDbGroupIdKey"] = config.SiteDbGroupId;
        httpContextAccessorStub.HttpContext.Items.Received(1)["CustDbServerKey"] = config.CustDbServer;
        httpContextAccessorStub.HttpContext.Items.Received(1)["SiteDbServerKey"] = config.SiteDbServer;
        httpContextAccessorStub.HttpContext.Items.Received(1)["SiteKey"] = config.Site;
    }

    protected void AssertDidNotReceivedHttpContextAccessor(ServerDbConfig config, IHttpContextAccessor httpContextAccessorStub)
    {
        if (httpContextAccessorStub.HttpContext is null)
        {
            return;
        }

        httpContextAccessorStub.HttpContext.Items.DidNotReceive()["CustDbGroupIdKey"] = config.CustDbGroupId;
        httpContextAccessorStub.HttpContext.Items.DidNotReceive()["SiteDbGroupIdKey"] = config.SiteDbGroupId;
        httpContextAccessorStub.HttpContext.Items.DidNotReceive()["CustDbServerKey"] = config.CustDbServer;
        httpContextAccessorStub.HttpContext.Items.DidNotReceive()["SiteDbServerKey"] = config.SiteDbServer;
        httpContextAccessorStub.HttpContext.Items.DidNotReceive()["SiteKey"] = config.Site;
    }
}