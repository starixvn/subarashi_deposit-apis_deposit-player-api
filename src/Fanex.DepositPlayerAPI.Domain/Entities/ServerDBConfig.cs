namespace Fanex.DepositPlayerAPI.Domain.Entities;

public class ServerDbConfig
{
    public string Site { get; set; }

    public long CustId { get; set; }

    public int CustDbGroupId { get; set; }

    public int SiteDbGroupId { get; set; }

    public string CustDbServer { get; set; }

    public string SiteDbServer { get; set; }
}