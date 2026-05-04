import { AxiosError, type AxiosResponse } from 'axios'
import type { ApiResponse } from '@/types/api'

/**
 * Convierte cualquier error/respuesta a un mensaje legible para el usuario.
 * Jerarquia:
 *   1. response.data.message
 *   2. response.data.error.code -> "Error tecnico: <code>"
 *   3. Network Error / ECONNABORTED
 *   4. error.message
 *   5. Fallback generico
 */
export function extractErrorMessage(input: unknown): string {
  // AxiosResponse exitoso pero con success=false
  if (isAxiosResponseLike(input)) {
    const data = input.data
    if (data?.message) return data.message
    if (data?.error?.code) return `Error técnico: ${data.error.code}`
  }

  // AxiosError
  if (input instanceof AxiosError) {
    const data = input.response?.data as Partial<ApiResponse<unknown>> | undefined
    if (data?.message) return data.message
    if (data?.error?.code) return `Error técnico: ${data.error.code}`
    if (input.message === 'Network Error') {
      return 'No se pudo establecer conexión con el servidor. Verifica tu conexión.'
    }
    if (input.code === 'ECONNABORTED') {
      return 'La solicitud tardó demasiado tiempo. Intenta de nuevo.'
    }
    if (input.message) return input.message
  }

  // Error generico
  if (input instanceof Error && input.message) return input.message

  return 'Ocurrió un error inesperado. Por favor, contacta al soporte.'
}

function isAxiosResponseLike(value: unknown): value is AxiosResponse<Partial<ApiResponse<unknown>>> {
  return typeof value === 'object'
    && value !== null
    && 'data' in value
    && 'status' in value
}
