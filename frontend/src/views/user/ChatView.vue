<script setup lang="ts">
import { ref, nextTick, watch, onMounted } from 'vue'
import { useChatStore } from '@/stores/chat'
import { Button } from '@/components/ui/button'
import { Textarea } from '@/components/ui/textarea'
import { ScrollArea } from '@/components/ui/scroll-area'
import { Avatar, AvatarFallback } from '@/components/ui/avatar'
import { Sheet, SheetContent, SheetDescription, SheetHeader, SheetTitle, SheetTrigger, SheetFooter, SheetClose } from '@/components/ui/sheet'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Send, Settings, User, Bot, Plus, PanelLeftClose, PanelLeftOpen, MessageSquare } from 'lucide-vue-next'
import { useI18n } from 'vue-i18n'

const { t } = useI18n()
const chatStore = useChatStore()
const inputMessage = ref('')
const scrollAreaRef = ref<any>(null)
const isSidebarOpen = ref(true)

onMounted(() => {
  chatStore.fetchHistory()
})

const toggleSidebar = () => {
  isSidebarOpen.value = !isSidebarOpen.value
}

const scrollToBottom = async () => {
  await nextTick()
  if (scrollAreaRef.value) {
    const el = scrollAreaRef.value.$el || scrollAreaRef.value
    const viewport = el?.querySelector?.('[data-radix-scroll-area-viewport]')
    if (viewport) {
      viewport.scrollTop = viewport.scrollHeight
    }
  }
}

const handleSendMessage = async () => {
  if (!inputMessage.value.trim() || chatStore.isLoading) return

  const content = inputMessage.value
  inputMessage.value = ''

  await chatStore.sendMessage(content)
}

// Watch for new messages to scroll to bottom
watch(() => chatStore.messages.length, scrollToBottom)

// Config state
const tempConfig = ref({ ...chatStore.config })

const saveConfig = () => {
  chatStore.config = { ...tempConfig.value }
}
</script>

