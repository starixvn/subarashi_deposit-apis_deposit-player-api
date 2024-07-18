namespace Fanex.DepositPlayerAPI.Domain.Params;

public class InsertDefaultCommissionParam
{
    public string Site { get; set; }

    public int CustId { get; set; }

    public int? GroupDiscountId { get; set; }
}