namespace Fanex.DepositPlayerAPI.Domain.Params;

using Entities;

public class InsertNewPlayerSettingParam
{
    public int CustId { get; set; }

    public string Username { get; set; }

    public string Username2 { get; set; }

    public int DepositUserId { get; set; }

    public int Recommend { get; set; }

    public string Password { get; set; }

    public string OddsType { get; set; }

    public string Country { get; set; }

    public int? SubsidiaryId { get; set; }

    public int? AffiliateId { get; set; }

    public string DefaultCommissions { get; set; }

    public int? GroupDiscountId { get; set; }

    public string Site { get; set; }
}

public class InsertNewPlayerSettingOutputParam
{
    public int Currency { get; set; }

    public LogData LogData { get; set; }
}