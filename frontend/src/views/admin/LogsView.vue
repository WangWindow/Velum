<script setup lang="ts">
import { ref, onMounted, watch } from 'vue'
import { useI18n } from 'vue-i18n'
import api from '@/lib/api'
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from '@/components/ui/card'
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table'
import { Badge } from '@/components/ui/badge'
import { Loader2, RefreshCw, Trash2, Search } from 'lucide-vue-next'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Checkbox } from '@/components/ui/checkbox'
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select'
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog'

const { t } = useI18n()
const logs = ref<any[]>([])
const isLoading = ref(false)
const searchQuery = ref('')
const selectedLevel = ref<string>('all')
const selectedLogs = ref<number[]>([])

const showDeleteDialog = ref(false)
const logToDelete = ref<number | null>(null)
const showBatchDeleteDialog = ref(false)
const isDeleting = ref(false)

const fetchLogs = async () => {
  isLoading.value = true
  try {
    const params: any = {}
    if (searchQuery.value) params.search = searchQuery.value
    if (selectedLevel.value && selectedLevel.value !== 'all') params.level = selectedLevel.value

    const response = await api.get('/logs', { params })
    logs.value = response.data
    selectedLogs.value = [] // Clear selection on refresh
  } catch (error) {
    console.error('Failed to fetch logs:', error)
  } finally {
    isLoading.value = false
  }
}

// Debounce search
let searchTimeout: any
watch(searchQuery, () => {
  clearTimeout(searchTimeout)
  searchTimeout = setTimeout(fetchLogs, 500)
})

watch(selectedLevel, () => {
  fetchLogs()
})

onMounted(() => {
  fetchLogs()
})

const getLevelVariant = (level: string) => {
  switch (level.toLowerCase()) {
    case 'error': return 'destructive'
    case 'warning': return 'secondary' // or yellow if available
    default: return 'default'
  }
}

const toggleSelectAll = (checked: boolean) => {
  if (checked) {
    selectedLogs.value = logs.value.map(l => l.id)
  } else {
    selectedLogs.value = []
  }
}

const toggleSelectLog = (id: number, checked: boolean) => {
  if (checked) {
    selectedLogs.value.push(id)
  } else {
    selectedLogs.value = selectedLogs.value.filter(logId => logId !== id)
  }
}

const confirmDeleteLog = (id: number) => {
  logToDelete.value = id
  showDeleteDialog.value = true
}

const handleDeleteLog = async () => {
  if (!logToDelete.value) return

  isDeleting.value = true
  try {
    await api.delete(`/logs/${logToDelete.value}`)
    await fetchLogs()
    showDeleteDialog.value = false
    logToDelete.value = null
  } catch (error) {
    console.error('Failed to delete log:', error)
  } finally {
    isDeleting.value = false
  }
}

const confirmDeleteSelected = () => {
  if (selectedLogs.value.length === 0) return
  showBatchDeleteDialog.value = true
}

