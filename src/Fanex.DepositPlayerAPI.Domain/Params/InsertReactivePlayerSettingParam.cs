namespace Fanex.DepositPlayerAPI.Domain.Params;

using Entities;

public class InsertReactivePlayerSettingParam
{
    public int CustId { get; set; }

    public string CustomerInfo { get; set; }

    public int? SubsidiaryId { get; set; }

    public string DefaultCommissions { get; set; }

    public int? GroupDiscountId { get; set; }
}

public class InsertReactivePlayerSettingOutputParam
{
    public LogTracking LogTracking { get; set; }

    public LogData LogData { get; set; }

    public decimal ClosingBalance { get; set; }

    public int CustIdBalanceDummy { get; set; }
}