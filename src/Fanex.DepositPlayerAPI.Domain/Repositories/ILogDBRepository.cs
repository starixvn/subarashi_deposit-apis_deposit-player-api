namespace Fanex.DepositPlayerAPI.Domain.Repositories;

using Entities;

public interface ILogDbRepository
{
    Task WriteLogDataDBLOGAsync(LogData logData);

    Task WriteLogDataDBCUSTAsync(LogData logData);

    Task WriteLogTrackingDBLOGAsync(LogTracking logTracking);
}