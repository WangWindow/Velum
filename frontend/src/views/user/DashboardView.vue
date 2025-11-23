<script setup lang="ts">
import { onMounted } from 'vue'
import { useI18n } from 'vue-i18n'
import { useTasksStore } from '@/stores/tasks'
import { useAssessmentStore } from '@/stores/assessment'
import { storeToRefs } from 'pinia'
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Badge } from '@/components/ui/badge'
import { ClipboardList, Clock } from 'lucide-vue-next'
import { useRouter } from 'vue-router'

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

const navigateToAssessment = () => {
  router.push('/user/assessment')
}
</script>

<template>
  <div class="space-y-6">
    <div>
      <h1 class="text-3xl font-bold">{{ t('dashboard.title') }}</h1>
      <p class="text-muted-foreground">{{ t('dashboard.welcome') }}</p>
    </div>

    <div class="grid gap-6 md:grid-cols-2">
      <!-- Pending Tasks -->
      <Card>
        <CardHeader>
          <CardTitle class="flex items-center gap-2">
            <ClipboardList class="h-5 w-5" />
            {{ t('dashboard.activeTasks') }}
          </CardTitle>
          <CardDescription>{{ t('dashboard.tasksDescription') }}</CardDescription>
        </CardHeader>
        <CardContent>
          <div v-if="tasks.length === 0" class="text-center py-6 text-muted-foreground">
            {{ t('dashboard.noTasks') }}
          </div>
          <div v-else class="space-y-4">
            <div v-for="task in tasks" :key="task.id"
              class="flex items-center justify-between border-b pb-2 last:border-0">
              <div>
                <p class="font-medium">{{ task.questionnaire?.title }}</p>
                <p class="text-sm text-muted-foreground">Due: {{ task.dueDate ? new
                  Date(task.dueDate).toLocaleDateString() : 'No due date' }}</p>
              </div>
              <div class="flex items-center gap-2">
                <Badge :variant="task.status === 'Completed' ? 'default' : 'secondary'">
                  {{ task.status }}
                </Badge>
                <Button v-if="task.status !== 'Completed'" size="sm" @click="navigateToAssessment">
                  Start
                </Button>
              </div>
            </div>
          </div>
        </CardContent>
      </Card>

      <!-- Recent History -->
      <Card>
        <CardHeader>
          <CardTitle class="flex items-center gap-2">
            <Clock class="h-5 w-5" />
            {{ t('dashboard.recentActivity') }}
          </CardTitle>
          <CardDescription>Your recent assessment history.</CardDescription>
        </CardHeader>
        <CardContent>
          <div v-if="myAssessments.length === 0" class="text-center py-6 text-muted-foreground">
            No history available.
          </div>
          <div v-else class="space-y-4">
            <div v-for="assessment in myAssessments" :key="assessment.id"
              class="flex items-center justify-between border-b pb-2 last:border-0">
              <div>
                <p class="font-medium">{{ assessment.questionnaire?.title }}</p>
                <p class="text-sm text-muted-foreground">{{ new Date(assessment.date).toLocaleDateString() }}</p>
              </div>
              <div class="text-right">
                <p class="font-bold text-lg">{{ assessment.score }}</p>
                <p class="text-xs text-muted-foreground">Score</p>
              </div>
            </div>
          </div>
        </CardContent>
      </Card>
    </div>
  </div>
</template>
