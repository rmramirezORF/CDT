<script setup lang="ts">
import { cn } from '@/lib/utils'
import { computed } from 'vue'

interface Props {
  modelValue?: string | number
  type?: string
  placeholder?: string
  disabled?: boolean
  readonly?: boolean
  id?: string
  name?: string
  autocomplete?: string
  inputmode?: 'none' | 'text' | 'tel' | 'url' | 'email' | 'numeric' | 'decimal' | 'search'
  maxlength?: number
  class?: string
}

const props = withDefaults(defineProps<Props>(), { type: 'text', modelValue: '' })

const emit = defineEmits<{
  'update:modelValue': [value: string]
  blur: [event: FocusEvent]
  focus: [event: FocusEvent]
}>()

const value = computed({
  get: () => props.modelValue,
  set: (v) => emit('update:modelValue', v == null ? '' : String(v)),
})
</script>

<template>
  <input
    :id="id"
    :name="name"
    v-model="value"
    :type="type"
    :placeholder="placeholder"
    :disabled="disabled"
    :readonly="readonly"
    :autocomplete="autocomplete"
    :inputmode="inputmode"
    :maxlength="maxlength"
    @blur="(e) => emit('blur', e)"
    @focus="(e) => emit('focus', e)"
    :class="cn(
      'flex h-10 w-full rounded-md border border-neutral-300 bg-white px-3 py-2 text-sm text-neutral-900 placeholder:text-neutral-400',
      'focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-neutral-400 focus-visible:ring-offset-2',
      'disabled:cursor-not-allowed disabled:opacity-50',
      'dark:border-neutral-700 dark:bg-neutral-900 dark:text-neutral-50 dark:placeholder:text-neutral-500',
      props.class,
    )"
  />
</template>
