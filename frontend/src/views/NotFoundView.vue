<script setup lang="ts">
import { useRouter } from 'vue-router'
import { useI18n } from 'vue-i18n'
import { Button } from '@/components/ui/button'
import { AlertCircle } from 'lucide-vue-next'

const router = useRouter()
const { t } = useI18n()

const goBack = () => {
  router.back()
}

const goHome = () => {
  router.push('/')
}
</script>

<template>
  <div class="min-h-screen flex flex-col items-center justify-center bg-background text-foreground p-4">
    <div class="text-center space-y-6 max-w-md">
      <div class="flex justify-center">
        <AlertCircle class="h-24 w-24 text-destructive" />
      </div>

      <h1 class="text-4xl font-bold tracking-tight">{{ t('error.notFound.title') }}</h1>

      <p class="text-xl text-muted-foreground">
        {{ t('error.notFound.description') }}
      </p>

      <div class="flex flex-col sm:flex-row gap-4 justify-center mt-8">
        <Button variant="outline" size="lg" @click="goBack">
          {{ t('common.back') }}
        </Button>
        <Button size="lg" @click="goHome">
          {{ t('common.home') }}
        </Button>
      </div>
    </div>
  </div>
</template>
