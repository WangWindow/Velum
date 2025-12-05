<script setup lang="ts">
import { onMounted, ref, computed } from 'vue'
import { useAnalysisStore } from '@/stores/analysis'
import { useUsersStore } from '@/stores/users'
import { storeToRefs } from 'pinia'
import { useI18n } from 'vue-i18n'
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from '@/components/ui/card'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import { Loader2 } from 'lucide-vue-next'
import { Bar, Line } from 'vue-chartjs'
import {
  Chart as ChartJS,
  Title,
  Tooltip,
  Legend,
  BarElement,
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement
} from 'chart.js'

ChartJS.register(CategoryScale, LinearScale, BarElement, Title, Tooltip, Legend, PointElement, LineElement)

const { t } = useI18n()
const analysisStore = useAnalysisStore()
const usersStore = useUsersStore()
const { overallStats, currentUserAnalysis, isLoading } = storeToRefs(analysisStore)
const { users } = storeToRefs(usersStore)

const selectedUserId = ref<string>('')

onMounted(() => {
  analysisStore.fetchOverallStats()
  usersStore.fetchUsers()
})

const handleUserChange = (userId: string) => {
  if (userId) {
    analysisStore.fetchUserAnalysis(Number(userId))
  }
}

// Chart Data for Overall Stats
const barChartData = computed(() => {
  if (!overallStats.value) return { labels: [], datasets: [] }

  return {
    labels: overallStats.value.questionnaireStats.map(s => s.title),
    datasets: [
      {
        label: 'Average Score',
        backgroundColor: '#f87979',
        data: overallStats.value.questionnaireStats.map(s => s.averageScore)
      }
    ]
  }
})

// Chart Data for User History
const lineChartData = computed(() => {
  if (!currentUserAnalysis.value) return { labels: [], datasets: [] }

  const history = currentUserAnalysis.value.history
  // Get all unique dates and sort them chronologically
  const allDates = [...new Set(history.map(h => h.date))].sort((a, b) => new Date(a).getTime() - new Date(b).getTime())

  // Group by questionnaire
  const grouped = history.reduce((acc, curr) => {
    const key = curr.questionnaireTitle
    if (!acc[key]) {
      acc[key] = []
    }
    acc[key].push(curr)
    return acc
  }, {} as Record<string, typeof history>)

  const datasets = Object.entries(grouped).map(([title, items], index) => {
    const color = getColor(index)

    // Map data to the common timeline
    const data = allDates.map(date => {
      const item = items.find(i => i.date === date)
      return item ? item.score : null
    })

    return {
      label: title,
      borderColor: color,
      backgroundColor: color,
      data: data,
      spanGaps: true,
      tension: 0.3
    }
  })

  return {
    labels: allDates.map(d => new Date(d).toLocaleDateString()),
    datasets
  }
})

function getColor(index: number) {
  const colors = ['#36a2eb', '#ff6384', '#4bc0c0', '#ff9f40', '#9966ff', '#ffcd56']
  return colors[index % colors.length]
}

const chartOptions = {
  responsive: true,
  maintainAspectRatio: false,
  interaction: {
    mode: 'index' as const,
    intersect: false,
  },
  plugins: {
    tooltip: {
      callbacks: {
        label: function (context: any) {
          let label = context.dataset.label || '';
          if (label) {
            label += ': ';
          }
          if (context.parsed.y !== null) {
            label += context.parsed.y;
          }
          return label;
        }
      }
    }
  }
}
</script>

<template>
  <div class="space-y-6">
    <div class="flex items-center justify-between">
      <div>
        <h2 class="text-3xl font-bold tracking-tight">{{ t('analysis.title') }}</h2>
        <p class="text-muted-foreground">{{ t('analysis.description') }}</p>
      </div>
    </div>

    <div v-if="isLoading" class="flex justify-center p-8">
      <Loader2 class="h-8 w-8 animate-spin" />
    </div>

    <div v-else class="space-y-6">
      <!-- Overall Stats -->
      <div class="grid gap-6 md:grid-cols-2">
        <Card>
          <CardHeader>
            <CardTitle>{{ t('analysis.systemOverview') }}</CardTitle>
          </CardHeader>
          <CardContent>
            <div class="grid grid-cols-2 gap-4 text-center">
              <div class="p-4 bg-muted rounded-lg">
                <div class="text-2xl font-bold">{{ overallStats?.totalUsers || 0 }}</div>
                <div class="text-sm text-muted-foreground">{{ t('analysis.totalUsers') }}</div>
              </div>
              <div class="p-4 bg-muted rounded-lg">
                <div class="text-2xl font-bold">{{ overallStats?.totalAssessments || 0 }}</div>
                <div class="text-sm text-muted-foreground">{{ t('analysis.totalAssessments') }}</div>
              </div>
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle>{{ t('analysis.averageScores') }}</CardTitle>
          </CardHeader>
          <CardContent class="h-[300px]">
            <Bar v-if="overallStats" :data="barChartData" :options="chartOptions" />
          </CardContent>
        </Card>
      </div>

      <!-- Individual Analysis -->
      <Card>
        <CardHeader>
          <CardTitle>{{ t('analysis.individualAnalysis') }}</CardTitle>
          <CardDescription>{{ t('analysis.selectUserDesc') }}</CardDescription>
        </CardHeader>
        <CardContent class="space-y-6">
          <div class="w-[300px]">
            <Select v-model="selectedUserId" @update:model-value="handleUserChange">
              <SelectTrigger>
                <SelectValue :placeholder="t('analysis.selectUser')" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem v-for="user in users" :key="user.id" :value="String(user.id)">
                  {{ user.username }}
                </SelectItem>
              </SelectContent>
            </Select>
          </div>

          <div v-if="currentUserAnalysis" class="space-y-6">
            <div class="h-[300px]">
              <Line :data="lineChartData" :options="chartOptions" />
            </div>

            <div class="rounded-md border">
              <table class="w-full text-sm text-left">
                <thead class="bg-muted/50 text-muted-foreground">
                  <tr>
                    <th class="p-4 font-medium">{{ t('analysis.date') }}</th>
                    <th class="p-4 font-medium">{{ t('analysis.scale') }}</th>
                    <th class="p-4 font-medium">{{ t('analysis.score') }}</th>
                    <th class="p-4 font-medium">{{ t('analysis.result') }}</th>
                  </tr>
                </thead>
                <tbody>
                  <tr v-for="item in currentUserAnalysis.history" :key="item.id" class="border-t">
                    <td class="p-4">{{ new Date(item.date).toLocaleDateString() }}</td>
                    <td class="p-4">{{ item.questionnaireTitle }}</td>
                    <td class="p-4 font-bold">{{ item.score }}</td>
                    <td class="p-4 text-muted-foreground">{{ item.result || '-' }}</td>
                  </tr>
                </tbody>
              </table>
            </div>
          </div>
        </CardContent>
      </Card>
    </div>
  </div>
</template>
