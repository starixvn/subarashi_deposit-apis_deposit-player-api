namespace Fanex.DepositPlayerAPI.Domain.UnitTests.Entities;

using AutoFixture;
using Domain.Entities;
using FluentAssertions;

public class ShadowAccountTest
{
    private readonly IFixture _fixture = new Fixture();

    [Fact]
    public void CustId_Should_SetAndGetCorrectly()
    {
        // Arrange
        var shadowAccount = new ShadowAccount();
        var custId = _fixture.Create<int>();

        // Act
        shadowAccount.CustId = custId;

        // Assert
        shadowAccount.CustId.Should().Be(custId);
    }

    [Fact]
    public void Username_Should_SetAndGetCorrectly()
    {
        // Arrange
        var shadowAccount = new ShadowAccount();
        var username = _fixture.Create<string>();

        // Act
        shadowAccount.Username = username;

        // Assert
        shadowAccount.Username.Should().Be(username);
    }

    [Fact]
    public void ShadowAccount_Should_InstantiateWithDefaultValues()
    {
        // Act
        var shadowAccount = new ShadowAccount();

        // Assert
        shadowAccount.CustId.Should().Be(0);
        shadowAccount.Username.Should().BeNull();
    }

    [Fact]
    public void ShadowAccount_Should_SupportRecordEquality()
    {
        // Arrange
        var custId = _fixture.Create<int>();
        var username = _fixture.Create<string>();
        
        var shadowAccount1 = new ShadowAccount { CustId = custId, Username = username };
        var shadowAccount2 = new ShadowAccount { CustId = custId, Username = username };
        var shadowAccount3 = new ShadowAccount { CustId = custId + 1, Username = username };

        // Assert
        shadowAccount1.Should().Be(shadowAccount2); // Records with same values are equal
        shadowAccount1.Should().NotBe(shadowAccount3); // Records with different values are not equal
    }

    [Fact]
    public void ShadowAccount_Should_SupportRecordHashCode()
    {
        // Arrange
        var custId = _fixture.Create<int>();
        var username = _fixture.Create<string>();
        
        var shadowAccount1 = new ShadowAccount { CustId = custId, Username = username };
        var shadowAccount2 = new ShadowAccount { CustId = custId, Username = username };

        // Assert
        shadowAccount1.GetHashCode().Should().Be(shadowAccount2.GetHashCode());
    }

    [Fact]
    public void ShadowAccount_Should_SupportWithExpressions()
    {
        // Arrange
        var originalCustId = _fixture.Create<int>();
        var originalUsername = _fixture.Create<string>();
        var newCustId = _fixture.Create<int>();
        
        var original = new ShadowAccount { CustId = originalCustId, Username = originalUsername };

        // Act
        var modified = original with { CustId = newCustId };

        // Assert
        modified.CustId.Should().Be(newCustId);
        modified.Username.Should().Be(originalUsername);
        original.CustId.Should().Be(originalCustId); // Original should be unchanged
    }
}