<template>
  <div class="flex h-[calc(100vh-8rem)] flex-row gap-4 overflow-hidden">
    <!-- Sidebar (History) -->
    <div class="flex flex-col gap-2 border-r transition-all duration-300 ease-in-out overflow-hidden"
      :class="[isSidebarOpen ? 'w-64 pr-4 opacity-100' : 'w-0 pr-0 opacity-0 border-r-0']">
      <Button variant="outline" class="justify-start gap-2 w-full mb-2" @click="chatStore.createSession()">
        <Plus class="h-4 w-4" />
        {{ t('chat.newChat') }}
      </Button>

      <div class="flex-1 overflow-auto py-2">
        <div class="text-xs font-medium text-muted-foreground mb-2 px-2">{{ t('chat.recent') }}</div>
        <Button v-for="session in chatStore.sessions" :key="session.id" variant="ghost"
          class="w-full justify-start gap-2 px-2 font-normal text-muted-foreground hover:text-foreground"
          :class="{ 'bg-secondary text-foreground': chatStore.currentSessionId === session.id }"
          @click="chatStore.selectSession(session.id)">
          <MessageSquare class="h-4 w-4" />
          <span class="truncate">{{ session.title }}</span>
        </Button>
      </div>
    </div>

    <!-- Main Chat Area -->
    <div class="flex flex-1 flex-col gap-4 min-w-0">
      <!-- Header / Toolbar -->
      <div class="flex items-center justify-between border-b pb-2">
        <div class="flex items-center gap-2">
          <Button variant="ghost" size="icon" @click="toggleSidebar"
            class="h-8 w-8 text-muted-foreground hover:text-foreground">
            <PanelLeftClose v-if="isSidebarOpen" class="h-4 w-4" />
            <PanelLeftOpen v-else class="h-4 w-4" />
          </Button>
          <h2 class="text-lg font-semibold">{{ t('chat.title') }}</h2>
          <span class="text-xs text-muted-foreground bg-secondary px-2 py-0.5 rounded-full">
            {{ chatStore.config.mode === 'default' ? t('chat.default') : t('chat.custom') }}
          </span>
        </div>

        <Sheet>
          <SheetTrigger as-child>
            <Button variant="ghost" size="icon">
              <Settings class="h-5 w-5" />
            </Button>
          </SheetTrigger>
          <SheetContent>
            <SheetHeader>
              <SheetTitle>{{ t('chat.config') }}</SheetTitle>
              <SheetDescription>
                {{ t('chat.configDesc') }}
              </SheetDescription>
            </SheetHeader>
            <div class="grid gap-4 py-4">
              <div class="grid gap-2">
                <Label>{{ t('chat.mode') }}</Label>
                <Select v-model="tempConfig.mode">
                  <SelectTrigger>
                    <SelectValue placeholder="Select mode" />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="default">{{ t('chat.default') }}</SelectItem>
                    <SelectItem value="custom">{{ t('chat.custom') }}</SelectItem>
                  </SelectContent>
                </Select>
              </div>

              <template v-if="tempConfig.mode === 'custom'">
                <div class="grid gap-2">
                  <Label>{{ t('chat.apiUrl') }}</Label>
                  <Input v-model="tempConfig.customUrl" placeholder="https://api.openai.com/v1" />
                </div>
                <div class="grid gap-2">
                  <Label>{{ t('chat.apiKey') }}</Label>
                  <Input v-model="tempConfig.customApiKey" type="password" placeholder="sk-..." />
                </div>
                <div class="grid gap-2">
                  <Label>{{ t('chat.modelName') }}</Label>
                  <Input v-model="tempConfig.customModel" placeholder="gpt-4o" />
                </div>
                <div class="grid gap-2">
                  <Label>{{ t('chat.systemPrompt') }}</Label>
                  <Textarea v-model="tempConfig.customPrompt" placeholder="You are a helpful assistant..." />
                </div>
              </template>
            </div>
            <SheetFooter>
              <SheetClose as-child>
                <Button @click="saveConfig">{{ t('chat.save') }}</Button>
              </SheetClose>
            </SheetFooter>
          </SheetContent>
        </Sheet>
      </div>

      <!-- Messages Area -->
      <ScrollArea ref="scrollAreaRef" class="flex-1 rounded-md border p-4">
        <div class="flex flex-col gap-4">
          <div v-for="msg in chatStore.messages" :key="msg.id" class="flex gap-3 group"
            :class="msg.role === 'user' ? 'flex-row-reverse' : ''">
            <Avatar class="h-8 w-8 transition-transform duration-200 group-hover:scale-110">
              <AvatarFallback :class="msg.role === 'assistant' ? 'bg-primary text-primary-foreground' : 'bg-secondary'">
                <Bot v-if="msg.role === 'assistant'" class="h-4 w-4" />
                <User v-else class="h-4 w-4" />
              </AvatarFallback>
            </Avatar>
            <div class="rounded-lg px-3 py-2 text-sm max-w-[80%] shadow-sm transition-all duration-200 hover:shadow-md"
              :class="msg.role === 'user' ? 'bg-primary text-primary-foreground' : 'bg-muted'">
              {{ msg.content }}
            </div>
          </div>
          <div v-if="chatStore.isLoading" class="flex gap-3">
            <Avatar class="h-8 w-8">
              <AvatarFallback class="bg-primary text-primary-foreground">
                <Bot class="h-4 w-4" />
              </AvatarFallback>
            </Avatar>
            <div class="rounded-lg px-3 py-2 text-sm bg-muted flex items-center gap-1">
              <span class="animate-bounce">.</span>
              <span class="animate-bounce delay-100">.</span>
              <span class="animate-bounce delay-200">.</span>
            </div>
          </div>
        </div>
      </ScrollArea>

      <!-- Input Area -->
      <div class="relative">
        <Textarea v-model="inputMessage" :placeholder="t('chat.typeMessage')"
          class="min-h-[60px] max-h-[200px] resize-none pr-14 py-3 bg-background border-input focus-visible:ring-1 focus-visible:ring-ring rounded-xl"
          @keydown.enter.prevent="handleSendMessage" />
        <Button size="icon" class="absolute right-2 bottom-2 h-8 w-8 rounded-full transition-transform active:scale-95"
          @click="handleSendMessage" :disabled="chatStore.isLoading || !inputMessage.trim()">
          <Send class="h-4 w-4" />
        </Button>
      </div>
    </div>
  </div>
</template>
