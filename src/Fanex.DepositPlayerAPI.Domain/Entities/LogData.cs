namespace Fanex.DepositPlayerAPI.Domain.Entities;

using Converters;
using Newtonsoft.Json;

public class LogData
{
    public int UserId { get; set; }

    public int CustId { get; set; }

    public int ActionId { get; set; }

    [JsonConverter(typeof(RawJsonConverter))]
    public string TrackingValue { get; set; }

    [JsonConverter(typeof(RawJsonConverter))]
    public string TrackingValue2 { get; set; }
}