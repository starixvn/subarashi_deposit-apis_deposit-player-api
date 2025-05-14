namespace Fanex.DepositPlayerAPI.Domain.Errors;

public static class PlayerError
{
    public static readonly ServiceError CreateCustomerFailed = new(911, "Create customer failed. Please try again.");
}