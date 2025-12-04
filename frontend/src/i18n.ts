import { createI18n } from 'vue-i18n'

const messages = {
  en: {
    common: {
      welcome: 'Welcome',
      login: 'Login',
      register: 'Register',
      logout: 'Logout',
      profile: 'Profile',
      settings: 'Settings',
      dashboard: 'Dashboard',
      users: 'Users',
      tasks: 'Tasks',
      chat: 'Chat',
      theme: {
        light: 'Light Mode',
        dark: 'Dark Mode',
        system: 'System'
      },
      back: 'Go Back',
      home: 'Go Home'
    },
    error: {
      notFound: {
        title: 'Page Not Found',
        description: 'Sorry, we couldn\'t find the page you\'re looking for.'
      }
    },
    nav: {
      dashboard: 'Dashboard',
      assessment: 'Assessment',
      chat: 'Chat',
      settings: 'Settings',
      users: 'Users',
      tasks: 'Tasks',
      scales: 'Scales'
    },
    settings: {
      title: 'Settings',
      description: 'Manage your account settings and preferences.',
      language: 'Language',
      theme: 'Theme',
      avatar: 'Avatar',
      avatarDesc: 'Select an avatar from the library.',
      avatarUpdated: 'Avatar Updated',
      avatarUpdatedDesc: 'Your profile avatar has been updated successfully.'
    },
    dashboard: {
      title: 'Dashboard',
      welcome: 'Welcome back!',
      activeTasks: 'Active Tasks',
      tasksDescription: 'Tasks assigned to you.',
      noTasks: 'No pending tasks.',
      completedTasks: 'Completed Tasks',
      pendingReviews: 'Pending Reviews',
      recentActivity: 'Recent Activity',
      quickActions: 'Quick Actions',
      newTask: 'New Task',
      viewReports: 'View Reports',
      totalUsers: 'Total Users',
      completedAssessments: 'Completed Assessments',
      systemHealth: 'System Health',
      overview: 'Overview',
      recentActivityDesc: 'Recent system activities.',
      recentHistoryDesc: 'Your recent assessment history.',
      noHistory: 'No history available.'
    },
    users: {
      title: 'User Management',
      description: 'Manage users and their roles.',
      addUser: 'Add User',
      create: 'Create',
      name: 'Name',
      email: 'Email',
      role: 'Role',
      actions: 'Actions',
      edit: 'Edit',
      delete: 'Delete',
      roles: {
        admin: 'Admin',
        user: 'User'
      }
    },
    tasks: {
      title: 'Task Management',
      description: 'Assign and monitor assessment tasks for users.',
      assignTask: 'Assign Task',
      assignNewTask: 'Assign New Task',
      assignDescription: 'Select a user and a questionnaire to assign.',
      user: 'User',
      scale: 'Scale',
      selectUser: 'Select a user',
      selectScale: 'Select a scale',
      assigning: 'Assigning...',
      id: 'ID',
      questionnaire: 'Questionnaire',
      status: 'Status',
      assignedDate: 'Assigned Date',
      dueDate: 'Due Date',
      completed: 'Completed',
      pending: 'Pending'
    },
    scales: {
      title: 'Scale Import',
      description: 'Import psychological scales using AI analysis.',
      rawContent: 'Raw Content',
      rawContentDesc: 'Paste the questionnaire text here (PDF/Word content).',
      placeholder: 'Paste your scale content here...\nExample:\n1. I feel down-hearted and blue.\n   A. None of the time (0)\n   B. A little of the time (1)\n   ...',
      clear: 'Clear',
      analyze: 'Analyze with AI',
      preview: 'Preview & Import',
      previewDesc: 'Review the AI parsed structure before saving.',
      noData: 'Parsed result will appear here',
      questions: 'Questions',
      import: 'Import to Database',
      saving: 'Saving...'
    },
    assessment: {
      title: 'Assessments',
      description: 'Select an assessment to begin.',
      questions: 'Questions',
      start: 'Start',
      progress: 'Progress',
      submit: 'Submit Assessment',
      submitting: 'Submitting...',
      submitted: 'Assessment submitted successfully!',
      submittedDesc: 'Your assessment has been submitted successfully.',
      exit: 'Exit',
      score: 'Score'
    },
    chat: {
      title: 'AI Assistant',
      clear: 'Clear Chat',
      newChat: 'New Chat',
      recent: 'Recent',
      mode: 'Mode',
      default: 'Default (Managed)',
      custom: 'Custom (BYOK)',
      config: 'Chat Configuration',
      configDesc: 'Configure the AI model settings.',
      apiUrl: 'API URL',
      apiKey: 'API Key',
      modelName: 'Model Name',
      systemPrompt: 'System Prompt',
      save: 'Save changes',
      typeMessage: 'Type your message...'
    },
    auth: {
      email: 'Email',
      password: 'Password',
      confirmPassword: 'Confirm Password',
      name: 'Full Name',
      loginButton: 'Sign In',
      registerButton: 'Sign Up',
      haveAccount: 'Already have an account? Sign in',
      noAccount: 'Don\'t have an account? Sign up',
      loginSuccess: 'Login successful',
      registerSuccess: 'Registration successful',
      logoutSuccess: 'Logged out successfully'
    },
    login: {
      title: 'Welcome back',
      description: 'Enter your credentials to access your account',
      createAccount: 'Create an account',
      createDescription: 'Enter your information to create an account',
      username: 'Username',
      usernamePlaceholder: 'Enter your username',
      email: 'Email',
      fullName: 'Full Name',
      password: 'Password',
      submit: 'Sign In',
      signUp: 'Sign Up',
      loggingIn: 'Signing in...',
      creatingAccount: 'Creating account...',
      dontHaveAccount: 'Don\'t have an account?',
      alreadyHaveAccount: 'Already have an account?',
      loginLink: 'Sign in',
      registrationSuccess: 'Registration successful',
      registrationSuccessDesc: 'Please login with your new account.',
      registrationFailed: 'Registration failed',
      loginFailed: 'Login failed',
      loginFailedDesc: 'Please check your credentials and try again.'
    },
    app: {
      userTitle: 'Velum User',
      adminTitle: 'Velum Admin'
    }
  },
  zh: {
    common: {
      welcome: '欢迎',
      login: '登录',
      register: '注册',
      logout: '退出登录',
      profile: '个人资料',
      settings: '设置',
      dashboard: '仪表盘',
      users: '用户管理',
      tasks: '任务',
      chat: '聊天',
      theme: {
        light: '亮色模式',
        dark: '暗色模式',
        system: '跟随系统'
      },
      back: '返回',
      home: '回到首页'
    },
    error: {
      notFound: {
        title: '页面未找到',
        description: '抱歉，我们找不到您要访问的页面。'
      }
    },
    nav: {
      dashboard: '仪表盘',
      assessment: '评估',
      chat: '聊天',
      settings: '设置',
      users: '用户管理',
      tasks: '任务管理',
      scales: '量表管理'
    },
    settings: {
      title: '设置',
      description: '管理您的帐户设置和首选项。',
      language: '语言',
      theme: '主题',
      avatar: '头像',
      avatarDesc: '从库中选择一个头像。',
      avatarUpdated: '头像已更新',
      avatarUpdatedDesc: '您的个人资料头像已成功更新。'
    },
    dashboard: {
      title: '仪表盘',
      welcome: '欢迎回来!',
      activeTasks: '进行中的任务',
      tasksDescription: '分配给您的任务。',
      noTasks: '暂无待处理任务。',
      completedTasks: '已完成任务',
      pendingReviews: '待审核',
      recentActivity: '最近活动',
      quickActions: '快捷操作',
      newTask: '新建任务',
      viewReports: '查看报告',
      totalUsers: '总用户数',
      completedAssessments: '已完成评估',
      systemHealth: '系统健康度',
      overview: '概览',
      recentActivityDesc: '最近的系统活动。',
      recentHistoryDesc: '您的近期评估历史。',
      noHistory: '暂无历史记录。'
    },
    users: {
      title: '用户管理',
      description: '管理用户及其角色。',
      addUser: '添加用户',
      create: '创建',
      name: '姓名',
      email: '邮箱',
      role: '角色',
      actions: '操作',
      edit: '编辑',
      delete: '删除',
      roles: {
        admin: '管理员',
        user: '普通用户'
      }
    },
    tasks: {
      title: '任务管理',
      description: '分配和监控用户的评估任务。',
      assignTask: '分配任务',
      assignNewTask: '分配新任务',
      assignDescription: '选择用户和问卷进行分配。',
      user: '用户',
      scale: '量表',
      selectUser: '选择用户',
      selectScale: '选择量表',
      assigning: '分配中...',
      id: 'ID',
      questionnaire: '问卷',
      status: '状态',
      assignedDate: '分配日期',
      dueDate: '截止日期',
      completed: '已完成',
      pending: '待处理'
    },
    scales: {
      title: '量表导入',
      description: '使用 AI 分析导入心理量表。',
      rawContent: '原始内容',
      rawContentDesc: '在此粘贴问卷文本（PDF/Word 内容）。',
      placeholder: '在此粘贴您的量表内容...\n例如：\n1. 我感到沮丧和忧郁。\n   A. 没有时间 (0)\n   B. 很少时间 (1)\n   ...',
      clear: '清空',
      analyze: '使用 AI 分析',
      preview: '预览与导入',
      previewDesc: '保存前查看 AI 解析的结构。',
      noData: '解析结果将显示在这里',
      questions: '问题',
      import: '导入数据库',
      saving: '保存中...'
    },
    assessment: {
      title: '评估',
      description: '选择一个评估开始。',
      questions: '问题',
      start: '开始',
      progress: '进度',
      submit: '提交评估',
      submitting: '提交中...',
      submitted: '评估提交成功！',
      submittedDesc: '您的评估已成功提交。',
      exit: '退出',
      score: '得分'
    },
    chat: {
      title: 'AI 助手',
      clear: '清空聊天',
      newChat: '新对话',
      recent: '最近对话',
      mode: '模式',
      default: '默认 (托管)',
      custom: '自定义 (BYOK)',
      config: '聊天配置',
      configDesc: '配置 AI 模型设置。',
      apiUrl: 'API 地址',
      apiKey: 'API 密钥',
      modelName: '模型名称',
      systemPrompt: '系统提示词',
      save: '保存更改',
      typeMessage: '输入您的消息...'
    },
    auth: {
      email: '邮箱',
      password: '密码',
      confirmPassword: '确认密码',
      name: '全名',
      loginButton: '登录',
      registerButton: '注册',
      haveAccount: '已有账号？去登录',
      noAccount: '没有账号？去注册',
      loginSuccess: '登录成功',
      registerSuccess: '注册成功',
      logoutSuccess: '已退出登录'
    },
    login: {
      title: '欢迎回来',
      description: '输入您的凭据以访问您的帐户',
      createAccount: '创建帐户',
      createDescription: '输入您的信息以创建帐户',
      username: '用户名',
      usernamePlaceholder: '输入您的用户名',
      email: '邮箱',
      fullName: '全名',
      password: '密码',
      submit: '登录',
      signUp: '注册',
      loggingIn: '登录中...',
      creatingAccount: '创建帐户中...',
      dontHaveAccount: '没有帐户？',
      alreadyHaveAccount: '已有帐户？',
      loginLink: '登录',
      registrationSuccess: '注册成功',
      registrationSuccessDesc: '请使用您的新帐户登录。',
      registrationFailed: '注册失败',
      loginFailed: '登录失败',
      loginFailedDesc: '请检查您的凭据并重试。'
    },
    app: {
      userTitle: 'Velum 用户端',
      adminTitle: 'Velum 管理端'
    }
  }
}

const i18n = createI18n({
  legacy: false, // Use Composition API
  locale: 'zh', // Default locale
  fallbackLocale: 'en',
  globalInjection: true, // Allow global usage like $t
  messages
})

export default i18n
