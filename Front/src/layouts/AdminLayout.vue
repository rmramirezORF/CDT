<script setup lang="ts">
import { computed } from 'vue'
import { useRouter } from 'vue-router'
import { Users, Layers, FolderKanban, ListChecks, Columns3, LayoutDashboard, LogOut } from 'lucide-vue-next'
import { Button } from '@/components/ui/button'
import { useAuth, useAuthStore } from '@/modules/auth'

const router = useRouter()
const auth = useAuthStore()
const { isLoading, logout } = useAuth()

const initials = computed(() => {
  const name = auth.usuario?.nombre || ''
  return name.split(' ').map((p) => p[0]).slice(0, 2).join('').toUpperCase() || '?'
})

async function handleLogout() {
  await logout()
  router.replace({ name: 'login' })
}
</script>

<template>
  <div class="flex min-h-screen bg-neutral-50 dark:bg-neutral-950 text-neutral-900 dark:text-neutral-50">
    <!-- Sidebar -->
    <aside class="w-60 border-r border-neutral-200 dark:border-neutral-800 bg-white dark:bg-neutral-900 flex flex-col">
      <div class="px-4 py-4 border-b border-neutral-200 dark:border-neutral-800">
        <h1 class="text-lg font-semibold leading-none">CDT</h1>
        <p class="text-xs text-neutral-500 mt-1">Control de Tareas — ORF</p>
      </div>

      <nav class="p-2 space-y-0.5 flex-1">
        <RouterLink
          to="/admin/usuarios"
          active-class="bg-neutral-100 dark:bg-neutral-800 font-medium"
          class="flex items-center gap-2 px-3 py-2 rounded text-sm hover:bg-neutral-100 dark:hover:bg-neutral-800"
        >
          <Users :size="16" /> Usuarios
        </RouterLink>
        <RouterLink
          to="/admin/catalogos"
          active-class="bg-neutral-100 dark:bg-neutral-800 font-medium"
          class="flex items-center gap-2 px-3 py-2 rounded text-sm hover:bg-neutral-100 dark:hover:bg-neutral-800"
        >
          <Layers :size="16" /> Catálogos
        </RouterLink>
        <RouterLink
          to="/admin/equipos"
          active-class="bg-neutral-100 dark:bg-neutral-800 font-medium"
          class="flex items-center gap-2 px-3 py-2 rounded text-sm hover:bg-neutral-100 dark:hover:bg-neutral-800"
        >
          <FolderKanban :size="16" /> Equipos
        </RouterLink>
        <RouterLink
          to="/admin/proyectos"
          active-class="bg-neutral-100 dark:bg-neutral-800 font-medium"
          class="flex items-center gap-2 px-3 py-2 rounded text-sm hover:bg-neutral-100 dark:hover:bg-neutral-800"
        >
          <ListChecks :size="16" /> Proyectos
        </RouterLink>
        <RouterLink
          to="/admin/listas"
          active-class="bg-neutral-100 dark:bg-neutral-800 font-medium"
          class="flex items-center gap-2 px-3 py-2 rounded text-sm hover:bg-neutral-100 dark:hover:bg-neutral-800"
        >
          <Columns3 :size="16" /> Listas
        </RouterLink>
        <RouterLink
          to="/admin/tablero"
          active-class="bg-neutral-100 dark:bg-neutral-800 font-medium"
          class="flex items-center gap-2 px-3 py-2 rounded text-sm hover:bg-neutral-100 dark:hover:bg-neutral-800"
        >
          <LayoutDashboard :size="16" /> Tablero
        </RouterLink>
      </nav>

      <div class="p-3 border-t border-neutral-200 dark:border-neutral-800 text-xs text-neutral-500">
        v0.1 — auth + admin
      </div>
    </aside>

    <!-- Main -->
    <div class="flex-1 flex flex-col min-w-0">
      <header class="h-14 border-b border-neutral-200 dark:border-neutral-800 bg-white dark:bg-neutral-900 px-4 flex items-center justify-end gap-3">
        <span class="text-sm text-neutral-600 dark:text-neutral-400">{{ auth.usuario?.nombre }}</span>
        <span class="text-[10px] uppercase font-medium px-2 py-0.5 rounded bg-neutral-200 dark:bg-neutral-800 text-neutral-700 dark:text-neutral-300">
          {{ auth.rolGlobal }}
        </span>
        <div class="w-8 h-8 rounded-full bg-neutral-900 text-white dark:bg-neutral-100 dark:text-neutral-900 flex items-center justify-center text-xs font-semibold">
          {{ initials }}
        </div>
        <Button variant="ghost" size="sm" :disabled="isLoading" @click="handleLogout">
          <LogOut :size="16" /> Salir
        </Button>
      </header>

      <main class="flex-1 p-6 overflow-y-auto">
        <RouterView />
      </main>
    </div>
  </div>
</template>
