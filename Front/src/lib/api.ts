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
      // TODO: cuando exista el módulo auth, llamar authService.refresh()
      // const newToken = await authService.refresh()
      // localStorage.setItem('cdt:token', newToken)
      // flushQueue(newToken)
      // original.headers = { ...(original.headers || {}), Authorization: `Bearer ${newToken}` }
      // original._retry = true
      // return api(original)
      throw new Error('refresh not implemented yet')
    } catch (refreshError) {
      flushQueue(null)
      localStorage.removeItem('cdt:token')
      if (typeof window !== 'undefined' && window.location.pathname !== '/login') {
        window.location.href = '/login'
      }
      return Promise.reject(refreshError)
    } finally {
      isRefreshing = false
    }
  },
)
