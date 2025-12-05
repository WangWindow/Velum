import { defineStore } from 'pinia'
import { ref } from 'vue'
import api from '@/lib/api'

// Define types based on backend models
export interface Option {
  text: string
  score: number
}

export interface Question {
  id: number
  text: string
  type: 'SingleChoice' | 'MultipleChoice' | 'Text' | 'Scale'
  options: Option[]
}

export interface QuestionnaireTemplate {
  title: string
  description: string
  interpretationGuide?: string
  questions: Question[]
}

export interface BilingualQuestionnaireTemplate {
  en: QuestionnaireTemplate
  zh: QuestionnaireTemplate
}

export const useScalesStore = defineStore('scales', () => {
  const parsedTemplate = ref<QuestionnaireTemplate | null>(null)
  const parsedBilingualTemplate = ref<BilingualQuestionnaireTemplate | null>(null)
  const scales = ref<any[]>([])
  const isAnalyzing = ref(false)
  const isSaving = ref(false)
  const isLoading = ref(false)
  const error = ref<string | null>(null)

  async function fetchScales() {
    isLoading.value = true
    try {
      const response = await api.get('/questionnaire')
      scales.value = response.data
    } catch (err) {
      console.error('Failed to fetch scales:', err)
    } finally {
      isLoading.value = false
    }
  }

  async function deleteScale(id: number) {
    isLoading.value = true
    try {
      await api.delete(`/questionnaire/${id}`)
      scales.value = scales.value.filter(s => s.id !== id)
      return true
    } catch (err: any) {
      console.error('Failed to delete scale:', err)
      error.value = err.response?.data || 'Failed to delete scale.'
      return false
    } finally {
      isLoading.value = false
    }
  }

  async function parseScale(text: string) {
    isAnalyzing.value = true
    error.value = null
    try {
      const response = await api.post<QuestionnaireTemplate>('/questionnaire/parse', { text })
      parsedTemplate.value = response.data
    } catch (err: any) {
      console.error('Failed to parse scale:', err)
      error.value = err.response?.data || 'Failed to parse scale text.'
    } finally {
      isAnalyzing.value = false
    }
  }

  async function saveScale() {
    if (!parsedTemplate.value) return

    isSaving.value = true
    error.value = null
    try {
      await api.post('/questionnaire', parsedTemplate.value)
      parsedTemplate.value = null
      await fetchScales() // Refresh list
      return true
    } catch (err: any) {
      console.error('Failed to save scale:', err)
      error.value = err.response?.data || 'Failed to save scale.'
      return false
    } finally {
      isSaving.value = false
    }
  }

  async function parseBilingualScale(text: string) {
    isAnalyzing.value = true
    error.value = null
    try {
      const response = await api.post<BilingualQuestionnaireTemplate>('/questionnaire/parse-bilingual', { text })
      console.log('Bilingual response:', response.data)
      parsedBilingualTemplate.value = response.data
    } catch (err: any) {
      console.error('Failed to parse bilingual scale:', err)
      error.value = err.response?.data || 'Failed to parse scale text.'
    } finally {
      isAnalyzing.value = false
    }
  }

  async function saveBilingualScale() {
    if (!parsedBilingualTemplate.value) return

    isSaving.value = true
    error.value = null
    try {
      await api.post('/questionnaire/bilingual', parsedBilingualTemplate.value)
      parsedBilingualTemplate.value = null
      await fetchScales() // Refresh list
      return true
    } catch (err: any) {
      console.error('Failed to save bilingual scale:', err)
      error.value = err.response?.data || 'Failed to save bilingual scale.'
      return false
    } finally {
      isSaving.value = false
    }
  }

  function clear() {
    parsedTemplate.value = null
    error.value = null
  }

  return {
    parsedTemplate,
    parsedBilingualTemplate,
    scales,
    isAnalyzing,
    isSaving,
    isLoading,
    error,
    fetchScales,
    deleteScale,
    parseScale,
    saveScale,
    parseBilingualScale,
    saveBilingualScale,
    clear
  }
})
