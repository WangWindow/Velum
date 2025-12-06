<script setup lang="ts">
import { onMounted, ref, computed } from 'vue'
import { useI18n } from 'vue-i18n'
import { useAuthStore } from '@/stores/auth'
import { useSettingsStore } from '@/stores/settings'
import { Card, CardContent, CardDescription, CardHeader, CardTitle, CardFooter } from '@/components/ui/card'
import { Label } from '@/components/ui/label'
import { Input } from '@/components/ui/input'
import { Button } from '@/components/ui/button'
import { Textarea } from '@/components/ui/textarea'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs'
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
  DialogClose
} from '@/components/ui/dialog'
import ThemeToggle from '@/components/ThemeToggle.vue'
import { User, Smile, Cat, Dog, Ghost, Bot, Zap, Star, Loader2 } from 'lucide-vue-next'

const { t, locale } = useI18n()
const authStore = useAuthStore()
const settingsStore = useSettingsStore()

const isSaving = ref(false)

// Profile State
const profileForm = ref({
  fullName: authStore.user?.fullName || '',
  email: authStore.user?.email || '',
  avatar: authStore.user?.avatar || 'Default'
})

const avatars = [
  { name: 'Default', icon: null },
  { name: 'User', icon: User },
  { name: 'Smile', icon: Smile },
  { name: 'Cat', icon: Cat },
  { name: 'Dog', icon: Dog },
  { name: 'Ghost', icon: Ghost },
  { name: 'Bot', icon: Bot },
  { name: 'Zap', icon: Zap },
  { name: 'Star', icon: Star },
]

const selectAvatar = (avatarName: string) => {
  profileForm.value.avatar = avatarName
}

// System Settings State
const systemSettings = ref({
  aiApiUrl: '',
  aiApiKey: '',
  aiModel: '',
  systemPrompt: ''
})

onMounted(async () => {
  await settingsStore.fetchSettings()
  systemSettings.value = {
    aiApiUrl: settingsStore.getSettingValue('AiApiUrl'),
    aiApiKey: settingsStore.getSettingValue('AiApiKey'),
    aiModel: settingsStore.getSettingValue('AiModel'),
    systemPrompt: settingsStore.getSettingValue('SystemPrompt')
  }
})

const changeLanguage = (val: string) => {
  locale.value = val
}

const handleUpdateProfile = async () => {
  isSaving.value = true
  await authStore.updateProfile(profileForm.value)
  isSaving.value = false
}

const handleUpdateSystemSettings = async () => {
  isSaving.value = true
  await settingsStore.updateSettings([
    { key: 'AiApiUrl', value: systemSettings.value.aiApiUrl },
    { key: 'AiApiKey', value: systemSettings.value.aiApiKey },
    { key: 'AiModel', value: systemSettings.value.aiModel },
    { key: 'SystemPrompt', value: systemSettings.value.systemPrompt }
  ])
  isSaving.value = false
}

const handleResetSystemSettings = async () => {
  isSaving.value = true
  await settingsStore.resetSettings()
  systemSettings.value = {
    aiApiUrl: settingsStore.getSettingValue('AiApiUrl'),
    aiApiKey: settingsStore.getSettingValue('AiApiKey'),
    aiModel: settingsStore.getSettingValue('AiModel'),
    systemPrompt: settingsStore.getSettingValue('SystemPrompt')
  }
  isSaving.value = false
}

const userInitials = computed(() => {
  const name = authStore.user?.fullName || authStore.user?.username || 'U'
  return name.substring(0, 2).toUpperCase()
})
</script>

