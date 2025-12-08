# Velum

> 一个面向心理评测的智能后台与数据分析系统。

Velum 致力于为心理健康服务提供**数据驱动的洞察**与**安全可靠的交互体验**，支持客户端（桌面/Web）与管理后台一体化部署。

---

### 🌒 项目名称由来：*Velum*

> **Velum** 源自拉丁语，意为“面纱”或“帷幕”。

在古典剧场中，帷幕升起，故事开始；帷幕落下，角色退场。**Ave Mujica** 的成员以面具示人，在真实与扮演之间演绎命运的独白。

本项目以此为隐喻——
每一次心理测评或对话，都是用户轻轻掀起内心帷幕的一角；
而 Velum 所做的，不是窥探，而是**在帷幕后安静地倾听、理解与守护**。

有趣的是，**Velum** 的发音也让人联想到一句日常对话中的犹豫开场：
> “**We’ll… um…**” —— 那些欲言又止、不知从何说起的瞬间。

这正是 Velum 存在的意义：
我们相信，每一次心理测评或对话，都是用户轻轻掀起内心帷幕的一角；
而系统所要做的，不是催促或评判，而是**安静地在帷幕后等待、倾听、理解**。

同时，“Velum”也呼应了系统的架构哲学：
- **前端如轻纱**：基于 Vue 3 与 Tauri 构建的流畅界面，贴近用户
- **后端如幕布之后**：由 .NET 驱动的稳健 API 与 OpenAI 分析服务，默默支撑全局


名虽隐，意已至。
欢迎来到 *Velum* —— 一个关于倾诉、理解与成长的空间。

---

## 🚀 部署指南

<details>
<summary><strong>📦 端口配置一览表</strong></summary>

| 服务组件 | 端口 | 说明 | 备注 |
| :--- | :--- | :--- | :--- |
| **前端开发** | `14514` | 本地开发服务器 (Vite) | `npm run dev` 启动 |
| **后端 HTTP** | `17597` | API 服务 (非加密) | 用于本地代理或重定向 |
| **后端 HTTPS** | `16796` | API 服务 (加密) | **生产环境主要入口** |

</details>

<details>
<summary><strong>🛠️ 环境变量与配置文件</strong></summary>

### 1. 后端配置
后端支持通过 `appsettings.json` (默认) 或 `appsettings.local.json` (本地覆盖，**不会提交到 Git**) 进行配置。

**关键配置项说明：**

| 节点 | 键 | 说明 | 示例/默认值 |
| :--- | :--- | :--- | :--- |
| `ConnectionStrings` | `DefaultConnection` | 数据库连接字符串 | `Data Source=velum.db` (SQLite) |
| `Jwt` | `Key` | **[必改]** JWT 签名密钥 (≥32字符) | `YourSuperSecretKey...` |
| `Jwt` | `Issuer` | JWT 发行者 | `VelumApi` |
| `Jwt` | `Audience` | JWT 接收者 | `VelumClient` |
| `OpenAI` | `BaseUrl` | AI 服务地址 | `https://api.openai.com/v1` 或 DeepSeek 地址 |
| `OpenAI` | `ApiKey` | **[必填]** AI 服务密钥 | `sk-...` |
| `OpenAI` | `Model` | 使用的模型名称 | `gpt-4o` / `deepseek-chat` |
| `AdminSettings` | `RegistrationKey` | 管理员注册邀请码 | `114514` |
| `Rsa` | `Enabled` | 是否启用前后端密码加密传输 | `true`/`false` |
| `Rsa` | `PublicKey` | RSA 公钥 (Base64) | 自动生成或手动填写 |
| `Rsa` | `PrivateKey` | RSA 私钥 (Base64) | 自动生成或手动填写 |

> **密码加密机制说明：**
> - 启用加密 (`Rsa:Enabled=true`) 且未配置密钥时，后端会自动生成一对 RSA 密钥并打印在终端。
> - 请将生成的密钥复制到 `appsettings.json` 或环境变量中，**公钥无需配置到前端，前端会自动通过 `/api/auth/publickey` 获取**。
> - 若禁用加密 (`Rsa:Enabled=false`)，前后端将以明文方式传输密码，仅建议用于本地调试。

