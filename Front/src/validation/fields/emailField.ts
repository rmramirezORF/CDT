import { z } from 'zod'

export interface EmailFieldOptions {
  label?: string
  max?: number
  required?: boolean
}

export function emailField({ label = 'correo', max = 100, required = true }: EmailFieldOptions = {}) {
  const schema = z.email(`El ${label} no es válido`).max(max, `El ${label} debe tener máximo ${max} caracteres`)
  if (!required) return schema.optional()
  return schema
}
