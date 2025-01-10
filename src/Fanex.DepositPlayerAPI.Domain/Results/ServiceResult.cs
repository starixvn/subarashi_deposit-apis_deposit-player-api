namespace Fanex.DepositPlayerAPI.Domain.Results;

using Errors;
using Newtonsoft.Json;

public class ServiceResult
{
    protected ServiceResult(bool isSuccess, ServiceError error, string extraInfo)
    {
        if ((isSuccess && error != ServiceError.None) ||
            (!isSuccess && error == ServiceError.None))
        {
            throw new ArgumentException("Invalid error", nameof(error));
        }

        IsSuccess = isSuccess;
        Error = error;
        ExtraInfo = extraInfo;
    }

    [JsonIgnore]
    public bool IsSuccess { get; }

    [JsonIgnore]
    public bool IsFailure => !IsSuccess;

    public ServiceError Error { get; }

    [JsonIgnore]
    public string ExtraInfo { get; set; }

    public static ServiceResult Success(string extraInfo = default) => new(true, ServiceError.None, extraInfo);

    public static ServiceResult Failure(ServiceError error, string extraInfo = default) => new(false, error, extraInfo);
}

public class ServiceResult<T>(bool isSuccess, ServiceError error, T result, string extraInfo)
    : ServiceResult(isSuccess, error, extraInfo)
    where T : class
{
    public T Result { get; } = result;

    public static ServiceResult<T> Success(T result, string extraInfo = default) => new(true, ServiceError.None, result, extraInfo);

    public new static ServiceResult<T> Failure(ServiceError error, string extraInfo = default) => new(false, error, default, extraInfo);

    public static ServiceResult<T> FailureWithData(ServiceError error, T result, string extraInfo = default) => new(false, error, result, extraInfo);
}