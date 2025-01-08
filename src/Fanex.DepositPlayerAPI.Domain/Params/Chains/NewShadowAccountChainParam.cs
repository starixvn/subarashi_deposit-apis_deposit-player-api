namespace Fanex.DepositPlayerAPI.Domain.Params.Chains;

using Entities;

public class NewShadowAccountChainParam
{
    public int AgentId { get; set; }

    public int SiteDbGroupId { get; set; }

    public string Username { get; set; }

    public int CustId { get; set; }

    public LogData LogData { get; set; }
}