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

export const useScalesStore = defineStore('scales', () => {
  const parsedTemplate = ref<QuestionnaireTemplate | null>(null)
  const isAnalyzing = ref(false)
  const isSaving = ref(false)
  const error = ref<string | null>(null)

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
      parsedTemplate.value = null // Clear after save
    } catch (err: any) {
      console.error('Failed to save scale:', err)
      error.value = err.response?.data || 'Failed to save scale.'
    } finally {
      isSaving.value = false
    }
  }

  return {
    parsedTemplate,
    isAnalyzing,
    isSaving,
    error,
    parseScale,
    saveScale
  }
})
