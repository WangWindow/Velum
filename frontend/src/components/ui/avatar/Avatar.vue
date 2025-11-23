<script setup lang="ts">
import { type HTMLAttributes } from 'vue'
import { AvatarRoot } from 'radix-vue'
import { cn } from '@/lib/utils'

const props = withDefaults(
  defineProps<{
    class?: HTMLAttributes['class']
    size?: 'sm' | 'base' | 'lg'
    shape?: 'circle' | 'square'
  }>(),
  {
    size: 'base',
    shape: 'circle',
  },
)
</script>

<template>
  <AvatarRoot :class="cn(
    'relative flex h-10 w-10 shrink-0 overflow-hidden',
    shape === 'circle' ? 'rounded-full' : 'rounded-md',
    props.class,
  )
    ">
    <slot />
  </AvatarRoot>
</template>
