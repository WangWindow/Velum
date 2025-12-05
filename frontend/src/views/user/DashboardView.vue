<script setup lang="ts">
import { onMounted, computed } from 'vue'
import { useI18n } from 'vue-i18n'
import { useTasksStore } from '@/stores/tasks'
import { useAssessmentStore } from '@/stores/assessment'
import { storeToRefs } from 'pinia'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { ScrollArea } from '@/components/ui/scroll-area'
import { ClipboardList, Clock, MessageSquarePlus, FileText, TrendingUp, CheckCircle } from 'lucide-vue-next'
import { useRouter } from 'vue-router'
import OverviewChart from '@/components/OverviewChart.vue'

const { t } = useI18n()
const router = useRouter()
const tasksStore = useTasksStore()
const assessmentStore = useAssessmentStore()

const { tasks } = storeToRefs(tasksStore)
const { myAssessments } = storeToRefs(assessmentStore)

onMounted(() => {
  tasksStore.fetchMyTasks()
  assessmentStore.fetchMyAssessments()
})

const navigateToAssessment = (questionnaireId: number) => {
  router.push({ path: '/user/assessment', query: { id: questionnaireId } })
}

const navigateToChat = () => router.push('/user/chat')
const navigateToAssessments = () => router.push('/user/assessment')

const averageScore = computed(() => {
  if (myAssessments.value.length === 0) return 0
  const sum = myAssessments.value.reduce((acc, curr) => acc + curr.score, 0)
  return Math.round(sum / myAssessments.value.length)
})

const chartData = computed(() => {
  const sorted = [...myAssessments.value].sort((a, b) => new Date(a.date).getTime() - new Date(b.date).getTime())
  // Take last 10 assessments for better visibility
  const recent = sorted.slice(-10)

  return {
    labels: recent.map(a => new Date(a.date).toLocaleDateString()),
    datasets: [
      {
        label: t('assessment.score'),
        data: recent.map(a => a.score),
        borderColor: 'hsl(var(--primary))',
        backgroundColor: 'hsl(var(--primary) / 0.1)',
        fill: true,
        tension: 0.4
      }
    ]
  }
})
</script>

<template>
  <div class="space-y-6">
    <div class="flex items-center justify-between">
      <div>
        <h2 class="text-3xl font-bold tracking-tight">{{ t('dashboard.title') }}</h2>
        <p class="text-muted-foreground">{{ t('dashboard.welcome') }}</p>
      </div>
      <div class="flex gap-2">
        <Button @click="navigateToChat">
          <MessageSquarePlus class="mr-2 h-4 w-4" />
          {{ t('dashboard.newChat') }}
        </Button>
        <Button variant="outline" @click="navigateToAssessments">
          <FileText class="mr-2 h-4 w-4" />
          {{ t('dashboard.browseAssessments') }}
        </Button>
      </div>
    </div>

    <!-- Stats Overview -->
    <div class="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
      <Card>
        <CardHeader class="flex flex-row items-center justify-between space-y-0 pb-2">
          <CardTitle class="text-sm font-medium">{{ t('dashboard.activeTasks') }}</CardTitle>
          <ClipboardList class="h-4 w-4 text-muted-foreground" />
        </CardHeader>
        <CardContent>
          <div class="text-2xl font-bold">{{ tasks.length }}</div>
        </CardContent>
      </Card>
      <Card>
        <CardHeader class="flex flex-row items-center justify-between space-y-0 pb-2">
          <CardTitle class="text-sm font-medium">{{ t('dashboard.completedAssessments') }}</CardTitle>
          <CheckCircle class="h-4 w-4 text-muted-foreground" />
        </CardHeader>
        <CardContent>
          <div class="text-2xl font-bold">{{ myAssessments.length }}</div>
        </CardContent>
      </Card>
      <Card>
        <CardHeader class="flex flex-row items-center justify-between space-y-0 pb-2">
          <CardTitle class="text-sm font-medium">{{ t('dashboard.averageScore') }}</CardTitle>
          <TrendingUp class="h-4 w-4 text-muted-foreground" />
        </CardHeader>
        <CardContent>
          <div class="text-2xl font-bold">{{ averageScore }}</div>
        </CardContent>
      </Card>
    </div>

    <div class="grid gap-6 lg:grid-cols-3">
      <!-- Progress Chart -->
      <Card class="lg:col-span-2">
        <CardHeader>
          <CardTitle>{{ t('dashboard.myProgress') }}</CardTitle>
        </CardHeader>
        <CardContent class="pl-2">
          <div v-if="myAssessments.length > 0" class="h-[350px]">
            <OverviewChart :data="chartData" />
          </div>
          <div v-else class="h-[350px] flex items-center justify-center text-muted-foreground">
            {{ t('dashboard.noHistory') }}
          </div>
        </CardContent>
      </Card>

      <!-- Pending Tasks & Recent History -->
      <div class="space-y-6">
        <Card class="flex flex-col">
          <CardHeader>
            <CardTitle class="flex items-center gap-2">
              <ClipboardList class="h-5 w-5" />
              {{ t('dashboard.activeTasks') }}
            </CardTitle>
          </CardHeader>
          <CardContent class="flex-1 p-0">
            <ScrollArea class="h-[150px]">
              <div class="p-6 pt-0">
                <div v-if="tasks.length === 0" class="text-center py-6 text-muted-foreground">
                  {{ t('dashboard.noTasks') }}
                </div>
                <div v-else class="space-y-4">
                  <div v-for="task in tasks" :key="task.id"
                    class="flex items-center justify-between border-b pb-2 last:border-0">
                    <div class="min-w-0 flex-1 mr-2">
                      <p class="font-medium truncate">{{ task.questionnaire?.title }}</p>
                      <p class="text-xs text-muted-foreground">{{ t('tasks.dueDate') }}: {{ task.dueDate ? new
                        Date(task.dueDate).toLocaleDateString() : t('tasks.noDueDate') }}</p>
                    </div>
                    <Button v-if="task.status !== 'Completed'" size="sm" variant="secondary"
                      @click="navigateToAssessment(task.questionnaireId)">
                      {{ t('assessment.start') }}
                    </Button>
                  </div>
                </div>
              </div>
            </ScrollArea>
          </CardContent>
        </Card>

        <Card class="flex flex-col">
          <CardHeader>
            <CardTitle class="flex items-center gap-2">
              <Clock class="h-5 w-5" />
              {{ t('dashboard.recentActivity') }}
            </CardTitle>
          </CardHeader>
          <CardContent class="flex-1 p-0">
            <ScrollArea class="h-[150px]">
              <div class="p-6 pt-0">
                <div v-if="myAssessments.length === 0" class="text-center py-6 text-muted-foreground">
                  {{ t('dashboard.noHistory') }}
                </div>
                <div v-else class="space-y-4">
                  <div v-for="assessment in myAssessments.slice(0, 10)" :key="assessment.id"
                    class="flex items-center justify-between border-b pb-2 last:border-0">
                    <div class="min-w-0 flex-1 mr-2">
                      <p class="font-medium truncate">{{ assessment.questionnaire?.title }}</p>
                      <p class="text-xs text-muted-foreground">{{ new Date(assessment.date).toLocaleDateString() }}</p>
                    </div>
                    <div class="text-right shrink-0">
                      <p class="font-bold">{{ assessment.score }}</p>
                    </div>
                  </div>
                </div>
              </div>
            </ScrollArea>
          </CardContent>
        </Card>
      </div>
    </div>
  </div>
</template>
