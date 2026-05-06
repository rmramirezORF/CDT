<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { Plus, Trash2, Pencil, X, Check, Users, ChevronRight, ChevronDown, UserPlus } from 'lucide-vue-next'
import { Button } from '@/components/ui/button'
import { Card } from '@/components/ui/card'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Select } from '@/components/ui/select'
import { extractErrorMessage } from '@/utils/errorMapper'
import equiposService from '../services/equiposService'
import usuariosService from '../services/usuariosService'
import type { EquipoListItem, Miembro, UsuarioListItem } from '../types/admin'

// ----- Estado -----
const equipos = ref<EquipoListItem[]>([])
const usuarios = ref<UsuarioListItem[]>([])
const isLoading = ref(false)
const error = ref<string | null>(null)

// Crear
const nuevoNombre = ref('')
const nuevoIdPadre = ref<number | null>(null)
const nuevoIdLider = ref<number | null>(null)
const isCreating = ref(false)

// Editar
const editingId = ref<number | null>(null)
const editNombre = ref('')
const editIdPadre = ref<number | null>(null)
const editIdLider = ref<number | null>(null)

// Expandir miembros
const expandedId = ref<number | null>(null)
const miembros = ref<Record<number, Miembro[]>>({})
const isLoadingMiembros = ref<number | null>(null)
const nuevoMiembroId = ref<number | null>(null)

// ----- Carga inicial -----
async function loadEquipos() {
  isLoading.value = true
  error.value = null
  try {
    equipos.value = await equiposService.listar()
  } catch (e) {
    error.value = extractErrorMessage(e)
  } finally {
    isLoading.value = false
  }
}

async function loadUsuarios() {
  try {
    const result = await usuariosService.listar({ pageSize: 100, estado: true })
    usuarios.value = result.items
  } catch {
    /* silencioso */
  }
}

loadEquipos()
loadUsuarios()

// ----- Jerarquía: ordenar con DFS y calcular nivel -----
type EquipoConNivel = EquipoListItem & { nivel: number }

const equiposJerarquicos = computed<EquipoConNivel[]>(() => {
  const result: EquipoConNivel[] = []
  const visitados = new Set<number>()

  function dfs(parentId: number | null, nivel: number) {
    const hijos: EquipoListItem[] = equipos.value
      .filter((e) => e.idEquipoPadre === parentId && !visitados.has(e.id))
      .sort((a, b) => a.nombre.localeCompare(b.nombre))
    for (const h of hijos) {
      visitados.add(h.id)
      const item: EquipoConNivel = { ...h, nivel }
      result.push(item)
      dfs(h.id, nivel + 1)
    }
  }

  dfs(null, 0)
  // Por si quedan huérfanos por inconsistencias, agregarlos al final con nivel 0
  for (const e of equipos.value) {
    if (!visitados.has(e.id)) result.push({ ...e, nivel: 0 })
  }
  return result
})

function padresPosibles(idEquipoActual: number): EquipoListItem[] {
  const desc = descendientesIds(idEquipoActual)
  return equipos.value.filter((x) => !desc.has(x.id))
}

// ----- Padres válidos para evitar ciclos -----
function descendientesIds(idEquipo: number): Set<number> {
  const res = new Set<number>()
  const cola = [idEquipo]
  while (cola.length) {
    const actual = cola.shift()!
    res.add(actual)
    for (const e of equipos.value) {
      if (e.idEquipoPadre === actual && !res.has(e.id)) cola.push(e.id)
    }
  }
  return res
}

// ----- Crear -----
async function onCrear() {
  if (!nuevoNombre.value.trim()) return
  isCreating.value = true
  try {
    const created = await equiposService.crear({
      nombre: nuevoNombre.value.trim(),
      idEquipoPadre: nuevoIdPadre.value,
      idLider: nuevoIdLider.value,
    })
    equipos.value.push(created)
    nuevoNombre.value = ''
    nuevoIdPadre.value = null
    nuevoIdLider.value = null
  } catch (e) {
    alert(extractErrorMessage(e))
  } finally {
    isCreating.value = false
  }
}

