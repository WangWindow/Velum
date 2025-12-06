import { defineStore } from 'pinia'
import { ref } from 'vue'
import api from '@/lib/api'

export interface AppSetting {
  key: string
  value: string
}

export const useSettingsStore = defineStore('settings', () => {
  const settings = ref<AppSetting[]>([])
  const isLoading = ref(false)

  async function fetchSettings() {
    isLoading.value = true
    try {
      const response = await api.get('/settings')
      settings.value = response.data
    } catch (error) {
      console.error('Failed to fetch settings:', error)
    } finally {
      isLoading.value = false
    }
  }

  async function updateSettings(newSettings: AppSetting[]) {
    isLoading.value = true
    try {
      await api.post('/settings', newSettings)
      await fetchSettings()
    } catch (error) {
      console.error('Failed to update settings:', error)
      throw error
    } finally {
      isLoading.value = false
    }
  }

  async function resetSettings() {
    isLoading.value = true
    try {
      await api.delete('/settings')
      await fetchSettings()
    } catch (error) {
      console.error('Failed to reset settings:', error)
      throw error
    } finally {
      isLoading.value = false
    }
  }

  function getSettingValue(key: string): string {
    const setting = settings.value.find(s => s.key === key)
    return setting ? setting.value : ''
  }

  return {
    settings,
    isLoading,
    fetchSettings,
    updateSettings,
    resetSettings,
    getSettingValue
  }
})
