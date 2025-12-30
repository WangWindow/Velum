<script setup lang="ts">
import { onMounted, ref, computed } from 'vue'
import { useAnalysisStore } from '@/stores/analysis'
import { useUsersStore } from '@/stores/users'
import { storeToRefs } from 'pinia'
import { useI18n } from 'vue-i18n'
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from '@/components/ui/card'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import { Input } from '@/components/ui/input'
import { Button } from '@/components/ui/button'
import { Accordion, AccordionContent, AccordionItem, AccordionTrigger } from '@/components/ui/accordion'
import { Loader2, Brain, Search, Play } from 'lucide-vue-next'
import MarkdownIt from 'markdown-it'
import { Chart } from '@/components/ui/chart'

const md = new MarkdownIt({
  html: false,
  linkify: true,
  breaks: true
})

const { t } = useI18n()
const analysisStore = useAnalysisStore()
const usersStore = useUsersStore()
const { overallStats, currentUserAnalysis, isLoading } = storeToRefs(analysisStore)
const { users } = storeToRefs(usersStore)

const selectedUserId = ref<string>('')
const userSearch = ref('')
const analyzingId = ref<number | null>(null)

const filteredUsers = computed(() => {
  let result = users.value.filter(u => u.role !== 'Admin')
  if (userSearch.value) {
    const search = userSearch.value.toLowerCase()
    result = result.filter(u =>
      u.username.toLowerCase().includes(search) ||
      (u.fullName && u.fullName.toLowerCase().includes(search))
    )
  }
  return result
})

onMounted(() => {
  analysisStore.fetchOverallStats()
  usersStore.fetchUsers()
})

const handleUserChange = (userId: string) => {
  if (userId) {
    analysisStore.fetchUserAnalysis(Number(userId))
  }
}

const handleRunBatchAnalysis = async () => {
  await analysisStore.runBatchAnalysis()
}

const handleAnalyzeAssessment = async (id: number) => {
  analyzingId.value = id
  await analysisStore.analyzeAssessment(id)
  analyzingId.value = null
}

// Chart Data for Overall Stats
const barOption = computed(() => {
  if (!overallStats.value) return {}

  return {
    tooltip: {
      trigger: 'axis',
      axisPointer: {
        type: 'shadow'
      }
    },
    grid: {
      left: '3%',
      right: '4%',
      bottom: '3%',
      containLabel: true
    },
    xAxis: {
      type: 'category',
      data: overallStats.value.questionnaireStats.map(s => s.title),
      axisLabel: {
        interval: 0,
        rotate: 30
      }
    },
    yAxis: {
      type: 'value',
    },
    series: [
      {
        name: t('analysis.score'),
        type: 'bar',
        data: overallStats.value.questionnaireStats.map(s => s.averageScore),
        itemStyle: {
          color: '#f87979'
        }
      }
    ]
  }
})

const pieOption = computed(() => {
  if (!overallStats.value) return {}

  return {
    tooltip: {
      trigger: 'item'
    },
    legend: {
      orient: 'vertical',
      left: 'left',
    },
    color: ['#41B883', '#E46651', '#00D8FF', '#DD1B16', '#36a2eb', '#ffcd56'],
    series: [
      {
        name: t('analysis.distribution'),
        type: 'pie',
        radius: '50%',
        data: overallStats.value.questionnaireStats.map(s => ({
          value: s.count,
          name: s.title
        })),
        emphasis: {
          itemStyle: {
            shadowBlur: 10,
            shadowOffsetX: 0,
            shadowColor: 'rgba(0, 0, 0, 0.5)'
          }
        }
      }
    ]
  }
})

// Chart Data for User History
const lineOption = computed(() => {
  if (!currentUserAnalysis.value) return {}

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

  const series = Object.entries(grouped).map(([title, items], index) => {
    const color = getColor(index)

    // Map data to the common timeline
    const data = allDates.map(date => {
      const item = items.find(i => i.date === date)
      return item ? item.score : null
    })

    return {
      name: title,
      type: 'line',
      data: data,
      connectNulls: true, // spanGaps equivalent
      smooth: true,
      itemStyle: {
        color: color
      }
    }
  })

  return {
    grid: {
      left: '3%',
      right: '4%',
      bottom: '3%',
      containLabel: true
    },
    xAxis: {
      type: 'category',
      boundaryGap: false,
      data: allDates.map(d => new Date(d).toLocaleDateString()),
    },
    yAxis: {
      type: 'value',
    },
    series
  }
})

function getColor(index: number) {
  const colors = ['#36a2eb', '#ff6384', '#4bc0c0', '#ff9f40', '#9966ff', '#ffcd56']
  return colors[index % colors.length]
}
</script>

