import { z } from 'zod'

export interface DateFieldOptions {
  label?: string
  required?: boolean
}

export function dateField({ label = 'fecha', required = true }: DateFieldOptions = {}) {
  const schema = z.coerce.date({ error: `La ${label} no es válida` })
  if (!required) return schema.optional()
  return schema
}
