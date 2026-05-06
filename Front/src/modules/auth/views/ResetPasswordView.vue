<script setup lang="ts">
import { useForm } from 'vee-validate'
import { useRoute, useRouter } from 'vue-router'
import { Button } from '@/components/ui/button'
import { Card } from '@/components/ui/card'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { useAuth } from '../composables/useAuth'
import { resetPasswordFormSchema, type ResetPasswordFormValues } from '../schemas/resetPasswordSchema'

const route = useRoute()
const router = useRouter()
const { isLoading, error, resetPassword } = useAuth()

const { defineField, handleSubmit, errors } = useForm<ResetPasswordFormValues>({
  validationSchema: resetPasswordFormSchema,
  initialValues: {
    correo: (route.query.correo as string) || '',
    codigo: '',
    nuevaPassword: '',
  },
})

const [correo, correoAttrs] = defineField('correo')
const [codigo, codigoAttrs] = defineField('codigo')
const [nuevaPassword, nuevaPasswordAttrs] = defineField('nuevaPassword')

const onSubmit = handleSubmit(async (values) => {
  const result = await resetPassword(values)
  if (result !== null) {
    router.replace({ name: 'login' })
  }
})
</script>

<template>
  <main class="min-h-screen flex items-center justify-center p-4 bg-neutral-50 dark:bg-neutral-950">
    <Card class="w-full max-w-md p-6 space-y-6">
      <div class="space-y-1 text-center">
        <h1 class="text-2xl font-semibold">Restablecer contraseña</h1>
        <p class="text-sm text-neutral-500 dark:text-neutral-400">
          Ingresa el código que recibiste y tu nueva contraseña.
        </p>
      </div>

      <form class="space-y-4" @submit="onSubmit">
        <div class="space-y-1.5">
          <Label for="correo">Correo</Label>
          <Input id="correo" type="email" autocomplete="email" v-model="correo" v-bind="correoAttrs" />
          <p v-if="errors.correo" class="text-sm text-red-600">{{ errors.correo }}</p>
        </div>

        <div class="space-y-1.5">
          <Label for="codigo">Código (6 dígitos)</Label>
          <Input
            id="codigo"
            inputmode="numeric"
            :maxlength="6"
            placeholder="000000"
            v-model="codigo"
            v-bind="codigoAttrs"
          />
          <p v-if="errors.codigo" class="text-sm text-red-600">{{ errors.codigo }}</p>
        </div>

        <div class="space-y-1.5">
          <Label for="nuevaPassword">Nueva contraseña</Label>
          <Input
            id="nuevaPassword"
            type="password"
            autocomplete="new-password"
            v-model="nuevaPassword"
            v-bind="nuevaPasswordAttrs"
          />
          <p v-if="errors.nuevaPassword" class="text-sm text-red-600">{{ errors.nuevaPassword }}</p>
        </div>

        <p v-if="error" class="text-sm text-red-600 text-center">{{ error }}</p>

        <Button type="submit" :disabled="isLoading" class="w-full">
          {{ isLoading ? 'Restableciendo…' : 'Restablecer contraseña' }}
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
