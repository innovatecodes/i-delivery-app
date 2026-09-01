using IDelivery.Application.Commands.Auth;
using IDelivery.Application.Abstractions.Persistence;
using IDelivery.Application.Abstractions.Security;
using IDelivery.Application.Abstractions.Services;
using IDelivery.Application.Common.Models;
using IDelivery.Domain.Users.Entities;
using IDelivery.Domain.Users.Enums;
using IDelivery.Domain.Roles;
using FluentAssertions;
using Moq;
using System;
using System.Threading;
using Xunit;

namespace IDelivery.UnitTests.Application;

public class RegisterCommandHandlerTests
{
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<IPasswordHasher> _mockPasswordHasher;
    private readonly Mock<ISecureTokenGenerator> _mockTokenGenerator;
    private readonly Mock<ITokenHasher> _mockTokenHasher;
    private readonly Mock<IEmailService> _mockEmailService;

    public RegisterCommandHandlerTests()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _mockPasswordHasher = new Mock<IPasswordHasher>();
        _mockTokenGenerator = new Mock<ISecureTokenGenerator>();
        _mockTokenHasher = new Mock<ITokenHasher>();
        _mockEmailService = new Mock<IEmailService>();
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldRegisterUser()
    {
        _mockPasswordHasher.Setup(x => x.Hash(It.IsAny<string>())).Returns("hashed-password");
        _mockTokenGenerator.Setup(x => x.Generate(32)).Returns("activation-token");
        _mockTokenHasher.Setup(x => x.Hash("activation-token")).Returns("token-hash");

        var command = new RegisterCommand(
            "john@test.com",
            "Password123!",
            "John Doe",
            "(11) 99999-9999");

        var handler = new RegisterCommandHandler(
            _mockUserRepository.Object,
            _mockPasswordHasher.Object,
            _mockTokenGenerator.Object,
            _mockTokenHasher.Object,
            _mockEmailService.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
        _mockUserRepository.Verify(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockEmailService.Verify(x => x.SendActivationEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithExistingEmail_ShouldFail()
    {
        _mockUserRepository.Setup(x => x.ExistsByEmailAsync("existing@test.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var command = new RegisterCommand(
            "existing@test.com",
            "Password123!",
            "John Doe");

        var handler = new RegisterCommandHandler(
            _mockUserRepository.Object,
            _mockPasswordHasher.Object,
            _mockTokenGenerator.Object,
            _mockTokenHasher.Object,
            _mockEmailService.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("User.EmailAlreadyExists");
    }
}

public class RegisterCommandValidatorTests
{
    [Fact]
    public void Validate_WithValidData_ShouldPass()
    {
        var command = new RegisterCommand(
            "john@test.com",
            "Password123!",
            "John Doe");

        var validator = new RegisterCommandValidator();

        var result = validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithEmptyEmail_ShouldFail()
    {
        var command = new RegisterCommand(
            "",
            "Password123!",
            "John Doe");

        var validator = new RegisterCommandValidator();

        var result = validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCountGreaterThan(0);
    }

    [Fact]
    public void Validate_WithInvalidEmail_ShouldFail()
    {
        var command = new RegisterCommand(
            "invalid-email",
            "Password123!",
            "John Doe");

        var validator = new RegisterCommandValidator();

        var result = validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCountGreaterThan(0);
    }

    [Fact]
    public void Validate_WithWeakPassword_ShouldFail()
    {
        var command = new RegisterCommand(
            "john@test.com",
            "weak",
            "John Doe");

        var validator = new RegisterCommandValidator();

        var result = validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCountGreaterThan(0);
    }

    [Fact]
    public void Validate_WithEmptyFullName_ShouldFail()
    {
        var command = new RegisterCommand(
            "john@test.com",
            "Password123!",
            "");

        var validator = new RegisterCommandValidator();

        var result = validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCountGreaterThan(0);
    }
}

public class LoginCommandHandlerTests
{
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<IPasswordHasher> _mockPasswordHasher;
    private readonly Mock<IJwtTokenService> _mockJwtTokenService;
    private readonly Mock<ISecureTokenGenerator> _mockTokenGenerator;
    private readonly Mock<ITokenHasher> _mockTokenHasher;

    public LoginCommandHandlerTests()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _mockPasswordHasher = new Mock<IPasswordHasher>();
        _mockJwtTokenService = new Mock<IJwtTokenService>();
        _mockTokenGenerator = new Mock<ISecureTokenGenerator>();
        _mockTokenHasher = new Mock<ITokenHasher>();
    }

    [Fact]
    public async Task Handle_WithValidCredentials_ShouldReturnAuthResult()
    {
        var userResult = User.Create("john@test.com", "password123!", "John Doe", Role.Customer, Guid.NewGuid());
        Assert.True(userResult.IsSuccess);
        var user = userResult.Value;
        user.ActivateByAdmin();

        _mockUserRepository.Setup(x => x.GetByEmailAsync("john@test.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _mockPasswordHasher.Setup(x => x.Verify("Password123!", user.PasswordHash!)).Returns(true);
        _mockJwtTokenService.Setup(x => x.GenerateAccessToken(user.Id, user.TenantId, It.IsAny<string[]>()))
            .Returns("jwt-token");
        _mockTokenGenerator.Setup(x => x.Generate(64)).Returns("refresh-token");
        _mockTokenHasher.Setup(x => x.Hash("refresh-token")).Returns("refresh-token-hash");

        var command = new LoginCommand("john@test.com", "Password123!");

        var handler = new LoginCommandHandler(
            _mockUserRepository.Object,
            _mockPasswordHasher.Object,
            _mockJwtTokenService.Object,
            _mockTokenGenerator.Object,
            _mockTokenHasher.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.AccessToken.Should().Be("jwt-token");
        result.Value.RefreshToken.Should().Be("refresh-token");
    }

    [Fact]
    public async Task Handle_WithNonExistentEmail_ShouldFail()
    {
        _mockUserRepository.Setup(x => x.GetByEmailAsync("nonexistent@test.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var command = new LoginCommand("nonexistent@test.com", "Password123!");

        var handler = new LoginCommandHandler(
            _mockUserRepository.Object,
            _mockPasswordHasher.Object,
            _mockJwtTokenService.Object,
            _mockTokenGenerator.Object,
            _mockTokenHasher.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Auth.InvalidCredentials");
    }

    [Fact]
    public async Task Handle_WithWrongPassword_ShouldFail()
    {
        var userResult = User.Create("john@test.com", "password123!", "John Doe", Role.Customer, Guid.NewGuid());
        Assert.True(userResult.IsSuccess);
        var user = userResult.Value;
        user.ActivateByAdmin();

        _mockUserRepository.Setup(x => x.GetByEmailAsync("john@test.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _mockPasswordHasher.Setup(x => x.Verify("WrongPassword!", user.PasswordHash!)).Returns(false);

        var command = new LoginCommand("john@test.com", "WrongPassword!");

        var handler = new LoginCommandHandler(
            _mockUserRepository.Object,
            _mockPasswordHasher.Object,
            _mockJwtTokenService.Object,
            _mockTokenGenerator.Object,
            _mockTokenHasher.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Auth.InvalidCredentials");
    }

    [Fact]
    public async Task Handle_WithInactiveAccount_ShouldFail()
    {
        var userResult = User.Create("john@test.com", "password123!", "John Doe", Role.Customer, Guid.NewGuid());
        Assert.True(userResult.IsSuccess);
        var user = userResult.Value;

        _mockUserRepository.Setup(x => x.GetByEmailAsync("john@test.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _mockPasswordHasher.Setup(x => x.Verify("Password123!", user.PasswordHash!)).Returns(true);

        var command = new LoginCommand("john@test.com", "Password123!");

        var handler = new LoginCommandHandler(
            _mockUserRepository.Object,
            _mockPasswordHasher.Object,
            _mockJwtTokenService.Object,
            _mockTokenGenerator.Object,
            _mockTokenHasher.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Auth.AccountNotActive");
    }
}

public class LoginCommandValidatorTests
{
    [Fact]
    public void Validate_WithValidData_ShouldPass()
    {
        var command = new LoginCommand("john@test.com", "Password123!");

        var validator = new LoginCommandValidator();

        var result = validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithEmptyEmail_ShouldFail()
    {
        var command = new LoginCommand("", "Password123!");

        var validator = new LoginCommandValidator();

        var result = validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCountGreaterThan(0);
    }

    [Fact]
    public void Validate_WithEmptyPassword_ShouldFail()
    {
        var command = new LoginCommand("john@test.com", "");

        var validator = new LoginCommandValidator();

        var result = validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCountGreaterThan(0);
    }
}

public class ActivateAccountCommandHandlerTests
{
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<ITokenHasher> _mockTokenHasher;

    public ActivateAccountCommandHandlerTests()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _mockTokenHasher = new Mock<ITokenHasher>();
    }

    [Fact]
    public async Task Handle_WithValidToken_ShouldActivate()
    {
        var userResult = User.Create("john@test.com", "password123!", "John Doe", Role.Customer, Guid.NewGuid());
        Assert.True(userResult.IsSuccess);
        var user = userResult.Value;

        _mockUserRepository.Setup(x => x.GetByStatusAsync(UserStatus.PendingActivation, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<User> { user });
        _mockTokenHasher.Setup(x => x.Verify("activation-token", "activation-token")).Returns(true);
        user.SetActivationToken("activation-token", DateTime.UtcNow.AddHours(1));

        var command = new ActivateAccountCommand("activation-token");

        var handler = new ActivateAccountCommandHandler(
            _mockUserRepository.Object,
            _mockTokenHasher.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithInvalidToken_ShouldFail()
    {
        _mockUserRepository.Setup(x => x.GetByStatusAsync(UserStatus.PendingActivation, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<User>());

        var command = new ActivateAccountCommand("invalid-token");

        var handler = new ActivateAccountCommandHandler(
            _mockUserRepository.Object,
            _mockTokenHasher.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Auth.InvalidActivationToken");
    }
}

public class ActivateAccountCommandValidatorTests
{
    [Fact]
    public void Validate_WithValidToken_ShouldPass()
    {
        var command = new ActivateAccountCommand("valid-token");

        var validator = new ActivateAccountCommandValidator();

        var result = validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithEmptyToken_ShouldFail()
    {
        var command = new ActivateAccountCommand("");

        var validator = new ActivateAccountCommandValidator();

        var result = validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCountGreaterThan(0);
    }
}

public class ForgotPasswordCommandHandlerTests
{
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<ISecureTokenGenerator> _mockTokenGenerator;
    private readonly Mock<ITokenHasher> _mockTokenHasher;
    private readonly Mock<IEmailService> _mockEmailService;

    public ForgotPasswordCommandHandlerTests()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _mockTokenGenerator = new Mock<ISecureTokenGenerator>();
        _mockTokenHasher = new Mock<ITokenHasher>();
        _mockEmailService = new Mock<IEmailService>();
    }

    [Fact]
    public async Task Handle_WithExistingEmail_ShouldSendEmail()
    {
        var userResult = User.Create("john@test.com", "password123!", "John Doe", Role.Customer, Guid.NewGuid());
        Assert.True(userResult.IsSuccess);
        var user = userResult.Value;
        user.ActivateByAdmin();

        _mockUserRepository.Setup(x => x.GetByEmailAsync("john@test.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _mockTokenGenerator.Setup(x => x.Generate(32)).Returns("reset-token");
        _mockTokenHasher.Setup(x => x.Hash("reset-token")).Returns("reset-token-hash");

        var command = new ForgotPasswordCommand("john@test.com");

        var handler = new ForgotPasswordCommandHandler(
            _mockUserRepository.Object,
            _mockTokenGenerator.Object,
            _mockTokenHasher.Object,
            _mockEmailService.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _mockEmailService.Verify(x => x.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentEmail_ShouldReturnSuccess()
    {
        _mockUserRepository.Setup(x => x.GetByEmailAsync("nonexistent@test.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var command = new ForgotPasswordCommand("nonexistent@test.com");

        var handler = new ForgotPasswordCommandHandler(
            _mockUserRepository.Object,
            _mockTokenGenerator.Object,
            _mockTokenHasher.Object,
            _mockEmailService.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _mockEmailService.Verify(x => x.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}

public class ForgotPasswordCommandValidatorTests
{
    [Fact]
    public void Validate_WithValidEmail_ShouldPass()
    {
        var command = new ForgotPasswordCommand("john@test.com");

        var validator = new ForgotPasswordCommandValidator();

        var result = validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithEmptyEmail_ShouldFail()
    {
        var command = new ForgotPasswordCommand("");

        var validator = new ForgotPasswordCommandValidator();

        var result = validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCountGreaterThan(0);
    }

    [Fact]
    public void Validate_WithInvalidEmail_ShouldFail()
    {
        var command = new ForgotPasswordCommand("invalid-email");

        var validator = new ForgotPasswordCommandValidator();

        var result = validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCountGreaterThan(0);
    }
}

public class ResetPasswordCommandHandlerTests
{
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<IPasswordHasher> _mockPasswordHasher;
    private readonly Mock<ITokenHasher> _mockTokenHasher;

    public ResetPasswordCommandHandlerTests()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _mockPasswordHasher = new Mock<IPasswordHasher>();
        _mockTokenHasher = new Mock<ITokenHasher>();
    }

    [Fact]
    public async Task Handle_WithValidToken_ShouldResetPassword()
    {
        var userResult = User.Create("john@test.com", "password123!", "John Doe", Role.Customer, Guid.NewGuid());
        Assert.True(userResult.IsSuccess);
        var user = userResult.Value;
        user.ActivateByAdmin();
        user.RequestPasswordReset("reset-token", DateTime.UtcNow.AddHours(1));

        _mockUserRepository.Setup(x => x.GetByStatusAsync(UserStatus.Active, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<User> { user });
        _mockTokenHasher.Setup(x => x.Verify("reset-token", "reset-token")).Returns(true);
        _mockPasswordHasher.Setup(x => x.Hash("NewPassword123!")).Returns("new-password-hash");

        var command = new ResetPasswordCommand("reset-token", "NewPassword123!");

        var handler = new ResetPasswordCommandHandler(
            _mockUserRepository.Object,
            _mockPasswordHasher.Object,
            _mockTokenHasher.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithInvalidToken_ShouldFail()
    {
        _mockUserRepository.Setup(x => x.GetByStatusAsync(UserStatus.Active, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<User>());

        var command = new ResetPasswordCommand("invalid-token", "NewPassword123!");

        var handler = new ResetPasswordCommandHandler(
            _mockUserRepository.Object,
            _mockPasswordHasher.Object,
            _mockTokenHasher.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Auth.InvalidResetToken");
    }
}

public class ResetPasswordCommandValidatorTests
{
    [Fact]
    public void Validate_WithValidData_ShouldPass()
    {
        var command = new ResetPasswordCommand("valid-token", "NewPassword123!");

        var validator = new ResetPasswordCommandValidator();

        var result = validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithEmptyToken_ShouldFail()
    {
        var command = new ResetPasswordCommand("", "NewPassword123!");

        var validator = new ResetPasswordCommandValidator();

        var result = validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCountGreaterThan(0);
    }

    [Fact]
    public void Validate_WithWeakPassword_ShouldFail()
    {
        var command = new ResetPasswordCommand("valid-token", "weak");

        var validator = new ResetPasswordCommandValidator();

        var result = validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCountGreaterThan(0);
    }
}

public class RefreshTokenCommandHandlerTests
{
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<IJwtTokenService> _mockJwtTokenService;
    private readonly Mock<ITokenHasher> _mockTokenHasher;
    private readonly Mock<ISecureTokenGenerator> _mockTokenGenerator;

    public RefreshTokenCommandHandlerTests()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _mockJwtTokenService = new Mock<IJwtTokenService>();
        _mockTokenHasher = new Mock<ITokenHasher>();
        _mockTokenGenerator = new Mock<ISecureTokenGenerator>();
    }

    [Fact]
    public async Task Handle_WithValidToken_ShouldReturnNewAuthResult()
    {
        var userResult = User.Create("john@test.com", "password123!", "John Doe", Role.Customer, Guid.NewGuid());
        Assert.True(userResult.IsSuccess);
        var user = userResult.Value;
        user.ActivateByAdmin();

        var claims = new System.Security.Claims.ClaimsPrincipal(new System.Security.Claims.ClaimsIdentity(new[]
        {
            new System.Security.Claims.Claim("sub", user.Id.ToString())
        }));

        _mockJwtTokenService.Setup(x => x.GetPrincipalFromExpiredToken("old-refresh-token"))
            .Returns(claims);
        _mockUserRepository.Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _mockTokenHasher.Setup(x => x.Verify("old-refresh-token", "old-refresh-token")).Returns(true);
        _mockTokenGenerator.Setup(x => x.Generate(64)).Returns("new-refresh-token");
        _mockTokenHasher.Setup(x => x.Hash("new-refresh-token")).Returns("new-refresh-token");
        _mockJwtTokenService.Setup(x => x.GenerateAccessToken(user.Id, user.TenantId, It.IsAny<string[]>()))
            .Returns("new-jwt-token");

        user.SetResetPasswordToken("old-refresh-token", DateTime.UtcNow.AddDays(7));

        var command = new RefreshTokenCommand("old-refresh-token");

        var handler = new RefreshTokenCommandHandler(
            _mockUserRepository.Object,
            _mockJwtTokenService.Object,
            _mockTokenHasher.Object,
            _mockTokenGenerator.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.AccessToken.Should().Be("new-jwt-token");
        result.Value.RefreshToken.Should().Be("new-refresh-token");
    }

    [Fact]
    public async Task Handle_WithInvalidToken_ShouldFail()
    {
        _mockJwtTokenService.Setup(x => x.GetPrincipalFromExpiredToken("invalid-token"))
            .Returns((System.Security.Claims.ClaimsPrincipal?)null);

        var command = new RefreshTokenCommand("invalid-token");

        var handler = new RefreshTokenCommandHandler(
            _mockUserRepository.Object,
            _mockJwtTokenService.Object,
            _mockTokenHasher.Object,
            _mockTokenGenerator.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Auth.InvalidRefreshToken");
    }
}

public class RefreshTokenCommandValidatorTests
{
    [Fact]
    public void Validate_WithValidToken_ShouldPass()
    {
        var command = new RefreshTokenCommand("valid-token");

        var validator = new RefreshTokenCommandValidator();

        var result = validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithEmptyToken_ShouldFail()
    {
        var command = new RefreshTokenCommand("");

        var validator = new RefreshTokenCommandValidator();

        var result = validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCountGreaterThan(0);
    }
}
