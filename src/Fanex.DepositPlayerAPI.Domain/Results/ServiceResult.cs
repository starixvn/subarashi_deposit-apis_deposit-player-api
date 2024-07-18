namespace Fanex.DepositPlayerAPI.Domain.Results;

using Errors;
using Newtonsoft.Json;

public class ServiceResult
{
    protected ServiceResult(bool isSuccess, ServiceErrors error, string extraInfo)
    {
        if ((isSuccess && error != ServiceErrors.None) ||
            (!isSuccess && error == ServiceErrors.None))
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

    public ServiceErrors Error { get; }

    [JsonIgnore]
    public string ExtraInfo { get; set; }

    public static ServiceResult Success(string extraInfo = default) => new(true, ServiceErrors.None, extraInfo);

    public static ServiceResult Failure(ServiceErrors error, string extraInfo = default) => new(false, error, extraInfo);
}

public class ServiceResult<T>(bool isSuccess, ServiceErrors error, T result, string extraInfo)
    : ServiceResult(isSuccess, error, extraInfo)
    where T : class
{
    public T Result { get; } = result;

    public static ServiceResult<T> Success(T result, string extraInfo = default) => new(true, ServiceErrors.None, result, extraInfo);

    public new static ServiceResult<T> Failure(ServiceErrors error, string extraInfo = default) => new(false, error, default, extraInfo);

    public static ServiceResult<T> FailureWithData(ServiceErrors error, T result, string extraInfo = default) => new(false, error, result, extraInfo);
}