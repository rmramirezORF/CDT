<script setup lang="ts">
import { ref, watch } from 'vue'
import { Search, Plus, Pencil, Trash2, X, Check, FolderKanban } from 'lucide-vue-next'
import { Button } from '@/components/ui/button'
import { Card } from '@/components/ui/card'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Select } from '@/components/ui/select'
import { extractErrorMessage } from '@/utils/errorMapper'
import proyectosService from '../services/proyectosService'
import equiposService from '../services/equiposService'
import type { EquipoListItem, Proyecto } from '../types/admin'

const items = ref<Proyecto[]>([])
const equipos = ref<EquipoListItem[]>([])
const isLoading = ref(false)
const error = ref<string | null>(null)

const search = ref('')
const filtroEquipo = ref<number | null>(null)

const total = ref(0)
const page = ref(1)
const pageSize = ref(20)

// Crear
const showCrear = ref(false)
const nuevoNombre = ref('')
const nuevaClave = ref('')
const nuevaDescripcion = ref('')
const nuevoIdEquipo = ref<number | null>(null)
const isCreating = ref(false)

// Editar
const editingId = ref<number | null>(null)
const editNombre = ref('')
const editDescripcion = ref('')
const editIdEquipo = ref<number | null>(null)
const updatingId = ref<number | null>(null)

async function load() {
  isLoading.value = true
  error.value = null
  try {
    const result = await proyectosService.listar({
      q: search.value.trim() || undefined,
      idEquipo: filtroEquipo.value,
      page: page.value,
      pageSize: pageSize.value,
    })
    items.value = result.items
    total.value = result.pagination.totalRecords
  } catch (e) {
    error.value = extractErrorMessage(e)
  } finally {
    isLoading.value = false
  }
}

async function loadEquipos() {
  try {
    equipos.value = await equiposService.listar()
  } catch {
    /* silencioso */
  }
}

let timeoutId: number | undefined
watch(search, () => {
  if (timeoutId) clearTimeout(timeoutId)
  timeoutId = window.setTimeout(() => {
    page.value = 1
    load()
  }, 300)
})

watch(filtroEquipo, () => {
  page.value = 1
  load()
})

watch(page, load)

load()
loadEquipos()

async function onCrear() {
  if (!nuevoNombre.value.trim() || !nuevaClave.value.trim() || !nuevoIdEquipo.value) return
  isCreating.value = true
  try {
    const created = await proyectosService.crear({
      nombre: nuevoNombre.value.trim(),
      clave: nuevaClave.value.trim().toUpperCase(),
      descripcion: nuevaDescripcion.value.trim() || null,
      idEquipo: nuevoIdEquipo.value,
    })
    items.value.unshift(created)
    total.value++
    nuevoNombre.value = ''
    nuevaClave.value = ''
    nuevaDescripcion.value = ''
    nuevoIdEquipo.value = null
    showCrear.value = false
  } catch (e) {
    alert(extractErrorMessage(e))
  } finally {
    isCreating.value = false
  }
}

function iniciarEdicion(p: Proyecto) {
  editingId.value = p.id
  editNombre.value = p.nombre
  editDescripcion.value = p.descripcion ?? ''
  editIdEquipo.value = p.idEquipo
}

function cancelarEdicion() {
  editingId.value = null
}

async function guardarEdicion(p: Proyecto) {
  if (!editIdEquipo.value) return
  updatingId.value = p.id
  try {
    const updated = await proyectosService.actualizar(p.id, {
      nombre: editNombre.value.trim(),
      descripcion: editDescripcion.value.trim() || null,
      idEquipo: editIdEquipo.value,
    })
    const idx = items.value.findIndex((x) => x.id === p.id)
    if (idx >= 0) items.value[idx] = updated
    cancelarEdicion()
  } catch (e) {
    alert(extractErrorMessage(e))
  } finally {
    updatingId.value = null
  }
}

async function onEliminar(p: Proyecto) {
  if (!confirm(`¿Eliminar el proyecto "${p.nombre}"?\n\nEsta acción no se puede deshacer.`)) return
  updatingId.value = p.id
  try {
    await proyectosService.eliminar(p.id)
    items.value = items.value.filter((x) => x.id !== p.id)
    total.value = Math.max(0, total.value - 1)
  } catch (e) {
    alert(extractErrorMessage(e))
  } finally {
    updatingId.value = null
  }
}

