<script setup lang="ts">
import { ref } from 'vue'
import { RouterView, RouterLink, useRoute } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { Button } from '@/components/ui/button'
import { Avatar, AvatarFallback, AvatarImage } from '@/components/ui/avatar'
import { Sheet, SheetContent, SheetTrigger } from '@/components/ui/sheet'
import { Menu, LayoutDashboard, ClipboardList, MessageSquare, Settings, LogOut, ChevronLeft, ChevronRight, User } from 'lucide-vue-next'
import ThemeToggle from '@/components/ThemeToggle.vue'
import LanguageToggle from '@/components/LanguageToggle.vue'
import { useI18n } from 'vue-i18n'

const { t } = useI18n()
const route = useRoute()
const authStore = useAuthStore()
const isCollapsed = ref(false)
const showUserMenu = ref(false)

const navigation = [
  { name: 'nav.dashboard', href: '/user/dashboard', icon: LayoutDashboard },
  { name: 'nav.assessment', href: '/user/assessment', icon: ClipboardList },
  { name: 'nav.chat', href: '/user/chat', icon: MessageSquare },
  { name: 'nav.settings', href: '/user/settings', icon: Settings },
]

const handleLogout = () => {
  authStore.logout()
}

const toggleSidebar = () => {
  isCollapsed.value = !isCollapsed.value
}
</script>

<template>
  <div class="min-h-screen bg-background flex flex-col">
    <header class="sticky top-0 z-30 flex h-16 items-center gap-4 border-b bg-background px-6">
      <Sheet>
        <SheetTrigger as-child>
          <Button variant="outline" size="icon" class="shrink-0 md:hidden">
            <Menu class="h-5 w-5" />
            <span class="sr-only">Toggle navigation menu</span>
          </Button>
        </SheetTrigger>
        <SheetContent side="left" class="w-[250px] sm:w-[300px]">
          <nav class="grid gap-6 text-lg font-medium">
            <RouterLink to="#" class="flex items-center gap-2 text-lg font-semibold">
              <span class="sr-only">Velum User</span>
              Velum User
            </RouterLink>
            <RouterLink v-for="item in navigation" :key="item.name" :to="item.href"
              class="flex items-center gap-4 px-2.5 text-muted-foreground hover:text-foreground"
              :class="{ 'text-foreground': route.path === item.href }">
              <component :is="item.icon" class="h-5 w-5" />
              {{ t(item.name) }}
            </RouterLink>
          </nav>
        </SheetContent>
      </Sheet>
      <div class="flex w-full items-center gap-4 md:ml-auto md:gap-2 lg:gap-4">
        <div class="ml-auto flex-1 sm:flex-initial">
          <!-- Search or other header items -->
        </div>
        <LanguageToggle />
        <ThemeToggle />

        <div class="relative">
          <Button variant="ghost" class="relative h-8 w-8 rounded-full" @click="showUserMenu = !showUserMenu">
            <Avatar class="h-8 w-8">
              <AvatarImage src="" alt="@user" />
              <AvatarFallback>{{ authStore.user?.username?.substring(0, 2).toUpperCase() || 'U' }}</AvatarFallback>
            </Avatar>
          </Button>

          <!-- Backdrop to close menu -->
          <div v-if="showUserMenu" class="fixed inset-0 z-40" @click="showUserMenu = false"></div>

          <!-- Dropdown Menu -->
          <div v-if="showUserMenu"
            class="absolute right-0 mt-2 w-56 rounded-md border bg-popover p-1 text-popover-foreground shadow-md z-50">
            <div class="px-2 py-1.5 text-sm font-semibold">
              {{ authStore.user?.fullName || authStore.user?.username }}
            </div>
            <div class="px-2 py-1.5 text-xs text-muted-foreground truncate">
              {{ authStore.user?.email }}
            </div>
            <div class="h-px bg-muted my-1" />
            <RouterLink to="/user/settings"
              class="relative flex cursor-pointer select-none items-center rounded-sm px-2 py-1.5 text-sm outline-none hover:bg-accent hover:text-accent-foreground"
              @click="showUserMenu = false">
              <Settings class="mr-2 h-4 w-4" />
              <span>{{ t('nav.settings') }}</span>
            </RouterLink>
            <div class="h-px bg-muted my-1" />
            <div
              class="relative flex cursor-pointer select-none items-center rounded-sm px-2 py-1.5 text-sm outline-none hover:bg-accent hover:text-accent-foreground"
              @click="handleLogout">
              <LogOut class="mr-2 h-4 w-4" />
              <span>{{ t('common.logout') }}</span>
            </div>
          </div>
        </div>
      </div>
    </header>
    <div class="flex flex-1">
      <aside class="hidden flex-col border-r bg-background md:flex transition-all duration-300"
        :class="isCollapsed ? 'w-[60px]' : 'w-[250px]'">
        <div class="flex items-center justify-end p-2">
          <Button variant="ghost" size="icon" @click="toggleSidebar" class="h-6 w-6">
            <ChevronLeft v-if="!isCollapsed" class="h-4 w-4" />
            <ChevronRight v-else class="h-4 w-4" />
          </Button>
        </div>
        <nav class="grid items-start gap-2 p-2 text-sm font-medium">
          <RouterLink v-for="item in navigation" :key="item.name" :to="item.href"
            class="flex items-center gap-3 rounded-lg px-3 py-2 text-muted-foreground transition-all hover:text-primary"
            :class="{ 'bg-muted text-primary': route.path === item.href, 'justify-center': isCollapsed }">
            <component :is="item.icon" class="h-4 w-4" />
            <span v-if="!isCollapsed">{{ t(item.name) }}</span>
          </RouterLink>
        </nav>
      </aside>
      <main class="flex-1 p-4 md:p-6">
        <RouterView />
      </main>
    </div>
  </div>
</template>
