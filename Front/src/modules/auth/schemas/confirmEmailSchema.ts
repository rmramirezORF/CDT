import { z } from 'zod'
import { toTypedSchema } from '@vee-validate/zod'
import { emailField } from '@/validation/fields/emailField'

export const confirmEmailSchema = z.object({
  correo: emailField({ label: 'correo' }),
  codigo: z.string()
    .regex(/^\d{6}$/, 'El código debe ser numérico de 6 dígitos'),
})

export type ConfirmEmailFormValues = z.infer<typeof confirmEmailSchema>
export const confirmEmailFormSchema = toTypedSchema(confirmEmailSchema)
