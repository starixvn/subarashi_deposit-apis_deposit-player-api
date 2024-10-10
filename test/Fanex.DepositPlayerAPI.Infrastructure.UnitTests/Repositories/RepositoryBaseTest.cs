namespace Fanex.DepositPlayerAPI.Infrastructure.UnitTests.Repositories;

using AutoFixture;
using Data;
using Infrastructure.DbSettingConfig;
using NSubstitute;
using NSubstitute.ClearExtensions;

public abstract class RepositoryBaseTest : IDisposable
{
    protected readonly DbFactoryFixture DbFactoryFixture;
    protected readonly IFixture Fixture;
    protected readonly IObjectDb ObjectDb;
    protected readonly IShardDbInfoAccessor ShardDbInfoAccessor;

    protected RepositoryBaseTest(DbFactoryFixture dbFactoryFixture)
    {
        Fixture = new Fixture();
        ObjectDb = Substitute.For<IObjectDb>();
        ShardDbInfoAccessor = Substitute.For<IShardDbInfoAccessor>();
        DbFactoryFixture = dbFactoryFixture;

        DbFactoryFixture.MockObjectDb((spKey, _) =>
        {
            ObjectDb.SettingKey.Returns(spKey);
            return ObjectDb;
        });
    }

    public virtual void Dispose()
    {
        ObjectDb.ClearSubstitute();
    }
}