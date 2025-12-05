<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from '@/components/ui/card'
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table'
import api from '@/lib/api'

const scores = ref<any[]>([])
const isLoading = ref(false)

const fetchScores = async () => {
  isLoading.value = true
  try {
    const response = await api.get('/games/all')
    scores.value = response.data
  } catch (e) {
    console.error(e)
  } finally {
    isLoading.value = false
  }
}

onMounted(() => {
  fetchScores()
})
</script>

<template>
  <div class="space-y-6">
    <div>
      <h2 class="text-3xl font-bold tracking-tight">Game Statistics</h2>
      <p class="text-muted-foreground">Monitor user engagement in mini-games.</p>
    </div>

    <Card>
      <CardHeader>
        <CardTitle>All Game Scores</CardTitle>
        <CardDescription>A list of all game sessions recorded.</CardDescription>
      </CardHeader>
      <CardContent>
        <div v-if="isLoading" class="flex justify-center py-8">
          <div class="animate-spin rounded-full h-8 w-8 border-b-2 border-primary"></div>
        </div>
        <Table v-else>
          <TableHeader>
            <TableRow>
              <TableHead>User</TableHead>
              <TableHead>Game</TableHead>
              <TableHead>Score</TableHead>
              <TableHead>Date</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            <TableRow v-for="score in scores" :key="score.id">
              <TableCell class="font-medium">{{ score.user?.username }}</TableCell>
              <TableCell>{{ score.gameName }}</TableCell>
              <TableCell>{{ score.score }}</TableCell>
              <TableCell>{{ new Date(score.playedAt).toLocaleString() }}</TableCell>
            </TableRow>
            <TableRow v-if="scores.length === 0">
              <TableCell colspan="4" class="text-center py-6 text-muted-foreground">
                No records found.
              </TableCell>
            </TableRow>
          </TableBody>
        </Table>
      </CardContent>
    </Card>
  </div>
</template>
