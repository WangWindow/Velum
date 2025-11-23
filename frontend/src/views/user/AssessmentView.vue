<script setup lang="ts">
import { useAssessmentStore } from '@/stores/assessment'
import { Button } from '@/components/ui/button'
import { Card, CardContent, CardDescription, CardFooter, CardHeader, CardTitle } from '@/components/ui/card'
import { Progress } from '@/components/ui/progress'
import { RadioGroup, RadioGroupItem } from '@/components/ui/radio-group'
import { Label } from '@/components/ui/label'
import { ArrowLeft, CheckCircle2, PlayCircle } from 'lucide-vue-next'
import { computed, onMounted } from 'vue'
import { useI18n } from 'vue-i18n'
import { useToast } from '@/components/ui/toast/use-toast'

const { t } = useI18n()
const { toast } = useToast()
const store = useAssessmentStore()

onMounted(() => {
  store.fetchAssessments()
})

const handleStart = (id: number) => {
  store.startAssessment(id)
}

const handleSubmit = async () => {
  const success = await store.submitAssessment()
  if (success) {
    toast({
      title: t('assessment.submitted'),
      description: t('assessment.submittedDesc'),
    })
  }
}

const isComplete = computed(() => {
  return store.progress === 100
})
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
        <Card v-for="assessment in store.assessments" :key="assessment.id" class="flex flex-col">
          <CardHeader>
            <CardTitle>{{ assessment.title }}</CardTitle>
            <CardDescription>{{ assessment.description }}</CardDescription>
          </CardHeader>
          <CardContent class="flex-1">
            <div class="flex items-center text-sm text-muted-foreground">
              <span class="flex items-center gap-1">
                <CheckCircle2 class="h-4 w-4" />
                {{ assessment.questions.length }} {{ t('assessment.questions') }}
              </span>
            </div>
          </CardContent>
          <CardFooter>
            <Button class="w-full" @click="handleStart(assessment.id)">
              <PlayCircle class="mr-2 h-4 w-4" />
              {{ t('assessment.start') }}
            </Button>
          </CardFooter>
        </Card>
      </div>
    </div>

    <!-- Taking Assessment View -->
    <div v-else class="max-w-3xl mx-auto space-y-8 pb-10">
      <div class="flex items-center gap-4">
        <Button variant="ghost" size="icon" @click="store.exitAssessment">
          <ArrowLeft class="h-4 w-4" />
        </Button>
        <div>
          <h2 class="text-2xl font-bold">{{ store.currentAssessment.title }}</h2>
          <p class="text-sm text-muted-foreground">Please answer all questions honestly.</p>
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
