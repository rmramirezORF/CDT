function getEnvVar(name: string, value: string | undefined, defaultValue?: string): string {
  if (value) return value
  if (defaultValue !== undefined) return defaultValue
  throw new Error(`FATAL: La variable de entorno ${name} no está definida y no tiene default.`)
}

export const ENV = {
  API_BASE_URL: getEnvVar('VITE_API_BASE_URL', import.meta.env.VITE_API_BASE_URL, 'http://localhost:5000'),
  API_PREFIX:   getEnvVar('VITE_API_PREFIX',   import.meta.env.VITE_API_PREFIX,   '/api'),
  APP_NAME:     getEnvVar('VITE_APP_NAME',     import.meta.env.VITE_APP_NAME,     'CDT'),
  IS_PROD:      import.meta.env.PROD,
} as const
