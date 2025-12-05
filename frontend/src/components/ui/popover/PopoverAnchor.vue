<script setup lang="ts">
import type { PopoverAnchorProps } from "radix-vue"
import { PopoverAnchor } from "radix-vue"

const props = defineProps<PopoverAnchorProps>()
</script>

<template>
  <PopoverAnchor data-slot="popover-anchor" v-bind="props">
    <slot />
  </PopoverAnchor>
</template>