function fmt(d: Date) {
  return d.toLocaleDateString('es-CO', { day: '2-digit', month: 'short', year: 'numeric' })
}
</script>

<template>
  <div class="space-y-4">
    <div class="flex items-center justify-between">
      <div>
        <h1 class="text-2xl font-semibold">Proyectos</h1>
        <p class="text-sm text-neutral-500">Cada proyecto pertenece a un equipo y agrupará listas y tareas.</p>
      </div>
      <div class="flex items-center gap-3">
        <span class="text-sm text-neutral-500">{{ total }} {{ total === 1 ? 'proyecto' : 'proyectos' }}</span>
        <Button @click="showCrear = !showCrear">
          <Plus :size="14" /> {{ showCrear ? 'Cancelar' : 'Nuevo proyecto' }}
        </Button>
      </div>
    </div>

    <!-- Form crear -->
    <Card v-if="showCrear" class="p-4 border-blue-200 dark:border-blue-900 bg-blue-50/30 dark:bg-blue-950/20">
      <Label class="text-xs uppercase text-neutral-500 mb-2 block">Crear proyecto</Label>
      <form class="space-y-3" @submit.prevent="onCrear">
        <div class="grid grid-cols-1 sm:grid-cols-3 gap-3">
          <div class="sm:col-span-2">
            <Label class="text-xs">Nombre</Label>
            <Input v-model="nuevoNombre" placeholder="Ej: Lanzamiento Q3" />
          </div>
          <div>
            <Label class="text-xs">Clave (Jira-style, MAYÚSCULAS)</Label>
            <Input
              v-model="nuevaClave"
              placeholder="Ej: ANI"
              :maxlength="10"
              style="text-transform: uppercase"
            />
          </div>
        </div>
        <div class="grid grid-cols-1 sm:grid-cols-2 gap-3">
          <div>
            <Label class="text-xs">Equipo</Label>
            <Select v-model="nuevoIdEquipo as any">
              <option :value="null">— Seleccionar equipo —</option>
              <option v-for="e in equipos" :key="e.id" :value="e.id">{{ e.nombre }}</option>
            </Select>
          </div>
          <div>
            <Label class="text-xs">Descripción (opcional)</Label>
            <Input v-model="nuevaDescripcion" placeholder="Breve descripción del proyecto" />
          </div>
        </div>
        <div class="flex justify-end gap-2">
          <Button type="button" variant="outline" @click="showCrear = false">Cancelar</Button>
          <Button
            type="submit"
            :disabled="isCreating || !nuevoNombre.trim() || !nuevaClave.trim() || !nuevoIdEquipo"
          >
            <Plus :size="14" /> Crear proyecto
          </Button>
        </div>
      </form>
    </Card>

    <!-- Filtros -->
    <Card class="p-4">
      <div class="grid grid-cols-1 sm:grid-cols-2 gap-3">
        <div class="relative">
          <Search :size="14" class="absolute left-3 top-1/2 -translate-y-1/2 text-neutral-400" />
          <Input v-model="search" placeholder="Buscar por nombre…" class="pl-9" />
        </div>
        <Select v-model="filtroEquipo as any">
          <option :value="null">Todos los equipos</option>
          <option v-for="e in equipos" :key="e.id" :value="e.id">{{ e.nombre }}</option>
        </Select>
      </div>
    </Card>

    <!-- Lista -->
    <Card class="overflow-hidden">
      <table class="w-full text-sm">
        <thead class="bg-neutral-50 dark:bg-neutral-900/50 border-b border-neutral-200 dark:border-neutral-800">
          <tr class="text-left">
            <th class="px-4 py-2 font-medium text-neutral-600 dark:text-neutral-400 w-20">Clave</th>
            <th class="px-4 py-2 font-medium text-neutral-600 dark:text-neutral-400">Nombre</th>
            <th class="px-4 py-2 font-medium text-neutral-600 dark:text-neutral-400">Equipo</th>
            <th class="px-4 py-2 font-medium text-neutral-600 dark:text-neutral-400">Creador</th>
            <th class="px-4 py-2 font-medium text-neutral-600 dark:text-neutral-400">Fecha</th>
            <th class="px-4 py-2 font-medium text-neutral-600 dark:text-neutral-400 text-right">Acciones</th>
          </tr>
        </thead>
        <tbody>
          <tr v-if="isLoading && !items.length">
            <td colspan="6" class="px-4 py-8 text-center text-neutral-500">Cargando…</td>
          </tr>
          <tr v-else-if="!items.length">
            <td colspan="6" class="px-4 py-12 text-center">
              <FolderKanban :size="32" class="mx-auto mb-2 text-neutral-300 dark:text-neutral-700" />
              <p class="text-neutral-500">No hay proyectos todavía.</p>
              <p class="text-xs text-neutral-400 mt-1">Click en "Nuevo proyecto" para crear el primero.</p>
            </td>
          </tr>
          <tr
            v-for="p in items"
            :key="p.id"
            class="border-b border-neutral-100 dark:border-neutral-800 last:border-b-0 hover:bg-neutral-50 dark:hover:bg-neutral-900/30"
          >
            <template v-if="editingId === p.id">
              <td class="px-4 py-2">
                <span class="font-mono text-xs px-2 py-0.5 rounded bg-neutral-100 dark:bg-neutral-800 text-neutral-500">
                  {{ p.clave }}
                </span>
              </td>
              <td class="px-4 py-2"><Input v-model="editNombre" /></td>
              <td class="px-4 py-2">
                <Select v-model="editIdEquipo as any">
                  <option :value="null">— Seleccionar —</option>
                  <option v-for="e in equipos" :key="e.id" :value="e.id">{{ e.nombre }}</option>
                </Select>
              </td>
              <td class="px-4 py-2 col-span-2" colspan="2">
                <Input v-model="editDescripcion" placeholder="Descripción" />
              </td>
              <td class="px-4 py-2 text-right space-x-1">
                <Button variant="ghost" size="sm" :disabled="updatingId === p.id" @click="guardarEdicion(p)">
                  <Check :size="14" />
                </Button>
                <Button variant="ghost" size="sm" @click="cancelarEdicion">
                  <X :size="14" />
                </Button>
              </td>
            </template>
            <template v-else>
              <td class="px-4 py-2">
                <span class="font-mono text-xs px-2 py-0.5 rounded bg-blue-100 dark:bg-blue-950 text-blue-700 dark:text-blue-300">
                  {{ p.clave }}
                </span>
              </td>
              <td class="px-4 py-2 font-medium">
                <div>{{ p.nombre }}</div>
                <div v-if="p.descripcion" class="text-xs text-neutral-500 mt-0.5 line-clamp-1">{{ p.descripcion }}</div>
              </td>
              <td class="px-4 py-2">
                <span class="inline-block text-xs px-2 py-0.5 rounded bg-neutral-100 dark:bg-neutral-800 text-neutral-700 dark:text-neutral-300">
                  {{ p.nombreEquipo }}
                </span>
              </td>
              <td class="px-4 py-2 text-neutral-600 dark:text-neutral-400 text-xs">
                {{ p.nombreCreador ?? '—' }}
              </td>
              <td class="px-4 py-2 text-neutral-500 text-xs">{{ fmt(p.fechaCreacion) }}</td>
              <td class="px-4 py-2 text-right space-x-1 whitespace-nowrap">
                <Button variant="ghost" size="sm" :disabled="updatingId === p.id" @click="iniciarEdicion(p)" title="Editar">
                  <Pencil :size="14" />
                </Button>
                <Button
                  variant="ghost"
                  size="sm"
                  class="text-red-600 hover:bg-red-50 dark:hover:bg-red-950"
                  :disabled="updatingId === p.id"
                  @click="onEliminar(p)"
                  title="Eliminar"
                >
                  <Trash2 :size="14" />
                </Button>
              </td>
            </template>
          </tr>
        </tbody>
      </table>

      <div v-if="total > pageSize" class="flex items-center justify-between p-3 border-t border-neutral-200 dark:border-neutral-800 text-sm">
        <span class="text-neutral-500">Página {{ page }} de {{ Math.ceil(total / pageSize) }}</span>
        <div class="flex gap-2">
          <Button variant="outline" size="sm" :disabled="page <= 1" @click="page--">Anterior</Button>
          <Button variant="outline" size="sm" :disabled="page >= Math.ceil(total / pageSize)" @click="page++">Siguiente</Button>
        </div>
      </div>
    </Card>

    <p v-if="error" class="text-sm text-red-600">{{ error }}</p>
  </div>
</template>
