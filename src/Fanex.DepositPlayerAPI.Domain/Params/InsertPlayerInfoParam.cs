namespace Fanex.DepositPlayerAPI.Domain.Params;

public class InsertPlayerInfoParam
{
    public string Username2 { get; set; }

    public int Recommend { get; set; }

    public int SiteDbGroupId { get; set; }

    public string Site { get; set; }

    public string CurrencyName { get; set; }

    public int RetryNumber { get; set; }

    public int? CustId { get; set; }
}

public class InsertPlayerInfoOutputParam
{
    public int CustId { get; set; }

    public string Username { get; set; }
}