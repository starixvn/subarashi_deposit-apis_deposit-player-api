namespace Fanex.DepositPlayerAPI.Domain.Params;

public class InitializeReactivatePlayerBalanceParam
{
    public int CustId { get; set; }

    public int CustIdBalanceDummy { get; set; }

    public decimal ClosingBalance { get; set; }
}