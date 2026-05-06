import { z } from 'zod'
import { toTypedSchema } from '@vee-validate/zod'
import { emailField } from '@/validation/fields/emailField'
import { passwordField } from '@/validation/fields/passwordField'

export const loginSchema = z.object({
  correo: emailField({ label: 'correo' }),
  password: passwordField({ label: 'contraseña', min: 6, max: 100 }),
})

export type LoginFormValues = z.infer<typeof loginSchema>
export const loginFormSchema = toTypedSchema(loginSchema)
