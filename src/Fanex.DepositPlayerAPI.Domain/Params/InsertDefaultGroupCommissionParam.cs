namespace Fanex.DepositPlayerAPI.Domain.Params;

using Converters;
using Newtonsoft.Json;

public class InsertDefaultGroupCommissionParam
{
    public string Site { get; set; }

    public string CommissionProducts { get; set; }
}

public class InsertDefaultGroupCommissionOutputParam
{
    [JsonConverter(typeof(RawJsonConverter))]
    public string DefaultCommissions { get; set; }

    public int GroupDiscountId { get; set; }
}