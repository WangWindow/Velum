import { defineStore } from 'pinia'
import { ref } from 'vue'
import api from '@/lib/api'

export interface Task {
  id: number
  userId: number
  user?: { username: string }
  questionnaireId: number
  questionnaire?: { title: string }
  status: string
  assignedAt: string
  dueDate?: string
}

export interface UserSummary {
  id: number
  username: string
}

export interface QuestionnaireSummary {
  id: number
  title: string
}

export const useTasksStore = defineStore('tasks', () => {
  const tasks = ref<Task[]>([])
  const users = ref<UserSummary[]>([])
  const questionnaires = ref<QuestionnaireSummary[]>([])
  const isLoading = ref(false)

  async function fetchTasks() {
    isLoading.value = true
    try {
      const response = await api.get<Task[]>('/tasks')
      tasks.value = response.data
    } catch (error) {
      console.error('Failed to fetch tasks:', error)
    } finally {
      isLoading.value = false
    }
  }

  async function fetchUsers() {
    try {
      const response = await api.get<UserSummary[]>('/users')
      users.value = response.data
    } catch (error) {
      console.error('Failed to fetch users:', error)
    }
  }

  async function fetchMyTasks() {
    isLoading.value = true
    try {
      const response = await api.get<Task[]>('/tasks/my')
      tasks.value = response.data
    } catch (error) {
      console.error('Failed to fetch my tasks:', error)
    } finally {
      isLoading.value = false
    }
  }

  async function fetchQuestionnaires() {
    try {
      const response = await api.get<QuestionnaireSummary[]>('/questionnaire')
      questionnaires.value = response.data
    } catch (error) {
      console.error('Failed to fetch questionnaires:', error)
    }
  }

  async function assignTask(userId: number, questionnaireId: number) {
    isLoading.value = true
    try {
      const response = await api.post('/tasks', { userId, questionnaireId })
      tasks.value.push(response.data)
    } catch (error) {
      console.error('Failed to assign task:', error)
    } finally {
      isLoading.value = false
    }
  }

  return {
    tasks,
    users,
    questionnaires,
    isLoading,
    fetchTasks,
    fetchUsers,
    fetchMyTasks,
    fetchQuestionnaires,
    assignTask
  }
})
