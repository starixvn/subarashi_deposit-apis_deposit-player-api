namespace Fanex.DepositPlayerAPI.Domain.Params;

public class MapSabaInfoParam
{
    public int CustId { get; set; }

    public long SabaId { get; set; }

    public byte CustType { get; set; }

    public bool IsTest { get; set; }
}