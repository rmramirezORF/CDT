using Backend.Application.Admin.UseCases;
using Backend.Application.Auth.Commands;
using Backend.Application.Auth.DTOs;
using Backend.Application.Auth.UseCases;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Controllers;

[ApiController]
[Route("api/auth")]
[Produces("application/json")]
public class AuthController : BaseApiController
{
    private readonly RegisterUseCase _register;
    private readonly ConfirmEmailUseCase _confirmEmail;
    private readonly LoginUseCase _login;
    private readonly RefreshTokenUseCase _refresh;
    private readonly LogoutUseCase _logout;
    private readonly ForgotPasswordUseCase _forgotPassword;
    private readonly ResetPasswordUseCase _resetPassword;
    private readonly DominiosPermitidosUseCases _dominios;

    public AuthController(
        RegisterUseCase register,
        ConfirmEmailUseCase confirmEmail,
        LoginUseCase login,
        RefreshTokenUseCase refresh,
        LogoutUseCase logout,
        ForgotPasswordUseCase forgotPassword,
        ResetPasswordUseCase resetPassword,
        DominiosPermitidosUseCases dominios)
    {
        _register = register;
        _confirmEmail = confirmEmail;
        _login = login;
        _refresh = refresh;
        _logout = logout;
        _forgotPassword = forgotPassword;
        _resetPassword = resetPassword;
        _dominios = dominios;
    }

    /// <summary>
    /// Devuelve la lista de dominios permitidos para registro (solo nombres, sin ids).
    /// Endpoint público — lo usa el form de registro para mostrar/validar dinámicamente.
    /// </summary>
    [HttpGet("allowed-domains")]
    [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
    public async Task<IActionResult> AllowedDomains(CancellationToken ct)
    {
        var dominios = await _dominios.ListarAsync(ct);
        return ApiOk(dominios.Select(d => d.Dominio).ToList());
    }

    /// <summary>Registra una cuenta nueva. Envía código de confirmación al correo.</summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(UsuarioDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Register([FromBody] RegisterCommand cmd, CancellationToken ct)
        => ApiOk(await _register.ExecuteAsync(cmd, ct));

    /// <summary>Confirma el correo con el código de 6 dígitos enviado al email.</summary>
    [HttpPost("confirm-email")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailCommand cmd, CancellationToken ct)
    {
        await _confirmEmail.ExecuteAsync(cmd, ct);
        return ApiOk(new { confirmed = true });
    }

    /// <summary>Inicia sesión. Devuelve JWT + refresh token + perfil del usuario.</summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Login([FromBody] LoginCommand cmd, CancellationToken ct)
        => ApiOk(await _login.ExecuteAsync(cmd, ct));

    /// <summary>Renueva el JWT con un refresh token válido. Rota el refresh.</summary>
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(RefreshTokenResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenCommand cmd, CancellationToken ct)
        => ApiOk(await _refresh.ExecuteAsync(cmd, ct));

    /// <summary>Cierra sesión revocando el refresh token. Idempotente.</summary>
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Logout([FromBody] LogoutCommand cmd, CancellationToken ct)
    {
        await _logout.ExecuteAsync(cmd, ct);
        return ApiOk(new { loggedOut = true });
    }

    /// <summary>Solicita código de 6 dígitos para restablecer contraseña.</summary>
    [HttpPost("forgot-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordCommand cmd, CancellationToken ct)
    {
        await _forgotPassword.ExecuteAsync(cmd, ct);
        return ApiOk(new { sent = true });
    }

    /// <summary>Restablece la contraseña con el código de 6 dígitos.</summary>
    [HttpPost("reset-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordCommand cmd, CancellationToken ct)
    {
        await _resetPassword.ExecuteAsync(cmd, ct);
        return ApiOk(new { reset = true });
    }
}
