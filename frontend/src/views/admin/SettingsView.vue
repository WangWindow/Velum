<script setup lang="ts">
import { useI18n } from 'vue-i18n'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { Label } from '@/components/ui/label'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import ThemeToggle from '@/components/ThemeToggle.vue'

const { t, locale } = useI18n()

const changeLanguage = (val: string) => {
  locale.value = val
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
          <CardTitle>{{ t('settings.language') }}</CardTitle>
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
