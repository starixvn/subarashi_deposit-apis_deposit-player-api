namespace Fanex.DepositPlayerAPI.Domain.Errors;

public sealed record ServiceError(int Code, string Message)
{
    public static readonly ServiceError None = new(0, string.Empty);

    public static readonly ServiceError SystemError = new(999, "System error.");

    public static ServiceError GetFromDbError(int code, string message) => new(code, message);
}