<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { useI18n } from 'vue-i18n'
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs'
import { Zap, Trophy, History, ChevronLeft, ChevronRight, User, Smile, Cat, Dog, Ghost, Bot, Star } from 'lucide-vue-next'
import api from '@/lib/api'

const { t } = useI18n()

const avatars: Record<string, any> = {
  'User': User,
  'Smile': Smile,
  'Cat': Cat,
  'Dog': Dog,
  'Ghost': Ghost,
  'Bot': Bot,
  'Zap': Zap,
  'Star': Star
}

// Reaction Game State
const gameState = ref<'idle' | 'waiting' | 'ready' | 'finished'>('idle')
const message = ref(t('games.clickToStart'))
const startTime = ref(0)
const reactionTime = ref(0)
const timeoutId = ref<any>(null)

// Leaderboard State
const myScores = ref<any[]>([])
const leaderboard = ref<any[]>([])

// Pagination & Sorting
const currentPage = ref(1)
const pageSize = 5
const sortBy = ref<'playedAt' | 'score'>('playedAt')

const sortedScores = computed(() => {
  const scores = [...myScores.value]
  if (sortBy.value === 'score') {
    return scores.sort((a, b) => a.score - b.score)
  } else {
    return scores.sort((a, b) => new Date(b.playedAt).getTime() - new Date(a.playedAt).getTime())
  }
})

const paginatedScores = computed(() => {
  const start = (currentPage.value - 1) * pageSize
  return sortedScores.value.slice(start, start + pageSize)
})

const totalPages = computed(() => Math.ceil(myScores.value.length / pageSize))

const nextPage = () => {
  if (currentPage.value < totalPages.value) currentPage.value++
}

const prevPage = () => {
  if (currentPage.value > 1) currentPage.value--
}

const toggleSort = () => {
  sortBy.value = sortBy.value === 'playedAt' ? 'score' : 'playedAt'
  currentPage.value = 1
}

const startGame = () => {
  gameState.value = 'waiting'
  message.value = t('games.waitForGreen')
  const delay = Math.floor(Math.random() * 2000) + 1000
  timeoutId.value = setTimeout(() => {
    gameState.value = 'ready'
    message.value = t('games.clickNow')
    startTime.value = Date.now()
  }, delay)
}

const handleClick = async () => {
  if (gameState.value === 'waiting') {
    clearTimeout(timeoutId.value)
    gameState.value = 'idle'
    message.value = t('games.tooEarly')
  } else if (gameState.value === 'ready') {
    const endTime = Date.now()
    reactionTime.value = endTime - startTime.value
    gameState.value = 'finished'
    message.value = `${reactionTime.value} ${t('games.ms')}`

    // Submit Score
    try {
      await api.post('/games/score', {
        gameName: 'ReactionTime',
        score: reactionTime.value, // Lower is better, but let's store raw ms
        duration: 0
      })
      fetchScores()
    } catch (e) {
      console.error(e)
    }
  }
}

const fetchScores = async () => {
  try {
    const myRes = await api.get('/games/my-scores')
    myScores.value = myRes.data.filter((s: any) => s.gameName === 'ReactionTime')

    const topRes = await api.get('/games/leaderboard/ReactionTime')
    leaderboard.value = topRes.data
  } catch (e) {
    console.error(e)
  }
}

onMounted(() => {
  fetchScores()
})
</script>

