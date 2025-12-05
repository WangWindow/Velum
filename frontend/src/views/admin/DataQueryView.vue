<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useAnalysisStore } from '@/stores/analysis'
import { storeToRefs } from 'pinia'
import { useI18n } from 'vue-i18n'
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from '@/components/ui/card'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import { Input } from '@/components/ui/input'
import { Button } from '@/components/ui/button'
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs'
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table'
import { Loader2, Search, Download, FileSpreadsheet, Filter } from 'lucide-vue-next'
import { Popover, PopoverContent, PopoverTrigger } from '@/components/ui/popover'
import { Label } from '@/components/ui/label'
import api from '@/lib/api'

const { t } = useI18n()
const analysisStore = useAnalysisStore()
const { overallStats, exportData } = storeToRefs(analysisStore)

// State for Assessments Tab
const selectedQuestionnaireId = ref<string>('')
const dataSearch = ref('')
const advancedFilters = ref({
  minScore: '',
  maxScore: '',
  dateFrom: '',
  dateTo: ''
})

// State for Games Tab
const gameScores = ref<any[]>([])
const isGameScoresLoading = ref(false)
const gameSearch = ref('')

// Computed for Assessments
const filteredExportRows = computed(() => {
  if (!exportData.value) return []
  let rows = exportData.value.rows

  // Basic Search
  if (dataSearch.value) {
    const search = dataSearch.value.toLowerCase()
    rows = rows.filter(row =>
      Object.values(row).some(val => String(val).toLowerCase().includes(search))
    )
  }

  // Advanced Filters (Client-side for now)
  // Note: This assumes 'Score' and 'Date' columns exist in the dynamic data, which might not always be true or named exactly so.
  // For a robust solution, we'd need standardized column keys from backend.
  // Here we just demonstrate the UI structure.

  return rows
})

// Computed for Games
const filteredGameScores = computed(() => {
  let scores = gameScores.value
  if (gameSearch.value) {
    const search = gameSearch.value.toLowerCase()
    scores = scores.filter(s =>
      s.user?.username.toLowerCase().includes(search) ||
      s.gameName.toLowerCase().includes(search)
    )
  }
  return scores
})

const fetchGameScores = async () => {
  isGameScoresLoading.value = true
  try {
    const response = await api.get('/games/all')
    gameScores.value = response.data
  } catch (e) {
    console.error(e)
  } finally {
    isGameScoresLoading.value = false
  }
}

const handleLoadData = () => {
  if (selectedQuestionnaireId.value) {
    analysisStore.fetchExportData(Number(selectedQuestionnaireId.value))
  }
}

onMounted(() => {
  analysisStore.fetchOverallStats()
  fetchGameScores()
})
</script>

