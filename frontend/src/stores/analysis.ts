import { defineStore } from 'pinia'
import { ref } from 'vue'
import api from '@/lib/api'

export interface QuestionnaireStat {
  questionnaireId: number
  title: string
  count: number
  averageScore: number
}

export interface OverallStats {
  totalAssessments: number
  totalUsers: number
  questionnaireStats: QuestionnaireStat[]
}

export interface AssessmentSummary {
  id: number
  questionnaireTitle: string
  date: string
  score: number
  result?: string
}

export interface UserAnalysisResult {
  userId: number
  username: string
  history: AssessmentSummary[]
}

export const useAnalysisStore = defineStore('analysis', () => {
  const overallStats = ref<OverallStats | null>(null)
  const currentUserAnalysis = ref<UserAnalysisResult | null>(null)
  const isLoading = ref(false)

  async function fetchOverallStats() {
    isLoading.value = true
    try {
      const response = await api.get<OverallStats>('/analysis/stats')
      overallStats.value = response.data
    } catch (error) {
      console.error('Failed to fetch overall stats:', error)
    } finally {
      isLoading.value = false
    }
  }

  async function fetchUserAnalysis(userId: number) {
    isLoading.value = true
    try {
      const response = await api.get<UserAnalysisResult>(`/analysis/user/${userId}`)
      currentUserAnalysis.value = response.data
    } catch (error) {
      console.error('Failed to fetch user analysis:', error)
    } finally {
      isLoading.value = false
    }
  }

  return {
    overallStats,
    currentUserAnalysis,
    isLoading,
    fetchOverallStats,
    fetchUserAnalysis
  }
})
