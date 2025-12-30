<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useScalesStore } from '@/stores/scales'
import { storeToRefs } from 'pinia'
import { Button } from '@/components/ui/button'
import { Textarea } from '@/components/ui/textarea'
import { Card, CardContent, CardDescription, CardFooter, CardHeader, CardTitle } from '@/components/ui/card'
import { Label } from '@/components/ui/label'
import { Accordion, AccordionContent, AccordionItem, AccordionTrigger } from '@/components/ui/accordion'
import { Loader2, FileText, Save, Trash2, Eye } from 'lucide-vue-next'
import { Badge } from '@/components/ui/badge'
import { useI18n } from 'vue-i18n'
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table'
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
  DialogFooter,
  DialogClose
} from '@/components/ui/dialog'

import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs'
import { Switch } from '@/components/ui/switch'

const { t } = useI18n()
const scalesStore = useScalesStore()
const { scales } = storeToRefs(scalesStore)
const rawText = ref('')
const viewMode = ref<'import' | 'list'>('list')
const selectedScale = ref<any>(null)
const isViewDialogOpen = ref(false)
const isBilingualMode = ref(false)
const isDeleteDialogOpen = ref(false)
const scaleToDeleteId = ref<number | null>(null)

onMounted(() => {
  scalesStore.fetchScales()
})

const handleView = (scale: any) => {
  try {
    selectedScale.value = {
      ...scale,
      questions: typeof scale.questionsJson === 'string'
        ? JSON.parse(scale.questionsJson)
        : scale.questionsJson
    }
    isViewDialogOpen.value = true
  } catch (e) {
    console.error('Failed to parse questions', e)
  }
}

const handleAnalyze = async () => {
  if (!rawText.value.trim()) return
  if (isBilingualMode.value) {
    await scalesStore.parseBilingualScale(rawText.value)
  } else {
    await scalesStore.parseScale(rawText.value)
  }
}

const handleSave = async () => {
  let success = false
  if (isBilingualMode.value) {
    success = (await scalesStore.saveBilingualScale()) ?? false
  } else {
    success = (await scalesStore.saveScale()) ?? false
  }

  if (success) {
    viewMode.value = 'list'
  }
}

const handleClear = () => {
  scalesStore.clear()
  rawText.value = ''
}

const handleDelete = (id: number) => {
  scaleToDeleteId.value = id
  isDeleteDialogOpen.value = true
}

const confirmDelete = async () => {
  if (scaleToDeleteId.value) {
    await scalesStore.deleteScale(scaleToDeleteId.value)
    isDeleteDialogOpen.value = false
    scaleToDeleteId.value = null
  }
}
</script>

