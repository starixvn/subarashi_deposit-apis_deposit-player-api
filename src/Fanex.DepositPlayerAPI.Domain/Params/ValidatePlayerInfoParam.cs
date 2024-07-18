namespace Fanex.DepositPlayerAPI.Domain.Params;

public class ValidatePlayerInfoParam
{
    public string DepositUserName { get; set; }

    public int AgentId { get; set; }
}

public class ValidatePlayerInfoOutputParam
{
    public string CurrencyName { get; set; }

    public string Site { get; set; }

    public string Username2 { get; set; }

    public int Recommend { get; set; }
}