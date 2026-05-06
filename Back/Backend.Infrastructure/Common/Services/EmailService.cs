using Backend.Application.Common.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using MimeKit.Text;

namespace Backend.Infrastructure.Common.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _config;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration config, ILogger<EmailService> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task SendAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken = default)
    {
        var host      = _config["Smtp:Host"] ?? throw new InvalidOperationException("Smtp:Host no configurado");
        var port      = int.Parse(_config["Smtp:Port"] ?? "1025");
        var user      = _config["Smtp:User"];
        var password  = _config["Smtp:Password"] ?? string.Empty;
        var from      = _config["Smtp:From"] ?? throw new InvalidOperationException("Smtp:From no configurado");
        var enableSsl = bool.Parse(_config["Smtp:EnableSsl"] ?? "false");

        var message = new MimeMessage();
        message.From.Add(MailboxAddress.Parse(from));
        message.To.Add(MailboxAddress.Parse(to));
        message.Subject = subject;
        message.Body = new TextPart(TextFormat.Html) { Text = htmlBody };

        using var smtp = new SmtpClient();
        try
        {
            await smtp.ConnectAsync(host, port,
                enableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None,
                cancellationToken);

            if (!string.IsNullOrEmpty(user))
            {
                await smtp.AuthenticateAsync(user, password, cancellationToken);
            }

            await smtp.SendAsync(message, cancellationToken);
            await smtp.DisconnectAsync(true, cancellationToken);

            _logger.LogInformation("Email enviado a {To} con asunto '{Subject}'", to, subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al enviar email a {To} con asunto '{Subject}'", to, subject);
            throw;
        }
    }
}
