import { z } from 'zod'
import { toTypedSchema } from '@vee-validate/zod'
import { emailField } from '@/validation/fields/emailField'
import { passwordField } from '@/validation/fields/passwordField'

export const resetPasswordSchema = z.object({
  correo: emailField({ label: 'correo' }),
  codigo: z.string().regex(/^\d{6}$/, 'El código debe ser numérico de 6 dígitos'),
  nuevaPassword: passwordField({ label: 'contraseña', min: 6, max: 100 }),
})

export type ResetPasswordFormValues = z.infer<typeof resetPasswordSchema>
export const resetPasswordFormSchema = toTypedSchema(resetPasswordSchema)
