<script setup lang="ts">
import { ref } from 'vue'
import { useScalesStore } from '@/stores/scales'
import { Button } from '@/components/ui/button'
import { Textarea } from '@/components/ui/textarea'
import { Card, CardContent, CardDescription, CardFooter, CardHeader, CardTitle } from '@/components/ui/card'
import { Label } from '@/components/ui/label'
import { Accordion, AccordionContent, AccordionItem, AccordionTrigger } from '@/components/ui/accordion'
import { Loader2, FileText, Save, Trash2 } from 'lucide-vue-next'
import { Badge } from '@/components/ui/badge'
import { useI18n } from 'vue-i18n'

const { t } = useI18n()
const scalesStore = useScalesStore()
const rawText = ref('')

const handleAnalyze = async () => {
  if (!rawText.value.trim()) return
  await scalesStore.parseScale(rawText.value)
}

const handleSave = async () => {
  await scalesStore.saveScale()
}

const handleClear = () => {
  scalesStore.clear()
  rawText.value = ''
}
</script>

<template>
  <div class="flex flex-col gap-6 h-[calc(100vh-8rem)]">
    <div class="flex items-center justify-between">
      <div>
        <h2 class="text-3xl font-bold tracking-tight">{{ t('scales.title') }}</h2>
        <p class="text-muted-foreground">{{ t('scales.description') }}</p>
      </div>
    </div>

    <div class="grid gap-6 md:grid-cols-2 h-full">
      <!-- Left Column: Input -->
      <Card class="flex flex-col h-full">
        <CardHeader>
          <CardTitle>{{ t('scales.rawContent') }}</CardTitle>
          <CardDescription>{{ t('scales.rawContentDesc') }}</CardDescription>
        </CardHeader>
        <CardContent class="flex-1">
          <Textarea v-model="rawText" :placeholder="t('scales.placeholder')"
            class="h-full min-h-[300px] resize-none font-mono text-sm" />
        </CardContent>
        <CardFooter class="justify-between">
          <Button variant="ghost" @click="handleClear" :disabled="scalesStore.isAnalyzing">
            <Trash2 class="mr-2 h-4 w-4" />
            {{ t('scales.clear') }}
          </Button>
          <Button @click="handleAnalyze" :disabled="!rawText.trim() || scalesStore.isAnalyzing">
            <Loader2 v-if="scalesStore.isAnalyzing" class="mr-2 h-4 w-4 animate-spin" />
            <span v-else>{{ t('scales.analyze') }}</span>
          </Button>
        </CardFooter>
      </Card>

      <!-- Right Column: Preview -->
      <Card class="flex flex-col h-full overflow-hidden">
        <CardHeader>
          <CardTitle>{{ t('scales.preview') }}</CardTitle>
          <CardDescription>{{ t('scales.previewDesc') }}</CardDescription>
        </CardHeader>
        <CardContent class="flex-1 overflow-auto">
          <div v-if="!scalesStore.parsedTemplate"
            class="flex h-full flex-col items-center justify-center text-muted-foreground space-y-4">
            <div class="rounded-full bg-muted p-4">
              <FileText class="h-8 w-8" />
            </div>
            <p>{{ t('scales.noData') }}</p>
          </div>

          <div v-else class="space-y-6">
            <div class="space-y-2">
              <Label class="text-lg font-semibold">{{ scalesStore.parsedTemplate.title }}</Label>
              <p class="text-sm text-muted-foreground">{{ scalesStore.parsedTemplate.description }}</p>
            </div>

            <div class="space-y-2">
              <div class="flex items-center justify-between">
                <Label>{{ t('scales.questions') }} ({{ scalesStore.parsedTemplate.questions.length }})</Label>
              </div>

              <Accordion type="single" collapsible class="w-full">
                <AccordionItem v-for="q in scalesStore.parsedTemplate.questions" :key="q.id" :value="q.id.toString()">
                  <AccordionTrigger class="text-left">
                    <div class="flex gap-2 items-center">
                      <Badge variant="outline">{{ q.id }}</Badge>
                      <span class="line-clamp-1">{{ q.text }}</span>
                    </div>
                  </AccordionTrigger>
                  <AccordionContent>
                    <div class="space-y-2 pl-2">
                      <p class="text-sm font-medium">Type: <span class="text-muted-foreground">{{ q.type }}</span></p>
                      <div class="grid gap-2">
                        <div v-for="(opt, idx) in q.options" :key="idx"
                          class="flex justify-between text-sm rounded-md border p-2">
                          <span>{{ opt.text }}</span>
                          <Badge variant="secondary">{{ opt.score }} pts</Badge>
                        </div>
                      </div>
                    </div>
                  </AccordionContent>
                </AccordionItem>
              </Accordion>
            </div>
          </div>
        </CardContent>
        <CardFooter v-if="scalesStore.parsedTemplate" class="justify-end border-t pt-4">
          <Button @click="handleSave" :disabled="scalesStore.isSaving">
            <Loader2 v-if="scalesStore.isSaving" class="mr-2 h-4 w-4 animate-spin" />
            <Save v-else class="mr-2 h-4 w-4" />
            {{ scalesStore.isSaving ? t('scales.saving') : t('scales.import') }}
          </Button>
        </CardFooter>
      </Card>
    </div>
  </div>
</template>
