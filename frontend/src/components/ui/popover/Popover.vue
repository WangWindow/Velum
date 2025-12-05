<script setup lang="ts">
import type { PopoverRootEmits, PopoverRootProps } from "radix-vue"
import { PopoverRoot, useForwardPropsEmits } from "radix-vue"

const props = defineProps<PopoverRootProps>()
const emits = defineEmits<PopoverRootEmits>()

const forwarded = useForwardPropsEmits(props, emits)
</script>

<template>
  <PopoverRoot v-slot="slotProps" data-slot="popover" v-bind="forwarded">
    <slot v-bind="slotProps" />
  </PopoverRoot>
</template>
