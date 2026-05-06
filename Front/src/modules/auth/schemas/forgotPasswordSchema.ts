import { z } from 'zod'
import { toTypedSchema } from '@vee-validate/zod'
import { emailField } from '@/validation/fields/emailField'

export const forgotPasswordSchema = z.object({
  correo: emailField({ label: 'correo' }),
})

export type ForgotPasswordFormValues = z.infer<typeof forgotPasswordSchema>
export const forgotPasswordFormSchema = toTypedSchema(forgotPasswordSchema)
