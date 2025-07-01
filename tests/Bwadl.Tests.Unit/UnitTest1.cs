using AutoFixture;
using AutoFixture.Xunit2;
using Bwadl.Domain.Entities;
using Bwadl.Domain.Exceptions;
using Bwadl.Domain.ValueObjects;
using FluentAssertions;

namespace Bwadl.Tests.Unit;

public class UserEntityTests
{
    private readonly Fixture _fixture = new();

    [Fact]
    public void User_Constructor_Should_Create_Valid_User()
    {
        // Arrange
        var name = "John Doe";
        var email = "john.doe@example.com";
        var type = UserType.Admin;

        // Act
        var user = new User(name, email, type);

        // Assert
        user.Id.Should().NotBeEmpty();
        user.Name.Should().Be(name);
        user.Email.Should().Be(email);
        user.Type.Should().Be(type);
        user.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        user.UpdatedAt.Should().BeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void User_Constructor_Should_Throw_ArgumentException_For_Invalid_Name(string invalidName)
    {
        // Arrange
        var email = "test@example.com";
        var type = UserType.Employee;

        // Act & Assert
        var action = () => new User(invalidName, email, type);
        action.Should().Throw<ArgumentException>()
            .WithParameterName("name");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void User_Constructor_Should_Throw_ArgumentException_For_Invalid_Email(string invalidEmail)
    {
        // Arrange
        var name = "John Doe";
        var type = UserType.Employee;

        // Act & Assert
        var action = () => new User(name, invalidEmail, type);
        action.Should().Throw<ArgumentException>()
            .WithParameterName("email");
    }

    [Fact]
    public void UpdateName_Should_Update_Name_And_UpdatedAt()
    {
        // Arrange
        var user = _fixture.Create<User>();
        var newName = "Updated Name";
        var originalUpdatedAt = user.UpdatedAt;

        // Act
        user.UpdateName(newName);

        // Assert
        user.Name.Should().Be(newName);
        user.UpdatedAt.Should().NotBe(originalUpdatedAt);
        user.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void UpdateEmail_Should_Update_Email_And_UpdatedAt()
    {
        // Arrange
        var user = _fixture.Create<User>();
        var newEmail = "updated@example.com";
        var originalUpdatedAt = user.UpdatedAt;

        // Act
        user.UpdateEmail(newEmail);

        // Assert
        user.Email.Should().Be(newEmail);
        user.UpdatedAt.Should().NotBe(originalUpdatedAt);
        user.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void UpdateType_Should_Update_Type_And_UpdatedAt()
    {
        // Arrange
        var user = _fixture.Create<User>();
        var newType = UserType.Manager;
        var originalUpdatedAt = user.UpdatedAt;

        // Act
        user.UpdateType(newType);

        // Assert
        user.Type.Should().Be(newType);
        user.UpdatedAt.Should().NotBe(originalUpdatedAt);
        user.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }
}