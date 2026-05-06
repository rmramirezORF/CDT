import { api } from '@/lib/api'
import { ENV } from '@/config/env'
import type { ApiResponse } from '@/types/api'
import { extractErrorMessage } from '@/utils/errorMapper'
import { usuarioListItemAdapter } from '../adapters/adminAdapter'
import type {
  ListarUsuariosFilters,
  PaginationApi,
  UsuarioListItem,
  UsuarioListItemApi,
} from '../types/admin'

export interface UsuariosListResult {
  items: UsuarioListItem[]
  pagination: PaginationApi
}

class UsuariosService {
  private readonly path = '/admin/usuarios'

  async listar(filters: ListarUsuariosFilters = {}): Promise<UsuariosListResult> {
    const url = `${ENV.API_PREFIX}${this.path}`
    const params: Record<string, string | number | boolean> = {}
    if (filters.q) params.q = filters.q
    if (filters.rol) params.rol = filters.rol
    if (filters.estado != null) params.estado = filters.estado
    params.page = filters.page ?? 1
    params.pageSize = filters.pageSize ?? 20

    const response = await api.get<ApiResponse<UsuarioListItemApi[]>>(url, { params })
    if (!response.data.success) throw new Error(extractErrorMessage(response))

    return {
      items: (response.data.data ?? []).map(usuarioListItemAdapter),
      pagination: response.data.pagination ?? { page: 1, pageSize: 20, totalRecords: 0 },
    }
  }

  async cambiarRol(id: number, rolGlobal: string): Promise<UsuarioListItem> {
    const url = `${ENV.API_PREFIX}${this.path}/${id}/rol`
    const response = await api.patch<ApiResponse<UsuarioListItemApi>>(url, { rolGlobal })
    if (!response.data.success) throw new Error(extractErrorMessage(response))
    return usuarioListItemAdapter(response.data.data!)
  }

  async cambiarEstado(id: number, estado: boolean): Promise<UsuarioListItem> {
    const url = `${ENV.API_PREFIX}${this.path}/${id}/estado`
    const response = await api.patch<ApiResponse<UsuarioListItemApi>>(url, { estado })
    if (!response.data.success) throw new Error(extractErrorMessage(response))
    return usuarioListItemAdapter(response.data.data!)
  }

  async eliminar(id: number): Promise<void> {
    const url = `${ENV.API_PREFIX}${this.path}/${id}`
    const response = await api.delete<ApiResponse<{ eliminado: boolean }>>(url)
    if (!response.data.success) throw new Error(extractErrorMessage(response))
  }

  async confirmarEmail(id: number): Promise<UsuarioListItem> {
    const url = `${ENV.API_PREFIX}${this.path}/${id}/confirmar-email`
    const response = await api.post<ApiResponse<UsuarioListItemApi>>(url, {})
    if (!response.data.success) throw new Error(extractErrorMessage(response))
    return usuarioListItemAdapter(response.data.data!)
  }
}

export default new UsuariosService()
