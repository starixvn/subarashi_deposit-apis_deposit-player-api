namespace Fanex.DepositPlayerAPI.Domain.Params.Chains;

using Converters;
using Entities;
using Newtonsoft.Json;

public class NewPlayerChainParam
{
    public int CustId { get; set; }

    public string DepositUserName { get; set; }

    public int DepositUserId { get; set; }

    public string Username { get; set; }

    public string Username2 { get; set; }

    public int UserID { get; set; }

    public string Password { get; set; }

    public string OddsType { get; set; }

    public int AgentId { get; set; }

    public int? AffiliateId { get; set; }

    public string Country { get; set; }

    public int? SubsidiaryId { get; set; }

    public int Recommend { get; set; }

    public string CurrencyName { get; set; }

    public int Currency { get; set; }

    public string Site { get; set; }

    public int SiteDbGroupId { get; set; }

    [JsonConverter(typeof(RawJsonConverter))]
    public string DefaultCommissions { get; set; }

    public int? GroupDiscountId { get; set; }

    public LogData LogData { get; set; }
}