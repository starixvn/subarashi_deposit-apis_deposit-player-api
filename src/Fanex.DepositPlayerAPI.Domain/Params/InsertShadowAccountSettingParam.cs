namespace Fanex.DepositPlayerAPI.Domain.Params;

public class InsertShadowAccountSettingParam
{
    public int CustId { get; set; }

    public string Username { get; set; }

    public string Username2 { get; set; }

    public int RealCustId { get; set; }

    public int AgentId { get; set; }
}