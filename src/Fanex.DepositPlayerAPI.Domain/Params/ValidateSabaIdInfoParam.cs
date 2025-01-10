namespace Fanex.DepositPlayerAPI.Domain.Params;

public class ValidateSabaIdInfoParam
{
    public long SabaId { get; set; }

    public int CustId { get; set; }

    public byte CustType { get; set; }
}

public class ValidateSabaIdInfoOutputParam
{
    public bool IsTest { get; set; }
}