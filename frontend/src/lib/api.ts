import axios from 'axios'
import { useAuthStore } from '@/stores/auth'

const isDev = import.meta.env.DEV
const baseURL = isDev ? '/api' : 'http://localhost:17597/api'

const api = axios.create({
  baseURL,
  timeout: 180000, // 3 minutes timeout for AI operations
  headers: {
    'Content-Type': 'application/json'
  }
})

api.interceptors.request.use(config => {
  const authStore = useAuthStore()
  if (authStore.token) {
    config.headers.Authorization = `Bearer ${authStore.token}`
  }
  return config
})

api.interceptors.response.use(
  response => response,
  error => {
    if (error.response?.status === 401) {
      const authStore = useAuthStore()
      authStore.logout()
    }
    return Promise.reject(error)
  }
)

export default api
