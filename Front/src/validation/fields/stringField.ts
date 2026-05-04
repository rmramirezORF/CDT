import { z } from 'zod'

export interface StringFieldOptions {
  label: string
  min?: number
  max?: number
  required?: boolean
}

export function stringField({ label, min, max, required = true }: StringFieldOptions) {
  let schema = z.string().trim()
  if (min !== undefined) schema = schema.min(min, `El ${label} debe tener mínimo ${min} caracteres`)
  if (max !== undefined) schema = schema.max(max, `El ${label} debe tener máximo ${max} caracteres`)
  if (required && min === undefined) {
    schema = schema.min(1, `El ${label} es obligatorio`)
  }
  if (!required) return schema.optional()
  return schema
}
