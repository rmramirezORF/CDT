<script setup lang="ts">
import { useForm } from 'vee-validate'
import { useRoute, useRouter } from 'vue-router'
import { Button } from '@/components/ui/button'
import { Card } from '@/components/ui/card'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { useAuth } from '../composables/useAuth'
import { loginFormSchema, type LoginFormValues } from '../schemas/loginSchema'

const router = useRouter()
const route = useRoute()
const { isLoading, error, login } = useAuth()

const { defineField, handleSubmit, errors } = useForm<LoginFormValues>({
  validationSchema: loginFormSchema,
})

const [correo, correoAttrs] = defineField('correo')
const [password, passwordAttrs] = defineField('password')

const onSubmit = handleSubmit(async (values) => {
  const result = await login(values)
  if (result) {
    const redirect = (route.query.redirect as string) || '/'
    router.replace(redirect)
  }
})
</script>

<template>
  <main class="min-h-screen flex items-center justify-center p-4 bg-neutral-50 dark:bg-neutral-950">
    <Card class="w-full max-w-md p-6 space-y-6">
      <div class="space-y-1 text-center">
        <h1 class="text-2xl font-semibold">Iniciar sesión</h1>
        <p class="text-sm text-neutral-500 dark:text-neutral-400">CDT — Control de Tareas (ORF)</p>
      </div>

      <form class="space-y-4" @submit="onSubmit">
        <div class="space-y-1.5">
          <Label for="correo">Correo</Label>
          <Input
            id="correo"
            type="email"
            placeholder="tu@orf.local"
            autocomplete="email"
            v-model="correo"
            v-bind="correoAttrs"
          />
          <p v-if="errors.correo" class="text-sm text-red-600">{{ errors.correo }}</p>
        </div>

        <div class="space-y-1.5">
          <Label for="password">Contraseña</Label>
          <Input
            id="password"
            type="password"
            autocomplete="current-password"
            v-model="password"
            v-bind="passwordAttrs"
          />
          <p v-if="errors.password" class="text-sm text-red-600">{{ errors.password }}</p>
        </div>

        <p v-if="error" class="text-sm text-red-600 text-center">{{ error }}</p>

        <Button type="submit" :disabled="isLoading" class="w-full">
          {{ isLoading ? 'Iniciando…' : 'Iniciar sesión' }}
        </Button>

        <div class="flex justify-between text-sm pt-1">
          <RouterLink to="/forgot-password" class="text-neutral-600 hover:underline dark:text-neutral-400">
            Olvidé mi contraseña
          </RouterLink>
          <RouterLink to="/register" class="text-neutral-900 hover:underline dark:text-neutral-100">
            Crear cuenta
          </RouterLink>
        </div>
      </form>
    </Card>
  </main>
</template>
