namespace Fanex.DepositPlayerAPI.Domain.Params;

public class NewPlayerAffiliateParam
{
    public int CustId { get; set; }

    public int AffiliateId { get; set; }

    public string Username2 { get; set; }

    public string Username { get; set; }

    public int DepositUserId { get; set; }

    public int Currency { get; set; }

    public int Recommend { get; set; }
}