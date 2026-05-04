import type { AxiosRequestConfig, AxiosResponse } from 'axios'
import { api } from './api'
import { ENV } from '@/config/env'
import type { ApiResponse } from '@/types/api'
import { extractErrorMessage } from '@/utils/errorMapper'

/**
 * Cliente HTTP por recurso. Construye URL con `${API_PREFIX}${resourcePath}${endpoint}`,
 * desempaqueta `ApiResponse<T>` y unifica errores con extractErrorMessage.
 */
export class HttpClient {
  constructor(private readonly resourcePath: string) {}

  private url(endpoint: string): string {
    return `${ENV.API_PREFIX}${this.resourcePath}${endpoint}`
  }

  private unwrap<T>(response: AxiosResponse<ApiResponse<T>>): T {
    if (!response.data.success) {
      throw new Error(extractErrorMessage(response))
    }
    return response.data.data as T
  }

  async get<T>(endpoint = '', config?: AxiosRequestConfig): Promise<T> {
    return this.unwrap<T>(await api.get(this.url(endpoint), config))
  }

  async post<T, B = unknown>(endpoint = '', body?: B, config?: AxiosRequestConfig): Promise<T> {
    return this.unwrap<T>(await api.post(this.url(endpoint), body, config))
  }

  async put<T, B = unknown>(endpoint = '', body?: B, config?: AxiosRequestConfig): Promise<T> {
    return this.unwrap<T>(await api.put(this.url(endpoint), body, config))
  }

  async patch<T, B = unknown>(endpoint = '', body?: B, config?: AxiosRequestConfig): Promise<T> {
    return this.unwrap<T>(await api.patch(this.url(endpoint), body, config))
  }

  async delete<T>(endpoint = '', config?: AxiosRequestConfig): Promise<T> {
    return this.unwrap<T>(await api.delete(this.url(endpoint), config))
  }
}
