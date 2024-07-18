namespace Fanex.DepositPlayerAPI.Infrastructure.Repositories;

using Constants;
using Domain.Entities;
using Domain.Repositories;
using Utils;

public class ServerDbConfigRepository(IDbFactory dbFactory) : IServerDbConfigRepository
{
    public async Task<List<ServerDbConfig>> GetAllServerDbConfigAsync()
    {
        const string SpKey = "Acc_Dep_ServerDBConfigSel";

        var result = new List<ServerDbConfig>();

        using var objectDb = dbFactory.GetObjectDb(SpKey);
        using var reader = await objectDb.ExecuteReaderAsync();

        while (reader.Read())
        {
            result.Add(new ServerDbConfig
            {
                Site = Converter.ToString(reader[DbParam.Site]),
                CustId = Converter.ToInt(reader[DbParam.CustId]),
                CustDbGroupId = Converter.ToInt(reader[DbParam.CustDbGroupId]),
                SiteDbGroupId = Converter.ToInt(reader[DbParam.SiteDbGroupId]),
                SiteDbServer = Converter.ToString(reader[DbParam.SiteDbServer]),
                CustDbServer = Converter.ToString(reader[DbParam.CustDbServer])
            });
        }

        return result;
    }

    public async Task<ServerDbConfig> GetServerDbConfigByAgentIdAsync(int agentId)
    {
        const string SpKey = "Acc_Dep_ServerDBConfigSelByCust";

        using var objectDb = dbFactory.GetObjectDb(SpKey);
        objectDb.SetParameterValue(DbParam.CustId, agentId);

        using var reader = await objectDb.ExecuteReaderAsync();

        if (reader.Read())
        {
            return new ServerDbConfig
            {
                Site = Converter.ToString(reader[DbParam.Site]),
                CustId = Converter.ToInt(reader[DbParam.CustId]),
                CustDbGroupId = Converter.ToInt(reader[DbParam.CustDbGroupId]),
                SiteDbGroupId = Converter.ToInt(reader[DbParam.SiteDbGroupId]),
                SiteDbServer = Converter.ToString(reader[DbParam.SiteDbServer]),
                CustDbServer = Converter.ToString(reader[DbParam.CustDbServer])
            };
        }

        return default;
    }
}