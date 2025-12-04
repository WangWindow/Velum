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
import {
  Send, Settings, User, Bot, Plus, PanelLeftClose, PanelLeftOpen,
  MessageSquare, Trash2, MoreHorizontal, History
} from 'lucide-vue-next'
import { useI18n } from 'vue-i18n'

const { t } = useI18n()
const chatStore = useChatStore()
const inputMessage = ref('')
const scrollAreaRef = ref<any>(null)
const isSidebarOpen = ref(true)
const isMobileHistoryOpen = ref(false)

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

const handleDeleteSession = async (e: Event, id: number) => {
  e.stopPropagation()
  if (confirm(t('chat.confirmDelete'))) {
    await chatStore.deleteSession(id)
  }
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
    <!-- Sidebar (Desktop) -->
    <div class="hidden md:flex flex-col border-r transition-all duration-300 ease-in-out overflow-hidden"
      :class="[isSidebarOpen ? 'w-72' : 'w-0 border-r-0']">
      <div class="w-72 pr-4 flex flex-col gap-2 h-full">
        <div class="flex items-center justify-between mb-2">
          <Button variant="outline" class="flex-1 justify-start gap-2" @click="chatStore.createSession()">
            <Plus class="h-4 w-4" />
            {{ t('chat.newChat') }}
          </Button>
        </div>

        <div class="flex items-center justify-between px-2 mb-2">
          <div class="text-xs font-medium text-muted-foreground">{{ t('chat.recent') }}</div>
          <Button variant="ghost" size="icon" class="h-6 w-6 text-muted-foreground">
            <MoreHorizontal class="h-4 w-4" />
          </Button>
        </div>

        <ScrollArea class="flex-1 -mr-3 pr-3">
          <div class="flex flex-col gap-1 pb-2">
            <div v-for="session in chatStore.sessions" :key="session.id"
              class="group flex items-center gap-2 rounded-md px-2 py-1.5 hover:bg-accent/50 transition-colors cursor-pointer"
              :class="{ 'bg-secondary text-foreground': chatStore.currentSessionId === session.id }"
              @click="chatStore.selectSession(session.id)">
              <MessageSquare class="h-4 w-4 text-muted-foreground shrink-0" />
              <span class="truncate text-sm flex-1 text-muted-foreground group-hover:text-foreground"
                :class="{ 'text-foreground': chatStore.currentSessionId === session.id }">
                {{ session.title }}
              </span>
              <Button variant="ghost" size="icon"
                class="h-6 w-6 opacity-0 group-hover:opacity-100 transition-opacity text-muted-foreground hover:text-destructive"
                @click="handleDeleteSession($event, session.id)">
                <Trash2 class="h-3 w-3" />
              </Button>
            </div>
          </div>
        </ScrollArea>
      </div>
    </div> <!-- Mobile History Sheet -->
    <Sheet v-model:open="isMobileHistoryOpen">
      <SheetContent side="left" class="w-[80%] sm:w-[350px] p-0">
        <div class="flex flex-col h-full p-4">
          <SheetHeader class="mb-4">
            <SheetTitle>{{ t('chat.history') }}</SheetTitle>
          </SheetHeader>
          <Button variant="outline" class="justify-start gap-2 w-full mb-4"
            @click="chatStore.createSession(); isMobileHistoryOpen = false">
            <Plus class="h-4 w-4" />
            {{ t('chat.newChat') }}
          </Button>
          <div class="flex items-center justify-between mb-2">
            <div class="text-xs font-medium text-muted-foreground">{{ t('chat.recent') }}</div>
            <Button variant="ghost" size="icon" class="h-6 w-6 text-muted-foreground">
              <MoreHorizontal class="h-4 w-4" />
            </Button>
          </div>
          <ScrollArea class="flex-1 -mr-3 pr-3">
            <div class="flex flex-col gap-1">
              <div v-for="session in chatStore.sessions" :key="session.id"
                class="flex items-center gap-2 rounded-md px-2 py-2 hover:bg-accent/50 transition-colors cursor-pointer"
                :class="{ 'bg-secondary text-foreground': chatStore.currentSessionId === session.id }"
                @click="chatStore.selectSession(session.id); isMobileHistoryOpen = false">
                <MessageSquare class="h-4 w-4 text-muted-foreground shrink-0" />
                <span class="truncate text-sm flex-1 text-muted-foreground"
                  :class="{ 'text-foreground': chatStore.currentSessionId === session.id }">
                  {{ session.title }}
                </span>
                <Button variant="ghost" size="icon" class="h-8 w-8 text-muted-foreground hover:text-destructive"
                  @click="handleDeleteSession($event, session.id)">
                  <Trash2 class="h-4 w-4" />
                </Button>
              </div>
            </div>
          </ScrollArea>
        </div>
      </SheetContent>
    </Sheet>

    <!-- Main Chat Area -->
    <div class="flex flex-1 flex-col gap-4 min-w-0 h-full">
      <!-- Header / Toolbar -->
      <div class="flex items-center justify-between border-b pb-2 shrink-0">
        <div class="flex items-center gap-2">
          <!-- Mobile Menu Trigger -->
          <Button variant="ghost" size="icon" class="md:hidden h-8 w-8" @click="isMobileHistoryOpen = true">
            <History class="h-4 w-4" />
          </Button>

          <!-- Desktop Sidebar Toggle -->
          <Button variant="ghost" size="icon" @click="toggleSidebar"
            class="hidden md:flex h-8 w-8 text-muted-foreground hover:text-foreground">
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
                    <SelectValue :placeholder="t('chat.selectMode')" />
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
                  <Input v-model="tempConfig.customUrl" :placeholder="t('chat.placeholder.apiUrl')" />
                </div>
                <div class="grid gap-2">
                  <Label>{{ t('chat.apiKey') }}</Label>
                  <Input v-model="tempConfig.customApiKey" type="password"
                    :placeholder="t('chat.placeholder.apiKey')" />
                </div>
                <div class="grid gap-2">
                  <Label>{{ t('chat.modelName') }}</Label>
                  <Input v-model="tempConfig.customModel" :placeholder="t('chat.placeholder.model')" />
                </div>
                <div class="grid gap-2">
                  <Label>{{ t('chat.systemPrompt') }}</Label>
                  <Textarea v-model="tempConfig.customPrompt" :placeholder="t('chat.placeholder.prompt')" />
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
      <ScrollArea ref="scrollAreaRef" class="flex-1 rounded-md border p-4 bg-background/50">
        <div class="flex flex-col gap-6">
          <div v-for="msg in chatStore.messages" :key="msg.id" class="flex gap-4 group"
            :class="msg.role === 'user' ? 'flex-row-reverse' : ''">
            <Avatar class="h-8 w-8 mt-1 transition-transform duration-200 group-hover:scale-110 shrink-0">
              <AvatarFallback :class="msg.role === 'assistant' ? 'bg-primary text-primary-foreground' : 'bg-secondary'">
                <Bot v-if="msg.role === 'assistant'" class="h-4 w-4" />
                <User v-else class="h-4 w-4" />
              </AvatarFallback>
            </Avatar>
            <div
              class="rounded-2xl px-4 py-2.5 text-sm max-w-[85%] shadow-sm transition-all duration-200 hover:shadow-md leading-relaxed"
              :class="msg.role === 'user' ? 'bg-primary text-primary-foreground rounded-tr-sm' : 'bg-muted rounded-tl-sm'">
              {{ msg.content }}
            </div>
          </div>
          <div v-if="chatStore.isLoading" class="flex gap-4">
            <Avatar class="h-8 w-8 mt-1">
              <AvatarFallback class="bg-primary text-primary-foreground">
                <Bot class="h-4 w-4" />
              </AvatarFallback>
            </Avatar>
            <div class="rounded-2xl rounded-tl-sm px-4 py-2.5 text-sm bg-muted flex items-center gap-1">
              <span class="animate-bounce">.</span>
              <span class="animate-bounce delay-100">.</span>
              <span class="animate-bounce delay-200">.</span>
            </div>
          </div>
        </div>
      </ScrollArea>

      <!-- Input Area -->
      <div class="relative shrink-0 pt-2 pb-2 px-2">
        <Textarea v-model="inputMessage" :placeholder="t('chat.typeMessage')"
          class="min-h-[60px] max-h-[200px] resize-none pr-14 py-3 bg-background border-input focus-visible:ring-1 focus-visible:ring-ring rounded-xl shadow-sm"
          @keydown.enter.prevent="handleSendMessage" />
        <Button size="icon"
          class="absolute right-4 bottom-4 h-8 w-8 rounded-full transition-transform active:scale-95 shadow-sm"
          @click="handleSendMessage" :disabled="chatStore.isLoading || !inputMessage.trim()">
          <Send class="h-4 w-4" />
        </Button>
      </div>
    </div>
  </div>
</template>
