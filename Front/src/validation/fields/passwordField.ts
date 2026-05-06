import { z } from 'zod'

export interface PasswordFieldOptions {
  label?: string
  min?: number
  max?: number
}

/**
 * Password requerido. Para opcional: passwordField().optional().
 */
export function passwordField({ label = 'contraseña', min = 6, max = 100 }: PasswordFieldOptions = {}) {
  return z.string()
    .min(min, `La ${label} debe tener mínimo ${min} caracteres`)
    .max(max, `La ${label} debe tener máximo ${max} caracteres`)
}
