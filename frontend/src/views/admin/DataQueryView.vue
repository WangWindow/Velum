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
import { exportCsv } from '@/lib/exportCsv'
// 导出CSV功能
function exportToCsv() {
  if (!exportData.value || !exportData.value.columns) return
  const columns = exportData.value.columns
  const rows = filteredExportRows.value
  if (!rows.length) return
  // 生成CSV内容
  const csvRows = []
  csvRows.push(columns.map(col => `"${col.replace(/"/g, '""')}"`).join(','))
  for (const row of rows) {
    csvRows.push(
      columns.map(col => {
        const val = row[col] ?? ''
        return `"${String(val).replace(/"/g, '""')}"`
      }).join(',')
    )
  }
  const csvContent = '\uFEFF' + csvRows.join('\r\n') // BOM for Excel兼容
  const filename = `export_${selectedQuestionnaireId.value || 'data'}.csv`
  exportCsv(filename, csvContent)
}

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
// 查询条件缓存，只有点击搜索按钮时才更新
const querySearch = ref('')
const queryFilters = ref({
  minScore: '',
  maxScore: '',
  dateFrom: '',
  dateTo: ''
})

function handleSearch() {
  querySearch.value = dataSearch.value
  queryFilters.value = { ...advancedFilters.value }
}

// State for Games Tab
const gameScores = ref<any[]>([])
const isGameScoresLoading = ref(false)
const gameSearch = ref('')

// Computed for Assessments
const filteredExportRows = computed(() => {
  if (!exportData.value) return []
  let rows = exportData.value.rows

  // 基础搜索
  if (querySearch.value) {
    const search = querySearch.value.toLowerCase()
    rows = rows.filter(row =>
      Object.values(row).some(val => String(val).toLowerCase().includes(search))
    )
  }

  // 高级筛选：分数和日期
  const minScore = queryFilters.value.minScore !== '' ? Number(queryFilters.value.minScore) : null
  const maxScore = queryFilters.value.maxScore !== '' ? Number(queryFilters.value.maxScore) : null
  const dateFrom = queryFilters.value.dateFrom ? new Date(queryFilters.value.dateFrom) : null
  const dateTo = queryFilters.value.dateTo ? new Date(queryFilters.value.dateTo) : null

  // 尝试自动识别分数和日期字段名
  // 优先找名为Score/score/分数，Date/date/日期的字段
  const scoreKey = exportData.value.columns.find(col => /score|分数/i.test(col))
  const dateKey = exportData.value.columns.find(col => /date|日期/i.test(col))

  if (scoreKey && (minScore !== null || maxScore !== null)) {
    rows = rows.filter(row => {
      const val = Number(row[scoreKey])
      if (isNaN(val)) return false
      if (minScore !== null && val < minScore) return false
      if (maxScore !== null && val > maxScore) return false
      return true
    })
  }
  if (dateKey && (dateFrom || dateTo)) {
    rows = rows.filter(row => {
      const d = row[dateKey] ? new Date(row[dateKey]) : null
      if (!d || isNaN(d.getTime())) return false
      if (dateFrom && d < dateFrom) return false
      if (dateTo) {
        // 包含当天
        const to = new Date(dateTo)
        to.setHours(23, 59, 59, 999)
        if (d > to) return false
      }
      return true
    })
  }
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
              <div class="w-full md:w-62.5">
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


              <div class="relative w-full md:w-75 flex items-center gap-2">
                <Search class="absolute left-2 top-2.5 h-4 w-4 text-muted-foreground" />
                <Input v-model="dataSearch" :placeholder="t('analysis.searchData')" class="pl-8"
                  @keyup.enter="handleSearch" />
                <Button variant="secondary" class="ml-2" @click="handleSearch">
                  {{ t('query.search') }}
                </Button>
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

              <Button variant="outline" class="ml-auto" @click="exportToCsv" :disabled="!exportData">
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
              <div class="relative w-75">
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
