<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useTasksStore } from '@/stores/tasks'
import { storeToRefs } from 'pinia'
import { Plus } from 'lucide-vue-next'
import { useI18n } from 'vue-i18n'

import { Button } from '@/components/ui/button'
import {
  Table,
  TableBody,
  TableCaption,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table'
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from '@/components/ui/dialog'
import {
  Select,
  SelectContent,
  SelectGroup,
  SelectItem,
  SelectLabel,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select'
import { Label } from '@/components/ui/label'

const { t } = useI18n()
const tasksStore = useTasksStore()
const { tasks, users, questionnaires, isLoading } = storeToRefs(tasksStore)

const isDialogOpen = ref(false)
const selectedUserId = ref<string>('')
const selectedQuestionnaireId = ref<string>('')

onMounted(() => {
  tasksStore.fetchTasks()
  tasksStore.fetchUsers()
  tasksStore.fetchQuestionnaires()
})

const handleAssign = async () => {
  if (!selectedUserId.value || !selectedQuestionnaireId.value) return

  await tasksStore.assignTask(Number(selectedUserId.value), Number(selectedQuestionnaireId.value))
  isDialogOpen.value = false
  selectedUserId.value = ''
  selectedQuestionnaireId.value = ''
}
</script>

<template>
  <div class="container mx-auto p-6 space-y-6">
    <div class="flex items-center justify-between">
      <div>
        <h1 class="text-3xl font-bold tracking-tight">{{ t('tasks.title') }}</h1>
        <p class="text-muted-foreground">{{ t('tasks.description') }}</p>
      </div>

      <Dialog v-model:open="isDialogOpen">
        <DialogTrigger as-child>
          <Button>
            <Plus class="mr-2 h-4 w-4" />
            {{ t('tasks.assignTask') }}
          </Button>
        </DialogTrigger>
        <DialogContent class="sm:max-w-[425px]">
          <DialogHeader>
            <DialogTitle>{{ t('tasks.assignNewTask') }}</DialogTitle>
            <DialogDescription>
              {{ t('tasks.assignDescription') }}
            </DialogDescription>
          </DialogHeader>
          <div class="grid gap-4 py-4">
            <div class="grid grid-cols-4 items-center gap-4">
              <Label for="user" class="text-right">
                {{ t('tasks.user') }}
              </Label>
              <div class="col-span-3">
                <Select v-model="selectedUserId">
                  <SelectTrigger>
                    <SelectValue :placeholder="t('tasks.selectUser')" />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectGroup>
                      <SelectLabel>{{ t('users.title') }}</SelectLabel>
                      <SelectItem v-for="user in users" :key="user.id" :value="String(user.id)">
                        {{ user.username }}
                      </SelectItem>
                    </SelectGroup>
                  </SelectContent>
                </Select>
              </div>
            </div>
            <div class="grid grid-cols-4 items-center gap-4">
              <Label for="questionnaire" class="text-right">
                {{ t('tasks.scale') }}
              </Label>
              <div class="col-span-3">
                <Select v-model="selectedQuestionnaireId">
                  <SelectTrigger>
                    <SelectValue :placeholder="t('tasks.selectScale')" />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectGroup>
                      <SelectLabel>{{ t('tasks.questionnaire') }}</SelectLabel>
                      <SelectItem v-for="q in questionnaires" :key="q.id" :value="String(q.id)">
                        {{ q.title }}
                      </SelectItem>
                    </SelectGroup>
                  </SelectContent>
                </Select>
              </div>
            </div>
          </div>
          <DialogFooter>
            <Button type="submit" @click="handleAssign" :disabled="isLoading">
              {{ isLoading ? t('tasks.assigning') : t('tasks.assignTask') }}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>

    <div class="rounded-md border">
      <Table>
        <TableCaption>A list of assigned assessment tasks.</TableCaption>
        <TableHeader>
          <TableRow>
            <TableHead class="w-[100px]">{{ t('tasks.id') }}</TableHead>
            <TableHead>{{ t('tasks.user') }}</TableHead>
            <TableHead>{{ t('tasks.questionnaire') }}</TableHead>
            <TableHead>{{ t('tasks.status') }}</TableHead>
            <TableHead>{{ t('tasks.assignedDate') }}</TableHead>
            <TableHead class="text-right">{{ t('tasks.dueDate') }}</TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          <TableRow v-for="task in tasks" :key="task.id">
            <TableCell class="font-medium">{{ task.id }}</TableCell>
            <TableCell>{{ task.user?.username }}</TableCell>
            <TableCell>{{ task.questionnaire?.title }}</TableCell>
            <TableCell>
              <span :class="{
                'inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium': true,
                'bg-green-100 text-green-800': task.status === 'Completed',
                'bg-yellow-100 text-yellow-800': task.status === 'Pending'
              }">
                {{ task.status === 'Completed' ? t('tasks.completed') : t('tasks.pending') }}
              </span>
            </TableCell>
            <TableCell>{{ new Date(task.assignedAt).toLocaleDateString() }}</TableCell>
            <TableCell class="text-right">{{ task.dueDate ? new Date(task.dueDate).toLocaleDateString() : '-' }}
            </TableCell>
          </TableRow>
        </TableBody>
      </Table>
    </div>
  </div>
</template>
