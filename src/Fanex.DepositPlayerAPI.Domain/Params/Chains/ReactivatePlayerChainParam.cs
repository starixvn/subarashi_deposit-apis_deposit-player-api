namespace Fanex.DepositPlayerAPI.Domain.Params.Chains;

using Converters;
using Entities;
using Newtonsoft.Json;

public class ReactivatePlayerChainParam
{
    public int CustId { get; set; }

    public int? SubsidiaryId { get; set; }

    public string Site { get; set; }

    public int CustIdBalanceDummy { get; set; }

    public decimal ClosingBalance { get; set; }

    public int Recommend { get; set; }

    [JsonConverter(typeof(RawJsonConverter))]
    public string CustomerInfo { get; set; }

    [JsonConverter(typeof(RawJsonConverter))]
    public string DefaultCommissions { get; set; }

    public int? GroupDiscountId { get; set; }

    public LogData LogData { get; set; }

    public LogTracking LogTracking { get; set; }
}