<template>
  <div class="space-y-6">
    <div class="flex items-center justify-between">
      <div>
        <h2 class="text-3xl font-bold tracking-tight">{{ t('analysis.title') }}</h2>
        <p class="text-muted-foreground">{{ t('analysis.description') }}</p>
      </div>
      <Button @click="handleRunBatchAnalysis">
        <Brain class="mr-2 h-4 w-4" />
        {{ t('analysis.runBatch') }}
      </Button>
    </div>

    <div v-if="isLoading" class="flex justify-center p-8">
      <Loader2 class="h-8 w-8 animate-spin" />
    </div>

    <div v-else class="space-y-6">
      <!-- Overall Stats -->
      <div class="grid gap-6 md:grid-cols-3">
        <Card class="col-span-1">
          <CardHeader>
            <CardTitle>{{ t('analysis.systemOverview') }}</CardTitle>
          </CardHeader>
          <CardContent>
            <div class="grid grid-cols-1 gap-4 text-center">
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

        <Card class="col-span-1">
          <CardHeader>
            <CardTitle>{{ t('analysis.averageScores') }}</CardTitle>
          </CardHeader>
          <CardContent class="h-75">
            <Chart v-if="overallStats" :option="barOption" />
          </CardContent>
        </Card>

        <Card class="col-span-1">
          <CardHeader>
            <CardTitle>{{ t('analysis.distribution') }}</CardTitle>
          </CardHeader>
          <CardContent class="h-75 flex justify-center">
            <Chart v-if="overallStats" :option="pieOption" />
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
          <div class="flex gap-4 items-center flex-wrap">
            <div class="relative w-full md:w-50">
              <Search class="absolute left-2 top-2.5 h-4 w-4 text-muted-foreground" />
              <Input v-model="userSearch" :placeholder="t('analysis.searchUser')" class="pl-8" />
            </div>
            <div class="w-full md:w-75">
              <Select v-model="selectedUserId" @update:model-value="handleUserChange">
                <SelectTrigger>
                  <SelectValue :placeholder="t('analysis.selectUser')" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem v-for="user in filteredUsers" :key="user.id" :value="String(user.id)">
                    {{ user.username }}
                  </SelectItem>
                </SelectContent>
              </Select>
            </div>
          </div>

          <div v-if="currentUserAnalysis" class="space-y-6">
            <div class="h-75">
              <Chart :option="lineOption" />
            </div>

            <div class="space-y-4">
              <h3 class="text-lg font-semibold">{{ t('analysis.history') }}</h3>
              <Accordion type="single" collapsible class="w-full">
                <AccordionItem v-for="assessment in currentUserAnalysis.history" :key="assessment.id"
                  :value="String(assessment.id)">
                  <AccordionTrigger>
                    <div class="flex gap-4 items-center w-full pr-4">
                      <span class="font-medium">{{ assessment.questionnaireTitle }}</span>
                      <span class="text-muted-foreground text-sm">{{ new Date(assessment.date).toLocaleDateString()
                      }}</span>
                      <span class="ml-auto font-bold">Score: {{ assessment.score }}</span>
                    </div>
                  </AccordionTrigger>
                  <AccordionContent>
                    <div class="space-y-4 pt-2">
                      <div class="grid gap-2">
                        <div class="font-semibold">{{ t('analysis.resultSummary') }}:</div>
                        <div class="p-2 bg-muted rounded-md text-sm">{{ assessment.result || 'No result summary' }}
                        </div>
                      </div>

                      <div class="grid gap-2">
                        <div class="flex items-center justify-between">
                          <div class="font-semibold flex items-center gap-2">
                            <Brain class="h-4 w-4" />
                            {{ t('analysis.aiAnalysis') }}
                          </div>
                          <Button v-if="!assessment.analysisJson" size="sm" variant="outline"
                            @click="handleAnalyzeAssessment(assessment.id)" :disabled="analyzingId === assessment.id">
                            <Loader2 v-if="analyzingId === assessment.id" class="mr-2 h-3 w-3 animate-spin" />
                            <Play v-else class="mr-2 h-3 w-3" />
                            {{ t('analysis.analyze') }}
                          </Button>
                        </div>
                        <div v-if="assessment.analysisJson"
                          class="p-4 bg-secondary/20 rounded-md text-sm prose prose-sm dark:prose-invert max-w-none"
                          v-html="md.render(assessment.analysisJson)">
                        </div>
                        <div v-else class="text-sm text-muted-foreground italic">
                          {{ t('analysis.noAnalysis') }}
                        </div>
                      </div>
                    </div>
                  </AccordionContent>
                </AccordionItem>
              </Accordion>
            </div>
          </div>
        </CardContent>
      </Card>
    </div>
  </div>
</template>

<style scoped>
.chart {
  height: 100%;
  width: 100%;
}
</style>
