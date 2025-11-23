<script setup lang="ts">
import { onMounted } from 'vue'
import { useDashboardStore } from '@/stores/dashboard'
import { storeToRefs } from 'pinia'
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from '@/components/ui/card'
import { Users, Activity, CreditCard, Server } from 'lucide-vue-next'
import OverviewChart from '@/components/OverviewChart.vue'
import { useI18n } from 'vue-i18n'

const { t } = useI18n()
const dashboardStore = useDashboardStore()
const { stats, chartData, isLoading } = storeToRefs(dashboardStore)

onMounted(() => {
  dashboardStore.fetchStats()
  dashboardStore.fetchChartData()
})
</script>

<template>
  <div class="flex-1 space-y-4 p-8 pt-6">
    <div class="flex items-center justify-between space-y-2">
      <h2 class="text-3xl font-bold tracking-tight">{{ t('dashboard.title') }}</h2>
    </div>

    <div v-if="isLoading" class="flex items-center justify-center h-64">
      <div class="animate-spin rounded-full h-8 w-8 border-b-2 border-primary"></div>
    </div>

    <div v-else class="space-y-4">
      <div class="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
        <Card>
          <CardHeader class="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle class="text-sm font-medium">{{ t('dashboard.totalUsers') }}</CardTitle>
            <Users class="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div class="text-2xl font-bold">{{ stats?.totalUsers || 0 }}</div>
          </CardContent>
        </Card>
        <Card>
          <CardHeader class="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle class="text-sm font-medium">{{ t('dashboard.activeTasks') }}</CardTitle>
            <Activity class="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div class="text-2xl font-bold">{{ stats?.activeTasks || 0 }}</div>
          </CardContent>
        </Card>
        <Card>
          <CardHeader class="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle class="text-sm font-medium">{{ t('dashboard.completedAssessments') }}</CardTitle>
            <CreditCard class="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div class="text-2xl font-bold">{{ stats?.completedAssessments || 0 }}</div>
          </CardContent>
        </Card>
        <Card>
          <CardHeader class="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle class="text-sm font-medium">{{ t('dashboard.systemHealth') }}</CardTitle>
            <Server class="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div class="text-2xl font-bold">{{ stats?.systemHealth || 'Unknown' }}</div>
          </CardContent>
        </Card>
      </div>

      <div class="grid gap-4 md:grid-cols-2 lg:grid-cols-7">
        <Card class="col-span-4">
          <CardHeader>
            <CardTitle>{{ t('dashboard.overview') }}</CardTitle>
          </CardHeader>
          <CardContent class="pl-2">
            <OverviewChart v-if="chartData" :data="chartData" />
            <div v-else class="h-[300px] flex items-center justify-center text-muted-foreground">
              No data available
            </div>
          </CardContent>
        </Card>
        <Card class="col-span-3">
          <CardHeader>
            <CardTitle>{{ t('dashboard.recentActivity') }}</CardTitle>
            <CardDescription>
              {{ t('dashboard.recentActivityDesc') }}
            </CardDescription>
          </CardHeader>
          <CardContent>
            <!-- Recent Sales / Activity List -->
          </CardContent>
        </Card>
      </div>
    </div>
  </div>
</template>
