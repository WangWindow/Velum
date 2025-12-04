<script setup lang="ts">
import { useI18n } from 'vue-i18n'
import { Button } from '@/components/ui/button'
import { onMounted } from 'vue'

const { locale } = useI18n()

onMounted(() => {
  const savedLocale = localStorage.getItem('locale')
  if (savedLocale && ['en', 'zh'].includes(savedLocale)) {
    locale.value = savedLocale
  }
})

const toggleLanguage = () => {
  const newLocale = locale.value === 'en' ? 'zh' : 'en'
  locale.value = newLocale
  localStorage.setItem('locale', newLocale)
}
</script>

<template>
  <Button variant="ghost" size="icon" class="relative" @click="toggleLanguage" title="Switch Language">
    <span class="h-[1.2rem] w-[1.2rem]">{{ locale.toUpperCase() }}</span>
  </Button>
</template>
