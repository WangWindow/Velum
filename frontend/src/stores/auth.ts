import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { useRouter } from 'vue-router'
import api from '@/lib/api'

export interface User {
  id?: number
  username: string
  fullName?: string
  email?: string
  avatar?: string
}

export interface AuthState {
  token: string | null
  role: 'user' | 'admin' | null
  user: User | null
}

export const useAuthStore = defineStore('auth', () => {
  const token = ref<string | null>(localStorage.getItem('token'))
  const role = ref<'user' | 'admin' | null>(localStorage.getItem('role') as 'user' | 'admin' | null)
  const user = ref<User | null>(JSON.parse(localStorage.getItem('user') || 'null'))

  const router = useRouter()

  const isAuthenticated = computed(() => !!token.value)
  const isAdmin = computed(() => role.value === 'admin')

  async function login(username: string, password: string) {
    try {
      const response = await api.post('/auth/login', { username, password })
      const data = response.data

      token.value = data.token
      role.value = data.role?.toLowerCase()
      user.value = data.user

      localStorage.setItem('token', data.token)
      localStorage.setItem('role', data.role?.toLowerCase())
      localStorage.setItem('user', JSON.stringify(data.user))

      // Redirect based on role
      if (role.value === 'admin') {
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

  async function updateProfile(updatedUser: Partial<User>) {
    if (!user.value || !user.value.id) return false
    try {
      // We need to send the full user object or at least what the backend expects.
      // The backend UpdateUser expects a User object and checks ID.
      // It updates Email, FullName, Avatar, Role.
      // We should probably fetch the latest user data first or merge with current.

      const payload = {
        ...user.value,
        ...updatedUser,
        id: user.value.id
      }

      await api.put(`/users/${user.value.id}`, payload)

      // Update local state
      user.value = { ...user.value, ...updatedUser }
      localStorage.setItem('user', JSON.stringify(user.value))
      return true
    } catch (error) {
      console.error('Update profile failed:', error)
      return false
    }
  }

  return {
    token,
    role,
    user,
    isAuthenticated,
    isAdmin,
    login,
    register,
    logout,
    updateProfile
  }
})
