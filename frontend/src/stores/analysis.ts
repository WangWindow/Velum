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
  analysisJson?: string
}

export interface UserAnalysisResult {
  userId: number
  username: string
  history: AssessmentSummary[]
}

export interface AssessmentExportData {
  columns: string[]
  rows: Record<string, any>[]
}

export const useAnalysisStore = defineStore('analysis', () => {
  const overallStats = ref<OverallStats | null>(null)
  const currentUserAnalysis = ref<UserAnalysisResult | null>(null)
  const exportData = ref<AssessmentExportData | null>(null)
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

  async function fetchExportData(questionnaireId: number) {
    isLoading.value = true
    try {
      const response = await api.get<AssessmentExportData>(`/analysis/export/${questionnaireId}`)
      exportData.value = response.data
    } catch (error) {
      console.error('Failed to fetch export data:', error)
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

  async function runBatchAnalysis() {
    try {
      await api.post('/analysis/run')
    } catch (error) {
      console.error('Failed to run batch analysis:', error)
    }
  }

  async function analyzeAssessment(assessmentId: number) {
    try {
      const response = await api.post<{ analysis: string }>(`/analysis/analyze/${assessmentId}`)
      // Update local state
      if (currentUserAnalysis.value) {
        const assessment = currentUserAnalysis.value.history.find(a => a.id === assessmentId)
        if (assessment) {
          assessment.analysisJson = response.data.analysis
        }
      }
      return response.data.analysis
    } catch (error) {
      console.error('Failed to analyze assessment:', error)
    }
  }

  return {
    overallStats,
    currentUserAnalysis,
    exportData,
    isLoading,
    fetchOverallStats,
    fetchUserAnalysis,
    runBatchAnalysis,
    analyzeAssessment,
    fetchExportData
  }
})
