<script setup lang="ts">
import { ref, watch } from 'vue'
import { Plus, Trash2, Pencil, X, Check, AtSign } from 'lucide-vue-next'
import { Button } from '@/components/ui/button'
import { Card } from '@/components/ui/card'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { extractErrorMessage } from '@/utils/errorMapper'
import catalogosService from '../services/catalogosService'
import type { CatalogoItem, CatalogoTipo, DominioPermitido } from '../types/admin'

type TabId = CatalogoTipo | 'dominios'

const tabs: { id: TabId; label: string }[] = [
  { id: 'estados', label: 'Estados' },
  { id: 'prioridades', label: 'Prioridades' },
  { id: 'etiquetas', label: 'Etiquetas' },
  { id: 'dominios', label: 'Dominios permitidos' },
]

const tipoActivo = ref<TabId>('estados')

// Catálogos visuales (estados / prioridades / etiquetas)
const items = ref<CatalogoItem[]>([])
const isLoading = ref(false)
const error = ref<string | null>(null)

const nuevoNombre = ref('')
const nuevoColor = ref('#3b82f6')
const nuevoOrden = ref(0)
const isCreating = ref(false)

const editingId = ref<number | null>(null)
const editNombre = ref('')
const editColor = ref('')
const editOrden = ref(0)

// Dominios
const dominios = ref<DominioPermitido[]>([])
const isLoadingDominios = ref(false)
const errorDominios = ref<string | null>(null)
const nuevoDominio = ref('')
const isCreatingDominio = ref(false)

async function loadCatalogo() {
  isLoading.value = true
  error.value = null
  try {
    items.value = await catalogosService.listar(tipoActivo.value as CatalogoTipo)
  } catch (e) {
    error.value = extractErrorMessage(e)
  } finally {
    isLoading.value = false
  }
}

async function loadDominios() {
  isLoadingDominios.value = true
  errorDominios.value = null
  try {
    dominios.value = await catalogosService.listarDominios()
  } catch (e) {
    errorDominios.value = extractErrorMessage(e)
  } finally {
    isLoadingDominios.value = false
  }
}

watch(tipoActivo, (nuevo) => {
  cancelarEdicion()
  if (nuevo === 'dominios') {
    loadDominios()
  } else {
    loadCatalogo()
  }
})

// ----- Catálogos: crear / editar / eliminar -----

async function onCrear() {
  if (!nuevoNombre.value.trim()) return
  isCreating.value = true
  try {
    const created = await catalogosService.crear(tipoActivo.value as CatalogoTipo, {
      nombre: nuevoNombre.value.trim(),
      color: nuevoColor.value,
      orden: nuevoOrden.value || items.value.length + 1,
    })
    items.value.push(created)
    items.value.sort((a, b) => a.orden - b.orden || a.nombre.localeCompare(b.nombre))
    nuevoNombre.value = ''
    nuevoColor.value = '#3b82f6'
    nuevoOrden.value = 0
  } catch (e) {
    alert(extractErrorMessage(e))
  } finally {
    isCreating.value = false
  }
}

function iniciarEdicion(item: CatalogoItem) {
  editingId.value = item.id
  editNombre.value = item.nombre
  editColor.value = item.color
  editOrden.value = item.orden
}

function cancelarEdicion() {
  editingId.value = null
}

async function guardarEdicion(item: CatalogoItem) {
  try {
    const updated = await catalogosService.actualizar(tipoActivo.value as CatalogoTipo, item.id, {
      nombre: editNombre.value.trim(),
      color: editColor.value,
      orden: editOrden.value,
    })
    Object.assign(item, updated)
    cancelarEdicion()
    items.value.sort((a, b) => a.orden - b.orden || a.nombre.localeCompare(b.nombre))
  } catch (e) {
    alert(extractErrorMessage(e))
  }
}

async function eliminar(item: CatalogoItem) {
  if (!confirm(`¿Eliminar "${item.nombre}"?`)) return
  try {
    await catalogosService.eliminar(tipoActivo.value as CatalogoTipo, item.id)
    items.value = items.value.filter((i) => i.id !== item.id)
  } catch (e) {
    alert(extractErrorMessage(e))
  }
}

// ----- Dominios: crear / eliminar -----

async function onCrearDominio() {
  const valor = nuevoDominio.value.trim().toLowerCase().replace(/^@/, '')
  if (!valor) return
  isCreatingDominio.value = true
  try {
    const created = await catalogosService.crearDominio(valor)
    dominios.value.push(created)
    dominios.value.sort((a, b) => a.dominio.localeCompare(b.dominio))
    nuevoDominio.value = ''
  } catch (e) {
    alert(extractErrorMessage(e))
  } finally {
    isCreatingDominio.value = false
  }
}

