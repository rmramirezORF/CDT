using Backend.Application.Auth.Commands;
using Backend.Application.Auth.Common;
using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Persistence;
using Backend.Domain.Entities.Auth;
using Microsoft.EntityFrameworkCore;

namespace Backend.Application.Auth.UseCases;

public class ForgotPasswordUseCase
{
    private readonly ICdtDbContext _context;
    private readonly ITokenService _tokens;
    private readonly IEmailService _email;

    public ForgotPasswordUseCase(ICdtDbContext context, ITokenService tokens, IEmailService email)
    {
        _context = context;
        _tokens = tokens;
        _email = email;
    }

    public async Task ExecuteAsync(ForgotPasswordCommand cmd, CancellationToken ct = default)
    {
        var correo = cmd.Correo.Trim().ToLowerInvariant();
        var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Correo == correo, ct);
        if (usuario is null) return; // silencioso por seguridad — no revelar si el correo existe

        // Invalidar tokens previos pendientes
        var pendientes = await _context.TokensResetPassword
            .Where(t => t.IdUsuario == usuario.Id && !t.Usado)
            .ToListAsync(ct);
        foreach (var t in pendientes) t.Usado = true;

        var codigo = _tokens.GenerateSixDigitCode();
        _context.TokensResetPassword.Add(new TokenResetPasswordEntity
        {
            IdUsuario = usuario.Id,
            Codigo6Digitos = codigo,
            FechaExpiracion = DateTime.UtcNow.AddHours(1),
        });
        await _context.SaveChangesAsync(ct);

        var (subject, html) = AuthEmailTemplates.ResetPassword(usuario.Nombre, codigo);
        try
        {
            await _email.SendAsync(usuario.Correo, subject, html, ct);
        }
        catch (Exception)
        {
            // El token ya esta persistido; el usuario puede reintentar el flujo si no llega el correo.
        }
    }
}
