import { computed, watch } from 'vue'
import { useStorage, usePreferredDark } from '@vueuse/core'

type Theme = 'light' | 'dark'

const stored = useStorage<Theme | null>('cdt:theme', null)
const userChose = useStorage<boolean>('cdt:theme:user-chose', false)

export function useTheme() {
  const prefersDark = usePreferredDark()

  const theme = computed<Theme>(() =>
    userChose.value && stored.value
      ? stored.value
      : prefersDark.value ? 'dark' : 'light',
  )

  function setTheme(t: Theme) {
    stored.value = t
    userChose.value = true
  }

  function toggle() {
    setTheme(theme.value === 'light' ? 'dark' : 'light')
  }

  watch(theme, (t) => {
    document.documentElement.classList.remove('light', 'dark')
    document.documentElement.classList.add(t)
  }, { immediate: true })

  return { theme, setTheme, toggle }
}
