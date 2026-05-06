import { HttpClient } from '@/lib/HttpClient'
import type {
  CatalogoItem,
  CatalogoItemPayload,
  CatalogoTipo,
  DominioPermitido,
} from '../types/admin'

class CatalogosService {
  private readonly clients: Record<CatalogoTipo, HttpClient> = {
    estados: new HttpClient('/admin/catalogos/estados'),
    prioridades: new HttpClient('/admin/catalogos/prioridades'),
    etiquetas: new HttpClient('/admin/catalogos/etiquetas'),
  }

  // ----- Catalogos visuales (estados / prioridades / etiquetas) -----

  listar(tipo: CatalogoTipo): Promise<CatalogoItem[]> {
    return this.clients[tipo].get<CatalogoItem[]>()
  }

  crear(tipo: CatalogoTipo, payload: CatalogoItemPayload): Promise<CatalogoItem> {
    return this.clients[tipo].post<CatalogoItem, CatalogoItemPayload>('', payload)
  }

  actualizar(tipo: CatalogoTipo, id: number, payload: CatalogoItemPayload): Promise<CatalogoItem> {
    return this.clients[tipo].patch<CatalogoItem, CatalogoItemPayload>(`/${id}`, payload)
  }

  eliminar(tipo: CatalogoTipo, id: number): Promise<{ eliminado: boolean }> {
    return this.clients[tipo].delete<{ eliminado: boolean }>(`/${id}`)
  }

  // ----- Dominios permitidos (lista blanca para registro) -----

  private readonly dominiosClient = new HttpClient('/admin/catalogos/dominios')

  listarDominios(): Promise<DominioPermitido[]> {
    return this.dominiosClient.get<DominioPermitido[]>()
  }

  crearDominio(dominio: string): Promise<DominioPermitido> {
    return this.dominiosClient.post<DominioPermitido, { dominio: string }>('', { dominio })
  }

  eliminarDominio(id: number): Promise<{ eliminado: boolean }> {
    return this.dominiosClient.delete<{ eliminado: boolean }>(`/${id}`)
  }
}

export default new CatalogosService()