<template>
  <div class="flex flex-col gap-6 h-[calc(100vh-8rem)]">
    <div class="flex items-center justify-between">
      <div>
        <h2 class="text-3xl font-bold tracking-tight">{{ t('scales.title') }}</h2>
        <p class="text-muted-foreground">{{ t('scales.description') }}</p>
      </div>
      <div class="flex gap-2">
        <Button :variant="viewMode === 'list' ? 'default' : 'outline'" @click="viewMode = 'list'">
          {{ t('scales.manage') }}
        </Button>
        <Button :variant="viewMode === 'import' ? 'default' : 'outline'" @click="viewMode = 'import'">
          {{ t('scales.importMode') }}
        </Button>
      </div>
    </div>

    <div v-if="scalesStore.error" class="bg-destructive/15 text-destructive px-4 py-2 rounded-md">
      {{ scalesStore.error }}
    </div>

    <div v-if="viewMode === 'import'" class="grid gap-6 md:grid-cols-2 h-full">
      <!-- Left Column: Input -->
      <Card class="flex flex-col h-full">
        <CardHeader>
          <div class="flex items-center justify-between">
            <CardTitle>{{ t('scales.rawContent') }}</CardTitle>
            <div class="flex items-center space-x-2">
              <Switch id="bilingual-mode" :checked="isBilingualMode" @update:checked="isBilingualMode = $event" />
              <Label for="bilingual-mode">{{ t('scales.bilingualMode') }}</Label>
            </div>
          </div>
          <CardDescription>{{ t('scales.rawContentDesc') }}</CardDescription>
        </CardHeader>
        <CardContent class="flex-1">
          <Textarea v-model="rawText" :placeholder="t('scales.placeholder')"
            class="h-full min-h-75 resize-none font-mono text-sm" />
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
          <div v-if="!scalesStore.parsedTemplate && !scalesStore.parsedBilingualTemplate"
            class="flex h-full flex-col items-center justify-center text-muted-foreground space-y-4">
            <div class="rounded-full bg-muted p-4">
              <FileText class="h-8 w-8" />
            </div>
            <p>{{ t('scales.noData') }}</p>
          </div>

          <div v-else-if="isBilingualMode && scalesStore.parsedBilingualTemplate" class="h-full">
            <Tabs default-value="en" class="h-full flex flex-col">
              <TabsList class="grid w-full grid-cols-2">
                <TabsTrigger value="en">{{ t('scales.english') }}</TabsTrigger>
                <TabsTrigger value="zh">{{ t('scales.chinese') }}</TabsTrigger>
              </TabsList>
              <TabsContent value="en" class="flex-1 overflow-auto mt-4">
                <div class="space-y-6">
                  <div class="space-y-2">
                    <Label class="text-lg font-semibold">{{ scalesStore.parsedBilingualTemplate.en.title }}</Label>
                    <p class="text-sm text-muted-foreground">{{ scalesStore.parsedBilingualTemplate.en.description }}
                    </p>
                  </div>
                  <div class="space-y-2">
                    <Accordion type="single" collapsible class="w-full">
                      <AccordionItem v-for="q in scalesStore.parsedBilingualTemplate.en.questions" :key="q.id"
                        :value="q.id.toString()">
                        <AccordionTrigger class="text-left">
                          <div class="flex gap-2 items-center">
                            <Badge variant="outline">{{ q.id }}</Badge>
                            <span class="line-clamp-1">{{ q.text }}</span>
                          </div>
                        </AccordionTrigger>
                        <AccordionContent>
                          <div class="space-y-2 pl-2">
                            <p class="text-sm font-medium">{{ t('scales.type') }}: <span
                                class="text-muted-foreground">{{ q.type }}</span>
                            </p>
                            <div class="grid gap-2">
                              <div v-for="(opt, idx) in q.options" :key="idx"
                                class="flex justify-between text-sm rounded-md border p-2">
                                <span>{{ opt.text }}</span>
                                <Badge variant="secondary">{{ opt.score }} {{ t('scales.pts') }}</Badge>
                              </div>
                            </div>
                          </div>
                        </AccordionContent>
                      </AccordionItem>
                    </Accordion>
                  </div>
                </div>
              </TabsContent>
              <TabsContent value="zh" class="flex-1 overflow-auto mt-4">
                <div class="space-y-6">
                  <div class="space-y-2">
                    <Label class="text-lg font-semibold">{{ scalesStore.parsedBilingualTemplate.zh.title }}</Label>
                    <p class="text-sm text-muted-foreground">{{ scalesStore.parsedBilingualTemplate.zh.description }}
                    </p>
                  </div>
                  <div class="space-y-2">
                    <Accordion type="single" collapsible class="w-full">
                      <AccordionItem v-for="q in scalesStore.parsedBilingualTemplate.zh.questions" :key="q.id"
                        :value="q.id.toString()">
                        <AccordionTrigger class="text-left">
                          <div class="flex gap-2 items-center">
                            <Badge variant="outline">{{ q.id }}</Badge>
                            <span class="line-clamp-1">{{ q.text }}</span>
                          </div>
                        </AccordionTrigger>
                        <AccordionContent>
                          <div class="space-y-2 pl-2">
                            <p class="text-sm font-medium">{{ t('scales.type') }}: <span
                                class="text-muted-foreground">{{ q.type }}</span>
                            </p>
                            <div class="grid gap-2">
                              <div v-for="(opt, idx) in q.options" :key="idx"
                                class="flex justify-between text-sm rounded-md border p-2">
                                <span>{{ opt.text }}</span>
                                <Badge variant="secondary">{{ opt.score }} {{ t('scales.pts') }}</Badge>
                              </div>
                            </div>
                          </div>
                        </AccordionContent>
                      </AccordionItem>
                    </Accordion>
                  </div>
                </div>
              </TabsContent>
            </Tabs>
          </div>

          <div v-else-if="!isBilingualMode && scalesStore.parsedTemplate" class="space-y-6">
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
                      <p class="text-sm font-medium">{{ t('scales.type') }}: <span class="text-muted-foreground">{{
                        q.type }}</span></p>
                      <div class="grid gap-2">
                        <div v-for="(opt, idx) in q.options" :key="idx"
                          class="flex justify-between text-sm rounded-md border p-2">
                          <span>{{ opt.text }}</span>
                          <Badge variant="secondary">{{ opt.score }} {{ t('scales.pts') }}</Badge>
                        </div>
                      </div>
                    </div>
                  </AccordionContent>
                </AccordionItem>
              </Accordion>
            </div>
          </div>
        </CardContent>
        <CardFooter v-if="scalesStore.parsedTemplate || scalesStore.parsedBilingualTemplate"
          class="justify-end border-t pt-4">
          <Button @click="handleSave" :disabled="scalesStore.isSaving">
            <Loader2 v-if="scalesStore.isSaving" class="mr-2 h-4 w-4 animate-spin" />
            <Save v-else class="mr-2 h-4 w-4" />
            {{ scalesStore.isSaving ? t('scales.saving') : t('scales.import') }}
          </Button>
        </CardFooter>
      </Card>
    </div>

    <div v-else class="rounded-md border">
      <Table>
        <TableHeader>
          <TableRow>
            <TableHead>{{ t('scales.id') }}</TableHead>
            <TableHead>{{ t('scales.columnTitle') }}</TableHead>
            <TableHead>{{ t('scales.columnDescription') }}</TableHead>
            <TableHead class="text-right">{{ t('common.actions') }}</TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          <TableRow v-for="scale in scales" :key="scale.id">
            <TableCell>{{ scale.id }}</TableCell>
            <TableCell class="font-medium">{{ scale.title }}</TableCell>
            <TableCell>{{ scale.description }}</TableCell>
            <TableCell class="text-right">
              <div class="flex justify-end gap-2">
                <Button variant="ghost" size="icon" @click="handleView(scale)">
                  <Eye class="h-4 w-4" />
                </Button>
                <Button variant="ghost" size="icon" class="text-destructive" @click="handleDelete(scale.id)">
                  <Trash2 class="h-4 w-4" />
                </Button>
              </div>
            </TableCell>
          </TableRow>
        </TableBody>
      </Table>
    </div>

    <Dialog v-model:open="isViewDialogOpen">
      <DialogContent class="max-w-3xl max-h-[80vh] overflow-y-auto">
        <DialogHeader>
          <DialogTitle>{{ selectedScale?.title }}</DialogTitle>
          <DialogDescription>{{ selectedScale?.description }}</DialogDescription>
        </DialogHeader>

        <div v-if="selectedScale?.questions" class="space-y-4 py-4">
          <Accordion type="single" collapsible class="w-full">
            <AccordionItem v-for="q in selectedScale.questions" :key="q.id" :value="String(q.id)">
              <AccordionTrigger class="text-left hover:no-underline">
                <div class="flex gap-2 items-center">
                  <Badge variant="outline">{{ q.id }}</Badge>
                  <span class="text-sm font-medium">{{ q.text }}</span>
                </div>
              </AccordionTrigger>
              <AccordionContent>
                <div class="pl-2 space-y-2">
                  <div class="text-xs text-muted-foreground">Type: {{ q.type }}</div>
                  <div class="grid gap-2">
                    <div v-for="(opt, idx) in q.options" :key="idx"
                      class="flex justify-between text-sm rounded-md border p-2 bg-muted/50">
                      <span>{{ opt.text }}</span>
                      <Badge variant="secondary">{{ opt.score }} pts</Badge>
                    </div>
                  </div>
                </div>
              </AccordionContent>
            </AccordionItem>
          </Accordion>
        </div>
      </DialogContent>
    </Dialog>

    <Dialog v-model:open="isDeleteDialogOpen">
      <DialogContent>
        <DialogHeader>
          <DialogTitle>{{ t('scales.deleteTitle') }}</DialogTitle>
          <DialogDescription>
            {{ t('scales.deleteConfirm') }}
          </DialogDescription>
        </DialogHeader>
        <DialogFooter>
          <DialogClose as-child>
            <Button variant="outline">{{ t('common.cancel') }}</Button>
          </DialogClose>
          <Button variant="destructive" @click="confirmDelete">{{ t('common.delete') }}</Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  </div>
</template>
