import { z } from 'zod'
import { toTypedSchema } from '@vee-validate/zod'
import { passwordField } from '@/validation/fields/passwordField'
import { stringField } from '@/validation/fields/stringField'

/** Defaults de fallback si la API no responde con la lista al cargar la pantalla. */
export const FALLBACK_ALLOWED_DOMAINS = ['orf.com.co', 'tdh.com.co'] as const

/**
 * Construye el schema de registro con la lista de dominios permitidos.
 * Como los dominios son parametrizables desde el panel admin, el schema se genera
 * dinámicamente en la view (computed) tras consultar GET /api/auth/allowed-domains.
 */
export function buildRegisterSchema(allowedDomains: readonly string[]) {
  const dominios = allowedDomains.length > 0 ? allowedDomains : FALLBACK_ALLOWED_DOMAINS
  const label = dominios.map((d) => `@${d}`).join(' o ')

  return z.object({
    nombre: stringField({ label: 'nombre', min: 2, max: 120 }),
    correo: z
      .email('El correo no es válido')
      .max(120, 'El correo debe tener máximo 120 caracteres')
      .refine(
        (value) => {
          const domain = value.split('@')[1]?.toLowerCase()
          return domain ? dominios.includes(domain) : false
        },
        { message: `Solo se aceptan correos ${label}` },
      ),
    password: passwordField({ label: 'contraseña', min: 6, max: 100 }),
  })
}

export type RegisterFormValues = z.infer<ReturnType<typeof buildRegisterSchema>>

export function buildRegisterFormSchema(allowedDomains: readonly string[]) {
  return toTypedSchema(buildRegisterSchema(allowedDomains))
}