> **生产环境建议**：在 Docker 或云平台中，建议使用环境变量覆盖上述配置（如 `Jwt__Key`, `OpenAI__ApiKey`, `Rsa__Enabled`, `Rsa__PublicKey`, `Rsa__PrivateKey`），而不是修改文件。

### 2. Docker 部署配置
使用 `scripts/backend-docker.sh` 部署时，需注意：
- **证书路径**：脚本默认从 `/opt/1panel/tmp/ssl/` 读取证书（适配 1Panel）。
- **证书文件**：需确保 `fullchain.pem` 和 `privkey.pem` 存在。
- **端口映射**：脚本会自动映射 `17597` (HTTP) 和 `16796` (HTTPS)。

### 3. Vercel 部署配置 (前端)
由于后端已支持 HTTPS，前端默认**直连**后端，无需额外配置。

**模式 A：直连模式 (默认，推荐)**
- 前端直接请求 `https://azure.modestwang.cn:16796/api`。
- **无需**配置 Vercel 环境变量。
- **无需配置 RSA 公钥，前端自动获取。**

**模式 B：代理模式 (可选)**
如果您希望隐藏后端端口或解决某些跨域问题，可启用 Vercel 代理：
1. 设置环境变量 `VITE_API_BASE_URL` = `/api`
2. 设置环境变量 `BACKEND_URL` = `https://azure.modestwang.cn:16796` (注意不要带 `/api` 后缀)

</details>

<details>
<summary><strong>📜 常用脚本命令</strong></summary>

| 脚本 | 平台 | 作用 |
| :--- | :--- | :--- |
| `scripts/start-dev.ps1` | Windows | 启动全栈开发环境 (前端 + 后端 HTTPS) |
| `scripts/backend-docker.sh` | Linux | 构建并运行后端 Docker 容器 (自动挂载证书) |

</details>

---

## 🧩 API & 管理后台 UI 说明

### 1. API 文档与调试

- 后端默认集成了 OpenAPI/Swagger 文档。
- 启动后端服务后，访问：
  - `https://localhost:16796/swagger` 或 `http://localhost:17597/swagger`
  - 可在线查看所有接口、参数说明，并直接调试。
- 主要接口包括：
  - `/api/auth/*`：登录、注册、获取公钥
  - `/api/users/*`：用户管理
  - `/api/assessments/*`：测评相关
  - `/api/chat/*`：AI 对话与分析
  - `/api/dashboard/*`：数据统计与仪表盘

> **小贴士**：开发环境下建议用 Swagger 进行接口联调，生产环境建议关闭或加密访问。

### 2. 管理后台 UI

- 前端采用 Vue 3 + Pinia + Vite 构建，界面简洁流畅。
- 管理后台入口：
  - 开发环境：`http://localhost:14514`（`npm run dev` 或 `bun dev` 启动）
  - 生产环境：部署后访问对应域名或端口
- 功能模块：
  - 用户登录/注册（支持 RSA 加密）
  - 测评问卷管理与数据分析
  - AI 聊天与心理分析
  - 管理员仪表盘与权限控制
- 支持桌面端（Tauri）与 Web 端一体化体验。

> **前端与后端联动说明**：
> - 前端所有 API 请求基于统一的 `VITE_API_BASE_URL`（默认直连后端，无需代理）。
> - 密码加密由前端自动获取公钥并加密，无需手动配置。

---

### ✨ 小贴士

> 💡 **发音**：/ˈviː.ləm/（“Vee-lum”）

> 🎭 **彩蛋**：在浏览器控制台输入 `console.log("velum")`，或许会听到一声猫头鹰的轻鸣 🦉

---

🌱 *Velum is still under active development. Contributions and feedback are welcome!*

**Made with 💕**
