<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import {
  Plus, X, Search, Bug, Bookmark, Zap, LifeBuoy, CircleCheck,
  CalendarDays, User as UserIcon,
} from 'lucide-vue-next'
import { Button } from '@/components/ui/button'
import { Card } from '@/components/ui/card'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Select } from '@/components/ui/select'
import { extractErrorMessage } from '@/utils/errorMapper'
import listasService from '../services/listasService'
import proyectosService from '../services/proyectosService'
import tareasService from '../services/tareasService'
import type { Lista, Proyecto, Tarea } from '../types/admin'

const proyectos = ref<Proyecto[]>([])
const proyectoSel = ref<number | null>(null)
const listas = ref<Lista[]>([])
const tareas = ref<Tarea[]>([])

const filtroQ = ref('')

const isLoading = ref(false)
const error = ref<string | null>(null)

// Crear tarea
const showCrear = ref<number | null>(null) // id de la lista activa para crear
const nuevoTitulo = ref('')
const isCreating = ref(false)

// Drag & drop
const dragTareaId = ref<number | null>(null)
const dragOverListaId = ref<number | null>(null)

// Modal de detalle
const tareaSeleccionada = ref<Tarea | null>(null)

const proyectoActual = computed(() =>
  proyectos.value.find((p) => p.id === proyectoSel.value) ?? null,
)

const tareasFiltradas = computed(() => {
  const q = filtroQ.value.trim().toLowerCase()
  if (!q) return tareas.value
  return tareas.value.filter(
    (t) =>
      t.titulo.toLowerCase().includes(q) ||
      t.clave.toLowerCase().includes(q) ||
      (t.nombreResponsable ?? '').toLowerCase().includes(q),
  )
})

function tareasDeLista(idLista: number): Tarea[] {
  return tareasFiltradas.value
    .filter((t) => t.idLista === idLista)
    .sort((a, b) => a.orden - b.orden)
}

async function loadProyectos() {
  try {
    const result = await proyectosService.listar({ pageSize: 200 })
    proyectos.value = result.items
    if (proyectoSel.value === null && proyectos.value.length > 0) {
      proyectoSel.value = proyectos.value[0].id
    }
  } catch (e) {
    error.value = extractErrorMessage(e)
  }
}

async function loadTablero() {
  if (proyectoSel.value === null) {
    listas.value = []
    tareas.value = []
    return
  }
  isLoading.value = true
  error.value = null
  try {
    const [ls, ts] = await Promise.all([
      listasService.listar(proyectoSel.value),
      tareasService.listar({ idProyecto: proyectoSel.value, pageSize: 500 }),
    ])
    listas.value = ls
    tareas.value = ts.items
  } catch (e) {
    error.value = extractErrorMessage(e)
  } finally {
    isLoading.value = false
  }
}

watch(proyectoSel, () => {
  showCrear.value = null
  loadTablero()
})

loadProyectos().then(loadTablero)

// ----- Iconos por tipo de actividad (mismo set que el seeder) -----
const ICONOS_TIPO: Record<string, typeof CircleCheck> = {
  Tarea: CircleCheck,
  Historia: Bookmark,
  Bug: Bug,
  'Épica': Zap,
  Soporte: LifeBuoy,
}

function iconoDeTipo(nombre: string | null) {
  if (!nombre) return CircleCheck
  return ICONOS_TIPO[nombre] ?? CircleCheck
}

function fechaCorta(d: Date | null): string {
  if (!d) return ''
  const hoy = new Date()
  const ms = d.getTime() - hoy.getTime()
  const dias = Math.round(ms / 86400000)
  if (dias < -1) return `Vencida hace ${-dias}d`
  if (dias === -1) return 'Vencida ayer'
  if (dias === 0) return 'Vence hoy'
  if (dias === 1) return 'Vence mañana'
  if (dias < 7) return `En ${dias}d`
  return d.toLocaleDateString('es-CO', { day: '2-digit', month: 'short' })
}

function colorVencimiento(d: Date | null): string {
  if (!d) return 'text-neutral-400'
  const ms = d.getTime() - Date.now()
  if (ms < 0) return 'text-red-600'
  if (ms < 86400000 * 2) return 'text-orange-500'
  return 'text-neutral-500'
}

// ----- Crear tarea inline -----
async function onCrear(idLista: number) {
  if (!nuevoTitulo.value.trim()) return
  isCreating.value = true
  try {
    const created = await tareasService.crear({
      titulo: nuevoTitulo.value.trim(),
      idLista,
    })
    tareas.value.push(created)
    nuevoTitulo.value = ''
    showCrear.value = null
  } catch (e) {
    alert(extractErrorMessage(e))
  } finally {
    isCreating.value = false
  }
}

