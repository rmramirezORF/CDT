import { z } from 'zod'

export interface StringFieldOptions {
  label: string
  min?: number
  max?: number
}

/**
 * String requerido por defecto (min=1). Para opcional: stringField({...}).optional().
 */
export function stringField({ label, min = 1, max }: StringFieldOptions) {
  let schema = z.string().trim().min(min, `El ${label} debe tener mínimo ${min} caracteres`)
  if (max !== undefined) schema = schema.max(max, `El ${label} debe tener máximo ${max} caracteres`)
  return schema
}
