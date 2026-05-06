<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { Plus, Pencil, Trash2, X, Check, ListChecks, ChevronUp, ChevronDown } from 'lucide-vue-next'
import { Button } from '@/components/ui/button'
import { Card } from '@/components/ui/card'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Select } from '@/components/ui/select'
import { extractErrorMessage } from '@/utils/errorMapper'
import listasService from '../services/listasService'
import proyectosService from '../services/proyectosService'
import type { Lista, Proyecto } from '../types/admin'

const proyectos = ref<Proyecto[]>([])
const proyectoSel = ref<number | null>(null)
const items = ref<Lista[]>([])
const isLoading = ref(false)
const error = ref<string | null>(null)

// Crear
const showCrear = ref(false)
const nuevoNombre = ref('')
const nuevoColor = ref('')
const isCreating = ref(false)

// Editar
const editingId = ref<number | null>(null)
const editNombre = ref('')
const editColor = ref('')
const updatingId = ref<number | null>(null)

// Reordenar
const isReordering = ref(false)

const proyectoActual = computed(() =>
  proyectos.value.find((p) => p.id === proyectoSel.value) ?? null,
)

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

async function loadListas() {
  if (proyectoSel.value === null) {
    items.value = []
    return
  }
  isLoading.value = true
  error.value = null
  try {
    items.value = await listasService.listar(proyectoSel.value)
  } catch (e) {
    error.value = extractErrorMessage(e)
  } finally {
    isLoading.value = false
  }
}

watch(proyectoSel, () => {
  cancelarEdicion()
  showCrear.value = false
  loadListas()
})

loadProyectos().then(loadListas)

async function onCrear() {
  if (!nuevoNombre.value.trim() || proyectoSel.value === null) return
  isCreating.value = true
  try {
    const created = await listasService.crear({
      nombre: nuevoNombre.value.trim(),
      color: nuevoColor.value.trim() || null,
      idProyecto: proyectoSel.value,
    })
    items.value.push(created)
    items.value.sort((a, b) => a.orden - b.orden || a.id - b.id)
    nuevoNombre.value = ''
    nuevoColor.value = ''
    showCrear.value = false
  } catch (e) {
    alert(extractErrorMessage(e))
  } finally {
    isCreating.value = false
  }
}

function iniciarEdicion(l: Lista) {
  editingId.value = l.id
  editNombre.value = l.nombre
  editColor.value = l.color ?? ''
}

function cancelarEdicion() {
  editingId.value = null
}

async function guardarEdicion(l: Lista) {
  updatingId.value = l.id
  try {
    const updated = await listasService.actualizar(l.id, {
      nombre: editNombre.value.trim(),
      color: editColor.value.trim() || null,
    })
    const idx = items.value.findIndex((x) => x.id === l.id)
    if (idx >= 0) items.value[idx] = updated
    cancelarEdicion()
  } catch (e) {
    alert(extractErrorMessage(e))
  } finally {
    updatingId.value = null
  }
}

async function onEliminar(l: Lista) {
  if (!confirm(`¿Eliminar la lista "${l.nombre}"?\n\nEsta acción no se puede deshacer.`)) return
  updatingId.value = l.id
  try {
    await listasService.eliminar(l.id)
    items.value = items.value.filter((x) => x.id !== l.id)
  } catch (e) {
    alert(extractErrorMessage(e))
  } finally {
    updatingId.value = null
  }
}

async function moverArriba(idx: number) {
  if (idx <= 0 || isReordering.value || proyectoSel.value === null) return
  await intercambiar(idx, idx - 1)
}

async function moverAbajo(idx: number) {
  if (idx >= items.value.length - 1 || isReordering.value || proyectoSel.value === null) return
  await intercambiar(idx, idx + 1)
}

