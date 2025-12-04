import { defineStore } from 'pinia'
import { ref } from 'vue'
import axios from 'axios'
import api from '@/lib/api'

export interface Message {
  id: string
  role: 'user' | 'assistant'
  content: string
  timestamp: number
}

export interface ChatConfig {
  mode: 'default' | 'custom'
  customUrl?: string
  customApiKey?: string
  customModel?: string
  customPrompt?: string
}

export interface ChatSession {
  id: number
  title: string
  updatedAt: string
}

export const useChatStore = defineStore('chat', () => {
  const messages = ref<Message[]>([])
  const sessions = ref<ChatSession[]>([])
  const currentSessionId = ref<number | null>(null)
  const config = ref<ChatConfig>({
    mode: 'default'
  })
  const isLoading = ref(false)

  async function fetchSessions() {
    try {
      const response = await api.get<ChatSession[]>('/chat/sessions')
      sessions.value = response.data
    } catch (error) {
      console.error('Failed to fetch sessions:', error)
    }
  }

  async function createSession(title?: string) {
    try {
      const response = await api.post<ChatSession>('/chat/sessions', { title })
      sessions.value.unshift(response.data)
      currentSessionId.value = response.data.id
      messages.value = [] // Clear messages for new session
      return response.data
    } catch (error) {
      console.error('Failed to create session:', error)
    }
  }

  async function selectSession(sessionId: number) {
    if (currentSessionId.value === sessionId) return

    currentSessionId.value = sessionId
    isLoading.value = true
    try {
      const response = await api.get<any>(`/chat/sessions/${sessionId}`)
      // response.data is the session object with messages included
      const sessionData = response.data

      messages.value = sessionData.messages.map((m: any) => ({
        id: m.id.toString(),
        role: m.role.toLowerCase(),
        content: m.content,
        timestamp: new Date(m.timestamp).getTime()
      }))
    } catch (error) {
      console.error('Failed to fetch session messages:', error)
    } finally {
      isLoading.value = false
    }
  }

  async function deleteSession(sessionId: number) {
    try {
      await api.delete(`/chat/sessions/${sessionId}`)
      sessions.value = sessions.value.filter(s => s.id !== sessionId)
      if (currentSessionId.value === sessionId) {
        currentSessionId.value = null
        messages.value = []
      }
    } catch (error) {
      console.error('Failed to delete session:', error)
    }
  }

  async function fetchHistory() {
    // Legacy support or load most recent session?
    // Let's load sessions and select the first one if available
    await fetchSessions()
    if (sessions.value.length > 0 && sessions.value[0]) {
      await selectSession(sessions.value[0].id)
    } else {
      // Create a new session automatically if none exist?
      // Or just leave it empty and let user create one?
      // Let's create one for better UX
      await createSession()
    }
  }

  async function sendMessage(content: string) {
    // Ensure we have a session
    if (!currentSessionId.value && config.value.mode === 'default') {
      await createSession(content.substring(0, 20))
    }

    // Add user message immediately for UI responsiveness
    const tempId = Date.now().toString()
    messages.value.push({
      id: tempId,
      role: 'user',
      content,
      timestamp: Date.now()
    })

    isLoading.value = true

    try {
      if (config.value.mode === 'custom' && config.value.customUrl) {
        // Client-side call for custom mode
        const response = await axios.post(
          config.value.customUrl,
          {
            model: config.value.customModel || 'gpt-3.5-turbo',
            messages: [
              { role: 'system', content: config.value.customPrompt || 'You are a helpful assistant.' },
              ...messages.value.map(m => ({ role: m.role, content: m.content }))
            ]
          },
          {
            headers: {
              'Authorization': `Bearer ${config.value.customApiKey}`,
              'Content-Type': 'application/json'
            }
          }
        )

        const aiContent = response.data.choices[0].message.content
        messages.value.push({
          id: Date.now().toString(),
          role: 'assistant',
          content: aiContent,
          timestamp: Date.now()
        })

      } else {
        // Default mode: use backend
        const response = await api.post('/chat/send', {
          message: content,
          sessionId: currentSessionId.value
        })
        const aiMessage = response.data

        messages.value.push({
          id: aiMessage.id.toString(),
          role: 'assistant',
          content: aiMessage.content,
          timestamp: new Date(aiMessage.timestamp).getTime()
        })

        // Refresh sessions to update title/timestamp if needed
        // await fetchSessions() // Might be too heavy?
      }
    } catch (error) {
      console.error('Failed to send message:', error)
      messages.value.push({
        id: Date.now().toString(),
        role: 'assistant',
        content: 'Sorry, I encountered an error processing your request.',
        timestamp: Date.now()
      })
    } finally {
      isLoading.value = false
    }
  }

  async function clearHistory() {
    // Clear current session messages?
    // Or clear all history?
    // Let's make it clear all sessions for now as per previous implementation
    try {
      await api.delete('/chat/history')
      messages.value = []
      sessions.value = []
      currentSessionId.value = null
    } catch (error) {
      console.error('Failed to clear chat history:', error)
    }
  }

  return {
    messages,
    sessions,
    currentSessionId,
    config,
    isLoading,
    fetchHistory,
    fetchSessions,
    createSession,
    selectSession,
    deleteSession,
    sendMessage,
    clearHistory
  }
})
