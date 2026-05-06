// ----- Tipos espejo del backend -----

export interface UsuarioListItemApi {
  id: number
  nombre: string
  correo: string
  rolGlobal: string
  estado: boolean
  fechaCreacion: string
  fechaConfirmacionEmail: string | null
}

export interface CatalogoItemApi {
  id: number
  nombre: string
  color: string
  orden: number
}

export interface PaginationApi {
  page: number
  pageSize: number
  totalRecords: number
}

// ----- Tipos del frontend -----

export type RolGlobal = 'Admin' | 'Lider' | 'Miembro'

export interface UsuarioListItem {
  id: number
  nombre: string
  correo: string
  rolGlobal: RolGlobal | string
  estado: boolean
  fechaCreacion: Date
  fechaConfirmacionEmail: Date | null
}

export interface CatalogoItem {
  id: number
  nombre: string
  color: string
  orden: number
}

export type CatalogoTipo = 'estados' | 'prioridades' | 'etiquetas'

// ----- Dominios permitidos para registro -----

export interface DominioPermitidoApi {
  id: number
  dominio: string
}

export interface DominioPermitido {
  id: number
  dominio: string
}

export interface ListarUsuariosFilters {
  q?: string
  rol?: RolGlobal | ''
  estado?: boolean | null
  page?: number
  pageSize?: number
}

export interface CatalogoItemPayload {
  nombre: string
  color: string
  orden: number
}

// ----- Equipos jerárquicos -----

export interface EquipoListItemApi {
  id: number
  nombre: string
  idEquipoPadre: number | null
  nombreEquipoPadre: string | null
  idLider: number | null
  nombreLider: string | null
  correoLider: string | null
  totalMiembros: number
  totalSubEquipos: number
  fechaCreacion: string
}

export interface MiembroApi {
  id: number
  nombre: string
  correo: string
  rolGlobal: string
  fechaAgregado: string
}

export interface EquipoDetalleApi {
  id: number
  nombre: string
  idEquipoPadre: number | null
  nombreEquipoPadre: string | null
  idLider: number | null
  nombreLider: string | null
  correoLider: string | null
  fechaCreacion: string
  miembros: MiembroApi[]
  subEquipos: EquipoListItemApi[]
}

export interface EquipoListItem {
  id: number
  nombre: string
  idEquipoPadre: number | null
  nombreEquipoPadre: string | null
  idLider: number | null
  nombreLider: string | null
  correoLider: string | null
  totalMiembros: number
  totalSubEquipos: number
  fechaCreacion: Date
}

export interface Miembro {
  id: number
  nombre: string
  correo: string
  rolGlobal: string
  fechaAgregado: Date
}

export interface EquipoPayload {
  nombre: string
  idEquipoPadre: number | null
  idLider: number | null
}

// ----- Proyectos -----

export interface ProyectoApi {
  id: number
  nombre: string
  clave: string
  descripcion: string | null
  idEquipo: number
  nombreEquipo: string
  idCreador: number | null
  nombreCreador: string | null
  fechaCreacion: string
  fechaModificacion: string
}

export interface Proyecto {
  id: number
  nombre: string
  clave: string
  descripcion: string | null
  idEquipo: number
  nombreEquipo: string
  idCreador: number | null
  nombreCreador: string | null
  fechaCreacion: Date
  fechaModificacion: Date
}

export interface ListarProyectosFilters {
  q?: string
  idEquipo?: number | null
  page?: number
  pageSize?: number
}

export interface ProyectoPayload {
  nombre: string
  clave: string
  descripcion: string | null
  idEquipo: number
}

export interface ActualizarProyectoPayload {
  nombre: string
  descripcion: string | null
  idEquipo: number
}

// ----- Listas -----

export interface ListaApi {
  id: number
  nombre: string
  color: string | null
  orden: number
  idProyecto: number
  nombreProyecto: string
  idCreador: number | null
  nombreCreador: string | null
  fechaCreacion: string
  fechaModificacion: string
}

export interface Lista {
  id: number
  nombre: string
  color: string | null
  orden: number
  idProyecto: number
  nombreProyecto: string
  idCreador: number | null
  nombreCreador: string | null
  fechaCreacion: Date
  fechaModificacion: Date
}

export interface CrearListaPayload {
  nombre: string
  color: string | null
  idProyecto: number
  orden?: number | null
}

export interface ActualizarListaPayload {
  nombre: string
  color: string | null
}

export interface ReordenarListasPayload {
  idProyecto: number
  items: { id: number; orden: number }[]
}

// ----- Tareas (estilo Jira) -----

export interface TareaApi {
  id: number
  clave: string
  numeroEnProyecto: number
  titulo: string
  descripcion: string | null

  idProyecto: number
  nombreProyecto: string
  claveProyecto: string

  idLista: number
  nombreLista: string

  idTipoActividad: number | null
  nombreTipoActividad: string | null
  colorTipoActividad: string | null

  idEstado: number | null
  nombreEstado: string | null
  colorEstado: string | null

  idPrioridad: number | null
  nombrePrioridad: string | null
  colorPrioridad: string | null

  idResponsable: number | null
  nombreResponsable: string | null

  idInformador: number | null
  nombreInformador: string | null

  fechaVencimiento: string | null
  orden: number

  fechaCreacion: string
  fechaModificacion: string
}

export interface Tarea {
  id: number
  clave: string
  numeroEnProyecto: number
  titulo: string
  descripcion: string | null

  idProyecto: number
  nombreProyecto: string
  claveProyecto: string

  idLista: number
  nombreLista: string

  idTipoActividad: number | null
  nombreTipoActividad: string | null
  colorTipoActividad: string | null

  idEstado: number | null
  nombreEstado: string | null
  colorEstado: string | null

  idPrioridad: number | null
  nombrePrioridad: string | null
  colorPrioridad: string | null

  idResponsable: number | null
  nombreResponsable: string | null

  idInformador: number | null
  nombreInformador: string | null

  fechaVencimiento: Date | null
  orden: number

  fechaCreacion: Date
  fechaModificacion: Date
}

export interface ListarTareasFilters {
  idProyecto?: number | null
  idLista?: number | null
  idResponsable?: number | null
  idEstado?: number | null
  idPrioridad?: number | null
  q?: string
  page?: number
  pageSize?: number
}

export interface CrearTareaPayload {
  titulo: string
  descripcion?: string | null
  idLista: number
  idTipoActividad?: number | null
  idEstado?: number | null
  idPrioridad?: number | null
  idResponsable?: number | null
  fechaVencimiento?: string | null
}

export interface ActualizarTareaPayload {
  titulo: string
  descripcion?: string | null
  idLista?: number | null
  idTipoActividad?: number | null
  idEstado?: number | null
  idPrioridad?: number | null
  idResponsable?: number | null
  fechaVencimiento?: string | null
}