// ----- Editar -----
function iniciarEdicion(equipo: EquipoListItem) {
  editingId.value = equipo.id
  editNombre.value = equipo.nombre
  editIdPadre.value = equipo.idEquipoPadre
  editIdLider.value = equipo.idLider
}

function cancelarEdicion() {
  editingId.value = null
}

async function guardarEdicion(equipo: EquipoListItem) {
  try {
    const updated = await equiposService.actualizar(equipo.id, {
      nombre: editNombre.value.trim(),
      idEquipoPadre: editIdPadre.value,
      idLider: editIdLider.value,
    })
    const idx = equipos.value.findIndex((e) => e.id === equipo.id)
    if (idx >= 0) equipos.value[idx] = updated
    cancelarEdicion()
  } catch (e) {
    alert(extractErrorMessage(e))
  }
}

// ----- Eliminar -----
async function onEliminar(equipo: EquipoListItem) {
  if (!confirm(`¿Eliminar el equipo "${equipo.nombre}"?`)) return
  try {
    await equiposService.eliminar(equipo.id)
    equipos.value = equipos.value.filter((e) => e.id !== equipo.id)
    if (expandedId.value === equipo.id) expandedId.value = null
  } catch (e) {
    alert(extractErrorMessage(e))
  }
}

// ----- Expandir miembros -----
async function toggleMiembros(equipo: EquipoListItem) {
  if (expandedId.value === equipo.id) {
    expandedId.value = null
    return
  }
  expandedId.value = equipo.id
  if (!miembros.value[equipo.id]) {
    isLoadingMiembros.value = equipo.id
    try {
      miembros.value[equipo.id] = await equiposService.listarMiembros(equipo.id)
    } catch (e) {
      alert(extractErrorMessage(e))
    } finally {
      isLoadingMiembros.value = null
    }
  }
}

async function onAgregarMiembro(equipo: EquipoListItem) {
  if (!nuevoMiembroId.value) return
  try {
    const m = await equiposService.agregarMiembro(equipo.id, nuevoMiembroId.value)
    miembros.value[equipo.id] = [...(miembros.value[equipo.id] ?? []), m]
    equipo.totalMiembros++
    nuevoMiembroId.value = null
  } catch (e) {
    alert(extractErrorMessage(e))
  }
}

async function onQuitarMiembro(equipo: EquipoListItem, m: Miembro) {
  if (!confirm(`¿Quitar a "${m.nombre}" del equipo "${equipo.nombre}"?`)) return
  try {
    await equiposService.quitarMiembro(equipo.id, m.id)
    miembros.value[equipo.id] = (miembros.value[equipo.id] ?? []).filter((x) => x.id !== m.id)
    equipo.totalMiembros = Math.max(0, equipo.totalMiembros - 1)
  } catch (e) {
    alert(extractErrorMessage(e))
  }
}

// Helpers para usuarios disponibles para agregar (excluye los que ya son miembros)
function usuariosDisponibles(idEquipo: number): UsuarioListItem[] {
  const yaSon = new Set((miembros.value[idEquipo] ?? []).map((m) => m.id))
  return usuarios.value.filter((u) => !yaSon.has(u.id))
}

// Watch para cancelar edición cuando se cambia de equipo expandido
watch(expandedId, () => {
  cancelarEdicion()
})
</script>

