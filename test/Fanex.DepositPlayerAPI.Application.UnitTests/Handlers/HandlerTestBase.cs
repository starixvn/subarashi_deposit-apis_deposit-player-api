namespace Fanex.DepositPlayerAPI.Application.UnitTests.Handlers;

using Application.Chains;
using AutoFixture;
using Logging;
using NSubstitute;
using NSubstitute.ClearExtensions;
using Services;

public abstract class HandlerTestBase : IDisposable
{
    protected readonly IChainHandlerFactory ChainHandlerFactory = Substitute.For<IChainHandlerFactory>();
    protected readonly IFixture Fixture = new Fixture();
    protected readonly ILogAggregate LogAggregate = Substitute.For<ILogAggregate>();
    protected readonly ILogger Logger = Substitute.For<ILogger>();

    public virtual void Dispose()
    {
        ChainHandlerFactory.ClearSubstitute();
        LogAggregate.ClearSubstitute();
        Logger.ClearSubstitute();
    }
}