<template>
  <div class="space-y-6">
    <div class="flex items-center justify-between">
      <div>
        <h2 class="text-3xl font-bold tracking-tight">{{ t('query.title') }}</h2>
        <p class="text-muted-foreground">{{ t('query.description') }}</p>
      </div>
    </div>

    <Tabs defaultValue="assessments" class="space-y-4">
      <TabsList>
        <TabsTrigger value="assessments">{{ t('query.assessments') }}</TabsTrigger>
        <TabsTrigger value="games">{{ t('query.games') }}</TabsTrigger>
      </TabsList>

      <!-- Assessments Tab -->
      <TabsContent value="assessments" class="space-y-4">
        <Card>
          <CardHeader>
            <CardTitle>{{ t('query.assessmentData') }}</CardTitle>
            <CardDescription>{{ t('query.assessmentDataDesc') }}</CardDescription>
          </CardHeader>
          <CardContent class="space-y-6">
            <!-- Toolbar -->
            <div class="flex flex-col md:flex-row gap-4 items-start md:items-center">
              <div class="w-full md:w-[250px]">
                <Select v-model="selectedQuestionnaireId" @update:model-value="handleLoadData">
                  <SelectTrigger>
                    <SelectValue :placeholder="t('analysis.selectQuestionnaire')" />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem v-for="q in overallStats?.questionnaireStats" :key="q.questionnaireId"
                      :value="String(q.questionnaireId)">
                      {{ q.title }}
                    </SelectItem>
                  </SelectContent>
                </Select>
              </div>

              <div class="relative w-full md:w-[300px]">
                <Search class="absolute left-2 top-2.5 h-4 w-4 text-muted-foreground" />
                <Input v-model="dataSearch" :placeholder="t('analysis.searchData')" class="pl-8" />
              </div>

              <!-- Advanced Filter Popover -->
              <Popover>
                <PopoverTrigger as-child>
                  <Button variant="outline" class="gap-2">
                    <Filter class="h-4 w-4" />
                    {{ t('query.advancedFilter') }}
                  </Button>
                </PopoverTrigger>
                <PopoverContent class="w-80">
                  <div class="grid gap-4">
                    <div class="space-y-2">
                      <h4 class="font-medium leading-none">{{ t('query.filters') }}</h4>
                      <p class="text-sm text-muted-foreground">{{ t('query.filtersDesc') }}</p>
                    </div>
                    <div class="grid gap-2">
                      <div class="grid grid-cols-2 gap-2">
                        <div class="grid gap-1">
                          <Label>{{ t('query.minScore') }}</Label>
                          <Input v-model="advancedFilters.minScore" type="number" placeholder="0" />
                        </div>
                        <div class="grid gap-1">
                          <Label>{{ t('query.maxScore') }}</Label>
                          <Input v-model="advancedFilters.maxScore" type="number" placeholder="100" />
                        </div>
                      </div>
                      <div class="grid gap-1">
                        <Label>{{ t('query.dateFrom') }}</Label>
                        <Input v-model="advancedFilters.dateFrom" type="date" />
                      </div>
                      <div class="grid gap-1">
                        <Label>{{ t('query.dateTo') }}</Label>
                        <Input v-model="advancedFilters.dateTo" type="date" />
                      </div>
                    </div>
                  </div>
                </PopoverContent>
              </Popover>

              <Button variant="outline" class="ml-auto">
                <Download class="mr-2 h-4 w-4" />
                {{ t('analysis.exportCsv') }}
              </Button>
            </div>

            <!-- Data Table -->
            <div v-if="exportData" class="border rounded-md overflow-x-auto">
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead v-for="col in exportData.columns" :key="col" class="whitespace-nowrap">{{ col }}
                    </TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  <TableRow v-for="(row, i) in filteredExportRows" :key="i">
                    <TableCell v-for="col in exportData.columns" :key="col" class="whitespace-nowrap">
                      {{ row[col] }}
                    </TableCell>
                  </TableRow>
                </TableBody>
              </Table>
            </div>
            <div v-else-if="selectedQuestionnaireId" class="flex justify-center p-8 text-muted-foreground">
              <Loader2 class="h-8 w-8 animate-spin" />
            </div>
            <div v-else
              class="flex flex-col items-center justify-center p-12 text-muted-foreground border-2 border-dashed rounded-lg">
              <FileSpreadsheet class="h-12 w-12 mb-4 opacity-50" />
              <p>{{ t('analysis.selectToViewData') }}</p>
            </div>
          </CardContent>
        </Card>
      </TabsContent>

      <!-- Games Tab -->
      <TabsContent value="games" class="space-y-4">
        <Card>
          <CardHeader>
            <CardTitle>{{ t('analysis.gameStats') }}</CardTitle>
            <CardDescription>{{ t('analysis.gameStatsDesc') }}</CardDescription>
          </CardHeader>
          <CardContent class="space-y-6">
            <div class="flex items-center gap-4">
              <div class="relative w-[300px]">
                <Search class="absolute left-2 top-2.5 h-4 w-4 text-muted-foreground" />
                <Input v-model="gameSearch" :placeholder="t('query.searchGames')" class="pl-8" />
              </div>
            </div>

            <div v-if="isGameScoresLoading" class="flex justify-center py-8">
              <Loader2 class="h-8 w-8 animate-spin" />
            </div>
            <Table v-else>
              <TableHeader>
                <TableRow>
                  <TableHead>{{ t('users.name') }}</TableHead>
                  <TableHead>{{ t('analysis.game') }}</TableHead>
                  <TableHead>{{ t('analysis.score') }}</TableHead>
                  <TableHead>{{ t('analysis.date') }}</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                <TableRow v-for="score in filteredGameScores" :key="score.id">
                  <TableCell class="font-medium">{{ score.user?.username }}</TableCell>
                  <TableCell>{{ score.gameName }}</TableCell>
                  <TableCell>{{ score.score }}</TableCell>
                  <TableCell>{{ new Date(score.playedAt).toLocaleString() }}</TableCell>
                </TableRow>
                <TableRow v-if="filteredGameScores.length === 0">
                  <TableCell colspan="4" class="text-center py-6 text-muted-foreground">
                    {{ t('analysis.noGameRecords') }}
                  </TableCell>
                </TableRow>
              </TableBody>
            </Table>
          </CardContent>
        </Card>
      </TabsContent>
    </Tabs>
  </div>
</template>
