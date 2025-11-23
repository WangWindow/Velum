<script setup lang="ts">
import { useI18n } from 'vue-i18n'
import { Button } from '@/components/ui/button'
import { Languages } from 'lucide-vue-next'
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
    <Languages class="h-[1.2rem] w-[1.2rem]" />
    <span class="sr-only">Toggle language</span>
    <span class="absolute -bottom-1 right-0 text-[10px] font-bold">{{ locale.toUpperCase() }}</span>
  </Button>
</template>
