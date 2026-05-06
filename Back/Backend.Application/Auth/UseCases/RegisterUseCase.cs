using Backend.Application.Auth.Commands;
using Backend.Application.Auth.Common;
using Backend.Application.Auth.DTOs;
using Backend.Application.Common.Exceptions;
using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Persistence;
using Backend.Domain.Entities.Auth;
using Microsoft.EntityFrameworkCore;

namespace Backend.Application.Auth.UseCases;

public class RegisterUseCase
{
    private readonly ICdtDbContext _context;
    private readonly ICryptoService _crypto;
    private readonly ITokenService _tokens;
    private readonly IEmailService _email;

    public RegisterUseCase(ICdtDbContext context, ICryptoService crypto, ITokenService tokens, IEmailService email)
    {
        _context = context;
        _crypto = crypto;
        _tokens = tokens;
        _email = email;
    }

    public async Task<UsuarioDto> ExecuteAsync(RegisterCommand cmd, CancellationToken ct = default)
    {
        var correo = cmd.Correo.Trim().ToLowerInvariant();

        // Validar dominio leyendo la lista blanca de la BD (gestionable desde admin).
        var dominio = correo.Split('@').LastOrDefault();
        if (string.IsNullOrEmpty(dominio))
            throw new BusinessException("EMAIL_DOMAIN_NOT_ALLOWED", "Correo inválido.");

        var dominioPermitido = await _context.DominiosPermitidos.AnyAsync(d => d.Dominio == dominio, ct);
        if (!dominioPermitido)
        {
            var permitidos = await _context.DominiosPermitidos
                .OrderBy(d => d.Dominio)
                .Select(d => "@" + d.Dominio)
                .ToListAsync(ct);
            var permitidosStr = permitidos.Count == 0 ? "(ninguno configurado)" : string.Join(" o ", permitidos);
            throw new BusinessException(
                "EMAIL_DOMAIN_NOT_ALLOWED",
                $"Solo se aceptan correos con dominio {permitidosStr}.");
        }

        var existe = await _context.Usuarios.AnyAsync(u => u.Correo == correo, ct);
        if (existe)
            throw new BusinessException("EMAIL_ALREADY_EXISTS", "Ya existe una cuenta con ese correo.");

        var codigo = _tokens.GenerateSixDigitCode();
        var usuario = new UsuarioEntity
        {
            Nombre = cmd.Nombre.Trim(),
            Correo = correo,
            ClaveHash = _crypto.HashPassword(cmd.Password),
            Estado = true,
            RolGlobal = "Miembro",
            CodigoConfirmacionEmail = codigo,
            FechaExpiracionConfirmacion = DateTime.UtcNow.AddHours(24),
        };

        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync(ct);

        var (subject, html) = AuthEmailTemplates.ConfirmacionEmail(usuario.Nombre, codigo);
        try
        {
            await _email.SendAsync(usuario.Correo, subject, html, ct);
        }
        catch (Exception)
        {
            // No revertir el registro: el usuario podra solicitar reenvio del codigo (V1.5+).
        }

        return new UsuarioDto
        {
            Id = usuario.Id,
            Nombre = usuario.Nombre,
            Correo = usuario.Correo,
            RolGlobal = usuario.RolGlobal,
            Estado = usuario.Estado,
            FechaCreacion = usuario.FechaCreacion,
            FechaConfirmacionEmail = usuario.FechaConfirmacionEmail,
        };
    }
}
