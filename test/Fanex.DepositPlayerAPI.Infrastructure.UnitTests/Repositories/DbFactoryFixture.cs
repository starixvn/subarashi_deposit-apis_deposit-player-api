namespace Fanex.DepositPlayerAPI.Infrastructure.UnitTests.Repositories;

using Data;
using Infrastructure.Repositories;

public class DbFactoryFixture : IDbFactory, IDisposable
{
    public DbFactoryFixture()
    {
        DbSettingProviderManager.StartTestingSession();
    }

    public IObjectDb GetObjectDb(string spName, int timeout = 120) => ObjectDbFactory.CreateInstance(spName, timeout);

    public void Dispose() => DbSettingProviderManager.ClearSession();
    public void MockObjectDb(CreateObjectDbFunc func) => ObjectDbFactory.Mock(func);
}

[CollectionDefinition("DbFactoryFixture")]
public class DbFactoryFixtureCollection : ICollectionFixture<DbFactoryFixture>
{
    // This class has no code, and is never created. Its purpose is simply
    // to be the place to apply [CollectionDefinition] and all the
    // ICollectionFixture<> interfaces.
}