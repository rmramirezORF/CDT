<script setup lang="ts">
import { ENV } from '@/config/env'
</script>

<template>
  <main class="min-h-screen flex flex-col items-center justify-center p-8 gap-4">
    <h1 class="text-3xl font-semibold">{{ ENV.APP_NAME }}</h1>
    <p class="text-sm text-neutral-500">Andamiaje listo. Próximo: módulo de auth.</p>
  </main>
</template>
