namespace Fanex.DepositPlayerAPI.Infrastructure.Services;

using Application.Services;

public class SystemDateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTimeOffset.UtcNow.DateTime;

    public DateTime MinValue => DateTimeOffset.MinValue.DateTime;
}