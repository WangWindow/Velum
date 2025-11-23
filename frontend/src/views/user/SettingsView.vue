<script setup lang="ts">
import { useI18n } from 'vue-i18n'
import { useAuthStore } from '@/stores/auth'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { Label } from '@/components/ui/label'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import { Button } from '@/components/ui/button'
import ThemeToggle from '@/components/ThemeToggle.vue'
import { User, Smile, Cat, Dog, Ghost, Bot, Zap, Star } from 'lucide-vue-next'
import { ref } from 'vue'
import { useToast } from '@/components/ui/toast/use-toast'

const { t, locale } = useI18n()
const authStore = useAuthStore()
const { toast } = useToast()

const changeLanguage = (val: string) => {
  locale.value = val
}

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

const selectedAvatar = ref(authStore.user?.avatar || 'Default')

const saveAvatar = async (avatarName: string) => {
  const valueToSave = avatarName === 'Default' ? '' : avatarName
  selectedAvatar.value = avatarName
  const success = await authStore.updateProfile({ avatar: valueToSave })
  if (success) {
    toast({
      title: t('settings.avatarUpdated'),
      description: t('settings.avatarUpdatedDesc'),
    })
  }
}
</script>

<template>
  <div class="space-y-6">
    <div class="flex items-center justify-between">
      <div>
        <h2 class="text-3xl font-bold tracking-tight">{{ t('settings.title') }}</h2>
        <p class="text-muted-foreground">{{ t('settings.description') }}</p>
      </div>
    </div>
    <div class="grid gap-6">
      <Card>
        <CardHeader>
          <CardTitle>{{ t('settings.avatar') }}</CardTitle>
          <CardDescription>{{ t('settings.avatarDesc') }}</CardDescription>
        </CardHeader>
        <CardContent>
          <div class="flex flex-wrap gap-4">
            <Button v-for="avatar in avatars" :key="avatar.name" variant="outline" class="h-16 w-16 rounded-full p-0"
              :class="{ 'ring-2 ring-primary': selectedAvatar === avatar.name }" @click="saveAvatar(avatar.name)">
              <span v-if="avatar.name === 'Default'" class="text-lg font-bold">
                {{ authStore.user?.username?.substring(0, 2).toUpperCase() || 'U' }}
              </span>
              <component v-else :is="avatar.icon" class="h-8 w-8" />
            </Button>
          </div>
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>{{ t('settings.language') }}</CardTitle>
          <CardDescription>Select your preferred language.</CardDescription>
        </CardHeader>
        <CardContent>
          <div class="flex items-center space-x-4">
            <Label for="language">{{ t('settings.language') }}</Label>
            <Select :model-value="locale" @update:model-value="changeLanguage">
              <SelectTrigger id="language" class="w-[180px]">
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
          <CardDescription>Select your preferred theme.</CardDescription>
        </CardHeader>
        <CardContent>
          <div class="flex items-center space-x-4">
            <Label>{{ t('settings.theme') }}</Label>
            <ThemeToggle />
          </div>
        </CardContent>
      </Card>
    </div>
  </div>
</template>
