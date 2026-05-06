<script setup lang="ts">
import { cn } from '@/lib/utils'
import { computed } from 'vue'

interface Props {
  modelValue?: string | number | null
  disabled?: boolean
  id?: string
  class?: string
}

const props = withDefaults(defineProps<Props>(), { modelValue: '' })

const emit = defineEmits<{
  'update:modelValue': [value: string]
  change: [value: string]
}>()

const value = computed({
  get: () => props.modelValue ?? '',
  set: (v) => emit('update:modelValue', String(v ?? '')),
})

function onChange(e: Event) {
  emit('change', (e.target as HTMLSelectElement).value)
}
</script>

<template>
  <select
    :id="id"
    v-model="value"
    :disabled="disabled"
    @change="onChange"
    :class="cn(
      'flex h-9 w-full rounded-md border border-neutral-300 bg-white px-2 text-sm text-neutral-900',
      'focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-neutral-400 focus-visible:ring-offset-1',
      'disabled:cursor-not-allowed disabled:opacity-50',
      'dark:border-neutral-700 dark:bg-neutral-900 dark:text-neutral-50',
      props.class,
    )"
  >
    <slot />
  </select>
</template>
