import { api } from '@/lib/api'
import { ENV } from '@/config/env'
import type { ApiResponse } from '@/types/api'
import { extractErrorMessage } from '@/utils/errorMapper'
import type {
  ActualizarListaPayload,
  CrearListaPayload,
  Lista,
  ListaApi,
  ReordenarListasPayload,
} from '../types/admin'

function adaptLista(api: ListaApi): Lista {
  return {
    id: api.id,
    nombre: api.nombre,
    color: api.color,
    orden: api.orden,
    idProyecto: api.idProyecto,
    nombreProyecto: api.nombreProyecto,
    idCreador: api.idCreador,
    nombreCreador: api.nombreCreador,
    fechaCreacion: new Date(api.fechaCreacion),
    fechaModificacion: new Date(api.fechaModificacion),
  }
}

class ListasService {
  private readonly path = '/admin/listas'

  async listar(idProyecto: number): Promise<Lista[]> {
    const url = `${ENV.API_PREFIX}${this.path}`
    const response = await api.get<ApiResponse<ListaApi[]>>(url, {
      params: { idProyecto },
    })
    if (!response.data.success) throw new Error(extractErrorMessage(response))
    return (response.data.data ?? []).map(adaptLista)
  }

  async crear(payload: CrearListaPayload): Promise<Lista> {
    const url = `${ENV.API_PREFIX}${this.path}`
    const response = await api.post<ApiResponse<ListaApi>>(url, payload)
    if (!response.data.success) throw new Error(extractErrorMessage(response))
    return adaptLista(response.data.data!)
  }

  async actualizar(id: number, payload: ActualizarListaPayload): Promise<Lista> {
    const url = `${ENV.API_PREFIX}${this.path}/${id}`
    const response = await api.patch<ApiResponse<ListaApi>>(url, payload)
    if (!response.data.success) throw new Error(extractErrorMessage(response))
    return adaptLista(response.data.data!)
  }

  async eliminar(id: number): Promise<void> {
    const url = `${ENV.API_PREFIX}${this.path}/${id}`
    const response = await api.delete<ApiResponse<{ eliminado: boolean }>>(url)
    if (!response.data.success) throw new Error(extractErrorMessage(response))
  }

  async reordenar(payload: ReordenarListasPayload): Promise<void> {
    const url = `${ENV.API_PREFIX}${this.path}/reordenar`
    const response = await api.post<ApiResponse<{ reordenado: boolean }>>(url, payload)
    if (!response.data.success) throw new Error(extractErrorMessage(response))
  }
}

export default new ListasService()
