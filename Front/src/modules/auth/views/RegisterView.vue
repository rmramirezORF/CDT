<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { useForm } from 'vee-validate'
import { useRouter } from 'vue-router'
import { Button } from '@/components/ui/button'
import { Card } from '@/components/ui/card'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import authService from '../services/authService'
import { useAuth } from '../composables/useAuth'
import {
  buildRegisterFormSchema,
  FALLBACK_ALLOWED_DOMAINS,
  type RegisterFormValues,
} from '../schemas/registerSchema'

const router = useRouter()
const { isLoading, error, register } = useAuth()

const allowedDomains = ref<readonly string[]>(FALLBACK_ALLOWED_DOMAINS)

const validationSchema = computed(() => buildRegisterFormSchema(allowedDomains.value))

const allowedDomainsLabel = computed(() => allowedDomains.value.map((d) => `@${d}`).join(' o '))

const { defineField, handleSubmit, errors } = useForm<RegisterFormValues>({
  validationSchema,
})

const [nombre, nombreAttrs] = defineField('nombre')
const [correo, correoAttrs] = defineField('correo')
const [password, passwordAttrs] = defineField('password')

onMounted(async () => {
  try {
    const list = await authService.getAllowedDomains()
    if (Array.isArray(list) && list.length > 0) allowedDomains.value = list
  } catch {
    // si la API no responde, mantenemos los fallback
  }
})

const onSubmit = handleSubmit(async (values) => {
  const result = await register(values)
  if (result) {
    router.replace({ name: 'confirm-email', query: { correo: values.correo } })
  }
})
</script>

<template>
  <main class="min-h-screen flex items-center justify-center p-4 bg-neutral-50 dark:bg-neutral-950">
    <Card class="w-full max-w-md p-6 space-y-6">
      <div class="space-y-1 text-center">
        <h1 class="text-2xl font-semibold">Crear cuenta</h1>
        <p class="text-sm text-neutral-500 dark:text-neutral-400">
          Solo correos <strong>{{ allowedDomainsLabel }}</strong>.
        </p>
        <p class="text-xs text-neutral-400 dark:text-neutral-500">
          Te enviaremos un código de 6 dígitos para confirmar.
        </p>
      </div>

      <form class="space-y-4" @submit="onSubmit">
        <div class="space-y-1.5">
          <Label for="nombre">Nombre completo</Label>
          <Input id="nombre" autocomplete="name" v-model="nombre" v-bind="nombreAttrs" />
          <p v-if="errors.nombre" class="text-sm text-red-600">{{ errors.nombre }}</p>
        </div>

        <div class="space-y-1.5">
          <Label for="correo">Correo</Label>
          <Input id="correo" type="email" autocomplete="email" v-model="correo" v-bind="correoAttrs" />
          <p v-if="errors.correo" class="text-sm text-red-600">{{ errors.correo }}</p>
        </div>

        <div class="space-y-1.5">
          <Label for="password">Contraseña</Label>
          <Input id="password" type="password" autocomplete="new-password" v-model="password" v-bind="passwordAttrs" />
          <p v-if="errors.password" class="text-sm text-red-600">{{ errors.password }}</p>
        </div>

        <p v-if="error" class="text-sm text-red-600 text-center">{{ error }}</p>

        <Button type="submit" :disabled="isLoading" class="w-full">
          {{ isLoading ? 'Registrando…' : 'Registrarme' }}
        </Button>

        <p class="text-center text-sm pt-1">
          ¿Ya tienes cuenta?
          <RouterLink to="/login" class="text-neutral-900 hover:underline dark:text-neutral-100">
            Iniciar sesión
          </RouterLink>
        </p>
      </form>
    </Card>
  </main>
</template>
