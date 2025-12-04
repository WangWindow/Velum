import { defineStore } from 'pinia'
import { ref } from 'vue'
import api from '@/lib/api'

export interface User {
  id: number
  username: string
  email?: string
  fullName?: string
  role: 'User' | 'Admin'
  createdAt: string
  lastLogin?: string
}

export const useUsersStore = defineStore('users', () => {
  const users = ref<User[]>([])
  const isLoading = ref(false)
  const error = ref<string | null>(null)

  async function fetchUsers() {
    isLoading.value = true
    error.value = null
    try {
      const response = await api.get<User[]>('/users')
      users.value = response.data
    } catch (err: any) {
      console.error('Failed to fetch users:', err)
      error.value = 'Failed to fetch users.'
    } finally {
      isLoading.value = false
    }
  }

  async function createUser(user: Partial<User> & { password?: string }) {
    isLoading.value = true
    try {
      const response = await api.post<User>('/users', user)
      users.value.push(response.data)
      return true
    } catch (err: any) {
      console.error('Failed to create user:', err)
      error.value = err.response?.data || 'Failed to create user.'
      return false
    } finally {
      isLoading.value = false
    }
  }

  async function updateUser(id: number, user: Partial<User> & { password?: string }) {
    isLoading.value = true
    try {
      await api.put(`/users/${id}`, user)
      const index = users.value.findIndex(u => u.id === id)
      if (index !== -1) {
        // Refresh users list to get updated data or manually update
        await fetchUsers()
      }
      return true
    } catch (err: any) {
      console.error('Failed to update user:', err)
      error.value = err.response?.data || 'Failed to update user.'
      return false
    } finally {
      isLoading.value = false
    }
  }

  async function deleteUser(id: number) {
    isLoading.value = true
    try {
      await api.delete(`/users/${id}`)
      users.value = users.value.filter(u => u.id !== id)
      return true
    } catch (err: any) {
      console.error('Failed to delete user:', err)
      error.value = err.response?.data || 'Failed to delete user.'
      return false
    } finally {
      isLoading.value = false
    }
  }

  return {
    users,
    isLoading,
    error,
    fetchUsers,
    createUser,
    updateUser,
    deleteUser
  }
})
