import { z } from 'zod'

export interface EmailFieldOptions {
  label?: string
  max?: number
}

/**
 * Email requerido. Para opcional: emailField().optional().
 */
export function emailField({ label = 'correo', max = 100 }: EmailFieldOptions = {}) {
  return z.email(`El ${label} no es válido`).max(max, `El ${label} debe tener máximo ${max} caracteres`)
}