// ----- Drag & drop -----
function onDragStart(t: Tarea, ev: DragEvent) {
  dragTareaId.value = t.id
  if (ev.dataTransfer) {
    ev.dataTransfer.effectAllowed = 'move'
    ev.dataTransfer.setData('text/plain', String(t.id))
  }
}

function onDragOver(idLista: number, ev: DragEvent) {
  ev.preventDefault()
  if (ev.dataTransfer) ev.dataTransfer.dropEffect = 'move'
  dragOverListaId.value = idLista
}

function onDragLeave(idLista: number) {
  if (dragOverListaId.value === idLista) dragOverListaId.value = null
}

async function onDrop(idLista: number, ev: DragEvent) {
  ev.preventDefault()
  dragOverListaId.value = null
  const idTarea = dragTareaId.value
  dragTareaId.value = null
  if (!idTarea) return

  const tarea = tareas.value.find((t) => t.id === idTarea)
  if (!tarea || tarea.idLista === idLista) return

  // Optimistic update: encontrar la lista destino y mover
  const listaDestino = listas.value.find((l) => l.id === idLista)
  if (!listaDestino) return

  // Estado optimista
  const original = { ...tarea }
  tarea.idLista = idLista
  tarea.nombreLista = listaDestino.nombre

  try {
    const updated = await tareasService.actualizar(tarea.id, {
      titulo: tarea.titulo,
      descripcion: tarea.descripcion,
      idLista,
      idTipoActividad: tarea.idTipoActividad,
      idEstado: tarea.idEstado,
      idPrioridad: tarea.idPrioridad,
      idResponsable: tarea.idResponsable,
      fechaVencimiento: tarea.fechaVencimiento ? tarea.fechaVencimiento.toISOString() : null,
    })
    const idx = tareas.value.findIndex((t) => t.id === tarea.id)
    if (idx >= 0) tareas.value[idx] = updated
  } catch (e) {
    // Rollback
    Object.assign(tarea, original)
    alert(extractErrorMessage(e))
  }
}

function abrirDetalle(t: Tarea) {
  tareaSeleccionada.value = t
}

function cerrarDetalle() {
  tareaSeleccionada.value = null
}
</script>

