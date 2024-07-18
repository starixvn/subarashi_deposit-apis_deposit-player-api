namespace Fanex.DepositPlayerAPI.Domain.Entities;

public class LogTracking
{
    public int CustId { get; set; }

    public string Action { get; set; }

    public string Status { get; set; }

    public string Message { get; set; }
}