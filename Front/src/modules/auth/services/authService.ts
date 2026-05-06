import { HttpClient } from '@/lib/HttpClient'
import { loginResponseAdapter, usuarioAdapter } from '../adapters/authAdapter'
import type {
  AuthSession,
  AuthTokens,
  ConfirmEmailPayload,
  ForgotPasswordPayload,
  LoginCredentials,
  LoginResponseApi,
  RefreshTokenResponseApi,
  RegisterPayload,
  ResetPasswordPayload,
  Usuario,
  UsuarioApi,
} from '../types/auth'

class AuthService {
  private readonly client = new HttpClient('/auth')

  async register(payload: RegisterPayload): Promise<Usuario> {
    const data = await this.client.post<UsuarioApi, RegisterPayload>('/register', payload)
    return usuarioAdapter(data)
  }

  async confirmEmail(payload: ConfirmEmailPayload): Promise<void> {
    await this.client.post<{ confirmed: boolean }, ConfirmEmailPayload>('/confirm-email', payload)
  }

  async login(credentials: LoginCredentials): Promise<AuthSession> {
    const data = await this.client.post<LoginResponseApi, LoginCredentials>('/login', credentials)
    return loginResponseAdapter(data)
  }

  async refresh(refreshToken: string): Promise<AuthTokens> {
    const data = await this.client.post<RefreshTokenResponseApi, { refreshToken: string }>(
      '/refresh',
      { refreshToken },
    )
    return { token: data.token, refreshToken: data.refreshToken }
  }

  async logout(refreshToken: string): Promise<void> {
    await this.client.post<{ loggedOut: boolean }, { refreshToken: string }>('/logout', { refreshToken })
  }

  async forgotPassword(payload: ForgotPasswordPayload): Promise<void> {
    await this.client.post<{ sent: boolean }, ForgotPasswordPayload>('/forgot-password', payload)
  }

  async resetPassword(payload: ResetPasswordPayload): Promise<void> {
    await this.client.post<{ reset: boolean }, ResetPasswordPayload>('/reset-password', payload)
  }

  /**
   * Devuelve los dominios permitidos para registro (lista blanca, gestionada por admin).
   * Endpoint público — no requiere autenticación.
   */
  async getAllowedDomains(): Promise<string[]> {
    return this.client.get<string[]>('/allowed-domains')
  }
}

export default new AuthService()
