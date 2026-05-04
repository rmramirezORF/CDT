import { z } from 'zod'

export interface PasswordFieldOptions {
  label?: string
  min?: number
  max?: number
  required?: boolean
}

export function passwordField({ label = 'contraseña', min = 6, max = 100, required = true }: PasswordFieldOptions = {}) {
  const schema = z.string()
    .min(min, `La ${label} debe tener mínimo ${min} caracteres`)
    .max(max, `La ${label} debe tener máximo ${max} caracteres`)
  if (!required) return schema.optional()
  return schema
}
