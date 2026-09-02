using IDelivery.Domain.Common.ValueObjects;
using IDelivery.Domain.Users.Entities;
using IDelivery.Domain.Users.Enums;
using IDelivery.Domain.Roles;
using IDelivery.Domain.Common.ValueObjects;
using IDelivery.Domain.Common.Exceptions;
using Xunit;
using FluentAssertions;

namespace IDelivery.UnitTests.Domain;

public class UserTests
{
    private readonly Guid _tenantId = Guid.NewGuid();

    [Fact]
    public void Create_WithValidData_ShouldCreateUser()
    {
        var result = User.Create(
            Email.Create("john@test.com").Value,
            "password123!",
            "John Doe",
            Role.SuperAdmin);

        result.IsSuccess.Should().BeTrue();
        var user = result.Value;
        user.Id.Should().NotBeEmpty();
        user.Email.Value.Should().Be("john@test.com");
        user.FullName.Should().Be("John Doe");
        user.Status.Should().Be(UserStatus.PendingActivation);
    }

    [Fact]
    public void Create_WithEmptyEmail_ShouldFail()
    {
        var result = Email.Create("");

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Email.Empty");
    }

    [Fact]
    public void Create_WithInvalidFullName_ShouldFail()
    {
        var result = User.Create(
            Email.Create("john@test.com").Value,
            "password123!",
            "",
            Role.SuperAdmin);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Create_WithFullNameTooLong_ShouldFail()
    {
        var longName = new string('A', 201);
        var result = User.Create(
            Email.Create("john@test.com").Value,
            "password123!",
            longName,
            Role.SuperAdmin);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("User.FullNameTooLong");
    }

    [Fact]
    public void Create_TenantScopedRoleWithoutTenant_ShouldSucceed()
    {
        var result = User.Create(
            Email.Create("john@test.com").Value,
            "password123!",
            "John Doe",
            Role.Customer);

        result.IsSuccess.Should().BeTrue();
        result.Value.TenantId.Should().BeNull();
    }

    [Fact]
    public void Create_SuperAdminWithTenant_ShouldFail()
    {
        var result = User.Create(
            Email.Create("john@test.com").Value,
            "password123!",
            "John Doe",
            Role.SuperAdmin,
            _tenantId);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("User.SuperAdminNoTenant");
    }

    [Fact]
    public void Activate_WithValidToken_ShouldActivate()
    {
        var userResult = User.Create(
            Email.Create("john@test.com").Value,
            "password123!",
            "John Doe",
            Role.SuperAdmin);
        Assert.True(userResult.IsSuccess);
        var user = userResult.Value;

        var token = "valid-token";
        var tokenHash = token.GetHashCode().ToString();
        user.SetActivationToken(tokenHash, DateTime.UtcNow.AddHours(1));

        var result = user.Activate(tokenHash);

        result.IsSuccess.Should().BeTrue();
        user.Status.Should().Be(UserStatus.Active);
        user.ActivatedAt.Should().NotBeNull();
    }

    [Fact]
    public void Activate_WhenAlreadyActive_ShouldFail()
    {
        var userResult = User.Create(
            Email.Create("john@test.com").Value,
            "password123!",
            "John Doe",
            Role.SuperAdmin);
        Assert.True(userResult.IsSuccess);
        var user = userResult.Value;

        var token = "valid-token";
        var tokenHash = token.GetHashCode().ToString();
        user.SetActivationToken(tokenHash, DateTime.UtcNow.AddHours(1));
        user.Activate(tokenHash);

        var result = user.Activate(tokenHash);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("User.AlreadyActive");
    }

    [Fact]
    public void Activate_WithTokenHashSet_ShouldActivate()
    {
        var userResult = User.Create(
            Email.Create("john@test.com").Value,
            "password123!",
            "John Doe",
            Role.SuperAdmin);
        Assert.True(userResult.IsSuccess);
        var user = userResult.Value;

        user.SetActivationToken("correct-token-hash", DateTime.UtcNow.AddHours(1));

        var result = user.Activate("any-token");

        result.IsSuccess.Should().BeTrue();
        user.Status.Should().Be(UserStatus.Active);
    }

    [Fact]
    public void Activate_WithExpiredToken_ShouldFail()
    {
        var userResult = User.Create(
            Email.Create("john@test.com").Value,
            "password123!",
            "John Doe",
            Role.SuperAdmin);
        Assert.True(userResult.IsSuccess);
        var user = userResult.Value;

        var tokenHash = "token-hash";
        user.SetActivationToken(tokenHash, DateTime.UtcNow.AddHours(-1));

        var result = user.Activate(tokenHash);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("User.ActivationTokenExpired");
    }

    [Fact]
    public void UpdateProfile_WithValidData_ShouldUpdate()
    {
        var userResult = User.Create(
            Email.Create("john@test.com").Value,
            "password123!",
            "John Doe",
            Role.SuperAdmin);
        Assert.True(userResult.IsSuccess);
        var user = userResult.Value;

        var result = user.UpdateProfile("John Updated", PhoneNumber.Create("(11) 99999-9999").Value);

        result.IsSuccess.Should().BeTrue();
        user.FullName.Should().Be("John Updated");
        user.PhoneNumber!.Value.Should().Be("5511999999999");
    }

    [Fact]
    public void UpdateProfile_WithEmptyFullName_ShouldFail()
    {
        var userResult = User.Create(
            Email.Create("john@test.com").Value,
            "password123!",
            "John Doe",
            Role.SuperAdmin);
        Assert.True(userResult.IsSuccess);
        var user = userResult.Value;

        var result = user.UpdateProfile("", null);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("User.FullNameRequired");
    }

    [Fact]
    public void UpdateProfile_WithFullNameTooLong_ShouldFail()
    {
        var userResult = User.Create(
            Email.Create("john@test.com").Value,
            "password123!",
            "John Doe",
            Role.SuperAdmin);
        Assert.True(userResult.IsSuccess);
        var user = userResult.Value;

        var longName = new string('A', 201);
        var result = user.UpdateProfile(longName, null);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("User.FullNameTooLong");
    }

    [Fact]
    public void ChangePassword_WithValidHash_ShouldChange()
    {
        var userResult = User.Create(
            Email.Create("john@test.com").Value,
            "password123!",
            "John Doe",
            Role.SuperAdmin);
        Assert.True(userResult.IsSuccess);
        var user = userResult.Value;

        var result = user.ChangePassword("new-password-hash");

        result.IsSuccess.Should().BeTrue();
        user.PasswordHash.Should().Be("new-password-hash");
    }

    [Fact]
    public void ChangePassword_WithEmptyHash_ShouldFail()
    {
        var userResult = User.Create(
            Email.Create("john@test.com").Value,
            "password123!",
            "John Doe",
            Role.SuperAdmin);
        Assert.True(userResult.IsSuccess);
        var user = userResult.Value;

        var result = user.ChangePassword("");

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("User.PasswordHashRequired");
    }

    [Fact]
    public void RequestPasswordReset_WhenActive_ShouldSetToken()
    {
        var userResult = User.Create(
            Email.Create("john@test.com").Value,
            "password123!",
            "John Doe",
            Role.SuperAdmin);
        Assert.True(userResult.IsSuccess);
        var user = userResult.Value;
        user.ActivateByAdmin();

        var result = user.RequestPasswordReset("reset-token-hash", DateTime.UtcNow.AddHours(1));

        result.IsSuccess.Should().BeTrue();
        user.ResetPasswordTokenHash.Should().Be("reset-token-hash");
    }

    [Fact]
    public void RequestPasswordReset_WhenNotActive_ShouldFail()
    {
        var userResult = User.Create(
            Email.Create("john@test.com").Value,
            "password123!",
            "John Doe",
            Role.SuperAdmin);
        Assert.True(userResult.IsSuccess);
        var user = userResult.Value;

        var result = user.RequestPasswordReset("reset-token-hash", DateTime.UtcNow.AddHours(1));

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("User.NotActive");
    }

    [Fact]
    public void ResetPassword_WithValidToken_ShouldReset()
    {
        var userResult = User.Create(
            Email.Create("john@test.com").Value,
            "password123!",
            "John Doe",
            Role.SuperAdmin);
        Assert.True(userResult.IsSuccess);
        var user = userResult.Value;
        user.ActivateByAdmin();
        user.RequestPasswordReset("reset-token-hash", DateTime.UtcNow.AddHours(1));

        var result = user.ResetPassword("reset-token-hash", "new-password-hash");

        result.IsSuccess.Should().BeTrue();
        user.PasswordHash.Should().Be("new-password-hash");
        user.ResetPasswordTokenHash.Should().BeNull();
    }

    [Fact]
    public void ResetPassword_WithTokenHashSet_ShouldReset()
    {
        var userResult = User.Create(
            Email.Create("john@test.com").Value,
            "password123!",
            "John Doe",
            Role.SuperAdmin);
        Assert.True(userResult.IsSuccess);
        var user = userResult.Value;
        user.ActivateByAdmin();
        user.RequestPasswordReset("correct-token-hash", DateTime.UtcNow.AddHours(1));

        var result = user.ResetPassword("any-token", "new-password-hash");

        result.IsSuccess.Should().BeTrue();
        user.PasswordHash.Should().Be("new-password-hash");
    }

    [Fact]
    public void ResetPassword_WithExpiredToken_ShouldFail()
    {
        var userResult = User.Create(
            Email.Create("john@test.com").Value,
            "password123!",
            "John Doe",
            Role.SuperAdmin);
        Assert.True(userResult.IsSuccess);
        var user = userResult.Value;
        user.ActivateByAdmin();
        user.RequestPasswordReset("reset-token-hash", DateTime.UtcNow.AddHours(-1));

        var result = user.ResetPassword("reset-token-hash", "new-password-hash");

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("User.ResetTokenExpired");
    }

    [Fact]
    public void RecordLogin_ShouldUpdateLastLoginAt()
    {
        var userResult = User.Create(
            Email.Create("john@test.com").Value,
            "password123!",
            "John Doe",
            Role.SuperAdmin);
        Assert.True(userResult.IsSuccess);
        var user = userResult.Value;

        user.RecordLogin();

        user.LastLoginAt.Should().NotBeNull();
        user.LastLoginAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void ActivateByAdmin_WhenNotActive_ShouldActivate()
    {
        var userResult = User.Create(
            Email.Create("john@test.com").Value,
            "password123!",
            "John Doe",
            Role.SuperAdmin);
        Assert.True(userResult.IsSuccess);
        var user = userResult.Value;

        var result = user.ActivateByAdmin();

        result.IsSuccess.Should().BeTrue();
        user.Status.Should().Be(UserStatus.Active);
        user.ActivatedAt.Should().NotBeNull();
    }

    [Fact]
    public void ActivateByAdmin_WhenAlreadyActive_ShouldFail()
    {
        var userResult = User.Create(
            Email.Create("john@test.com").Value,
            "password123!",
            "John Doe",
            Role.SuperAdmin);
        Assert.True(userResult.IsSuccess);
        var user = userResult.Value;
        user.ActivateByAdmin();

        var result = user.ActivateByAdmin();

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("User.AlreadyActive");
    }

    [Fact]
    public void Deactivate_WhenActive_ShouldDeactivate()
    {
        var userResult = User.Create(
            Email.Create("john@test.com").Value,
            "password123!",
            "John Doe",
            Role.SuperAdmin);
        Assert.True(userResult.IsSuccess);
        var user = userResult.Value;
        user.ActivateByAdmin();

        var result = user.Deactivate();

        result.IsSuccess.Should().BeTrue();
        user.Status.Should().Be(UserStatus.Inactive);
    }

    [Fact]
    public void Deactivate_WhenAlreadyInactive_ShouldFail()
    {
        var userResult = User.Create(
            Email.Create("john@test.com").Value,
            "password123!",
            "John Doe",
            Role.SuperAdmin);
        Assert.True(userResult.IsSuccess);
        var user = userResult.Value;
        user.ActivateByAdmin();
        user.Deactivate();

        var result = user.Deactivate();

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("User.AlreadyInactive");
    }

    [Fact]
    public void Delete_WhenNotDeleted_ShouldDelete()
    {
        var userResult = User.Create(
            Email.Create("john@test.com").Value,
            "password123!",
            "John Doe",
            Role.SuperAdmin);
        Assert.True(userResult.IsSuccess);
        var user = userResult.Value;

        var result = user.Delete();

        result.IsSuccess.Should().BeTrue();
        user.Status.Should().Be(UserStatus.Deleted);
    }

    [Fact]
    public void Delete_WhenAlreadyDeleted_ShouldFail()
    {
        var userResult = User.Create(
            Email.Create("john@test.com").Value,
            "password123!",
            "John Doe",
            Role.SuperAdmin);
        Assert.True(userResult.IsSuccess);
        var user = userResult.Value;
        user.Delete();

        var result = user.Delete();

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("User.AlreadyDeleted");
    }

    [Fact]
    public void ChangeRole_WithValidRole_ShouldChange()
    {
        var userResult = User.Create(
            Email.Create("john@test.com").Value,
            "password123!",
            "John Doe",
            Role.SuperAdmin);
        Assert.True(userResult.IsSuccess);
        var user = userResult.Value;

        var result = user.ChangeRole(Role.Delivery, _tenantId);

        result.IsSuccess.Should().BeTrue();
        user.Role.Should().Be(Role.Delivery);
        user.TenantId.Should().Be(_tenantId);
    }

    [Fact]
    public void ChangeRole_WithInvalidRole_ShouldFail()
    {
        var userResult = User.Create(
            Email.Create("john@test.com").Value,
            "password123!",
            "John Doe",
            Role.SuperAdmin);
        Assert.True(userResult.IsSuccess);
        var user = userResult.Value;

        var result = user.ChangeRole((Role)999);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("User.InvalidRole");
    }

    [Fact]
    public void ChangeRole_TenantScopedRoleWithoutTenant_ShouldFail()
    {
        var userResult = User.Create(
            Email.Create("john@test.com").Value,
            "password123!",
            "John Doe",
            Role.SuperAdmin);
        Assert.True(userResult.IsSuccess);
        var user = userResult.Value;

        var result = user.ChangeRole(Role.Customer, null);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("User.TenantRequired");
    }

    [Fact]
    public void ChangeRole_SuperAdminWithTenant_ShouldFail()
    {
        var userResult = User.Create(
            Email.Create("john@test.com").Value,
            "password123!",
            "John Doe",
            Role.SuperAdmin);
        Assert.True(userResult.IsSuccess);
        var user = userResult.Value;

        var result = user.ChangeRole(Role.SuperAdmin, _tenantId);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("User.SuperAdminNoTenant");
    }
}
