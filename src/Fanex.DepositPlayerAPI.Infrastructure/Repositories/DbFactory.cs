namespace Fanex.DepositPlayerAPI.Infrastructure.Repositories;

using Data;

public class DbFactory : IDbFactory
{
    public IObjectDb GetObjectDb(string spName, int timeout = 120)
        => ObjectDbFactory.CreateInstance(spName, timeout);
}