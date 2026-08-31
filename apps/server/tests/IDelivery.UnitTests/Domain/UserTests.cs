using IDelivery.Domain.Users.Entities;
using IDelivery.Domain.Roles;
using Xunit;
using FluentAssertions;

namespace IDelivery.UnitTests.Domain;

public class UserTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateUser()
    {
        var result = User.Create(
            "john@test.com",
            "password123!",
            "John Doe",
            Role.SuperAdmin);

        result.IsSuccess.Should().BeTrue();
        var user = result.Value;
        user.Id.Should().NotBeEmpty();
        user.Email.Value.Should().Be("john@test.com");
        user.FullName.Should().Be("John Doe");
        user.Status.Should().Be(IDelivery.Domain.Users.Enums.UserStatus.PendingActivation);
    }

    [Fact]
    public void Create_WithEmptyEmail_ShouldFail()
    {
        var result = User.Create(
            "",
            "password123!",
            "John Doe",
            Role.SuperAdmin);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Create_WithInvalidFullName_ShouldFail()
    {
        var result = User.Create(
            "john@test.com",
            "password123!",
            "",
            Role.SuperAdmin);

        result.IsFailure.Should().BeTrue();
    }
}