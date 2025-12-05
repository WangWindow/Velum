<script setup lang="ts">
import { onMounted } from 'vue'
import { useDashboardStore } from '@/stores/dashboard'
import { storeToRefs } from 'pinia'
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from '@/components/ui/card'
import { Users, Activity, CreditCard, Server, FileText } from 'lucide-vue-next'
import OverviewChart from '@/components/OverviewChart.vue'
import { useI18n } from 'vue-i18n'
import { useRouter } from 'vue-router'
import { Button } from '@/components/ui/button'
import { Avatar, AvatarFallback } from '@/components/ui/avatar'

const { t } = useI18n()
const router = useRouter()
const dashboardStore = useDashboardStore()
const { stats, chartData, isLoading } = storeToRefs(dashboardStore)

onMounted(() => {
  dashboardStore.fetchStats()
  dashboardStore.fetchChartData()
})

const navigateToUsers = () => router.push('/admin/users')
const navigateToAnalysis = () => router.push('/admin/analysis')
</script>

<template>
  <div class="space-y-4">
    <div class="flex items-center justify-between space-y-2">
      <h2 class="text-3xl font-bold tracking-tight">{{ t('dashboard.title') }}</h2>
      <div class="flex items-center space-x-2">
        <Button @click="navigateToUsers">
          <Users class="mr-2 h-4 w-4" />
          {{ t('users.title') }}
        </Button>
        <Button variant="outline" @click="navigateToAnalysis">
          <FileText class="mr-2 h-4 w-4" />
          {{ t('nav.analysis') }}
        </Button>
      </div>
    </div>

    <div v-if="isLoading" class="flex items-center justify-center h-64">
      <div class="animate-spin rounded-full h-8 w-8 border-b-2 border-primary"></div>
    </div>

    <div v-else class="space-y-4">
      <div class="grid gap-4 grid-cols-1 md:grid-cols-2 lg:grid-cols-4">
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

      <div class="grid gap-4 grid-cols-1 md:grid-cols-2 lg:grid-cols-7">
        <Card class="col-span-1 md:col-span-2 lg:col-span-4">
          <CardHeader>
            <CardTitle>{{ t('dashboard.overview') }}</CardTitle>
          </CardHeader>
          <CardContent class="pl-2">
            <OverviewChart v-if="chartData" :data="chartData" />
            <div v-else class="h-[300px] flex items-center justify-center text-muted-foreground">
              {{ t('dashboard.noData') }}
            </div>
          </CardContent>
        </Card>
        <Card class="col-span-1 md:col-span-2 lg:col-span-3">
          <CardHeader>
            <CardTitle>{{ t('dashboard.recentActivity') }}</CardTitle>
            <CardDescription>
              {{ t('dashboard.recentActivityDesc') }}
            </CardDescription>
          </CardHeader>
          <CardContent>
            <div v-if="!stats?.recentAssessments?.length" class="text-center py-6 text-muted-foreground">
              {{ t('dashboard.noRecentActivity') }}
            </div>
            <div v-else class="space-y-4">
              <div v-for="assessment in stats.recentAssessments" :key="assessment.id"
                class="flex items-center justify-between border-b pb-2 last:border-0">
                <div class="flex items-center gap-3">
                  <Avatar class="h-9 w-9">
                    <AvatarFallback>{{ assessment.userName.substring(0, 2).toUpperCase() }}</AvatarFallback>
                  </Avatar>
                  <div>
                    <p class="text-sm font-medium leading-none">{{ assessment.userName }}</p>
                    <p class="text-xs text-muted-foreground mt-1">
                      {{ assessment.questionnaireTitle }}
                    </p>
                  </div>
                </div>
                <div class="text-right">
                  <p class="text-sm font-bold">{{ assessment.score }}</p>
                  <p class="text-xs text-muted-foreground">{{ new Date(assessment.date).toLocaleDateString() }}</p>
                </div>
              </div>
              <Button variant="ghost" class="w-full mt-2" @click="navigateToAnalysis">
                {{ t('dashboard.viewAll') }}
              </Button>
            </div>
          </CardContent>
        </Card>
      </div>
    </div>
  </div>
</template>
