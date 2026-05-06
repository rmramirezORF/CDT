<script setup lang="ts">
import { cn } from '@/lib/utils'

interface Props { class?: string }
const props = defineProps<Props>()
</script>

<template>
  <div
    :class="cn(
      'rounded-lg border border-neutral-200 bg-white text-neutral-950 shadow-sm',
      'dark:border-neutral-800 dark:bg-neutral-900 dark:text-neutral-50',
      props.class,
    )"
  >
    <slot />
  </div>
</template>