<template>
  <div class="space-y-6">
    <div class="flex items-center justify-between">
      <div>
        <h2 class="text-3xl font-bold tracking-tight">{{ t('settings.title') }}</h2>
        <p class="text-muted-foreground">{{ t('settings.description') }}</p>
      </div>
    </div>

    <Tabs default-value="general" class="space-y-4">
      <TabsList>
        <TabsTrigger value="general">{{ t('settings.general') }}</TabsTrigger>
        <TabsTrigger value="profile">{{ t('settings.profile') }}</TabsTrigger>
        <TabsTrigger value="system">{{ t('settings.system') }}</TabsTrigger>
      </TabsList>

      <TabsContent value="general" class="space-y-4">
        <Card>
          <CardHeader>
            <CardTitle>{{ t('settings.language') }}</CardTitle>
          </CardHeader>
          <CardContent>
            <div class="flex items-center space-x-4">
              <Label for="language">{{ t('settings.language') }}</Label>
              <Select :model-value="locale" @update:model-value="changeLanguage">
                <SelectTrigger id="language" class="w-[180px]">
                  <SelectValue :placeholder="t('settings.selectLanguage')" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="en">English</SelectItem>
                  <SelectItem value="zh">中文</SelectItem>
                </SelectContent>
              </Select>
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle>{{ t('settings.theme') }}</CardTitle>
          </CardHeader>
          <CardContent>
            <div class="flex items-center space-x-4">
              <Label>{{ t('settings.theme') }}</Label>
              <ThemeToggle />
            </div>
          </CardContent>
        </Card>
      </TabsContent>

      <TabsContent value="profile" class="space-y-4">
        <Card>
          <CardHeader>
            <CardTitle>{{ t('settings.profile') }}</CardTitle>
            <CardDescription>{{ t('settings.profileDesc') }}</CardDescription>
          </CardHeader>
          <CardContent class="space-y-6">
            <div class="space-y-4">
              <Label>{{ t('settings.avatar') }}</Label>
              <div class="flex flex-wrap gap-4">
                <Button v-for="avatar in avatars" :key="avatar.name" variant="outline"
                  class="h-16 w-16 rounded-full p-0"
                  :class="{ 'ring-2 ring-primary': profileForm.avatar === avatar.name || (avatar.name === 'Default' && !profileForm.avatar) }"
                  @click="selectAvatar(avatar.name)">
                  <span v-if="avatar.name === 'Default'" class="text-lg font-bold">
                    {{ userInitials }}
                  </span>
                  <component v-else :is="avatar.icon" class="h-8 w-8" />
                </Button>
              </div>
            </div>
            <div class="grid gap-4 md:grid-cols-2">
              <div class="space-y-2">
                <Label>{{ t('settings.fullName') }}</Label>
                <Input v-model="profileForm.fullName" />
              </div>
              <div class="space-y-2">
                <Label>{{ t('settings.email') }}</Label>
                <Input v-model="profileForm.email" />
              </div>
            </div>
          </CardContent>
          <CardFooter>
            <Button @click="handleUpdateProfile" :disabled="isSaving">
              <Loader2 v-if="isSaving" class="mr-2 h-4 w-4 animate-spin" />
              {{ t('common.save') }}
            </Button>
          </CardFooter>
        </Card>
      </TabsContent>

      <TabsContent value="system" class="space-y-4">
        <Card>
          <CardHeader>
            <CardTitle>{{ t('settings.aiConfig') }}</CardTitle>
            <CardDescription>{{ t('settings.aiConfigDesc') }}</CardDescription>
          </CardHeader>
          <CardContent class="space-y-4">
            <div class="grid gap-4 md:grid-cols-2">
              <div class="space-y-2">
                <Label>{{ t('settings.aiApiUrl') }}</Label>
                <Input v-model="systemSettings.aiApiUrl" placeholder="https://api.openai.com/v1" />
              </div>
              <div class="space-y-2">
                <Label>{{ t('settings.aiModel') }}</Label>
                <Input v-model="systemSettings.aiModel" placeholder="gpt-4" />
              </div>
            </div>
            <div class="space-y-2">
              <Label>{{ t('settings.aiApiKey') }}</Label>
              <Input v-model="systemSettings.aiApiKey" type="password" placeholder="sk-..." />
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle>{{ t('settings.prompts') }}</CardTitle>
            <CardDescription>{{ t('settings.promptsDesc') }}</CardDescription>
          </CardHeader>
          <CardContent class="space-y-4">
            <div class="space-y-2">
              <Label>{{ t('settings.systemPrompt') }}</Label>
              <Textarea v-model="systemSettings.systemPrompt" rows="6" placeholder="You are a helpful assistant..." />
            </div>
          </CardContent>
          <CardFooter class="flex justify-between">
            <Dialog>
              <DialogTrigger as-child>
                <Button variant="destructive" :disabled="isSaving">
                  {{ t('settings.reset') }}
                </Button>
              </DialogTrigger>
              <DialogContent>
                <DialogHeader>
                  <DialogTitle>{{ t('settings.resetTitle') }}</DialogTitle>
                  <DialogDescription>
                    {{ t('settings.confirmReset') }}
                  </DialogDescription>
                </DialogHeader>
                <DialogFooter>
                  <DialogClose as-child>
                    <Button variant="outline">{{ t('common.cancel') }}</Button>
                  </DialogClose>
                  <Button variant="destructive" @click="handleResetSystemSettings">
                    {{ t('common.confirm') }}
                  </Button>
                </DialogFooter>
              </DialogContent>
            </Dialog>
            <Button @click="handleUpdateSystemSettings" :disabled="isSaving">
              <Loader2 v-if="isSaving" class="mr-2 h-4 w-4 animate-spin" />
              {{ t('common.save') }}
            </Button>
          </CardFooter>
        </Card>
      </TabsContent>
    </Tabs>
  </div>
</template>
