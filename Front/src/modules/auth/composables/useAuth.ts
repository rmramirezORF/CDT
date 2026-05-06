import { ref } from 'vue'
import { extractErrorMessage } from '@/utils/errorMapper'
import authService from '../services/authService'
import { useAuthStore } from '../stores/auth'
import type {
  ConfirmEmailPayload,
  ForgotPasswordPayload,
  LoginCredentials,
  RegisterPayload,
  ResetPasswordPayload,
} from '../types/auth'

/**
 * Composable de operaciones de auth con estado de loading y error compartido.
 * Para casos sin estado (ej. desde stores), usar authService directamente.
 */
export function useAuth() {
  const store = useAuthStore()
  const isLoading = ref(false)
  const error = ref<string | null>(null)

  async function withState<T>(operation: () => Promise<T>): Promise<T | null> {
    isLoading.value = true
    error.value = null
    try {
      return await operation()
    } catch (e) {
      error.value = extractErrorMessage(e)
      return null
    } finally {
      isLoading.value = false
    }
  }

  return {
    isLoading,
    error,
    login: (creds: LoginCredentials) => withState(() => store.login(creds)),
    register: (payload: RegisterPayload) => withState(() => authService.register(payload)),
    confirmEmail: (payload: ConfirmEmailPayload) => withState(() => authService.confirmEmail(payload)),
    forgotPassword: (payload: ForgotPasswordPayload) => withState(() => authService.forgotPassword(payload)),
    resetPassword: (payload: ResetPasswordPayload) => withState(() => authService.resetPassword(payload)),
    logout: () => withState(() => store.logout()),
  }
}
