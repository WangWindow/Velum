<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useUsersStore } from '@/stores/users'
import { storeToRefs } from 'pinia'
import { useI18n } from 'vue-i18n'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import {
  Table,
  TableBody,
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
import { Plus } from 'lucide-vue-next'

const { t } = useI18n()
const usersStore = useUsersStore()
const { users } = storeToRefs(usersStore)

const isDialogOpen = ref(false)
const newUser = ref({
  username: '',
  password: '',
  fullName: '',
  email: ''
})

onMounted(() => {
  usersStore.fetchUsers()
})

const handleCreateUser = async () => {
  if (!newUser.value.username || !newUser.value.password) return

  const success = await usersStore.createUser(newUser.value)
  if (success) {
    isDialogOpen.value = false
    newUser.value = { username: '', password: '', fullName: '', email: '' }
  }
}
</script>

<template>
  <div class="space-y-6">
    <div class="flex items-center justify-between">
      <div>
        <h2 class="text-3xl font-bold tracking-tight">{{ t('users.title') }}</h2>
        <p class="text-muted-foreground">{{ t('users.description') }}</p>
      </div>
      <Dialog v-model:open="isDialogOpen">
        <DialogTrigger as-child>
          <Button>
            <Plus class="mr-2 h-4 w-4" />
            {{ t('users.addUser') }}
          </Button>
        </DialogTrigger>
        <DialogContent class="sm:max-w-[425px]">
          <DialogHeader>
            <DialogTitle>{{ t('users.addUser') }}</DialogTitle>
            <DialogDescription>
              Create a new user account.
            </DialogDescription>
          </DialogHeader>
          <div class="grid gap-4 py-4">
            <div class="grid grid-cols-4 items-center gap-4">
              <Label for="username" class="text-right">{{ t('login.username') }}</Label>
              <Input id="username" v-model="newUser.username" class="col-span-3" />
            </div>
            <div class="grid grid-cols-4 items-center gap-4">
              <Label for="password" class="text-right">{{ t('login.password') }}</Label>
              <Input id="password" type="password" v-model="newUser.password" class="col-span-3" />
            </div>
            <div class="grid grid-cols-4 items-center gap-4">
              <Label for="fullName" class="text-right">{{ t('auth.name') }}</Label>
              <Input id="fullName" v-model="newUser.fullName" class="col-span-3" />
            </div>
            <div class="grid grid-cols-4 items-center gap-4">
              <Label for="email" class="text-right">{{ t('users.email') }}</Label>
              <Input id="email" v-model="newUser.email" class="col-span-3" />
            </div>
          </div>
          <DialogFooter>
            <Button type="submit" @click="handleCreateUser">{{ t('users.create') }}</Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>

    <div class="rounded-md border">
      <Table>
        <TableHeader>
          <TableRow>
            <TableHead>ID</TableHead>
            <TableHead>{{ t('users.name') }}</TableHead>
            <TableHead>{{ t('auth.name') }}</TableHead>
            <TableHead>{{ t('users.email') }}</TableHead>
            <TableHead>{{ t('users.role') }}</TableHead>
            <TableHead>Created At</TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          <TableRow v-for="user in users" :key="user.id">
            <TableCell>{{ user.id }}</TableCell>
            <TableCell class="font-medium">{{ user.username }}</TableCell>
            <TableCell>{{ user.fullName || '-' }}</TableCell>
            <TableCell>{{ user.email || '-' }}</TableCell>
            <TableCell>{{ user.role }}</TableCell>
            <TableCell>{{ new Date(user.createdAt).toLocaleDateString() }}</TableCell>
          </TableRow>
        </TableBody>
      </Table>
    </div>
  </div>
</template>
