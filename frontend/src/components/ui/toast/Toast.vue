<script setup lang="ts">
import { ToastRoot, useForwardPropsEmits } from 'radix-vue'
import { cn } from '@/lib/utils'
import { computed } from 'vue'
import { cva, type VariantProps } from 'class-variance-authority'

const toastVariants = cva(
  'group pointer-events-auto relative flex w-full items-center justify-between space-x-4 overflow-hidden rounded-md border p-6 pr-8 shadow-lg transition-all data-[swipe=cancel]:translate-x-0 data-[swipe=end]:translate-x-[var(--radix-toast-swipe-end-x)] data-[swipe=move]:translate-x-[var(--radix-toast-swipe-move-x)] data-[swipe=move]:transition-none data-[state=open]:animate-in data-[state=closed]:animate-out data-[swipe=end]:animate-out data-[state=closed]:fade-out-80 data-[state=closed]:slide-out-to-right-full data-[state=open]:slide-in-from-top-full data-[state=open]:sm:slide-in-from-bottom-full',
  {
    variants: {
      variant: {
        default: 'border bg-background text-foreground',
        destructive:
          'destructive group border-destructive bg-destructive text-destructive-foreground',
      },
    },
    defaultVariants: {
      variant: 'default',
    },
  },
)

type ToastVariants = VariantProps<typeof toastVariants>

interface Props {
  class?: string
  variant?: ToastVariants['variant']
  onOpenChange?: (value: boolean) => void
  open?: boolean
  defaultOpen?: boolean
  duration?: number
  type?: 'foreground' | 'background'
}

const props = defineProps<Props>()
const emits = defineEmits(['update:open', 'escapeKeyDown', 'pause', 'resume', 'swipeStart', 'swipeMove', 'swipeCancel', 'swipeEnd'])

const delegatedProps = computed(() => {
  const { class: _, ...delegated } = props

  return delegated
})

const forwarded = useForwardPropsEmits(delegatedProps, emits)
</script>

<template>
  <ToastRoot v-bind="forwarded" :class="cn(toastVariants({ variant }), props.class)">
    <slot />
  </ToastRoot>
</template>
