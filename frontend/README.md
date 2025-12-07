# Vue 3 + TypeScript + Vite

This template should help get you started developing with Vue 3 and TypeScript in Vite. The template uses Vue 3 `<script setup>` SFCs, check out the [script setup docs](https://v3.vuejs.org/api/sfc-script-setup.html#sfc-script-setup) to learn more.

Learn more about the recommended Project Setup and IDE Support in the [Vue Docs TypeScript Guide](https://vuejs.org/guide/typescript/overview.html#project-setup).

# 部署前端 (Vercel)
1. 登录 Vercel Dashboard。
2. 点击 Add New ... -> Project。
3. 导入你的 GitHub 仓库。
4. 配置项目：
- Root Directory: 点击 Edit，选择 frontend 目录。
- Framework Preset: Vercel 应该会自动识别为 Vite。
- Build Command: npm run build (默认即可)
- Output Directory: dist (默认即可)
- Environment Variables (环境变量)：
5. 展开 Environment Variables 部分，添加：
- VITE_API_BASE_URL: 填入你的 Render 后端 URL，并在末尾加上 /api。
    - 例如: https://velum-backend.onrender.com/api
6. 点击 Deploy
