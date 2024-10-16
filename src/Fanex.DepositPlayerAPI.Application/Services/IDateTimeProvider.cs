namespace Fanex.DepositPlayerAPI.Application.Services;

public interface IDateTimeProvider
{
    public DateTime UtcNow { get; }

    public DateTime MinValue { get; }
}