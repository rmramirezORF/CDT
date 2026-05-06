import { api } from '@/lib/api'
import { ENV } from '@/config/env'
import type { ApiResponse } from '@/types/api'
import { extractErrorMessage } from '@/utils/errorMapper'
import type {
  ActualizarTareaPayload,
  CrearTareaPayload,
  ListarTareasFilters,
  Tarea,
  TareaApi,
} from '../types/admin'

function adapt(api: TareaApi): Tarea {
  return {
    id: api.id,
    clave: api.clave,
    numeroEnProyecto: api.numeroEnProyecto,
    titulo: api.titulo,
    descripcion: api.descripcion,
    idProyecto: api.idProyecto,
    nombreProyecto: api.nombreProyecto,
    claveProyecto: api.claveProyecto,
    idLista: api.idLista,
    nombreLista: api.nombreLista,
    idTipoActividad: api.idTipoActividad,
    nombreTipoActividad: api.nombreTipoActividad,
    colorTipoActividad: api.colorTipoActividad,
    idEstado: api.idEstado,
    nombreEstado: api.nombreEstado,
    colorEstado: api.colorEstado,
    idPrioridad: api.idPrioridad,
    nombrePrioridad: api.nombrePrioridad,
    colorPrioridad: api.colorPrioridad,
    idResponsable: api.idResponsable,
    nombreResponsable: api.nombreResponsable,
    idInformador: api.idInformador,
    nombreInformador: api.nombreInformador,
    fechaVencimiento: api.fechaVencimiento ? new Date(api.fechaVencimiento) : null,
    orden: api.orden,
    fechaCreacion: new Date(api.fechaCreacion),
    fechaModificacion: new Date(api.fechaModificacion),
  }
}

export interface TareasListResult {
  items: Tarea[]
  total: number
}

class TareasService {
  private readonly path = '/tareas'

  async listar(filters: ListarTareasFilters = {}): Promise<TareasListResult> {
    const url = `${ENV.API_PREFIX}${this.path}`
    const params: Record<string, string | number> = {}
    if (filters.idProyecto != null) params.idProyecto = filters.idProyecto
    if (filters.idLista != null) params.idLista = filters.idLista
    if (filters.idResponsable != null) params.idResponsable = filters.idResponsable
    if (filters.idEstado != null) params.idEstado = filters.idEstado
    if (filters.idPrioridad != null) params.idPrioridad = filters.idPrioridad
    if (filters.q) params.q = filters.q
    params.page = filters.page ?? 1
    params.pageSize = filters.pageSize ?? 200

    const response = await api.get<ApiResponse<TareaApi[]>>(url, { params })
    if (!response.data.success) throw new Error(extractErrorMessage(response))

    return {
      items: (response.data.data ?? []).map(adapt),
      total: response.data.pagination?.totalRecords ?? 0,
    }
  }

  async obtener(id: number): Promise<Tarea> {
    const url = `${ENV.API_PREFIX}${this.path}/${id}`
    const response = await api.get<ApiResponse<TareaApi>>(url)
    if (!response.data.success) throw new Error(extractErrorMessage(response))
    return adapt(response.data.data!)
  }

  async crear(payload: CrearTareaPayload): Promise<Tarea> {
    const url = `${ENV.API_PREFIX}${this.path}`
    const response = await api.post<ApiResponse<TareaApi>>(url, payload)
    if (!response.data.success) throw new Error(extractErrorMessage(response))
    return adapt(response.data.data!)
  }

  async actualizar(id: number, payload: ActualizarTareaPayload): Promise<Tarea> {
    const url = `${ENV.API_PREFIX}${this.path}/${id}`
    const response = await api.patch<ApiResponse<TareaApi>>(url, payload)
    if (!response.data.success) throw new Error(extractErrorMessage(response))
    return adapt(response.data.data!)
  }

  async eliminar(id: number): Promise<void> {
    const url = `${ENV.API_PREFIX}${this.path}/${id}`
    const response = await api.delete<ApiResponse<{ eliminado: boolean }>>(url)
    if (!response.data.success) throw new Error(extractErrorMessage(response))
  }
}

export default new TareasService()
