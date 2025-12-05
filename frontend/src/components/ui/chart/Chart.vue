<script setup lang="ts">
import { computed, provide } from 'vue'
import { useDark } from '@vueuse/core'
import VChart, { THEME_KEY } from 'vue-echarts'
import { use } from 'echarts/core'
import { CanvasRenderer } from 'echarts/renderers'
import { LineChart, BarChart, PieChart } from 'echarts/charts'
import {
  GridComponent,
  TooltipComponent,
  LegendComponent,
  TitleComponent
} from 'echarts/components'
import { cn } from '@/lib/utils'

// Register common ECharts components
use([
  CanvasRenderer,
  LineChart,
  BarChart,
  PieChart,
  GridComponent,
  TooltipComponent,
  LegendComponent,
  TitleComponent
])

const props = withDefaults(defineProps<{
  option: any
  height?: string
  width?: string
  class?: string
}>(), {
  height: '100%',
  width: '100%',
  class: ''
})

const isDark = useDark()
provide(THEME_KEY, computed(() => isDark.value ? 'dark' : 'light'))

const mergedOption = computed(() => {
  const textColor = isDark.value ? '#94a3b8' : '#64748b'
  const borderColor = isDark.value ? '#334155' : '#e2e8f0'
  const splitLineColor = isDark.value ? 'rgba(255, 255, 255, 0.1)' : 'rgba(0, 0, 0, 0.1)'
  const tooltipBg = isDark.value ? '#1e293b' : '#ffffff'

  const baseOption = {
    backgroundColor: 'transparent',
    textStyle: {
      fontFamily: 'inherit'
    },
    tooltip: {
      backgroundColor: tooltipBg,
      borderColor: borderColor,
      textStyle: {
        color: textColor
      },
      padding: [8, 12],
      extraCssText: 'box-shadow: 0 4px 6px -1px rgb(0 0 0 / 0.1), 0 2px 4px -2px rgb(0 0 0 / 0.1); border-radius: 0.5rem;'
    },
    legend: {
      textStyle: {
        color: textColor
      }
    }
  }

  // Helper to merge axis styles if axis exists in props.option
  const mergeAxis = (axisName: 'xAxis' | 'yAxis', defaultAxis: any) => {
    if (!props.option[axisName]) return {}

    const userAxis = props.option[axisName]
    const isArray = Array.isArray(userAxis)

    if (isArray) {
      return {
        [axisName]: userAxis.map((axis: any) => ({ ...defaultAxis, ...axis }))
      }
    }

    return {
      [axisName]: { ...defaultAxis, ...userAxis }
    }
  }

  const defaultXAxis = {
    axisLine: { show: false },
    axisTick: { show: false },
    axisLabel: { color: textColor },
    splitLine: { show: false }
  }

  const defaultYAxis = {
    axisLine: { show: false },
    axisTick: { show: false },
    axisLabel: { color: textColor },
    splitLine: {
      lineStyle: { color: splitLineColor }
    }
  }

  return {
    ...baseOption,
    ...props.option,
    tooltip: { ...baseOption.tooltip, ...props.option.tooltip },
    legend: { ...baseOption.legend, ...props.option.legend },
    ...mergeAxis('xAxis', defaultXAxis),
    ...mergeAxis('yAxis', defaultYAxis)
  }
})
</script>

<template>
  <div :class="cn('w-full h-full', props.class)" :style="{ height, width }">
    <v-chart :option="mergedOption" autoresize class="chart" />
  </div>
</template>

<style scoped>
.chart {
  height: 100%;
  width: 100%;
}
</style>
