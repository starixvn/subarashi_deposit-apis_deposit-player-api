namespace Fanex.DepositPlayerAPI.Domain.Params.Chains;

using Entities;

public class MapSabaIdChainParam
{
    public int CustId { get; set; }

    public byte CustType { get; set; }

    public long SabaId { get; set; }

    public bool IsTest { get; set; }

    public LogData LogData { get; set; }
}