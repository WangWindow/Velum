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
    <div>
      <h3 class="text-lg font-medium">{{ t('settings.title') }}</h3>
      <p class="text-sm text-muted-foreground">{{ t('settings.description') }}</p>
    </div>
    <div class="grid gap-6">
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
