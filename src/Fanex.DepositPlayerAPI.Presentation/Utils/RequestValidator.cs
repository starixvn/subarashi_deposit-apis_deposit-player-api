namespace Fanex.DepositPlayerAPI.Presentation.Utils;

using System.ComponentModel.DataAnnotations;

public static class RequestValidator
{
    public static (bool isValid, List<ValidationResult> results) IsValidRequest(object request)
    {
        var validationContext = new ValidationContext(request);
        var validationResults = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(request, validationContext, validationResults, true);

        return (isValid, validationResults);
    }

    public static string GetInvalidParamList(List<ValidationResult> results)
    {
        return string.Join(", ", results.Select(x => string.Join(", ", x.MemberNames)));
    }
}