async function intercambiar(a: number, b: number) {
  isReordering.value = true
  const reordered = [...items.value]
  ;[reordered[a], reordered[b]] = [reordered[b], reordered[a]]
  // Reasignar orden por posición
  reordered.forEach((it, i) => (it.orden = i))
  items.value = reordered
  try {
    await listasService.reordenar({
      idProyecto: proyectoSel.value!,
      items: reordered.map((it) => ({ id: it.id, orden: it.orden })),
    })
  } catch (e) {
    alert(extractErrorMessage(e))
    await loadListas()
  } finally {
    isReordering.value = false
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
        <h1 class="text-2xl font-semibold">Listas</h1>
        <p class="text-sm text-neutral-500">
          Las listas son las columnas del Kanban dentro de cada proyecto.
        </p>
      </div>
      <div class="flex items-center gap-3">
        <span v-if="proyectoActual" class="text-sm text-neutral-500">
          {{ items.length }} {{ items.length === 1 ? 'lista' : 'listas' }}
        </span>
        <Button :disabled="proyectoSel === null" @click="showCrear = !showCrear">
          <Plus :size="14" /> {{ showCrear ? 'Cancelar' : 'Nueva lista' }}
        </Button>
      </div>
    </div>

    <!-- Selector de proyecto -->
    <Card class="p-4">
      <Label class="text-xs">Proyecto</Label>
      <Select v-model="proyectoSel as any">
        <option :value="null" disabled>— Seleccionar proyecto —</option>
        <option v-for="p in proyectos" :key="p.id" :value="p.id">
          {{ p.nombre }} <template v-if="p.nombreEquipo">· {{ p.nombreEquipo }}</template>
        </option>
      </Select>
    </Card>

    <!-- Form crear -->
    <Card
      v-if="showCrear && proyectoSel !== null"
      class="p-4 border-blue-200 dark:border-blue-900 bg-blue-50/30 dark:bg-blue-950/20"
    >
      <Label class="text-xs uppercase text-neutral-500 mb-2 block">Crear lista</Label>
      <form class="space-y-3" @submit.prevent="onCrear">
        <div class="grid grid-cols-1 sm:grid-cols-3 gap-3">
          <div class="sm:col-span-2">
            <Label class="text-xs">Nombre</Label>
            <Input v-model="nuevoNombre" placeholder="Ej: Por hacer, En progreso, Hecho" />
          </div>
          <div>
            <Label class="text-xs">Color (opcional)</Label>
            <Input v-model="nuevoColor" placeholder="#3b82f6" :maxlength="20" />
          </div>
        </div>
        <div class="flex justify-end gap-2">
          <Button type="button" variant="outline" @click="showCrear = false">Cancelar</Button>
          <Button type="submit" :disabled="isCreating || !nuevoNombre.trim()">
            <Plus :size="14" /> Crear lista
          </Button>
        </div>
      </form>
    </Card>

    <!-- Lista -->
    <Card class="overflow-hidden">
      <table class="w-full text-sm">
        <thead class="bg-neutral-50 dark:bg-neutral-900/50 border-b border-neutral-200 dark:border-neutral-800">
          <tr class="text-left">
            <th class="px-4 py-2 font-medium text-neutral-600 dark:text-neutral-400 w-16">Orden</th>
            <th class="px-4 py-2 font-medium text-neutral-600 dark:text-neutral-400">Nombre</th>
            <th class="px-4 py-2 font-medium text-neutral-600 dark:text-neutral-400">Color</th>
            <th class="px-4 py-2 font-medium text-neutral-600 dark:text-neutral-400">Creador</th>
            <th class="px-4 py-2 font-medium text-neutral-600 dark:text-neutral-400">Fecha</th>
            <th class="px-4 py-2 font-medium text-neutral-600 dark:text-neutral-400 text-right">Acciones</th>
          </tr>
        </thead>
        <tbody>
          <tr v-if="proyectoSel === null">
            <td colspan="6" class="px-4 py-12 text-center text-neutral-500">
              Selecciona un proyecto para ver sus listas.
            </td>
          </tr>
          <tr v-else-if="isLoading && !items.length">
            <td colspan="6" class="px-4 py-8 text-center text-neutral-500">Cargando…</td>
          </tr>
          <tr v-else-if="!items.length">
            <td colspan="6" class="px-4 py-12 text-center">
              <ListChecks :size="32" class="mx-auto mb-2 text-neutral-300 dark:text-neutral-700" />
              <p class="text-neutral-500">Este proyecto aún no tiene listas.</p>
              <p class="text-xs text-neutral-400 mt-1">Click en "Nueva lista" para crear la primera.</p>
            </td>
          </tr>
          <tr
            v-for="(l, idx) in items"
            :key="l.id"
            class="border-b border-neutral-100 dark:border-neutral-800 last:border-b-0 hover:bg-neutral-50 dark:hover:bg-neutral-900/30"
          >
            <td class="px-4 py-2 text-neutral-500">
              <div class="flex flex-col">
                <button
                  type="button"
                  :disabled="idx === 0 || isReordering"
                  class="leading-none disabled:opacity-30"
                  @click="moverArriba(idx)"
                  title="Mover arriba"
                >
                  <ChevronUp :size="14" />
                </button>
                <button
                  type="button"
                  :disabled="idx === items.length - 1 || isReordering"
                  class="leading-none disabled:opacity-30"
                  @click="moverAbajo(idx)"
                  title="Mover abajo"
                >
                  <ChevronDown :size="14" />
                </button>
              </div>
            </td>
            <template v-if="editingId === l.id">
              <td class="px-4 py-2"><Input v-model="editNombre" /></td>
              <td class="px-4 py-2"><Input v-model="editColor" placeholder="#3b82f6" /></td>
              <td class="px-4 py-2 text-neutral-600 dark:text-neutral-400 text-xs">
                {{ l.nombreCreador ?? '—' }}
              </td>
              <td class="px-4 py-2 text-neutral-500 text-xs">{{ fmt(l.fechaCreacion) }}</td>
              <td class="px-4 py-2 text-right space-x-1">
                <Button variant="ghost" size="sm" :disabled="updatingId === l.id" @click="guardarEdicion(l)">
                  <Check :size="14" />
                </Button>
                <Button variant="ghost" size="sm" @click="cancelarEdicion">
                  <X :size="14" />
                </Button>
              </td>
            </template>
            <template v-else>
              <td class="px-4 py-2 font-medium">{{ l.nombre }}</td>
              <td class="px-4 py-2">
                <span v-if="l.color" class="inline-flex items-center gap-2 text-xs">
                  <span
                    class="inline-block w-4 h-4 rounded border border-neutral-300 dark:border-neutral-700"
                    :style="{ backgroundColor: l.color }"
                  />
                  <span class="text-neutral-500">{{ l.color }}</span>
                </span>
                <span v-else class="text-neutral-400 text-xs">—</span>
              </td>
              <td class="px-4 py-2 text-neutral-600 dark:text-neutral-400 text-xs">
                {{ l.nombreCreador ?? '—' }}
              </td>
              <td class="px-4 py-2 text-neutral-500 text-xs">{{ fmt(l.fechaCreacion) }}</td>
              <td class="px-4 py-2 text-right space-x-1 whitespace-nowrap">
                <Button variant="ghost" size="sm" :disabled="updatingId === l.id" @click="iniciarEdicion(l)" title="Editar">
                  <Pencil :size="14" />
                </Button>
                <Button
                  variant="ghost"
                  size="sm"
                  class="text-red-600 hover:bg-red-50 dark:hover:bg-red-950"
                  :disabled="updatingId === l.id"
                  @click="onEliminar(l)"
                  title="Eliminar"
                >
                  <Trash2 :size="14" />
                </Button>
              </td>
            </template>
          </tr>
        </tbody>
      </table>
    </Card>

    <p v-if="error" class="text-sm text-red-600">{{ error }}</p>
  </div>
</template>
