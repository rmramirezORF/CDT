<script setup lang="ts">
import { useTheme } from '@/composables/useTheme'

useTheme() // inicializa el tema y aplica las clases light/dark al <html>
</script>

<template>
  <RouterView />
</template>
