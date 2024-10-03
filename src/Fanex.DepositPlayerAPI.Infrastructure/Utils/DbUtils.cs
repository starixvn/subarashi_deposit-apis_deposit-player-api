namespace Fanex.DepositPlayerAPI.Infrastructure.Utils;

using System.Collections;
using System.Data;
using System.Data.Common;
using System.Reflection;
using System.Text;
using Constants;
using Data;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;

public static class DbUtils
{
    public static (int code, string message) GetErrorOutputParameters(Hashtable output)
    {
        var errorCode = Converter.ToInt(output[DbParam.ErrCode]);
        var errorMessage = Converter.ToString(output[DbParam.ErrMsg]);

        return (errorCode, errorMessage);
    }

    public static string GetDatabaseInfo(IObjectDb objectDb, object output = null, DbParameter[] otherAddedParameters = null)
    {
        var stringBuilder = new StringBuilder();
        var connString = new SqlConnectionStringBuilder(objectDb.DbSetting.ConnectionString);
        stringBuilder.AppendLine($"Server: {connString.DataSource}");
        stringBuilder.AppendLine($"DbName: {objectDb.DbSetting.DbName}");
        stringBuilder.AppendLine($"SpName: {objectDb.DbSetting.SpName}");
        stringBuilder.AppendLine($"Parameters: {GetParameters(objectDb.GetAddedParameters(), otherAddedParameters)}");
        stringBuilder.AppendLine($"OutputParameters: {GetOutputParameters(objectDb.GetOutputParams(), output)}");
        return stringBuilder.ToString();
    }

    public static DbParameter[] CaptureInputOutputParams(IObjectDb objectDb, string[] parameterNames)
    {
        if (parameterNames.Length == 0)
        {
            return [];
        }

        var addedParameter = objectDb.GetAddedParameters();

        return addedParameter
            .Where(x => x.Direction == ParameterDirection.InputOutput)
            .Where(x => Array.Exists(parameterNames, name => x.ParameterName.Contains(name, StringComparison.OrdinalIgnoreCase)))
            .Select(x => new SqlParameter
            {
                Direction = ParameterDirection.Input,
                ParameterName = x.ParameterName,
                Value = x.Value
            })
            .ToArray<DbParameter>();
    }

    private static string GetParameters(DbParameter[] parameters, DbParameter[] otherParameters)
    {
        List<DbParameter> allParameters = [];

        if (parameters is not null && parameters.Length > 0)
        {
            allParameters.AddRange(parameters);
        }

        if (otherParameters is not null && otherParameters.Length > 0)
        {
            allParameters.AddRange(otherParameters);
        }

        return allParameters.Count > 0
            ? allParameters
                .Where(param => param.Direction == ParameterDirection.Input)
                .Select(parameter => $"{parameter.ParameterName}={parameter.Value}")
                .Aggregate((firstPair, secondPair) => $"{firstPair};{secondPair}")
            : string.Empty;
    }

    private static string GetOutputParameters(Hashtable parameters, object output)
    {
        var outputParameters = GetErrorParameters(parameters);

        if (output is not null)
        {
            var outputType = output.GetType();
            IList<PropertyInfo> props = new List<PropertyInfo>(outputType.GetProperties());

            foreach (var prop in props)
            {
                var propValue = prop.GetValue(output, null);

                if (propValue is not null && propValue.GetType().IsClass)
                {
                    var value = JsonConvert.SerializeObject(propValue);
                    outputParameters.Add($"@{prop.Name}={value}");
                }
                else
                {
                    outputParameters.Add($"@{prop.Name}={propValue}");
                }
            }
        }

        return outputParameters.Aggregate((firstPair, secondPair) => $"{firstPair};{secondPair}");
    }

    private static List<string> GetErrorParameters(Hashtable parameters)
    {
        List<string> outputParameters = [];

        var code = Converter.ToInt(parameters[DbParam.ErrCode]);
        var message = Converter.ToString(parameters[DbParam.ErrMsg]);
        var extraInfo = Converter.ToString(parameters[DbParam.ErrExtraInfo]);

        outputParameters.Add($"@ErrCode={code}");
        outputParameters.Add($"@ErrMsg={message}");
        outputParameters.Add($"@ErrExtraInfo={JsonConvert.SerializeObject(extraInfo)}");

        return outputParameters;
    }
}