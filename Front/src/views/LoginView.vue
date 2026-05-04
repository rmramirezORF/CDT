<script setup lang="ts">
import { ENV } from '@/config/env'
</script>

<template>
  <main class="min-h-screen flex items-center justify-center p-8">
    <div class="w-full max-w-sm space-y-4 text-center">
      <h1 class="text-2xl font-semibold">{{ ENV.APP_NAME }}</h1>
      <p class="text-sm text-neutral-500">
        Pantalla de login (placeholder). Se completa en el módulo de auth.
      </p>
    </div>
  </main>
</template>
