<script setup lang="ts">
import { useForm } from 'vee-validate'
import { useRouter } from 'vue-router'
import { Button } from '@/components/ui/button'
import { Card } from '@/components/ui/card'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { useAuth } from '../composables/useAuth'
import { forgotPasswordFormSchema, type ForgotPasswordFormValues } from '../schemas/forgotPasswordSchema'

const router = useRouter()
const { isLoading, error, forgotPassword } = useAuth()

const { defineField, handleSubmit, errors } = useForm<ForgotPasswordFormValues>({
  validationSchema: forgotPasswordFormSchema,
})

const [correo, correoAttrs] = defineField('correo')

const onSubmit = handleSubmit(async (values) => {
  const result = await forgotPassword(values)
  if (result !== null) {
    router.replace({ name: 'reset-password', query: { correo: values.correo } })
  }
})
</script>

<template>
  <main class="min-h-screen flex items-center justify-center p-4 bg-neutral-50 dark:bg-neutral-950">
    <Card class="w-full max-w-md p-6 space-y-6">
      <div class="space-y-1 text-center">
        <h1 class="text-2xl font-semibold">Olvidé mi contraseña</h1>
        <p class="text-sm text-neutral-500 dark:text-neutral-400">
          Te enviaremos un código de 6 dígitos para restablecerla.
        </p>
      </div>

      <form class="space-y-4" @submit="onSubmit">
        <div class="space-y-1.5">
          <Label for="correo">Correo</Label>
          <Input id="correo" type="email" autocomplete="email" v-model="correo" v-bind="correoAttrs" />
          <p v-if="errors.correo" class="text-sm text-red-600">{{ errors.correo }}</p>
        </div>

        <p v-if="error" class="text-sm text-red-600 text-center">{{ error }}</p>

        <Button type="submit" :disabled="isLoading" class="w-full">
          {{ isLoading ? 'Enviando…' : 'Enviar código' }}
        </Button>

        <p class="text-center text-sm pt-1">
          <RouterLink to="/login" class="text-neutral-600 hover:underline dark:text-neutral-400">
            Volver a iniciar sesión
          </RouterLink>
        </p>
      </form>
    </Card>
  </main>
</template>
