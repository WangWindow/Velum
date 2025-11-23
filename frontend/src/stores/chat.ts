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

export const useChatStore = defineStore('chat', () => {
  const messages = ref<Message[]>([])
  const config = ref<ChatConfig>({
    mode: 'default'
  })
  const isLoading = ref(false)

  async function fetchHistory() {
    try {
      const response = await api.get<any[]>('/chat/history')
      // Transform backend messages to frontend format if needed
      // Backend: { id, userId, role, content, timestamp }
      messages.value = response.data.map((m: any) => ({
        id: m.id.toString(),
        role: m.role.toLowerCase(),
        content: m.content,
        timestamp: new Date(m.timestamp).getTime()
      }))
    } catch (error) {
      console.error('Failed to fetch chat history:', error)
    }
  }

  async function sendMessage(content: string) {
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
        const response = await api.post('/chat/send', { message: content })
        const aiMessage = response.data

        messages.value.push({
          id: aiMessage.id.toString(),
          role: 'assistant',
          content: aiMessage.content,
          timestamp: new Date(aiMessage.timestamp).getTime()
        })
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

  return {
    messages,
    config,
    isLoading,
    fetchHistory,
    sendMessage
  }
})
