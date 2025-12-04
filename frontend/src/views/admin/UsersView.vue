<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useUsersStore, type User } from '@/stores/users'
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
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select'
import { Plus, Pencil, Trash2 } from 'lucide-vue-next'

const { t } = useI18n()
const usersStore = useUsersStore()
const { users } = storeToRefs(usersStore)

const isDialogOpen = ref(false)
const isEditDialogOpen = ref(false)
const newUser = ref({
  username: '',
  password: '',
  fullName: '',
  email: '',
  role: 'User' as 'User' | 'Admin'
})
const editingUser = ref({
  id: 0,
  username: '',
  password: '',
  fullName: '',
  email: '',
  role: 'User' as 'User' | 'Admin'
})

onMounted(() => {
  usersStore.fetchUsers()
})

const handleCreateUser = async () => {
  if (!newUser.value.username || !newUser.value.password) return

  const success = await usersStore.createUser(newUser.value)
  if (success) {
    isDialogOpen.value = false
    newUser.value = { username: '', password: '', fullName: '', email: '', role: 'User' }
  }
}

const handleEditUser = (user: User) => {
  editingUser.value = {
    id: user.id,
    username: user.username,
    password: '', // Reset password field
    fullName: user.fullName || '',
    email: user.email || '',
    role: user.role
  }
  isEditDialogOpen.value = true
}

const handleUpdateUser = async () => {
  const success = await usersStore.updateUser(editingUser.value.id, editingUser.value)
  if (success) {
    isEditDialogOpen.value = false
  }
}

const handleDeleteUser = async (id: number) => {
  if (confirm('Are you sure you want to delete this user?')) {
    await usersStore.deleteUser(id)
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
            <div class="grid grid-cols-4 items-center gap-4">
              <Label for="role" class="text-right">{{ t('users.role') }}</Label>
              <Select v-model="newUser.role">
                <SelectTrigger class="col-span-3">
                  <SelectValue placeholder="Select a role" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="User">User</SelectItem>
                  <SelectItem value="Admin">Admin</SelectItem>
                </SelectContent>
              </Select>
            </div>
          </div>
          <DialogFooter>
            <Button type="submit" @click="handleCreateUser">{{ t('users.create') }}</Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      <Dialog v-model:open="isEditDialogOpen">
        <DialogContent class="sm:max-w-[425px]">
          <DialogHeader>
            <DialogTitle>{{ t('users.edit') }}</DialogTitle>
            <DialogDescription>
              Edit user details. Leave password blank to keep current.
            </DialogDescription>
          </DialogHeader>
          <div class="grid gap-4 py-4">
            <div class="grid grid-cols-4 items-center gap-4">
              <Label for="edit-username" class="text-right">{{ t('login.username') }}</Label>
              <Input id="edit-username" v-model="editingUser.username" class="col-span-3" disabled />
            </div>
            <div class="grid grid-cols-4 items-center gap-4">
              <Label for="edit-password" class="text-right">{{ t('login.password') }}</Label>
              <Input id="edit-password" type="password" v-model="editingUser.password" class="col-span-3"
                placeholder="(Unchanged)" />
            </div>
            <div class="grid grid-cols-4 items-center gap-4">
              <Label for="edit-fullName" class="text-right">{{ t('auth.name') }}</Label>
              <Input id="edit-fullName" v-model="editingUser.fullName" class="col-span-3" />
            </div>
            <div class="grid grid-cols-4 items-center gap-4">
              <Label for="edit-email" class="text-right">{{ t('users.email') }}</Label>
              <Input id="edit-email" v-model="editingUser.email" class="col-span-3" />
            </div>
            <div class="grid grid-cols-4 items-center gap-4">
              <Label for="edit-role" class="text-right">{{ t('users.role') }}</Label>
              <Select v-model="editingUser.role">
                <SelectTrigger class="col-span-3">
                  <SelectValue placeholder="Select a role" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="User">User</SelectItem>
                  <SelectItem value="Admin">Admin</SelectItem>
                </SelectContent>
              </Select>
            </div>
          </div>
          <DialogFooter>
            <Button type="submit" @click="handleUpdateUser">{{ t('common.save') || 'Save' }}</Button>
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
            <TableHead class="text-right">{{ t('users.actions') }}</TableHead>
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
            <TableCell class="text-right">
              <div class="flex justify-end gap-2">
                <Button variant="ghost" size="icon" @click="handleEditUser(user)">
                  <Pencil class="h-4 w-4" />
                </Button>
                <Button variant="ghost" size="icon" class="text-destructive" @click="handleDeleteUser(user.id)">
                  <Trash2 class="h-4 w-4" />
                </Button>
              </div>
            </TableCell>
          </TableRow>
        </TableBody>
      </Table>
    </div>
  </div>
</template>
