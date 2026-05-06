import type { UsuarioApi, Usuario, LoginResponseApi, AuthSession } from '../types/auth'

export function usuarioAdapter(data: UsuarioApi): Usuario {
  return {
    id: data.id,
    nombre: data.nombre,
    correo: data.correo,
    rolGlobal: data.rolGlobal,
    estado: data.estado,
    fechaCreacion: new Date(data.fechaCreacion),
    fechaConfirmacionEmail: data.fechaConfirmacionEmail ? new Date(data.fechaConfirmacionEmail) : null,
  }
}

export function loginResponseAdapter(data: LoginResponseApi): AuthSession {
  return {
    tokens: { token: data.token, refreshToken: data.refreshToken },
    usuario: usuarioAdapter(data.usuario),
  }
}
