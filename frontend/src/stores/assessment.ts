import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import api from '@/lib/api'

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

export interface Questionnaire {
  id: number
  title: string
  description: string
  questions: Question[]
}

export interface AssessmentResult {
  id: number
  questionnaire: { title: string }
  date: string
  score: number
  result?: string
}

export const useAssessmentStore = defineStore('assessment', () => {
  const assessments = ref<Questionnaire[]>([])
  const myAssessments = ref<AssessmentResult[]>([])
  const currentAssessment = ref<Questionnaire | null>(null)
  const answers = ref<Record<number, any>>({})
  const loading = ref(false)
  const isSubmitting = ref(false)
  const error = ref<string | null>(null)

  async function fetchAssessments() {
    loading.value = true
    try {
      const response = await api.get('/questionnaire')
      assessments.value = response.data.map((q: any) => {
        let questions: Question[] = []
        try {
          questions = typeof q.questionsJson === 'string' ? JSON.parse(q.questionsJson) : q.questionsJson
        } catch (e) { questions = [] }

        return {
          id: q.id,
          title: q.title,
          description: q.description,
          questions: questions
        }
      })
    } catch (err) {
      console.error(err)
      error.value = "Failed to load assessments"
    } finally {
      loading.value = false
    }
  }

  async function fetchMyAssessments() {
    loading.value = true
    try {
      const response = await api.get<AssessmentResult[]>('/assessments/my')
      myAssessments.value = response.data
    } catch (err) {
      console.error(err)
    } finally {
      loading.value = false
    }
  }

  async function fetchQuestionnaire(id: number) {
    loading.value = true
    try {
      const response = await api.get(`/questionnaire/${id}`)
      const data = response.data
      let questions: Question[] = []
      try {
        questions = typeof data.questionsJson === 'string' ? JSON.parse(data.questionsJson) : data.questionsJson
      } catch (e) { questions = [] }

      currentAssessment.value = {
        id: data.id,
        title: data.title,
        description: data.description,
        questions: questions
      }
      answers.value = {}
    } catch (err) {
      console.error(err)
      error.value = "Failed to load assessment"
    } finally {
      loading.value = false
    }
  }

  async function startAssessment(id: number) {
    let assessment = assessments.value.find(a => a.id === id)
    if (!assessment) {
      await fetchQuestionnaire(id)
      return
    }
    currentAssessment.value = assessment
    answers.value = {}
  }

  function exitAssessment() {
    currentAssessment.value = null
    answers.value = {}
  }

  function setAnswer(questionId: number, value: any) {
    answers.value[questionId] = value
  }

  const progress = computed(() => {
    if (!currentAssessment.value) return 0
    const total = currentAssessment.value.questions.length
    if (total === 0) return 0
    const answered = Object.keys(answers.value).length
    return (answered / total) * 100
  })

  async function submitAssessment(taskId?: number) {
    if (!currentAssessment.value) return false

    isSubmitting.value = true
    try {
      const formattedAnswers: Record<string, any> = {}
      for (const [key, value] of Object.entries(answers.value)) {
        formattedAnswers[key.toString()] = value
      }

      await api.post('/assessments', {
        questionnaireId: currentAssessment.value.id,
        taskId,
        answers: formattedAnswers
      })

      exitAssessment()
      return true
    } catch (err: any) {
      console.error('Failed to submit assessment', err)
      error.value = 'Failed to submit assessment.'
      return false
    } finally {
      isSubmitting.value = false
    }
  }

  return {
    assessments,
    currentAssessment,
    answers,
    myAssessments,
    loading,
    isSubmitting,
    error,
    progress,
    fetchAssessments,
    fetchMyAssessments,
    startAssessment,
    exitAssessment,
    setAnswer,
    submitAssessment
  }
})
