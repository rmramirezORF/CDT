import type { UsuarioListItemApi, UsuarioListItem } from '../types/admin'

export function usuarioListItemAdapter(data: UsuarioListItemApi): UsuarioListItem {
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
