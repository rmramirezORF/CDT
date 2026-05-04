import { z } from 'zod'

export interface NumberFieldOptions {
  label: string
  min?: number
  max?: number
  int?: boolean
  required?: boolean
}

export function numberField({ label, min, max, int = false, required = true }: NumberFieldOptions) {
  let schema: z.ZodNumber = z.number({ error: `El ${label} debe ser un número` })
  if (int) schema = schema.int(`El ${label} debe ser entero`)
  if (min !== undefined) schema = schema.min(min, `El ${label} debe ser mayor o igual a ${min}`)
  if (max !== undefined) schema = schema.max(max, `El ${label} debe ser menor o igual a ${max}`)
  if (!required) return schema.optional()
  return schema
}
