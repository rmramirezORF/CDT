<script setup lang="ts">
import { onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { Button } from '@/components/ui/button'
import { Card } from '@/components/ui/card'
import { useAuth, useAuthStore } from '@/modules/auth'

const router = useRouter()
const auth = useAuthStore()
const { isLoading, logout } = useAuth()

onMounted(() => {
  // Si es Admin, llevar a /admin directamente.
  if (auth.isAdmin) {
    router.replace({ name: 'admin-usuarios' })
  }
})

async function handleLogout() {
  await logout()
  router.replace({ name: 'login' })
}
</script>

<template>
  <main class="min-h-screen p-8 bg-neutral-50 dark:bg-neutral-950">
    <div class="max-w-3xl mx-auto space-y-6">
      <div class="flex items-center justify-between">
        <div>
          <h1 class="text-2xl font-semibold">CDT</h1>
          <p class="text-sm text-neutral-500 dark:text-neutral-400">Control de Tareas — ORF</p>
        </div>
        <Button variant="outline" :disabled="isLoading" @click="handleLogout">
          {{ isLoading ? 'Cerrando…' : 'Cerrar sesión' }}
        </Button>
      </div>

      <Card class="p-6 space-y-3">
        <h2 class="text-lg font-medium">Mi perfil</h2>
        <div class="text-sm space-y-1">
          <p><span class="font-medium">Nombre:</span> {{ auth.usuario?.nombre }}</p>
          <p><span class="font-medium">Correo:</span> {{ auth.usuario?.correo }}</p>
          <p><span class="font-medium">Rol:</span> {{ auth.usuario?.rolGlobal }}</p>
        </div>
      </Card>

      <Card class="p-6">
        <p class="text-sm text-neutral-600 dark:text-neutral-400">
          Próximamente vas a ver acá tus tareas asignadas, los proyectos donde participas y tus notificaciones.
        </p>
      </Card>
    </div>
  </main>
</template>
