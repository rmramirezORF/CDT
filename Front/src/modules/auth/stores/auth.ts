import { defineStore } from 'pinia'
import { computed, ref } from 'vue'
import authService from '../services/authService'
import type { AuthSession, AuthTokens, LoginCredentials, Usuario } from '../types/auth'

const TOKEN_KEY = 'cdt:token'
const REFRESH_KEY = 'cdt:refresh'
const USER_KEY = 'cdt:user'

function loadSession(): AuthSession | null {
  if (typeof window === 'undefined') return null
  const token = localStorage.getItem(TOKEN_KEY)
  const refreshToken = localStorage.getItem(REFRESH_KEY)
  const userJson = localStorage.getItem(USER_KEY)
  if (!token || !refreshToken || !userJson) return null
  try {
    const raw = JSON.parse(userJson)
    const usuario: Usuario = {
      ...raw,
      fechaCreacion: new Date(raw.fechaCreacion),
      fechaConfirmacionEmail: raw.fechaConfirmacionEmail ? new Date(raw.fechaConfirmacionEmail) : null,
    }
    return { tokens: { token, refreshToken }, usuario }
  } catch {
    return null
  }
}

function persistSession(s: AuthSession) {
  localStorage.setItem(TOKEN_KEY, s.tokens.token)
  localStorage.setItem(REFRESH_KEY, s.tokens.refreshToken)
  localStorage.setItem(USER_KEY, JSON.stringify(s.usuario))
}

function clearPersistedSession() {
  localStorage.removeItem(TOKEN_KEY)
  localStorage.removeItem(REFRESH_KEY)
  localStorage.removeItem(USER_KEY)
}

export const useAuthStore = defineStore('auth', () => {
  const session = ref<AuthSession | null>(loadSession())

  const isAuthenticated = computed(() => session.value !== null)
  const usuario = computed(() => session.value?.usuario ?? null)
  const rolGlobal = computed(() => session.value?.usuario.rolGlobal ?? null)
  const isAdmin = computed(() => rolGlobal.value === 'Admin')
  const isLider = computed(() => rolGlobal.value === 'Lider')

  async function login(credentials: LoginCredentials) {
    const result = await authService.login(credentials)
    session.value = result
    persistSession(result)
    return result
  }

  async function logout() {
    if (session.value?.tokens.refreshToken) {
      try {
        await authService.logout(session.value.tokens.refreshToken)
      } catch {
        /* logout local procede aunque el remoto falle */
      }
    }
    session.value = null
    clearPersistedSession()
  }

  function applyRefreshedTokens(tokens: AuthTokens) {
    if (!session.value) return
    session.value = { ...session.value, tokens }
    persistSession(session.value)
  }

  function getRefreshToken(): string | null {
    return session.value?.tokens.refreshToken ?? null
  }

  return {
    session,
    isAuthenticated,
    usuario,
    rolGlobal,
    isAdmin,
    isLider,
    login,
    logout,
    applyRefreshedTokens,
    getRefreshToken,
  }
})
