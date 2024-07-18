namespace Fanex.DepositPlayerAPI.Infrastructure.UnitTests.Utils;

using System.Collections;
using System.Data;
using NSubstitute;
using NSubstitute.Core;

public static class DbTestUtils
{
    public static void MapReaderData(IDataReader reader, object readerDataMapping, Dictionary<string, string>? columnMapping = null)
    {
        var func = CreateMappingFunc(readerDataMapping, columnMapping);

        reader[Arg.Any<string>()].Returns(func);
    }

    private static Func<CallInfo, object> CreateMappingFunc(object mappingData, Dictionary<string, string>? columnMapping)
    {
        switch (mappingData)
        {
            case IEnumerable:
            {
                var index = 0;
                HashSet<string> paramSet = [];

                return x =>
                {
                    var paramName = x.ArgAt<string>(0);

                    if (!paramSet.Add(paramName))
                    {
                        index++;
                        paramSet.Clear();
                        paramSet.Add(paramName);
                    }

                    var elementMappingData = ((IEnumerable<object>)mappingData).ToArray()[index];
                    var dictionary = ToDictionary(elementMappingData, columnMapping);

                    return dictionary.TryGetValue(paramName, out var value) ? value : string.Empty;
                };
            }

            default:
                return x =>
                {
                    var paramName = x.ArgAt<string>(0);
                    var dictionary = ToDictionary(mappingData, columnMapping);
                    return dictionary.TryGetValue(paramName, out var value) ? value : string.Empty;
                };
        }
    }

    private static IDictionary<string, object> ToDictionary(object data, Dictionary<string, string>? columnMapping)
    {
        if (data is IDictionary)
        {
            return (IDictionary<string, object>)data;
        }

        var type = data.GetType();
        var properties = type.GetProperties();
        return properties.ToDictionary(
            x => columnMapping is not null && columnMapping.TryGetValue(x.Name, out var columnName) ? columnName : x.Name,
            x => x.GetValue(data, null),
            StringComparer.OrdinalIgnoreCase)!;
    }
}