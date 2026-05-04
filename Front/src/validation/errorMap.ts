import { z } from 'zod'

/**
 * customError global de Zod 4: traduce los issue codes a mensajes en español.
 * Se importa en main.ts para que aplique a TODA validación del frontend.
 */
z.config({
  customError: (issue) => {
    switch (issue.code) {
      case 'invalid_type':
        if (issue.expected === 'string') return { message: 'Debe ser texto' }
        if (issue.expected === 'number') return { message: 'Debe ser un número' }
        if (issue.expected === 'date')   return { message: 'Debe ser una fecha válida' }
        if (issue.expected === 'boolean') return { message: 'Debe ser sí o no' }
        return { message: `Tipo inválido (esperado: ${issue.expected})` }

      case 'too_small':
        if (issue.origin === 'string') return { message: `Mínimo ${issue.minimum} caracteres` }
        if (issue.origin === 'number') return { message: `Debe ser mayor o igual a ${issue.minimum}` }
        if (issue.origin === 'array')  return { message: `Selecciona al menos ${issue.minimum}` }
        return { message: 'Valor demasiado pequeño' }

      case 'too_big':
        if (issue.origin === 'string') return { message: `Máximo ${issue.maximum} caracteres` }
        if (issue.origin === 'number') return { message: `Debe ser menor o igual a ${issue.maximum}` }
        if (issue.origin === 'array')  return { message: `Selecciona máximo ${issue.maximum}` }
        return { message: 'Valor demasiado grande' }

      case 'invalid_format':
        if (issue.format === 'email') return { message: 'Correo inválido' }
        if (issue.format === 'url')   return { message: 'URL inválida' }
        if (issue.format === 'uuid')  return { message: 'Identificador inválido' }
        return { message: 'Formato inválido' }

      case 'invalid_value':
        return { message: 'Valor no permitido' }

      case 'unrecognized_keys':
        return { message: 'Hay campos no reconocidos en el formulario' }

      default:
        return { message: 'Valor inválido' }
    }
  },
})
