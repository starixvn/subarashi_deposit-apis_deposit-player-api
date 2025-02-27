namespace Fanex.DepositPlayerAPI.Infrastructure.UnitTests.Utils;

using System.Collections;
using System.Data;
using Constants;
using Data;
using FluentAssertions;
using Infrastructure.Utils;
using Microsoft.Data.SqlClient;
using NSubstitute;

public class DbUtilsTest
{
    [Fact]
    public void GetErrorOutputParameters_Should_ReturnExpectedResult()
    {
        // Arrange
        var output = new Hashtable
        {
            { DbParam.ErrCode, 123 },
            { DbParam.ErrMsg, "Error message" }
        };

        // Act
        var result = DbUtils.GetErrorOutputParameters(output);

        // Assert
        result.code.Should().Be(123);
        result.message.Should().Be("Error message");
    }

    [Fact]
    public void GetDatabaseInfo_Should_ReturnExpectedResult()
    {
        // Arrange
        var dbSetting = Substitute.For<IDbSetting>();
        dbSetting.ConnectionString.Returns("Server=myServer;Database=myDb;User Id=myUser;Password=myPass;");
        dbSetting.DbName.Returns("myDb");
        dbSetting.SpName.Returns("mySp");

        var objectDb = Substitute.For<IObjectDb>();
        objectDb.DbSetting.Returns(dbSetting);
        objectDb.GetAddedParameters().Returns([
            new SqlParameter { ParameterName = "@param1", Value = "value1", Direction = ParameterDirection.Input },
            new SqlParameter { ParameterName = "@param2", Value = "value2", Direction = ParameterDirection.Input }
        ]);
        objectDb.GetOutputParams().Returns(new Hashtable
        {
            { DbParam.ErrCode, 123 },
            { DbParam.ErrMsg, "Error message" }
        });

        // Act
        var result = DbUtils.GetDatabaseInfo(objectDb);

        // Assert
        result.Should().Contain("Server: myServer");
        result.Should().Contain("DbName: myDb");
        result.Should().Contain("SpName: mySp");
        result.Should().Contain("Parameters: @param1=value1;@param2=value2");
        result.Should().Contain("OutputParameters: @ErrCode=123;@ErrMsg=Error message;@ErrExtraInfo=\"\"");
    }
}