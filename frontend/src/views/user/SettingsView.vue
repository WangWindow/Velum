<script setup lang="ts">
import { useI18n } from 'vue-i18n'
import { useAuthStore } from '@/stores/auth'
import { Card, CardContent, CardDescription, CardHeader, CardTitle, CardFooter } from '@/components/ui/card'
import { Label } from '@/components/ui/label'
import { Input } from '@/components/ui/input'
import { Button } from '@/components/ui/button'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import ThemeToggle from '@/components/ThemeToggle.vue'
import { User, Smile, Cat, Dog, Ghost, Bot, Zap, Star } from 'lucide-vue-next'
import { ref } from 'vue'
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs'
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
const fullName = ref(authStore.user?.fullName || '')
const email = ref(authStore.user?.email || '')

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

const saveProfile = async () => {
  const success = await authStore.updateProfile({ fullName: fullName.value, email: email.value })
  if (success) {
    toast({
      title: t('common.save'),
      description: t('settings.profileDesc'),
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
    <Tabs default-value="general" class="space-y-4">
      <TabsList>
        <TabsTrigger value="general">{{ t('settings.general') }}</TabsTrigger>
        <TabsTrigger value="profile">{{ t('settings.profile') }}</TabsTrigger>
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
                <SelectTrigger id="language" class="w-45">
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
            <CardTitle>{{ t('settings.profile') }}</CardTitle>
            <CardDescription>{{ t('settings.profileDesc') }}</CardDescription>
          </CardHeader>
          <CardContent>
            <div class="grid gap-4 md:grid-cols-2">
              <div class="space-y-2">
                <Label>{{ t('settings.fullName') }}</Label>
                <Input v-model="fullName" />
              </div>
              <div class="space-y-2">
                <Label>{{ t('settings.email') }}</Label>
                <Input v-model="email" />
              </div>
            </div>
          </CardContent>
          <CardFooter>
            <Button @click="saveProfile">
              {{ t('common.save') }}
            </Button>
          </CardFooter>
        </Card>
      </TabsContent>
    </Tabs>
  </div>
</template>
