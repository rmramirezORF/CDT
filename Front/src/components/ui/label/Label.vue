<script setup lang="ts">
import { cn } from '@/lib/utils'

interface Props {
  for?: string
  class?: string
}

const props = defineProps<Props>()
</script>

<template>
  <label
    :for="props.for"
    :class="cn('text-sm font-medium leading-none text-neutral-900 dark:text-neutral-50', props.class)"
  >
    <slot />
  </label>
</template>
