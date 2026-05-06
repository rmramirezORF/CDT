using System.Net;

namespace Backend.Application.Auth.Common;

/// <summary>
/// Plantillas HTML simples para correos de auth (confirmación, reset).
/// Devuelven (Subject, HtmlBody) para invocar IEmailService.SendAsync.
/// </summary>
public static class AuthEmailTemplates
{
    public static (string Subject, string Html) ConfirmacionEmail(string nombre, string codigo) =>
    (
        "Confirma tu cuenta en CDT",
        BaseTemplate(
            $$"""
            <p>Hola <strong>{{WebUtility.HtmlEncode(nombre)}}</strong>,</p>
            <p>Para confirmar tu cuenta en CDT ingresa el siguiente código:</p>
            <p style="font-size: 28px; font-weight: bold; letter-spacing: 6px; text-align: center; padding: 16px; background:#f3f4f6; border-radius: 8px;">{{codigo}}</p>
            <p>El código vence en 24 horas. Si no solicitaste esta cuenta, puedes ignorar este correo.</p>
            """)
    );

    public static (string Subject, string Html) ResetPassword(string nombre, string codigo) =>
    (
        "Restablecer contraseña en CDT",
        BaseTemplate(
            $$"""
            <p>Hola <strong>{{WebUtility.HtmlEncode(nombre)}}</strong>,</p>
            <p>Recibimos una solicitud para restablecer tu contraseña. Usa el siguiente código:</p>
            <p style="font-size: 28px; font-weight: bold; letter-spacing: 6px; text-align: center; padding: 16px; background:#f3f4f6; border-radius: 8px;">{{codigo}}</p>
            <p>El código vence en 1 hora. Si no solicitaste este cambio, ignora este correo.</p>
            """)
    );

    private static string BaseTemplate(string content) =>
        $$"""
        <!DOCTYPE html>
        <html>
          <body style="font-family: system-ui, -apple-system, Segoe UI, Roboto, sans-serif; max-width: 560px; margin: 0 auto; padding: 24px; color: #111;">
            <div style="border-bottom: 1px solid #e5e7eb; padding-bottom: 12px; margin-bottom: 24px;">
              <h1 style="margin: 0; font-size: 20px;">CDT</h1>
            </div>
            {{content}}
            <p style="margin-top: 32px; font-size: 12px; color: #6b7280;">Correo automático de CDT (ORF). No respondas a este mensaje.</p>
          </body>
        </html>
        """;
}
