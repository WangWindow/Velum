<script setup lang="ts">
import { ref } from 'vue'
import { useAuthStore } from '@/stores/auth'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Card, CardContent, CardDescription, CardFooter, CardHeader, CardTitle } from '@/components/ui/card'
import ThemeToggle from '@/components/ThemeToggle.vue'
import LanguageToggle from '@/components/LanguageToggle.vue'
import { useI18n } from 'vue-i18n'
import { useToast } from '@/components/ui/toast/use-toast'

const { t } = useI18n()
const { toast } = useToast()
const authStore = useAuthStore()

const isRegistering = ref(false)
const username = ref('')
const password = ref('')
const email = ref('')
const fullName = ref('')
const registrationKey = ref('')
const isLoading = ref(false)

const handleSubmit = async () => {
  if (!username.value || !password.value) return

  if (isRegistering.value && !registrationKey.value) {
    toast({
      title: t('login.registrationFailed'),
      description: t('login.registrationKeyRequired'),
      variant: 'destructive',
    })
    return
  }

  isLoading.value = true

  let success = false
  if (isRegistering.value) {
    const result = await authStore.register(username.value, password.value, email.value, fullName.value, registrationKey.value)
    if (result.success) {
      toast({
        title: t('login.registrationSuccess'),
        description: t('login.registrationSuccessDesc'),
      })
      isRegistering.value = false
    } else {
      toast({
        title: t('login.registrationFailed'),
        description: result.error,
        variant: 'destructive',
      })
    }
  } else {
    success = await authStore.login(username.value, password.value)
    if (!success) {
      toast({
        title: t('login.loginFailed'),
        description: t('login.loginFailedDesc'),
        variant: 'destructive',
      })
    }
  }

  isLoading.value = false
}
</script>

<template>
  <div class="flex min-h-screen items-center justify-center bg-background p-4 relative">
    <div class="absolute top-4 right-4 flex gap-2">
      <LanguageToggle />
      <ThemeToggle />
    </div>
    <Card class="w-full max-w-md">
      <CardHeader class="space-y-1">
        <CardTitle class="text-2xl font-bold text-center">
          {{ isRegistering ? t('login.createAccount') : t('login.title') }}
        </CardTitle>
        <CardDescription class="text-center">
          {{ isRegistering ? t('login.createDescription') : t('login.description') }}
        </CardDescription>
      </CardHeader>
      <CardContent class="space-y-4">
        <div class="space-y-2">
          <Label for="username">{{ t('login.username') }}</Label>
          <Input id="username" v-model="username" :placeholder="t('login.usernamePlaceholder')" />
        </div>
        <div v-if="isRegistering" class="space-y-2">
          <Label for="email">{{ t('login.email') }}</Label>
          <Input id="email" type="email" v-model="email" placeholder="m@example.com" />
        </div>
        <div v-if="isRegistering" class="space-y-2">
          <Label for="fullName">{{ t('login.fullName') }}</Label>
          <Input id="fullName" v-model="fullName" placeholder="John Doe" />
        </div>
        <div v-if="isRegistering" class="space-y-2">
          <Label for="registrationKey">{{ t('login.registrationKey') }}</Label>
          <Input id="registrationKey" v-model="registrationKey" type="password" placeholder="114514" />
        </div>
        <div class="space-y-2">
          <Label for="password">{{ t('login.password') }}</Label>
          <Input id="password" type="password" v-model="password" />
        </div>
      </CardContent>
      <CardFooter class="flex flex-col gap-4">
        <Button class="w-full" @click="handleSubmit" :disabled="isLoading">
          {{ isLoading ? (isRegistering ? t('login.creatingAccount') : t('login.loggingIn')) : (isRegistering ?
            t('login.signUp') : t('login.submit')) }}
        </Button>
        <div class="text-center text-sm">
          <span class="text-muted-foreground">
            {{ isRegistering ? t('login.alreadyHaveAccount') : t('login.dontHaveAccount') }}
          </span>
          <Button variant="link" class="p-0 h-auto ml-1" @click="isRegistering = !isRegistering">
            {{ isRegistering ? t('login.loginLink') : t('login.signUp') }}
          </Button>
        </div>
      </CardFooter>
    </Card>
  </div>
</template>
