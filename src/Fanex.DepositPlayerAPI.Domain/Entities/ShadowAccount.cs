namespace Fanex.DepositPlayerAPI.Domain.Entities;

public record ShadowAccount
{
    public int CustId { get; set; }

    public string Username { get; set; }
}