async function eliminarDominio(item: DominioPermitido) {
  if (!confirm(
    `¿Eliminar el dominio "${item.dominio}"?\n\n` +
    `Después de esto, NADIE con un correo @${item.dominio} podrá registrarse.\n` +
    `Los usuarios ya registrados con ese dominio NO se ven afectados.`,
  )) return

  try {
    await catalogosService.eliminarDominio(item.id)
    dominios.value = dominios.value.filter((d) => d.id !== item.id)
  } catch (e) {
    alert(extractErrorMessage(e))
  }
}

loadCatalogo()
</script>

<template>
  <div class="space-y-4">
    <div>
      <h1 class="text-2xl font-semibold">Catálogos</h1>
      <p class="text-sm text-neutral-500">Configura los valores parametrizables del sistema.</p>
    </div>

    <!-- Tabs -->
    <div class="flex gap-2 border-b border-neutral-200 dark:border-neutral-800 overflow-x-auto">
      <button
        v-for="t in tabs"
        :key="t.id"
        :class="[
          'px-4 py-2 text-sm border-b-2 -mb-px transition-colors whitespace-nowrap',
          tipoActivo === t.id
            ? 'border-neutral-900 dark:border-neutral-50 font-medium'
            : 'border-transparent text-neutral-500 hover:text-neutral-900 dark:hover:text-neutral-50',
        ]"
        @click="tipoActivo = t.id"
      >
        {{ t.label }}
      </button>
    </div>

    <!-- =====  CATALOGOS VISUALES  ===== -->
    <template v-if="tipoActivo !== 'dominios'">
      <Card class="p-4">
        <Label class="text-xs uppercase text-neutral-500 mb-2 block">Agregar nuevo</Label>
        <form class="flex flex-wrap items-end gap-2" @submit.prevent="onCrear">
          <div class="flex-1 min-w-[180px]">
            <Label class="text-xs">Nombre</Label>
            <Input v-model="nuevoNombre" placeholder="Ej: En curso" />
          </div>
          <div class="w-24">
            <Label class="text-xs">Color</Label>
            <input
              type="color"
              v-model="nuevoColor"
              class="h-10 w-full rounded border border-neutral-300 dark:border-neutral-700 cursor-pointer"
            />
          </div>
          <div class="w-20">
            <Label class="text-xs">Orden</Label>
            <Input v-model.number="nuevoOrden" type="number" min="0" />
          </div>
          <Button type="submit" :disabled="isCreating || !nuevoNombre.trim()">
            <Plus :size="14" /> Agregar
          </Button>
        </form>
      </Card>

      <Card class="overflow-hidden">
        <table class="w-full text-sm">
          <thead class="bg-neutral-50 dark:bg-neutral-900/50 border-b border-neutral-200 dark:border-neutral-800">
            <tr class="text-left">
              <th class="px-4 py-2 font-medium text-neutral-600 dark:text-neutral-400 w-16">Orden</th>
              <th class="px-4 py-2 font-medium text-neutral-600 dark:text-neutral-400">Nombre</th>
              <th class="px-4 py-2 font-medium text-neutral-600 dark:text-neutral-400 w-32">Color</th>
              <th class="px-4 py-2 font-medium text-neutral-600 dark:text-neutral-400 text-right">Acciones</th>
            </tr>
          </thead>
          <tbody>
            <tr v-if="isLoading && !items.length">
              <td colspan="4" class="px-4 py-8 text-center text-neutral-500">Cargando…</td>
            </tr>
            <tr v-else-if="!items.length">
              <td colspan="4" class="px-4 py-8 text-center text-neutral-500">Sin elementos.</td>
            </tr>
            <tr
              v-for="item in items"
              :key="item.id"
              class="border-b border-neutral-100 dark:border-neutral-800 last:border-b-0 hover:bg-neutral-50 dark:hover:bg-neutral-900/30"
            >
              <template v-if="editingId === item.id">
                <td class="px-4 py-2"><Input v-model.number="editOrden" type="number" class="w-16" /></td>
                <td class="px-4 py-2"><Input v-model="editNombre" /></td>
                <td class="px-4 py-2">
                  <input type="color" v-model="editColor" class="h-9 w-full rounded border border-neutral-300 dark:border-neutral-700 cursor-pointer" />
                </td>
                <td class="px-4 py-2 text-right space-x-1">
                  <Button variant="ghost" size="sm" @click="guardarEdicion(item)"><Check :size="14" /></Button>
                  <Button variant="ghost" size="sm" @click="cancelarEdicion"><X :size="14" /></Button>
                </td>
              </template>
              <template v-else>
                <td class="px-4 py-2 text-neutral-600 dark:text-neutral-400">{{ item.orden }}</td>
                <td class="px-4 py-2 font-medium">{{ item.nombre }}</td>
                <td class="px-4 py-2">
                  <span class="inline-flex items-center gap-2">
                    <span class="w-4 h-4 rounded" :style="{ backgroundColor: item.color }"></span>
                    <span class="text-xs text-neutral-500 font-mono">{{ item.color }}</span>
                  </span>
                </td>
                <td class="px-4 py-2 text-right space-x-1">
                  <Button variant="ghost" size="sm" @click="iniciarEdicion(item)"><Pencil :size="14" /></Button>
                  <Button variant="ghost" size="sm" @click="eliminar(item)"><Trash2 :size="14" /></Button>
                </td>
              </template>
            </tr>
          </tbody>
        </table>
      </Card>
      <p v-if="error" class="text-sm text-red-600">{{ error }}</p>
    </template>

    <!-- =====  DOMINIOS PERMITIDOS  ===== -->
    <template v-else>
      <Card class="p-4 bg-blue-50 dark:bg-blue-950/30 border-blue-200 dark:border-blue-900">
        <p class="text-sm text-blue-900 dark:text-blue-200">
          <strong>Lista blanca de dominios.</strong> Solo los correos cuyos dominios estén en esta lista podrán registrarse.
          Si eliminás un dominio, los usuarios ya registrados con ese dominio <strong>no</strong> se ven afectados — pero nadie nuevo podrá usarlo.
        </p>
      </Card>

      <Card class="p-4">
        <Label class="text-xs uppercase text-neutral-500 mb-2 block">Agregar dominio</Label>
        <form class="flex flex-wrap items-end gap-2" @submit.prevent="onCrearDominio">
          <div class="flex-1 min-w-[200px] relative">
            <AtSign :size="14" class="absolute left-3 top-1/2 -translate-y-1/2 text-neutral-400" />
            <Input v-model="nuevoDominio" placeholder="empresa.com" class="pl-9" />
          </div>
          <Button type="submit" :disabled="isCreatingDominio || !nuevoDominio.trim()">
            <Plus :size="14" /> Agregar
          </Button>
        </form>
        <p class="text-xs text-neutral-500 mt-2">
          Sin la arroba ni "http://". Ejemplos: <code>orf.com.co</code>, <code>tdh.com.co</code>, <code>cliente.gov</code>.
        </p>
      </Card>

      <Card class="overflow-hidden">
        <table class="w-full text-sm">
          <thead class="bg-neutral-50 dark:bg-neutral-900/50 border-b border-neutral-200 dark:border-neutral-800">
            <tr class="text-left">
              <th class="px-4 py-2 font-medium text-neutral-600 dark:text-neutral-400">Dominio</th>
              <th class="px-4 py-2 font-medium text-neutral-600 dark:text-neutral-400 text-right">Acciones</th>
            </tr>
          </thead>
          <tbody>
            <tr v-if="isLoadingDominios && !dominios.length">
              <td colspan="2" class="px-4 py-8 text-center text-neutral-500">Cargando…</td>
            </tr>
            <tr v-else-if="!dominios.length">
              <td colspan="2" class="px-4 py-8 text-center text-neutral-500">No hay dominios configurados — nadie podrá registrarse.</td>
            </tr>
            <tr
              v-for="d in dominios"
              :key="d.id"
              class="border-b border-neutral-100 dark:border-neutral-800 last:border-b-0 hover:bg-neutral-50 dark:hover:bg-neutral-900/30"
            >
              <td class="px-4 py-2 font-mono">@{{ d.dominio }}</td>
              <td class="px-4 py-2 text-right">
                <Button variant="ghost" size="sm" class="text-red-600 hover:bg-red-50 dark:hover:bg-red-950" @click="eliminarDominio(d)">
                  <Trash2 :size="14" /> Eliminar
                </Button>
              </td>
            </tr>
          </tbody>
        </table>
      </Card>
      <p v-if="errorDominios" class="text-sm text-red-600">{{ errorDominios }}</p>
    </template>
  </div>
</template>
