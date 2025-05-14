namespace Fanex.DepositPlayerAPI.Domain.Errors;

public static class SabaIdError
{
    public static readonly ServiceError InvalidMember = new(10, "Invalid member.");
}