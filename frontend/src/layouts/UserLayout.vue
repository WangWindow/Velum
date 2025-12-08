<script setup lang="ts">
import { ref } from 'vue'
import { RouterView, RouterLink, useRoute } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { Button } from '@/components/ui/button'
import { Avatar, AvatarFallback, AvatarImage } from '@/components/ui/avatar'
import { Sheet, SheetContent, SheetTrigger } from '@/components/ui/sheet'
import { Menu, LayoutDashboard, ClipboardList, MessageSquare, Settings, LogOut, ChevronLeft, ChevronRight, User, Smile, Cat, Dog, Ghost, Bot, Zap, Star } from 'lucide-vue-next'
import ThemeToggle from '@/components/ThemeToggle.vue'
import LanguageToggle from '@/components/LanguageToggle.vue'
import VelumLogo from '@/components/icons/VelumLogo.vue'
import { useI18n } from 'vue-i18n'

const { t } = useI18n()
const route = useRoute()
const authStore = useAuthStore()
const isCollapsed = ref(false)
const showUserMenu = ref(false)
const isMobileMenuOpen = ref(false)

const avatarMap: Record<string, any> = {
  User, Smile, Cat, Dog, Ghost, Bot, Zap, Star
}

const navigation = [
  { name: 'nav.dashboard', href: '/user/dashboard', icon: LayoutDashboard },
  { name: 'nav.assessment', href: '/user/assessment', icon: ClipboardList },
  { name: 'nav.chat', href: '/user/chat', icon: MessageSquare },
  { name: 'nav.games', href: '/user/games', icon: Zap },
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
  <div class="h-screen bg-background flex flex-col overflow-hidden">
    <header class="flex-none flex h-16 items-center gap-4 border-b bg-background px-6 z-30">
      <Sheet v-model:open="isMobileMenuOpen">
        <SheetTrigger as-child>
          <Button variant="outline" size="icon" class="shrink-0 md:hidden">
            <Menu class="h-5 w-5" />
          </Button>
        </SheetTrigger>
        <SheetContent side="left" class="w-[250px] sm:w-[300px]">
          <nav class="grid gap-6 text-lg font-medium">
            <RouterLink to="#" class="flex items-center gap-2 text-lg font-semibold" @click="isMobileMenuOpen = false">
              <VelumLogo class="h-6 w-6" />
              <span class="sr-only">{{ t('app.userTitle') }}</span>
              {{ t('app.userTitle') }}
            </RouterLink>
            <RouterLink v-for="item in navigation" :key="item.name" :to="item.href"
              class="flex items-center gap-4 px-2.5 text-muted-foreground hover:text-foreground"
              :class="{ 'text-foreground': route.path === item.href }" @click="isMobileMenuOpen = false">
              <component :is="item.icon" class="h-5 w-5" />
              {{ t(item.name) }}
            </RouterLink>
          </nav>
        </SheetContent>
      </Sheet>

      <!-- Desktop Logo -->
      <RouterLink to="/" class="hidden md:flex items-center gap-2 font-semibold mr-4 min-w-[180px]">
        <VelumLogo class="h-6 w-6" />
        <span class="truncate">{{ t('app.userTitle') }}</span>
      </RouterLink>

      <div class="flex w-full items-center gap-4 md:ml-auto md:gap-2 lg:gap-4">
        <div class="ml-auto flex-1 sm:flex-initial flex items-center gap-2 md:hidden">
          <VelumLogo class="h-6 w-6" />
          <span class="font-semibold">{{ t('app.userTitle') }}</span>
        </div>
        <div class="ml-auto flex-1 sm:flex-initial hidden md:block">
          <!-- Search or other header items -->
        </div>
        <LanguageToggle />
        <ThemeToggle />

        <div class="relative">
          <Button variant="ghost" class="relative h-8 w-8 rounded-full" @click="showUserMenu = !showUserMenu">
            <Avatar class="h-8 w-8">
              <AvatarImage src="" alt="@user" />
              <AvatarFallback>
                <component v-if="authStore.user?.avatar && avatarMap[authStore.user.avatar]"
                  :is="avatarMap[authStore.user.avatar]" class="h-5 w-5" />
                <span v-else>{{ authStore.user?.username?.substring(0, 2).toUpperCase() || 'U' }}</span>
              </AvatarFallback>
            </Avatar>
          </Button>

          <!-- Backdrop to close menu -->
          <div v-if="showUserMenu" class="fixed inset-0 z-40" @click="showUserMenu = false"></div>

          <!-- Dropdown Menu -->
          <div v-if="showUserMenu"
            class="absolute right-0 mt-2 w-56 rounded-md border bg-popover p-1 text-popover-foreground shadow-md z-50">
            <div class="px-2 py-1.5 text-sm font-semibold">
              <span v-if="authStore.user?.fullName">{{ authStore.user.fullName }}</span>
            </div>
            <div class="px-2 py-1.5 text-xs text-muted-foreground truncate">
              <span v-if="authStore.user?.username">ID: {{ authStore.user.username }}</span>
            </div>
            <div class="px-2 py-1.5 text-xs text-muted-foreground truncate">
              <span v-if="authStore.user?.email">{{ authStore.user.email }}</span>
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
    <div class="flex flex-1 overflow-hidden">
      <aside class="hidden flex-col border-r bg-background md:flex transition-all duration-300 overflow-y-auto"
        :class="isCollapsed ? 'w-[60px]' : 'w-[250px]'">
        <nav class="flex flex-col gap-2 p-2 text-sm font-medium flex-1">
          <RouterLink v-for="item in navigation" :key="item.name" :to="item.href"
            class="flex items-center rounded-lg py-2 text-muted-foreground transition-all hover:text-primary overflow-hidden"
            :class="[
              route.path === item.href ? 'bg-muted text-primary' : '',
              isCollapsed ? 'justify-center px-2' : 'px-4'
            ]">
            <component :is="item.icon" class="h-4 w-4 shrink-0" />
            <span class="whitespace-nowrap overflow-hidden transition-all duration-300"
              :class="isCollapsed ? 'max-w-0 opacity-0 ml-0' : 'max-w-[150px] opacity-100 ml-3'">
              {{ t(item.name) }}
            </span>
          </RouterLink>
        </nav>
        <div class="flex items-center justify-end p-2 border-t">
          <Button variant="ghost" size="icon" @click="toggleSidebar"
            class="flex items-center gap-3 rounded-lg px-3 py-2 text-muted-foreground transition-all hover:text-primary w-full justify-center">
            <ChevronLeft v-if="!isCollapsed" class="h-4 w-4" />
            <ChevronRight v-else class="h-4 w-4" />
          </Button>
        </div>
      </aside>
      <main class="flex-1 overflow-y-auto p-4 md:p-6">
        <RouterView />
      </main>
    </div>
  </div>
</template>
