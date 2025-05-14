namespace Fanex.DepositPlayerAPI.Infrastructure.UnitTests.Repositories;

using System.Collections;
using System.Data;
using AutoFixture;
using Data;
using Infrastructure.DbSettingConfig;
using NSubstitute;
using NSubstitute.ClearExtensions;
using NSubstitute.Core;

public abstract class RepositoryTestBase : IDisposable
{
    protected readonly DbFactoryFixture DbFactoryFixture;
    protected readonly IFixture Fixture;
    protected readonly IObjectDb ObjectDb;
    protected readonly IShardDbInfoAccessor ShardDbInfoAccessor;

    protected RepositoryTestBase(DbFactoryFixture dbFactoryFixture)
    {
        Fixture = new Fixture();
        ObjectDb = Substitute.For<IObjectDb>();
        ShardDbInfoAccessor = Substitute.For<IShardDbInfoAccessor>();
        DbFactoryFixture = dbFactoryFixture;

        DbFactoryFixture.MockObjectDb((spKey, _, _) =>
        {
            ObjectDb.SettingKey.Returns(spKey);
            return ObjectDb;
        });
    }

    public virtual void Dispose()
    {
        ObjectDb.ClearSubstitute();
    }

    #region Helper

    public virtual void MapReaderData(IDataReader reader, object readerDataMapping, Dictionary<string, string>? columnMapping = null)
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

    #endregion
}