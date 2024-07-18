namespace Fanex.DepositPlayerAPI.Domain.Params;

using Converters;
using Entities;
using Newtonsoft.Json;

public class GetArchivedPlayerInfoOutputParam
{
    [JsonConverter(typeof(RawJsonConverter))]
    public string CustomerInfo { get; set; }

    public int Recommend { get; set; }

    public LogTracking LogTracking { get; set; }
}