namespace Fanex.DepositPlayerAPI.Domain.Params;

public class ValidateSabaInfoParam
{
    public long SabaId { get; set; }

    public int CustId { get; set; }

    public byte CustType { get; set; }
}

public class ValidateSabaInfoOutputParam
{
    public bool IsTest { get; set; }
}