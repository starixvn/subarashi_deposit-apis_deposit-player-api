namespace Fanex.DepositPlayerAPI.Domain.Params;

public class InsertShadowAccountInfoParam
{
    public int RealCustId { get; set; }

    public int AgentId { get; set; }

    public int SiteDbGroupId { get; set; }
}

public class InsertShadowAccountInfoOutputParam
{
    public string Username { get; set; }

    public string Username2 { get; set; }

    public int CustId { get; set; }
}