import axios, { type AxiosError, type AxiosRequestConfig } from 'axios'
import { ENV } from '@/config/env'

export const api = axios.create({
  baseURL: ENV.API_BASE_URL,
  withCredentials: true,
  timeout: 30000,
})

// ----- Interceptor de request: inyecta Bearer -----
api.interceptors.request.use((config) => {
  const token = localStorage.getItem('cdt:token')
  if (token && config.headers) {
    config.headers['Authorization'] = `Bearer ${token}`
  }
  return config
})

// ----- Interceptor de response: maneja 401 con cola de refresh -----
let isRefreshing = false
type QueueCallback = (token: string | null) => void
let refreshQueue: QueueCallback[] = []

function flushQueue(token: string | null) {
  refreshQueue.forEach((cb) => cb(token))
  refreshQueue = []
}

api.interceptors.response.use(
  (response) => response,
  async (error: AxiosError) => {
    const original = error.config as (AxiosRequestConfig & { _retry?: boolean }) | undefined
    if (!original || error.response?.status !== 401 || original._retry) {
      return Promise.reject(error)
    }

    if (isRefreshing) {
      return new Promise((resolve, reject) => {
        refreshQueue.push((token) => {
          if (!token) return reject(error)
          original.headers = { ...(original.headers || {}), Authorization: `Bearer ${token}` }
          original._retry = true
          resolve(api(original))
        })
      })
    }

    isRefreshing = true
    try {
      // Lazy imports para evitar ciclo de dependencias con el modulo auth.
      const { useAuthStore } = await import('@/modules/auth/stores/auth')
      const { default: authService } = await import('@/modules/auth/services/authService')
      const store = useAuthStore()
      const refreshToken = store.getRefreshToken()
      if (!refreshToken) throw new Error('No hay refresh token disponible')

      const tokens = await authService.refresh(refreshToken)
      store.applyRefreshedTokens(tokens)

      flushQueue(tokens.token)
      original.headers = { ...(original.headers || {}), Authorization: `Bearer ${tokens.token}` }
      original._retry = true
      return api(original)
    } catch (refreshError) {
      flushQueue(null)
      // Limpieza local — no llamamos store.logout() porque ya estamos en flujo de fallo.
      localStorage.removeItem('cdt:token')
      localStorage.removeItem('cdt:refresh')
      localStorage.removeItem('cdt:user')
      if (typeof window !== 'undefined' && window.location.pathname !== '/login') {
        window.location.href = '/login'
      }
      return Promise.reject(refreshError)
    } finally {
      isRefreshing = false
    }
  },
)
