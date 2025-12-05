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
import { Avatar, AvatarImage, AvatarFallback } from '@/components/ui/avatar'
import ThemeToggle from '@/components/ThemeToggle.vue'
import { Loader2 } from 'lucide-vue-next'

const { t, locale } = useI18n()
const authStore = useAuthStore()
const settingsStore = useSettingsStore()

const isLoading = ref(false)
const isSaving = ref(false)

// Profile State
const profileForm = ref({
  fullName: authStore.user?.fullName || '',
  email: authStore.user?.email || '',
  avatar: authStore.user?.avatar || ''
})

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
            <CardDescription>{{ t('settings.languageDesc') }}</CardDescription>
          </CardHeader>
          <CardContent>
            <div class="flex items-center space-x-4">
              <Label for="language" class="w-[100px]">{{ t('settings.language') }}</Label>
              <Select :model-value="locale" @update:model-value="changeLanguage">
                <SelectTrigger id="language" class="w-[200px]">
                  <SelectValue placeholder="Select Language" />
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
            <CardDescription>{{ t('settings.themeDesc') }}</CardDescription>
          </CardHeader>
          <CardContent>
            <div class="flex items-center space-x-4">
              <Label class="w-[100px]">{{ t('settings.theme') }}</Label>
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
            <div class="flex items-center gap-6">
              <Avatar class="h-20 w-20">
                <AvatarImage :src="profileForm.avatar" />
                <AvatarFallback>{{ userInitials }}</AvatarFallback>
              </Avatar>
              <div class="space-y-2 flex-1">
                <Label>{{ t('settings.avatarUrl') }}</Label>
                <Input v-model="profileForm.avatar" placeholder="https://..." />
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
          <CardFooter>
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
