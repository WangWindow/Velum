import { defineStore } from 'pinia'
import { ref } from 'vue'
import api from '@/lib/api'

export interface ActivityItem {
  title: string
  time: string
  avatar: string
}

export interface DashboardStats {
  totalUsers: number
  activeTasks: number
  completedAssessments: number
  systemHealth: string
  activities: ActivityItem[]
}

export interface ChartData {
  labels: string[]
  datasets: {
    label: string
    data: number[]
    backgroundColor?: string
    borderColor?: string
    fill?: boolean
  }[]
}

export interface BackendChartData {
  trend: { date: string; count: number }[]
  distribution: { name: string; value: number }[]
}

export const useDashboardStore = defineStore('dashboard', () => {
  const stats = ref<DashboardStats | null>(null)
  const chartData = ref<ChartData | null>(null)
  const isLoading = ref(false)

  async function fetchStats() {
    isLoading.value = true
    try {
      const response = await api.get<DashboardStats>('/dashboard/stats')
      stats.value = response.data
    } catch (error) {
      console.error('Failed to fetch dashboard stats:', error)
    } finally {
      isLoading.value = false
    }
  }

  async function fetchChartData() {
    try {
      const response = await api.get<BackendChartData>('/dashboard/chart-data')
      const backendData = response.data

      chartData.value = {
        labels: backendData.trend.map(d => d.date),
        datasets: [
          {
            label: 'Assessments',
            data: backendData.trend.map(d => d.count),
            backgroundColor: 'hsl(var(--primary))',
            borderColor: 'hsl(var(--primary))',
            fill: false
          }
        ]
      }
    } catch (error) {
      console.error('Failed to fetch chart data:', error)
    }
  }

  return {
    stats,
    chartData,
    isLoading,
    fetchStats,
    fetchChartData
  }
})
