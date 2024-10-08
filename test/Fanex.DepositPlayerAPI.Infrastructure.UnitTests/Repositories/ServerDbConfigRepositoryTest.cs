namespace Fanex.DepositPlayerAPI.Infrastructure.UnitTests.Repositories;

using System.Data;
using AutoFixture;
using Constants;
using Domain.Entities;
using Domain.Repositories;
using FluentAssertions;
using Infrastructure.Repositories;
using NSubstitute;
using Utils;

[Collection("DbFactoryFixture")]
public class ServerDbConfigRepositoryTest : RepositoryBaseTest
{
    private static readonly Dictionary<string, string> ColumnMapping = new()
    {
        { "SiteDbServer", DbParam.SiteDbServer },
        { "CustDbServer", DbParam.CustDbServer }
    };

    private readonly IDataReader _dataReader;
    private readonly IServerDbConfigRepository _serverDbConfigRepository;

    public ServerDbConfigRepositoryTest(DbFactoryFixture dbFactoryFixture) : base(dbFactoryFixture)
    {
        _dataReader = Substitute.For<IDataReader>();
        _serverDbConfigRepository = new ServerDbConfigRepository(dbFactoryFixture);

        ObjectDb.ExecuteReaderAsync().Returns(_dataReader);
    }

    [Fact]
    public async Task GetAllServerDbConfigAsync_Should_ReturnEmpty_WhenDBReturnEmpty()
    {
        // Arrange
        _dataReader.Read().Returns(false);

        // Action
        var result = await _serverDbConfigRepository.GetAllServerDbConfigAsync();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllServerDbConfigAsync_Should_ReturnData_WhenDBReturnData()
    {
        // Arrange
        var totalRows = 5;
        var data = Fixture.CreateMany<ServerDbConfig>(totalRows);
        _dataReader.Read().Returns(_ => totalRows-- > 0);
        DbTestUtils.MapReaderData(_dataReader, data, ColumnMapping);

        // Action
        var result = await _serverDbConfigRepository.GetAllServerDbConfigAsync();

        // Asserts
        result.Should().BeEquivalentTo(data.ToDictionary(x => x.CustId, x => x));
    }

    [Fact]
    public async Task GetServerDbConfigByAgentIdAsync_Should_SetParameterValueCorrect()
    {
        // Arrange
        var agentId = Fixture.Create<int>();

        // Action
        await _serverDbConfigRepository.GetServerDbConfigByAgentIdAsync(agentId);

        // Assert
        ObjectDb.Received(1).SetParameterValue(DbParam.CustId, agentId);
    }

    [Fact]
    public async Task GetServerDbConfigByAgentIdAsync_Should_ReturnDefault_WhenDBReturnEmpty()
    {
        // Arrange
        var agentId = Fixture.Create<int>();
        _dataReader.Read().Returns(false);

        // Action
        var result = await _serverDbConfigRepository.GetServerDbConfigByAgentIdAsync(agentId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetServerDbConfigByAgentIdAsync_Should_ReturnData_WhenDBReturnData()
    {
        // Arrange
        var agentId = Fixture.Create<int>();
        var data = Fixture.Create<ServerDbConfig>();
        _dataReader.Read().Returns(true);
        DbTestUtils.MapReaderData(_dataReader, data, ColumnMapping);

        // Action
        var result = await _serverDbConfigRepository.GetServerDbConfigByAgentIdAsync(agentId);

        // Assert
        result.Should().BeEquivalentTo(data);
    }
}