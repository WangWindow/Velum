import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { useRouter } from 'vue-router'
import api from '@/lib/api'

export interface User {
  username: string
  fullName?: string
  email?: string
}

export interface AuthState {
  token: string | null
  role: 'User' | 'Admin' | null
  user: User | null
}

export const useAuthStore = defineStore('auth', () => {
  const token = ref<string | null>(localStorage.getItem('token'))
  const role = ref<'User' | 'Admin' | null>(localStorage.getItem('role') as 'User' | 'Admin' | null)
  const user = ref<User | null>(JSON.parse(localStorage.getItem('user') || 'null'))

  const router = useRouter()

  const isAuthenticated = computed(() => !!token.value)
  const isAdmin = computed(() => role.value === 'Admin')

  async function login(username: string, password: string) {
    try {
      const response = await api.post('/auth/login', { username, password })
      const data = response.data

      token.value = data.token
      role.value = data.role
      user.value = data.user

      localStorage.setItem('token', data.token)
      localStorage.setItem('role', data.role)
      localStorage.setItem('user', JSON.stringify(data.user))

      // Redirect based on role
      if (role.value === 'Admin') {
        router.push('/admin/dashboard')
      } else {
        router.push('/user/dashboard')
      }
      return true
    } catch (error) {
      console.error('Login failed:', error)
      return false
    }
  }

  async function register(username: string, password: string, email?: string, fullName?: string) {
    try {
      await api.post('/auth/register', { username, password, email, fullName })
      return { success: true }
    } catch (error: any) {
      console.error('Registration failed:', error)
      const msg = error.response?.data || 'Registration failed'
      return { success: false, error: typeof msg === 'string' ? msg : JSON.stringify(msg) }
    }
  }

  function logout() {
    token.value = null
    role.value = null
    user.value = null
    localStorage.removeItem('token')
    localStorage.removeItem('role')
    localStorage.removeItem('user')
    router.push('/login')
  }

  return {
    token,
    role,
    user,
    isAuthenticated,
    isAdmin,
    login,
    register,
    logout
  }
})
