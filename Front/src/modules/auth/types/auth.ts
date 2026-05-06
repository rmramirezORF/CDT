// ----- Tipos espejo del backend (sufijo Api) -----

export interface UsuarioApi {
  id: number
  nombre: string
  correo: string
  rolGlobal: string
  estado: boolean
  fechaCreacion: string
  fechaConfirmacionEmail: string | null
}

export interface LoginResponseApi {
  token: string
  refreshToken: string
  usuario: UsuarioApi
}

export interface RefreshTokenResponseApi {
  token: string
  refreshToken: string
}

// ----- Tipos del frontend -----

export type RolGlobal = 'Admin' | 'Lider' | 'Miembro'

export interface Usuario {
  id: number
  nombre: string
  correo: string
  rolGlobal: RolGlobal | string
  estado: boolean
  fechaCreacion: Date
  fechaConfirmacionEmail: Date | null
}

export interface AuthTokens {
  token: string
  refreshToken: string
}

export interface AuthSession {
  tokens: AuthTokens
  usuario: Usuario
}

// ----- Payloads de los formularios -----

export interface LoginCredentials {
  correo: string
  password: string
}

export interface RegisterPayload {
  nombre: string
  correo: string
  password: string
}

export interface ConfirmEmailPayload {
  correo: string
  codigo: string
}

export interface ForgotPasswordPayload {
  correo: string
}

export interface ResetPasswordPayload {
  correo: string
  codigo: string
  nuevaPassword: string
}
