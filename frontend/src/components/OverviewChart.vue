<script setup lang="ts">
import { computed, ref, onMounted } from 'vue'
import { useMutationObserver } from '@vueuse/core'
import { Chart } from '@/components/ui/chart'

const props = defineProps<{
  data: {
    labels: string[]
    datasets: any[]
  }
}>()

const isDark = ref(false)

onMounted(() => {
  isDark.value = document.documentElement.classList.contains('dark')
})

useMutationObserver(
  typeof document !== 'undefined' ? document.documentElement : null,
  () => {
    isDark.value = document.documentElement.classList.contains('dark')
  },
  {
    attributes: true,
    attributeFilter: ['class'],
  }
)

const option = computed(() => {
  return {
    grid: {
      left: '3%',
      right: '4%',
      bottom: '3%',
      containLabel: true
    },
    xAxis: {
      type: 'category',
      boundaryGap: false,
      data: props.data.labels,
    },
    yAxis: {
      type: 'value',
    },
    series: props.data.datasets.map(dataset => ({
      name: dataset.label,
      type: 'line',
      smooth: true,
      showSymbol: false,
      data: dataset.data,
      itemStyle: {
        color: isDark.value ? '#38bdf8' : '#0284c7'
      },
      lineStyle: {
        width: 3
      },
      areaStyle: {
        opacity: 0.1,
        color: isDark.value ? '#38bdf8' : '#0284c7'
      }
    }))
  }
})
</script>

<template>
  <div class="h-75 w-full">
    <Chart :option="option" />
  </div>
</template>