<template>
  <div class="space-y-6">
    <div>
      <h2 class="text-3xl font-bold tracking-tight">{{ t('games.title') }}</h2>
      <p class="text-muted-foreground">{{ t('games.description') }}</p>
    </div>

    <Tabs defaultValue="reaction" class="space-y-4">
      <TabsList>
        <TabsTrigger value="reaction">{{ t('games.reactionTime') }}</TabsTrigger>
        <!-- Add more games here later -->
      </TabsList>

      <TabsContent value="reaction" class="space-y-4">
        <div class="grid gap-4 md:grid-cols-2">
          <Card class="md:col-span-1">
            <CardHeader>
              <CardTitle class="flex items-center gap-2">
                <Zap class="h-5 w-5 text-yellow-500" />
                {{ t('games.reactionTest') }}
              </CardTitle>
              <CardDescription>{{ t('games.reactionDesc') }}</CardDescription>
            </CardHeader>
            <CardContent>
              <div class="h-64 rounded-lg flex items-center justify-center cursor-pointer transition-colors select-none"
                :class="{
                  'bg-secondary': gameState === 'idle' || gameState === 'finished',
                  'bg-red-500 text-white': gameState === 'waiting',
                  'bg-green-500 text-white': gameState === 'ready'
                }" @mousedown="handleClick">
                <div class="text-center">
                  <h3 class="text-3xl font-bold">{{ message }}</h3>
                  <p v-if="gameState === 'idle' || gameState === 'finished'" class="mt-2 text-sm text-muted-foreground">
                    {{ t('games.clickToStart') }}
                  </p>
                </div>
              </div>
              <div v-if="gameState === 'idle' || gameState === 'finished'" class="mt-4 flex justify-center">
                <Button @click="startGame">{{ t('games.clickToStart') }}</Button>
              </div>
            </CardContent>
          </Card>

          <div class="space-y-4">
            <Card>
              <CardHeader>
                <CardTitle class="flex items-center gap-2">
                  <Trophy class="h-5 w-5 text-yellow-500" />
                  {{ t('games.leaderboard') }}
                </CardTitle>
              </CardHeader>
              <CardContent>
                <div class="space-y-2">
                  <div v-for="(score, index) in leaderboard" :key="score.id"
                    class="flex items-center justify-between text-sm border-b pb-2 last:border-0">
                    <div class="flex items-center gap-2">
                      <span class="font-bold w-6">{{ index + 1 }}.</span>
                      <div v-if="score.avatar && avatars[score.avatar]"
                        class="w-6 h-6 flex items-center justify-center bg-muted rounded-full">
                        <component :is="avatars[score.avatar]" class="w-4 h-4" />
                      </div>
                      <div v-else class="w-6 h-6 flex items-center justify-center bg-muted rounded-full">
                        <span class="text-xs font-bold">{{ score.username.substring(0, 1).toUpperCase() }}</span>
                      </div>
                      <span>{{ score.username }}</span>
                    </div>
                    <span class="font-mono">{{ score.score }} {{ t('games.ms') }}</span>
                  </div>
                  <div v-if="leaderboard.length === 0" class="text-center text-muted-foreground py-4">
                    {{ t('analysis.noGameRecords') }}
                  </div>
                </div>
              </CardContent>
            </Card>

            <Card>
              <CardHeader class="flex flex-row items-center justify-between">
                <CardTitle class="flex items-center gap-2">
                  <History class="h-5 w-5" />
                  {{ t('games.myHistory') }}
                </CardTitle>
                <Button variant="ghost" size="sm" @click="toggleSort">
                  {{ sortBy === 'playedAt' ? t('games.sortByTime') : t('games.sortByScore') }}
                </Button>
              </CardHeader>
              <CardContent>
                <div class="space-y-2">
                  <div v-for="score in paginatedScores" :key="score.id"
                    class="flex items-center justify-between text-sm border-b pb-2 last:border-0">
                    <span class="text-muted-foreground">{{ new Date(score.playedAt).toLocaleString() }}</span>
                    <span class="font-mono">{{ score.score }} {{ t('games.ms') }}</span>
                  </div>
                </div>
                <div v-if="totalPages > 1" class="flex items-center justify-between mt-4">
                  <Button variant="outline" size="sm" @click="prevPage" :disabled="currentPage === 1">
                    <ChevronLeft class="h-4 w-4" />
                  </Button>
                  <span class="text-sm text-muted-foreground">{{ currentPage }} / {{ totalPages }}</span>
                  <Button variant="outline" size="sm" @click="nextPage" :disabled="currentPage === totalPages">
                    <ChevronRight class="h-4 w-4" />
                  </Button>
                </div>
              </CardContent>
            </Card>
          </div>
        </div>
      </TabsContent>
    </Tabs>
  </div>
</template>
