namespace Fanex.DepositPlayerAPI.Domain.Extensions;

using Errors;
using Results;

public static class ServiceResultExtensions
{
    public static T1 Match<T1, T2>(
        this ServiceResult<T2> result,
        Func<string, T1> onSuccess,
        Func<ServiceErrors, string, T1> onFailure) where T1 : class where T2 : class
    {
        return result.IsSuccess ? onSuccess(result.ExtraInfo) : onFailure(result.Error, result.ExtraInfo);
    }

    public static T1 Match<T1>(
        this ServiceResult result,
        Func<string, T1> onSuccess,
        Func<ServiceErrors, string, T1> onFailure) where T1 : class
    {
        return result.IsSuccess ? onSuccess(result.ExtraInfo) : onFailure(result.Error, result.ExtraInfo);
    }
}