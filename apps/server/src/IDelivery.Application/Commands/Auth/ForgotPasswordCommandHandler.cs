using IDelivery.Application.Abstractions.CQRS;
using IDelivery.Application.Abstractions.Messaging;
using IDelivery.Application.Abstractions.Persistence;
using IDelivery.Application.Abstractions.Security;
using IDelivery.SharedKernel.Common.Result;

namespace IDelivery.Application.Commands.Auth;

public sealed class ForgotPasswordCommandHandler : ICommandHandler<ForgotPasswordCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly ISecureTokenGenerator _tokenGenerator;
    private readonly ITokenHasher _tokenHasher;
    private readonly IEmailService _emailService;

    public ForgotPasswordCommandHandler(
        IUserRepository userRepository,
        ISecureTokenGenerator tokenGenerator,
        ITokenHasher tokenHasher,
        IEmailService emailService)
    {
        _userRepository = userRepository;
        _tokenGenerator = tokenGenerator;
        _tokenHasher = tokenHasher;
        _emailService = emailService;
    }

    public async Task<Result> Handle(ForgotPasswordCommand command, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByEmailAsync(command.Email, cancellationToken);

        if (user is null)
        {
            return Result.Success();
        }

        if (user.Status != IDelivery.Domain.Users.Enums.UserStatus.Active)
        {
            return Result.Success();
        }

        var resetToken = _tokenGenerator.Generate(32);
        var resetTokenHash = _tokenHasher.Hash(resetToken);
        var resetTokenExpiresAt = DateTime.UtcNow.AddHours(1);

        user.SetResetPasswordToken(resetTokenHash, resetTokenExpiresAt);
        user.RequestPasswordReset();

        var resetLink = $"https://app.idelivery.com/reset-password?token={resetToken}&email={Uri.EscapeDataString(command.Email)}";
        
        var subject = "Redefinição de senha - iDelivery";
        var body = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .button {{ display: inline-block; padding: 12px 24px; background-color: #2563eb; color: white; text-decoration: none; border-radius: 6px; }}
        .footer {{ margin-top: 20px; font-size: 12px; color: #666; }}
    </style>
</head>
<body>
    <div class='container'>
        <h2>Redefinição de senha</h2>
        <p>Você solicitou a redefinição de sua senha. Clique no botão abaixo para criar uma nova senha:</p>
        <p style='text-align: center; margin: 30px 0;'>
            <a href='{resetLink}' class='button'>Redefinir Senha</a>
        </p>
        <p>Ou copie e cole este link no navegador:</p>
        <p style='word-break: break-all; color: #2563eb;'>{resetLink}</p>
        <p>Este link expira em 1 hora.</p>
        <div class='footer'>
            <p>Se você não solicitou esta redefinição, por favor ignore este e-mail.</p>
            <p>&copy; {DateTime.UtcNow.Year} iDelivery. Todos os direitos reservados.</p>
        </div>
    </div>
</body>
</html>";

        await _emailService.SendAsync(command.Email, subject, body, cancellationToken);

        return Result.Success();
    }
}