<template>
  <div class="space-y-3 h-full flex flex-col">
    <!-- Header -->
    <div class="flex items-center justify-between gap-3 flex-shrink-0">
      <div class="min-w-0">
        <h1 class="text-2xl font-semibold flex items-center gap-2">
          <span v-if="proyectoActual" class="text-xs font-mono px-2 py-0.5 rounded bg-blue-100 dark:bg-blue-950 text-blue-700 dark:text-blue-300">
            {{ proyectoActual.clave }}
          </span>
          {{ proyectoActual?.nombre || 'Tablero' }}
        </h1>
        <p v-if="proyectoActual?.descripcion" class="text-sm text-neutral-500 truncate">
          {{ proyectoActual.descripcion }}
        </p>
      </div>
      <div class="flex items-center gap-2 flex-shrink-0">
        <div class="relative">
          <Search :size="14" class="absolute left-3 top-1/2 -translate-y-1/2 text-neutral-400" />
          <Input v-model="filtroQ" placeholder="Buscar tarea, clave, responsable…" class="pl-9 w-64" />
        </div>
      </div>
    </div>

    <!-- Selector proyecto -->
    <Card class="p-3 flex-shrink-0">
      <div class="flex items-center gap-3">
        <Label class="text-xs whitespace-nowrap">Proyecto:</Label>
        <Select v-model="proyectoSel as any" class="flex-1 max-w-md">
          <option :value="null" disabled>— Seleccionar —</option>
          <option v-for="p in proyectos" :key="p.id" :value="p.id">
            [{{ p.clave }}] {{ p.nombre }} · {{ p.nombreEquipo }}
          </option>
        </Select>
        <span v-if="proyectoActual" class="text-sm text-neutral-500">
          {{ tareas.length }} {{ tareas.length === 1 ? 'tarea' : 'tareas' }}
        </span>
      </div>
    </Card>

    <p v-if="error" class="text-sm text-red-600">{{ error }}</p>

    <!-- Tablero -->
    <div v-if="proyectoSel === null" class="flex-1 flex items-center justify-center text-neutral-500">
      Selecciona un proyecto para ver su tablero.
    </div>
    <div v-else-if="isLoading && !listas.length" class="flex-1 flex items-center justify-center text-neutral-500">
      Cargando tablero…
    </div>
    <div v-else-if="!listas.length" class="flex-1 flex items-center justify-center text-neutral-500">
      Este proyecto no tiene listas todavía. Ve a /admin/listas para crear las columnas del tablero.
    </div>

    <div
      v-else
      class="flex-1 overflow-x-auto overflow-y-hidden flex gap-3 pb-2"
    >
      <div
        v-for="lista in listas"
        :key="lista.id"
        class="w-72 flex-shrink-0 flex flex-col bg-neutral-100 dark:bg-neutral-900/60 rounded-lg border border-neutral-200 dark:border-neutral-800 transition-colors"
        :class="{ 'ring-2 ring-blue-400 ring-offset-1 dark:ring-offset-neutral-950': dragOverListaId === lista.id }"
        @dragover="onDragOver(lista.id, $event)"
        @dragleave="onDragLeave(lista.id)"
        @drop="onDrop(lista.id, $event)"
      >
        <!-- Header columna -->
        <div class="px-3 py-2 flex items-center justify-between border-b border-neutral-200 dark:border-neutral-800">
          <div class="flex items-center gap-2 min-w-0">
            <span
              v-if="lista.color"
              class="w-2.5 h-2.5 rounded-full flex-shrink-0"
              :style="{ backgroundColor: lista.color }"
            />
            <span class="font-medium text-sm truncate">{{ lista.nombre }}</span>
            <span class="text-xs text-neutral-500 flex-shrink-0">
              {{ tareasDeLista(lista.id).length }}
            </span>
          </div>
          <button
            class="p-1 rounded hover:bg-neutral-200 dark:hover:bg-neutral-800 text-neutral-500"
            title="Crear tarea"
            @click="showCrear = lista.id; nuevoTitulo = ''"
          >
            <Plus :size="14" />
          </button>
        </div>

        <!-- Tarjetas -->
        <div class="flex-1 overflow-y-auto p-2 space-y-2">
          <Card
            v-for="t in tareasDeLista(lista.id)"
            :key="t.id"
            class="p-2.5 cursor-grab active:cursor-grabbing hover:shadow-md hover:border-blue-300 dark:hover:border-blue-800 transition"
            draggable="true"
            @dragstart="onDragStart(t, $event)"
            @click="abrirDetalle(t)"
          >
            <!-- Línea 1: tipo + clave -->
            <div class="flex items-center gap-1.5 mb-1.5 text-xs text-neutral-500">
              <component
                :is="iconoDeTipo(t.nombreTipoActividad)"
                :size="14"
                :style="{ color: t.colorTipoActividad ?? '#6b7280' }"
              />
              <span class="font-mono">{{ t.clave }}</span>
              <span
                v-if="t.nombrePrioridad"
                class="ml-auto text-[10px] px-1.5 py-0.5 rounded uppercase font-medium"
                :style="{
                  backgroundColor: (t.colorPrioridad ?? '#94a3b8') + '20',
                  color: t.colorPrioridad ?? '#94a3b8',
                }"
              >
                {{ t.nombrePrioridad }}
              </span>
            </div>
            <!-- Título -->
            <div class="text-sm font-medium leading-snug line-clamp-2 mb-2">
              {{ t.titulo }}
            </div>
            <!-- Footer: vencimiento + responsable -->
            <div class="flex items-center justify-between text-xs">
              <span
                v-if="t.fechaVencimiento"
                class="flex items-center gap-1"
                :class="colorVencimiento(t.fechaVencimiento)"
              >
                <CalendarDays :size="12" />
                {{ fechaCorta(t.fechaVencimiento) }}
              </span>
              <span v-else />
              <span
                v-if="t.nombreResponsable"
                class="flex items-center gap-1 text-neutral-600 dark:text-neutral-400"
                :title="t.nombreResponsable"
              >
                <span class="w-5 h-5 rounded-full bg-neutral-300 dark:bg-neutral-700 text-[10px] font-medium flex items-center justify-center">
                  {{ t.nombreResponsable.split(' ').map(p => p[0]).slice(0, 2).join('').toUpperCase() }}
                </span>
              </span>
              <span v-else class="flex items-center gap-1 text-neutral-400">
                <UserIcon :size="12" /> Sin asignar
              </span>
            </div>
          </Card>

          <!-- Form crear tarea inline -->
          <div
            v-if="showCrear === lista.id"
            class="bg-white dark:bg-neutral-900 border border-blue-300 dark:border-blue-800 rounded p-2 space-y-2"
          >
            <Input
              v-model="nuevoTitulo"
              placeholder="Título de la tarea…"
              :autofocus="true"
              @keyup.enter="onCrear(lista.id)"
              @keyup.escape="showCrear = null"
            />
            <div class="flex justify-end gap-1">
              <Button size="sm" variant="ghost" @click="showCrear = null">Cancelar</Button>
              <Button size="sm" :disabled="isCreating || !nuevoTitulo.trim()" @click="onCrear(lista.id)">
                Crear
              </Button>
            </div>
          </div>
          <button
            v-else
            class="w-full text-left text-xs text-neutral-500 hover:text-neutral-700 dark:hover:text-neutral-300 px-2 py-1 rounded hover:bg-neutral-200 dark:hover:bg-neutral-800 flex items-center gap-1"
            @click="showCrear = lista.id; nuevoTitulo = ''"
          >
            <Plus :size="12" /> Agregar tarea
          </button>
        </div>
      </div>
    </div>

    <!-- Modal detalle de tarea -->
    <div
      v-if="tareaSeleccionada"
      class="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4"
      @click.self="cerrarDetalle"
    >
      <Card class="w-full max-w-2xl max-h-[90vh] overflow-y-auto p-6">
        <div class="flex items-start justify-between mb-4">
          <div>
            <div class="flex items-center gap-2 text-sm text-neutral-500 mb-1">
              <component
                :is="iconoDeTipo(tareaSeleccionada.nombreTipoActividad)"
                :size="14"
                :style="{ color: tareaSeleccionada.colorTipoActividad ?? '#6b7280' }"
              />
              <span class="font-mono">{{ tareaSeleccionada.clave }}</span>
              <span>·</span>
              <span>{{ tareaSeleccionada.nombreLista }}</span>
            </div>
            <h2 class="text-xl font-semibold">{{ tareaSeleccionada.titulo }}</h2>
          </div>
          <button class="p-1 rounded hover:bg-neutral-100 dark:hover:bg-neutral-800" @click="cerrarDetalle">
            <X :size="18" />
          </button>
        </div>

        <div v-if="tareaSeleccionada.descripcion" class="mb-6">
          <Label class="text-xs uppercase text-neutral-500 mb-1 block">Descripción</Label>
          <p class="text-sm whitespace-pre-wrap">{{ tareaSeleccionada.descripcion }}</p>
        </div>

        <div class="grid grid-cols-2 gap-4 text-sm">
          <div>
            <Label class="text-xs uppercase text-neutral-500 mb-1 block">Estado</Label>
            <span
              v-if="tareaSeleccionada.nombreEstado"
              class="inline-block px-2 py-1 rounded text-xs font-medium"
              :style="{
                backgroundColor: (tareaSeleccionada.colorEstado ?? '#94a3b8') + '20',
                color: tareaSeleccionada.colorEstado ?? '#94a3b8',
              }"
            >{{ tareaSeleccionada.nombreEstado }}</span>
            <span v-else class="text-neutral-400">—</span>
          </div>
          <div>
            <Label class="text-xs uppercase text-neutral-500 mb-1 block">Prioridad</Label>
            <span
              v-if="tareaSeleccionada.nombrePrioridad"
              class="inline-block px-2 py-1 rounded text-xs font-medium"
              :style="{
                backgroundColor: (tareaSeleccionada.colorPrioridad ?? '#94a3b8') + '20',
                color: tareaSeleccionada.colorPrioridad ?? '#94a3b8',
              }"
            >{{ tareaSeleccionada.nombrePrioridad }}</span>
            <span v-else class="text-neutral-400">—</span>
          </div>
          <div>
            <Label class="text-xs uppercase text-neutral-500 mb-1 block">Tipo</Label>
            <span class="text-sm">{{ tareaSeleccionada.nombreTipoActividad ?? '—' }}</span>
          </div>
          <div>
            <Label class="text-xs uppercase text-neutral-500 mb-1 block">Responsable</Label>
            <span class="text-sm">{{ tareaSeleccionada.nombreResponsable ?? 'Sin asignar' }}</span>
          </div>
          <div>
            <Label class="text-xs uppercase text-neutral-500 mb-1 block">Informador</Label>
            <span class="text-sm">{{ tareaSeleccionada.nombreInformador ?? '—' }}</span>
          </div>
          <div>
            <Label class="text-xs uppercase text-neutral-500 mb-1 block">Fecha de vencimiento</Label>
            <span class="text-sm" :class="colorVencimiento(tareaSeleccionada.fechaVencimiento)">
              {{ tareaSeleccionada.fechaVencimiento ? fechaCorta(tareaSeleccionada.fechaVencimiento) : '—' }}
            </span>
          </div>
        </div>

        <div class="mt-6 pt-4 border-t border-neutral-200 dark:border-neutral-800 text-xs text-neutral-500">
          Creada {{ tareaSeleccionada.fechaCreacion.toLocaleString('es-CO') }}
          · Actualizada {{ tareaSeleccionada.fechaModificacion.toLocaleString('es-CO') }}
        </div>
      </Card>
    </div>
  </div>
</template>
