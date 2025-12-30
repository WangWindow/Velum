<script setup lang="ts">
import { useAssessmentStore } from '@/stores/assessment'
import { useTasksStore } from '@/stores/tasks'
import { Button } from '@/components/ui/button'
import { Card, CardContent, CardFooter, CardHeader, CardTitle } from '@/components/ui/card'
import { Progress } from '@/components/ui/progress'
import { RadioGroup, RadioGroupItem } from '@/components/ui/radio-group'
import { Textarea } from '@/components/ui/textarea'
import { Checkbox } from '@/components/ui/checkbox'
import { Label } from '@/components/ui/label'
import { ArrowLeft, CheckCircle2, PlayCircle } from 'lucide-vue-next'
import { computed, onMounted, ref } from 'vue'
import { useI18n } from 'vue-i18n'
import { useToast } from '@/components/ui/toast/use-toast'
import { useRoute } from 'vue-router'

const { t } = useI18n()
const { toast } = useToast()
const route = useRoute()
const store = useAssessmentStore()
const tasksStore = useTasksStore()

const { tasks } = tasksStore
const pendingTasks = computed(() => tasks.filter(t => t.status !== 'Completed'))
import type { Task } from '@/stores/tasks'
const currentTask = ref<Task | null>(null)


onMounted(async () => {
  await tasksStore.fetchMyTasks()
  // 若通过路由参数 id 进入，自动进入评估
  const id = route.query.id
  if (id) {
    const task = tasks.find(t => t.id === Number(id))
    if (task) handleStart(task)
  }
})

const handleStart = (task: Task) => {
  currentTask.value = task
  store.startAssessment(task.questionnaireId)
}

const handleSubmit = async () => {
  const success = await store.submitAssessment(currentTask.value?.id)
  if (success) {
    toast({
      title: t('assessment.submitted'),
      description: t('assessment.submittedDesc'),
    })
    await tasksStore.fetchMyTasks()
    currentTask.value = null
  }
}

const isComplete = computed(() => {
  return store.progress === 100
})

const toggleSelection = (questionId: number, value: string) => {
  const current = store.answers[questionId] || []
  const currentArr = Array.isArray(current) ? current : []

  const index = currentArr.indexOf(value)
  if (index === -1) {
    store.setAnswer(questionId, [...currentArr, value])
  } else {
    const newArr = [...currentArr]
    newArr.splice(index, 1)
    store.setAnswer(questionId, newArr)
  }
}
</script>

<template>
  <div class="h-full">
    <!-- List View -->
    <div v-if="!store.currentAssessment" class="space-y-6">
      <div class="flex items-center justify-between">
        <div>
          <h2 class="text-3xl font-bold tracking-tight">{{ t('assessment.title') }}</h2>
          <p class="text-muted-foreground">{{ t('assessment.description') }}</p>
        </div>
      </div>

      <div class="grid gap-6 md:grid-cols-2 lg:grid-cols-3">
        <Card v-for="task in pendingTasks" :key="task.id" class="flex flex-col">
          <CardHeader>
            <CardTitle>{{ task.questionnaire?.title }}</CardTitle>
          </CardHeader>
          <CardContent class="flex-1">
            <div class="flex items-center text-sm text-muted-foreground">
              <span class="flex items-center gap-1">
                <CheckCircle2 class="h-4 w-4" />
                {{ t('assessment.questions') }}
              </span>
              <span class="ml-2">{{ t('tasks.dueDate') }}: {{ task.dueDate ? new Date(task.dueDate).toLocaleDateString()
                : t('tasks.noDueDate') }}</span>
            </div>
          </CardContent>
          <CardFooter>
            <Button class="w-full" @click="handleStart(task)">
              <PlayCircle class="mr-2 h-4 w-4" />
              {{ t('assessment.start') }}
            </Button>
          </CardFooter>
        </Card>
        <div v-if="pendingTasks.length === 0" class="col-span-full text-center text-muted-foreground py-8">
          {{ t('dashboard.noTasks') }}
        </div>
      </div>
    </div>

    <!-- Taking Assessment View -->
    <div v-else class="max-w-3xl mx-auto space-y-8 pb-10">
      <div class="flex items-center gap-4">
        <Button variant="ghost" size="icon" @click="() => { store.exitAssessment(); currentTask = null }">
          <ArrowLeft class="h-4 w-4" />
        </Button>
        <div>
          <h2 class="text-2xl font-bold">{{ store.currentAssessment.title }}</h2>
          <p class="text-sm text-muted-foreground">{{ t('assessment.instruction') }}</p>
        </div>
      </div>

      <div class="sticky top-0 z-10 bg-background/95 backdrop-blur py-4 border-b">
        <div class="flex justify-between text-sm mb-2">
          <span>{{ t('assessment.progress') }}</span>
          <span>{{ Math.round(store.progress) }}%</span>
        </div>
        <Progress :model-value="store.progress" />
      </div>

      <div class="space-y-8">
        <Card v-for="(question, index) in store.currentAssessment.questions" :key="question.id">
          <CardHeader>
            <CardTitle class="text-lg">
              <span class="mr-2 text-muted-foreground">{{ index + 1 }}.</span>
              {{ question.text }}
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div v-if="question.type === 'Text'">
              <Textarea :model-value="store.answers[question.id]"
                @update:model-value="(val) => store.setAnswer(question.id, val)"
                :placeholder="t('scales.placeholder') || 'Type your answer here...'" />
            </div>
            <div v-else-if="question.type === 'MultipleChoice'">
              <div v-for="(option, optIdx) in question.options" :key="optIdx" class="flex items-center space-x-2 py-2">
                <Checkbox :id="`q${question.id}-opt${optIdx}`"
                  :checked="Array.isArray(store.answers[question.id]) && store.answers[question.id].includes(option.score.toString())"
                  @update:checked="() => toggleSelection(question.id, option.score.toString())" />
                <Label :for="`q${question.id}-opt${optIdx}`" class="font-normal cursor-pointer">
                  {{ option.text }}
                </Label>
              </div>
            </div>
            <div v-else>
              <RadioGroup :model-value="store.answers[question.id]?.toString()"
                @update:model-value="(val: any) => store.setAnswer(question.id, val)">
                <div v-for="(option, optIdx) in question.options" :key="optIdx"
                  class="flex items-center space-x-2 space-y-1">
                  <RadioGroupItem :id="`q${question.id}-opt${optIdx}`" :value="option.score.toString()" />
                  <Label :for="`q${question.id}-opt${optIdx}`" class="font-normal cursor-pointer">
                    {{ option.text }}
                  </Label>
                </div>
              </RadioGroup>
            </div>
          </CardContent>
        </Card>
      </div>

      <div class="flex justify-end pt-4">
        <Button size="lg" @click="handleSubmit" :disabled="!isComplete || store.isSubmitting">
          {{ store.isSubmitting ? t('assessment.submitting') : t('assessment.submit') }}
        </Button>
      </div>
    </div>
  </div>
</template>
