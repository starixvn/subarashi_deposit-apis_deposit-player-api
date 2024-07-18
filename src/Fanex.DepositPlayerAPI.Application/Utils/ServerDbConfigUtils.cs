namespace Fanex.DepositPlayerAPI.Application.Utils;

using Domain.Entities;
using Extensions;
using Microsoft.AspNetCore.Http;

public static class ServerDbConfigUtils
{
    public static void SetServerDbConfigToHttpContext(ServerDbConfig config, IHttpContextAccessor contextAccessor)
    {
        contextAccessor.HttpContext.SetCustDbGroupId(config.CustDbGroupId);
        contextAccessor.HttpContext.SetSiteDbGroupId(config.SiteDbGroupId);
        contextAccessor.HttpContext.SetCustDbServer(config.CustDbServer);
        contextAccessor.HttpContext.SetSiteDbServer(config.SiteDbServer);
        contextAccessor.HttpContext.SetSite(config.Site);
    }

    public static ServerDbConfig GetServerDbConfigFromHttpContext(IHttpContextAccessor contextAccessor)
    {
        return new ServerDbConfig
        {
            CustDbGroupId = contextAccessor.HttpContext.GetCustDbGroupId(),
            CustDbServer = contextAccessor.HttpContext.GetCustDbServer(),
            Site = contextAccessor.HttpContext.GetSite(),
            SiteDbGroupId = contextAccessor.HttpContext.GetSiteDbGroupId(),
            SiteDbServer = contextAccessor.HttpContext.GetSiteDbServer()
        };
    }
}