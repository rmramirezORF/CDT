import { api } from '@/lib/api'
import { ENV } from '@/config/env'
import type { ApiResponse } from '@/types/api'
import { extractErrorMessage } from '@/utils/errorMapper'
import type {
  ActualizarProyectoPayload,
  ListarProyectosFilters,
  PaginationApi,
  Proyecto,
  ProyectoApi,
  ProyectoPayload,
} from '../types/admin'

function adaptProyecto(api: ProyectoApi): Proyecto {
  return {
    id: api.id,
    nombre: api.nombre,
    clave: api.clave,
    descripcion: api.descripcion,
    idEquipo: api.idEquipo,
    nombreEquipo: api.nombreEquipo,
    idCreador: api.idCreador,
    nombreCreador: api.nombreCreador,
    fechaCreacion: new Date(api.fechaCreacion),
    fechaModificacion: new Date(api.fechaModificacion),
  }
}

export interface ProyectosListResult {
  items: Proyecto[]
  pagination: PaginationApi
}

class ProyectosService {
  private readonly path = '/admin/proyectos'

  async listar(filters: ListarProyectosFilters = {}): Promise<ProyectosListResult> {
    const url = `${ENV.API_PREFIX}${this.path}`
    const params: Record<string, string | number> = {}
    if (filters.q) params.q = filters.q
    if (filters.idEquipo != null) params.idEquipo = filters.idEquipo
    params.page = filters.page ?? 1
    params.pageSize = filters.pageSize ?? 20

    const response = await api.get<ApiResponse<ProyectoApi[]>>(url, { params })
    if (!response.data.success) throw new Error(extractErrorMessage(response))

    return {
      items: (response.data.data ?? []).map(adaptProyecto),
      pagination: response.data.pagination ?? { page: 1, pageSize: 20, totalRecords: 0 },
    }
  }

  async crear(payload: ProyectoPayload): Promise<Proyecto> {
    const url = `${ENV.API_PREFIX}${this.path}`
    const response = await api.post<ApiResponse<ProyectoApi>>(url, payload)
    if (!response.data.success) throw new Error(extractErrorMessage(response))
    return adaptProyecto(response.data.data!)
  }

  async actualizar(id: number, payload: ActualizarProyectoPayload): Promise<Proyecto> {
    const url = `${ENV.API_PREFIX}${this.path}/${id}`
    const response = await api.patch<ApiResponse<ProyectoApi>>(url, payload)
    if (!response.data.success) throw new Error(extractErrorMessage(response))
    return adaptProyecto(response.data.data!)
  }

  async eliminar(id: number): Promise<void> {
    const url = `${ENV.API_PREFIX}${this.path}/${id}`
    const response = await api.delete<ApiResponse<{ eliminado: boolean }>>(url)
    if (!response.data.success) throw new Error(extractErrorMessage(response))
  }
}

export default new ProyectosService()
