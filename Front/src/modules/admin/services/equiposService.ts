import { HttpClient } from '@/lib/HttpClient'
import type {
  EquipoListItem,
  EquipoListItemApi,
  EquipoPayload,
  Miembro,
  MiembroApi,
} from '../types/admin'

function adaptEquipo(api: EquipoListItemApi): EquipoListItem {
  return {
    id: api.id,
    nombre: api.nombre,
    idEquipoPadre: api.idEquipoPadre,
    nombreEquipoPadre: api.nombreEquipoPadre,
    idLider: api.idLider,
    nombreLider: api.nombreLider,
    correoLider: api.correoLider,
    totalMiembros: api.totalMiembros,
    totalSubEquipos: api.totalSubEquipos,
    fechaCreacion: new Date(api.fechaCreacion),
  }
}

function adaptMiembro(api: MiembroApi): Miembro {
  return {
    id: api.id,
    nombre: api.nombre,
    correo: api.correo,
    rolGlobal: api.rolGlobal,
    fechaAgregado: new Date(api.fechaAgregado),
  }
}

class EquiposService {
  private readonly client = new HttpClient('/admin/equipos')

  async listar(): Promise<EquipoListItem[]> {
    const data = await this.client.get<EquipoListItemApi[]>()
    return data.map(adaptEquipo)
  }

  async crear(payload: EquipoPayload): Promise<EquipoListItem> {
    const data = await this.client.post<EquipoListItemApi, EquipoPayload>('', payload)
    return adaptEquipo(data)
  }

  async actualizar(id: number, payload: EquipoPayload): Promise<EquipoListItem> {
    const data = await this.client.patch<EquipoListItemApi, EquipoPayload>(`/${id}`, payload)
    return adaptEquipo(data)
  }

  async eliminar(id: number): Promise<void> {
    await this.client.delete<{ eliminado: boolean }>(`/${id}`)
  }

  async listarMiembros(idEquipo: number): Promise<Miembro[]> {
    // El detalle del equipo incluye los miembros directamente
    const detalle = await this.client.get<{ miembros: MiembroApi[] }>(`/${idEquipo}`)
    return (detalle.miembros ?? []).map(adaptMiembro)
  }

  async agregarMiembro(idEquipo: number, idUsuario: number): Promise<Miembro> {
    const data = await this.client.post<MiembroApi, { idUsuario: number }>(
      `/${idEquipo}/miembros`,
      { idUsuario },
    )
    return adaptMiembro(data)
  }

  async quitarMiembro(idEquipo: number, idUsuario: number): Promise<void> {
    await this.client.delete<{ eliminado: boolean }>(`/${idEquipo}/miembros/${idUsuario}`)
  }
}

export default new EquiposService()
