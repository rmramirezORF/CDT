<script setup lang="ts">
import { ref, watch } from 'vue'
import { Search, Power, PowerOff, Trash2, MailCheck } from 'lucide-vue-next'
import { Button } from '@/components/ui/button'
import { Card } from '@/components/ui/card'
import { Input } from '@/components/ui/input'
import { Select } from '@/components/ui/select'
import { extractErrorMessage } from '@/utils/errorMapper'
import { useAuthStore } from '@/modules/auth'
import usuariosService from '../services/usuariosService'
import type { RolGlobal, UsuarioListItem } from '../types/admin'

const auth = useAuthStore()

const items = ref<UsuarioListItem[]>([])
const isLoading = ref(false)
const error = ref<string | null>(null)

const search = ref('')
const filtroRol = ref<RolGlobal | ''>('')
const filtroEstado = ref<'all' | 'activo' | 'inactivo'>('all')

const total = ref(0)
const page = ref(1)
const pageSize = ref(20)

const updatingId = ref<number | null>(null)

async function load() {
  isLoading.value = true
  error.value = null
  try {
    const estado = filtroEstado.value === 'activo' ? true : filtroEstado.value === 'inactivo' ? false : null
    const result = await usuariosService.listar({
      q: search.value.trim() || undefined,
      rol: filtroRol.value || undefined,
      estado,
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

let timeoutId: number | undefined
watch(search, () => {
  if (timeoutId) clearTimeout(timeoutId)
  timeoutId = window.setTimeout(() => {
    page.value = 1
    load()
  }, 300)
})

watch([filtroRol, filtroEstado], () => {
  page.value = 1
  load()
})

watch(page, load)

async function onChangeRol(u: UsuarioListItem, nuevoRol: string) {
  updatingId.value = u.id
  try {
    const updated = await usuariosService.cambiarRol(u.id, nuevoRol)
    Object.assign(u, updated)
  } catch (e) {
    alert(extractErrorMessage(e))
  } finally {
    updatingId.value = null
  }
}

async function onToggleEstado(u: UsuarioListItem) {
  updatingId.value = u.id
  try {
    const updated = await usuariosService.cambiarEstado(u.id, !u.estado)
    Object.assign(u, updated)
  } catch (e) {
    alert(extractErrorMessage(e))
  } finally {
    updatingId.value = null
  }
}

async function onConfirmarEmail(u: UsuarioListItem) {
  updatingId.value = u.id
  try {
    const updated = await usuariosService.confirmarEmail(u.id)
    Object.assign(u, updated)
  } catch (e) {
    alert(extractErrorMessage(e))
  } finally {
    updatingId.value = null
  }
}

async function onEliminar(u: UsuarioListItem) {
  const confirmacion = window.confirm(
    `¿Eliminar permanentemente a "${u.nombre}" (${u.correo})?\n\n` +
    `Esto borra también sus refresh tokens y códigos de reset.\n` +
    `Esta acción NO se puede deshacer.`,
  )
  if (!confirmacion) return

  updatingId.value = u.id
  try {
    await usuariosService.eliminar(u.id)
    items.value = items.value.filter((x) => x.id !== u.id)
    total.value = Math.max(0, total.value - 1)
  } catch (e) {
    alert(extractErrorMessage(e))
  } finally {
    updatingId.value = null
  }
}

function isSelf(u: UsuarioListItem): boolean {
  return auth.usuario?.id === u.id
}

load()
</script>

<template>
  <div class="space-y-4">
    <div class="flex items-center justify-between">
      <div>
        <h1 class="text-2xl font-semibold">Usuarios</h1>
        <p class="text-sm text-neutral-500">Gestiona los miembros de la organización, sus roles y estado.</p>
      </div>
      <div class="text-sm text-neutral-500">{{ total }} {{ total === 1 ? 'usuario' : 'usuarios' }}</div>
    </div>

    <Card class="p-4">
      <div class="grid grid-cols-1 sm:grid-cols-3 gap-3">
        <div class="sm:col-span-1 relative">
          <Search :size="14" class="absolute left-3 top-1/2 -translate-y-1/2 text-neutral-400" />
          <Input v-model="search" placeholder="Buscar por nombre o correo…" class="pl-9" />
        </div>
        <Select v-model="filtroRol">
          <option value="">Todos los roles</option>
          <option value="Admin">Admin</option>
          <option value="Lider">Líder</option>
          <option value="Miembro">Miembro</option>
        </Select>
        <Select v-model="filtroEstado">
          <option value="all">Activos e inactivos</option>
          <option value="activo">Solo activos</option>
          <option value="inactivo">Solo inactivos</option>
        </Select>
      </div>
    </Card>

    <Card class="overflow-hidden">
      <div class="overflow-x-auto">
        <table class="w-full text-sm">
          <thead class="bg-neutral-50 dark:bg-neutral-900/50 border-b border-neutral-200 dark:border-neutral-800">
            <tr class="text-left">
              <th class="px-4 py-2 font-medium text-neutral-600 dark:text-neutral-400">Nombre</th>
              <th class="px-4 py-2 font-medium text-neutral-600 dark:text-neutral-400">Correo</th>
              <th class="px-4 py-2 font-medium text-neutral-600 dark:text-neutral-400">Rol</th>
              <th class="px-4 py-2 font-medium text-neutral-600 dark:text-neutral-400">Estado</th>
              <th class="px-4 py-2 font-medium text-neutral-600 dark:text-neutral-400">Email</th>
              <th class="px-4 py-2 font-medium text-neutral-600 dark:text-neutral-400 text-right">Acciones</th>
            </tr>
          </thead>
          <tbody>
            <tr v-if="isLoading && !items.length">
              <td colspan="6" class="px-4 py-8 text-center text-neutral-500">Cargando…</td>
            </tr>
            <tr v-else-if="!items.length">
              <td colspan="6" class="px-4 py-8 text-center text-neutral-500">Sin usuarios para los filtros aplicados.</td>
            </tr>
            <tr
              v-for="u in items"
              :key="u.id"
              class="border-b border-neutral-100 dark:border-neutral-800 last:border-b-0 hover:bg-neutral-50 dark:hover:bg-neutral-900/30"
            >
              <td class="px-4 py-2 font-medium">
                {{ u.nombre }}
                <span v-if="isSelf(u)" class="ml-1 text-[10px] uppercase font-medium px-1.5 py-0.5 rounded bg-blue-100 text-blue-700 dark:bg-blue-900/30 dark:text-blue-400">Tú</span>
              </td>
              <td class="px-4 py-2 text-neutral-600 dark:text-neutral-400">{{ u.correo }}</td>
              <td class="px-4 py-2">
                <Select
                  :model-value="u.rolGlobal"
                  :disabled="updatingId === u.id || isSelf(u)"
                  class="w-32"
                  @change="(v: string) => onChangeRol(u, v)"
                >
                  <option value="Admin">Admin</option>
                  <option value="Lider">Líder</option>
                  <option value="Miembro">Miembro</option>
                </Select>
              </td>
              <td class="px-4 py-2">
                <span
                  :class="[
                    'inline-block text-[10px] uppercase font-medium px-2 py-0.5 rounded',
                    u.estado
                      ? 'bg-green-100 text-green-700 dark:bg-green-900/30 dark:text-green-400'
                      : 'bg-red-100 text-red-700 dark:bg-red-900/30 dark:text-red-400',
                  ]"
                >
                  {{ u.estado ? 'Activo' : 'Inactivo' }}
                </span>
              </td>
              <td class="px-4 py-2">
                <span v-if="u.fechaConfirmacionEmail" class="inline-flex items-center gap-1 text-xs text-green-600 dark:text-green-400">
                  <MailCheck :size="14" /> Confirmado
                </span>
                <Button
                  v-else
                  variant="outline"
                  size="sm"
                  :disabled="updatingId === u.id"
                  @click="onConfirmarEmail(u)"
                >
                  <MailCheck :size="14" /> Confirmar
                </Button>
              </td>
              <td class="px-4 py-2 text-right space-x-1 whitespace-nowrap">
                <Button
                  variant="ghost"
                  size="sm"
                  :disabled="updatingId === u.id || isSelf(u)"
                  :title="isSelf(u) ? 'No puedes desactivar tu propia cuenta' : ''"
                  @click="onToggleEstado(u)"
                >
                  <component :is="u.estado ? PowerOff : Power" :size="14" />
                  {{ u.estado ? 'Desactivar' : 'Activar' }}
                </Button>
                <Button
                  variant="ghost"
                  size="sm"
                  class="text-red-600 hover:bg-red-50 hover:text-red-700 dark:text-red-400 dark:hover:bg-red-950"
                  :disabled="updatingId === u.id || isSelf(u)"
                  :title="isSelf(u) ? 'No puedes eliminar tu propia cuenta' : ''"
                  @click="onEliminar(u)"
                >
                  <Trash2 :size="14" /> Eliminar
                </Button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <div v-if="total > pageSize" class="flex items-center justify-between p-3 border-t border-neutral-200 dark:border-neutral-800 text-sm">
        <span class="text-neutral-500">
          Página {{ page }} de {{ Math.ceil(total / pageSize) }}
        </span>
        <div class="flex gap-2">
          <Button variant="outline" size="sm" :disabled="page <= 1" @click="page--">Anterior</Button>
          <Button variant="outline" size="sm" :disabled="page >= Math.ceil(total / pageSize)" @click="page++">Siguiente</Button>
        </div>
      </div>
    </Card>

    <p v-if="error" class="text-sm text-red-600">{{ error }}</p>
  </div>
</template>
