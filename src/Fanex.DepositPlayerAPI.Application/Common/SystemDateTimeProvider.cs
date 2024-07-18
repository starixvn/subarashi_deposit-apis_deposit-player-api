namespace Fanex.DepositPlayerAPI.Application.Common;

using Interfaces;

public class SystemDateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTimeOffset.UtcNow.DateTime;

    public DateTime MinValue => DateTimeOffset.MinValue.DateTime;
}