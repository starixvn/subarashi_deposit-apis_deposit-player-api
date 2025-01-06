namespace Fanex.DepositPlayerAPI.Domain.Params;

public class InsertShadowAccountSettingParam
{
    public int CustId { get; set; }

    public string Username { get; set; }

    public int AgentId { get; set; }

    public bool IsTest { get; set; }
}