const handleBatchDelete = async () => {
  isDeleting.value = true
  try {
    await api.delete('/logs', { data: selectedLogs.value })
    await fetchLogs()
    showBatchDeleteDialog.value = false
    selectedLogs.value = []
  } catch (error) {
    console.error('Failed to delete logs:', error)
  } finally {
    isDeleting.value = false
  }
}
</script><template>
  <div class="space-y-6">
    <div class="flex items-center justify-between">
      <div>
        <h2 class="text-3xl font-bold tracking-tight">{{ t('logs.title') }}</h2>
        <p class="text-muted-foreground">{{ t('logs.description') }}</p>
      </div>
      <div class="flex items-center gap-2">
        <Button v-if="selectedLogs.length > 0" variant="destructive" size="sm" @click="confirmDeleteSelected">
          <Trash2 class="mr-2 h-4 w-4" />
          {{ t('common.delete') }} ({{ selectedLogs.length }})
        </Button>
        <Button variant="outline" size="sm" @click="fetchLogs" :disabled="isLoading">
          <RefreshCw class="mr-2 h-4 w-4" :class="{ 'animate-spin': isLoading }" />
          {{ t('common.refresh') }}
        </Button>
      </div>
    </div>

    <div class="flex items-center gap-4">
      <div class="relative w-full max-w-sm">
        <Search class="absolute left-2 top-2.5 h-4 w-4 text-muted-foreground" />
        <Input v-model="searchQuery" :placeholder="t('common.search') || 'Search...'" class="pl-8" />
      </div>
      <Select v-model="selectedLevel">
        <SelectTrigger class="w-[180px]">
          <SelectValue placeholder="Filter by level" />
        </SelectTrigger>
        <SelectContent>
          <SelectItem value="all">All Levels</SelectItem>
          <SelectItem value="Info">Info</SelectItem>
          <SelectItem value="Warning">Warning</SelectItem>
          <SelectItem value="Error">Error</SelectItem>
        </SelectContent>
      </Select>
    </div>

    <Card>
      <CardHeader>
        <CardTitle>{{ t('logs.systemLogs') }}</CardTitle>
        <CardDescription>{{ t('logs.recentActivity') }}</CardDescription>
      </CardHeader>
      <CardContent>
        <div v-if="isLoading && logs.length === 0" class="flex justify-center p-8">
          <Loader2 class="h-8 w-8 animate-spin" />
        </div>
        <div v-else-if="logs.length === 0" class="text-center p-8 text-muted-foreground">
          {{ t('logs.noLogs') }}
        </div>
        <Table v-else>
          <TableHeader>
            <TableRow>
              <TableHead class="w-[50px]">
                <Checkbox :checked="selectedLogs.length === logs.length && logs.length > 0"
                  @update:checked="toggleSelectAll" />
              </TableHead>
              <TableHead>{{ t('logs.level') }}</TableHead>
              <TableHead>{{ t('logs.timestamp') }}</TableHead>
              <TableHead>{{ t('logs.user') }}</TableHead>
              <TableHead>{{ t('logs.action') }}</TableHead>
              <TableHead>{{ t('logs.message') }}</TableHead>
              <TableHead class="w-[50px]"></TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            <TableRow v-for="log in logs" :key="log.id">
              <TableCell>
                <Checkbox :checked="selectedLogs.includes(log.id)"
                  @update:checked="(checked) => toggleSelectLog(log.id, checked)" />
              </TableCell>
              <TableCell>
                <Badge :variant="getLevelVariant(log.level)">{{ log.level }}</Badge>
              </TableCell>
              <TableCell class="whitespace-nowrap text-muted-foreground text-xs">
                {{ new Date(log.timestamp).toLocaleString() }}
              </TableCell>
              <TableCell>{{ log.userName || '-' }}</TableCell>
              <TableCell>{{ log.action || '-' }}</TableCell>
              <TableCell class="max-w-[300px] truncate" :title="log.message">{{ log.message }}</TableCell>
              <TableCell>
                <Button variant="ghost" size="icon" @click="confirmDeleteLog(log.id)">
                  <Trash2 class="h-4 w-4 text-muted-foreground hover:text-destructive" />
                </Button>
              </TableCell>
            </TableRow>
          </TableBody>
        </Table>
      </CardContent>
    </Card>

    <!-- Delete Single Log Dialog -->
    <Dialog v-model:open="showDeleteDialog">
      <DialogContent>
        <DialogHeader>
          <DialogTitle>{{ t('common.delete') }}</DialogTitle>
          <DialogDescription>
            {{ t('common.confirmDelete') }}
          </DialogDescription>
        </DialogHeader>
        <DialogFooter>
          <Button variant="outline" @click="showDeleteDialog = false">
            {{ t('common.cancel') }}
          </Button>
          <Button variant="destructive" @click="handleDeleteLog" :disabled="isDeleting">
            <Loader2 v-if="isDeleting" class="mr-2 h-4 w-4 animate-spin" />
            {{ t('common.delete') }}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>

    <!-- Batch Delete Dialog -->
    <Dialog v-model:open="showBatchDeleteDialog">
      <DialogContent>
        <DialogHeader>
          <DialogTitle>{{ t('common.delete') }}</DialogTitle>
          <DialogDescription>
            {{ t('common.confirmDeleteSelected', { count: selectedLogs.length }) }}
          </DialogDescription>
        </DialogHeader>
        <DialogFooter>
          <Button variant="outline" @click="showBatchDeleteDialog = false">
            {{ t('common.cancel') }}
          </Button>
          <Button variant="destructive" @click="handleBatchDelete" :disabled="isDeleting">
            <Loader2 v-if="isDeleting" class="mr-2 h-4 w-4 animate-spin" />
            {{ t('common.delete') }}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  </div>
</template>