<template>
  <div class="space-y-4">
    <div class="flex items-center justify-between">
      <div>
        <h1 class="text-2xl font-semibold">Equipos</h1>
        <p class="text-sm text-neutral-500">Estructura jerárquica de equipos. Soporta sub-equipos en cascada.</p>
      </div>
      <div class="text-sm text-neutral-500">{{ equipos.length }} {{ equipos.length === 1 ? 'equipo' : 'equipos' }}</div>
    </div>

    <!-- Crear -->
    <Card class="p-4">
      <Label class="text-xs uppercase text-neutral-500 mb-2 block">Crear equipo</Label>
      <form class="grid grid-cols-1 sm:grid-cols-4 gap-2 items-end" @submit.prevent="onCrear">
        <div class="sm:col-span-1">
          <Label class="text-xs">Nombre</Label>
          <Input v-model="nuevoNombre" placeholder="Ej: Desarrollo" />
        </div>
        <div>
          <Label class="text-xs">Equipo padre (opcional)</Label>
          <Select v-model="nuevoIdPadre as any">
            <option :value="null">— Raíz (sin padre) —</option>
            <option v-for="e in equipos" :key="e.id" :value="e.id">{{ e.nombre }}</option>
          </Select>
        </div>
        <div>
          <Label class="text-xs">Líder (opcional)</Label>
          <Select v-model="nuevoIdLider as any">
            <option :value="null">— Sin asignar —</option>
            <option v-for="u in usuarios" :key="u.id" :value="u.id">{{ u.nombre }} ({{ u.correo }})</option>
          </Select>
        </div>
        <Button type="submit" :disabled="isCreating || !nuevoNombre.trim()">
          <Plus :size="14" /> Crear
        </Button>
      </form>
    </Card>

    <!-- Tabla -->
    <Card class="overflow-hidden">
      <div class="overflow-x-auto">
        <table class="w-full text-sm">
          <thead class="bg-neutral-50 dark:bg-neutral-900/50 border-b border-neutral-200 dark:border-neutral-800">
            <tr class="text-left">
              <th class="px-4 py-2 font-medium text-neutral-600 dark:text-neutral-400 w-8"></th>
              <th class="px-4 py-2 font-medium text-neutral-600 dark:text-neutral-400">Nombre</th>
              <th class="px-4 py-2 font-medium text-neutral-600 dark:text-neutral-400">Líder</th>
              <th class="px-4 py-2 font-medium text-neutral-600 dark:text-neutral-400">Miembros</th>
              <th class="px-4 py-2 font-medium text-neutral-600 dark:text-neutral-400 text-right">Acciones</th>
            </tr>
          </thead>
          <tbody>
            <tr v-if="isLoading && !equipos.length">
              <td colspan="5" class="px-4 py-8 text-center text-neutral-500">Cargando…</td>
            </tr>
            <tr v-else-if="!equipos.length">
              <td colspan="5" class="px-4 py-8 text-center text-neutral-500">
                No hay equipos todavía. Creá el primero arriba.
              </td>
            </tr>
            <template v-for="equipo in equiposJerarquicos" :key="equipo.id">
              <!-- Fila principal (modo lectura o edición) -->
              <tr
                class="border-b border-neutral-100 dark:border-neutral-800 hover:bg-neutral-50 dark:hover:bg-neutral-900/30"
                :class="{ 'bg-blue-50/40 dark:bg-blue-950/20': expandedId === equipo.id }"
              >
                <template v-if="editingId === equipo.id">
                  <td class="px-4 py-2"></td>
                  <td class="px-4 py-2"><Input v-model="editNombre" /></td>
                  <td class="px-4 py-2 col-span-2" colspan="2">
                    <div class="grid grid-cols-2 gap-2">
                      <Select v-model="editIdPadre as any">
                        <option :value="null">— Raíz —</option>
                        <option
                          v-for="e in padresPosibles(equipo.id)"
                          :key="e.id"
                          :value="e.id"
                        >{{ e.nombre }}</option>
                      </Select>
                      <Select v-model="editIdLider as any">
                        <option :value="null">— Sin líder —</option>
                        <option v-for="u in usuarios" :key="u.id" :value="u.id">{{ u.nombre }}</option>
                      </Select>
                    </div>
                  </td>
                  <td class="px-4 py-2 text-right space-x-1">
                    <Button variant="ghost" size="sm" @click="guardarEdicion(equipo)"><Check :size="14" /></Button>
                    <Button variant="ghost" size="sm" @click="cancelarEdicion"><X :size="14" /></Button>
                  </td>
                </template>
                <template v-else>
                  <td class="px-4 py-2">
                    <button
                      class="p-1 rounded hover:bg-neutral-200 dark:hover:bg-neutral-700 transition"
                      @click="toggleMiembros(equipo)"
                      :title="expandedId === equipo.id ? 'Cerrar' : 'Ver miembros'"
                    >
                      <component :is="expandedId === equipo.id ? ChevronDown : ChevronRight" :size="14" />
                    </button>
                  </td>
                  <td class="px-4 py-2 font-medium">
                    <span :style="{ paddingLeft: `${equipo.nivel * 20}px` }" class="inline-flex items-center gap-2">
                      <span v-if="equipo.nivel > 0" class="text-neutral-400">└</span>
                      {{ equipo.nombre }}
                    </span>
                  </td>
                  <td class="px-4 py-2 text-neutral-600 dark:text-neutral-400">
                    <span v-if="equipo.nombreLider">
                      {{ equipo.nombreLider }}
                      <span class="text-xs text-neutral-400">{{ equipo.correoLider }}</span>
                    </span>
                    <span v-else class="text-neutral-400 italic text-xs">Sin asignar</span>
                  </td>
                  <td class="px-4 py-2">
                    <span class="inline-flex items-center gap-1 text-xs">
                      <Users :size="12" class="text-neutral-400" /> {{ equipo.totalMiembros }}
                    </span>
                  </td>
                  <td class="px-4 py-2 text-right space-x-1 whitespace-nowrap">
                    <Button variant="ghost" size="sm" @click="iniciarEdicion(equipo)" title="Editar">
                      <Pencil :size="14" />
                    </Button>
                    <Button
                      variant="ghost"
                      size="sm"
                      class="text-red-600 hover:bg-red-50 dark:hover:bg-red-950"
                      @click="onEliminar(equipo)"
                      title="Eliminar"
                    >
                      <Trash2 :size="14" />
                    </Button>
                  </td>
                </template>
              </tr>

              <!-- Panel de miembros expandido -->
              <tr v-if="expandedId === equipo.id" class="bg-neutral-50/50 dark:bg-neutral-900/30 border-b border-neutral-100 dark:border-neutral-800">
                <td colspan="5" class="px-4 py-3">
                  <div class="ml-8 space-y-3">
                    <div class="flex items-center gap-2 text-sm font-medium">
                      <Users :size="14" class="text-neutral-500" />
                      <span>Miembros de "{{ equipo.nombre }}"</span>
                    </div>

                    <div v-if="isLoadingMiembros === equipo.id" class="text-sm text-neutral-500">Cargando miembros…</div>

                    <div v-else>
                      <div v-if="!(miembros[equipo.id]?.length)" class="text-sm text-neutral-500 italic">
                        Aún no hay miembros en este equipo.
                      </div>
                      <ul v-else class="space-y-1">
                        <li
                          v-for="m in miembros[equipo.id]"
                          :key="m.id"
                          class="flex items-center justify-between bg-white dark:bg-neutral-900 px-3 py-2 rounded border border-neutral-200 dark:border-neutral-800"
                        >
                          <div class="flex items-center gap-2 text-sm">
                            <div class="w-7 h-7 rounded-full bg-neutral-200 dark:bg-neutral-800 flex items-center justify-center text-[10px] font-semibold">
                              {{ m.nombre.split(' ').map(p => p[0]).slice(0,2).join('') }}
                            </div>
                            <div>
                              <div class="font-medium">{{ m.nombre }}</div>
                              <div class="text-xs text-neutral-500">{{ m.correo }} · {{ m.rolGlobal }}</div>
                            </div>
                          </div>
                          <Button variant="ghost" size="sm" class="text-red-600 hover:bg-red-50 dark:hover:bg-red-950" @click="onQuitarMiembro(equipo, m)">
                            <X :size="14" /> Quitar
                          </Button>
                        </li>
                      </ul>

                      <!-- Agregar miembro -->
                      <form class="flex items-end gap-2 mt-3" @submit.prevent="onAgregarMiembro(equipo)">
                        <div class="flex-1">
                          <Label class="text-xs">Agregar usuario</Label>
                          <Select v-model="nuevoMiembroId as any">
                            <option :value="null">— Seleccionar —</option>
                            <option
                              v-for="u in usuariosDisponibles(equipo.id)"
                              :key="u.id"
                              :value="u.id"
                            >{{ u.nombre }} ({{ u.correo }})</option>
                          </Select>
                        </div>
                        <Button type="submit" :disabled="!nuevoMiembroId">
                          <UserPlus :size="14" /> Agregar
                        </Button>
                      </form>
                    </div>
                  </div>
                </td>
              </tr>
            </template>
          </tbody>
        </table>
      </div>
    </Card>

    <p v-if="error" class="text-sm text-red-600">{{ error }}</p>
  </div>
</template>
