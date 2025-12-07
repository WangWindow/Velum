# 部署后端 (Render)

1. 登录 Render Dashboard。
2. 点击 New + -> Web Service。
3. 连接你的 GitHub 仓库。
4. 配置服务信息：
- Name: velum-backend (或你喜欢的名字)
- Root Directory: backend (非常重要，因为 Dockerfile 在这里)
- Runtime: 选择 Docker
- Region: 选择离你最近的节点 (如 Singapore)
- Instance Type: Free (注意：免费版闲置时会休眠，唤醒需要时间)
5. Environment Variables (环境变量)：
在 Render 的 "Environment" 标签页中添加以下变量（这些变量会覆盖 appsettings.json 中的设置）：
- Jwt__Key: 生成一个复杂的随机字符串（至少32位）。
- Jwt__Issuer: VelumApi
- Jwt__Audience: VelumClient
- OpenAI__ApiKey: 你的 OpenAI API 密钥 (sk-...)。
    - ConnectionStrings__DefaultConnection: Data Source=velum.db
注意：Render 的免费版和标准 Web Service 文件系统是临时的。每次重新部署或重启，SQLite 数据库 (velum.db) 都会被重置。如果需要持久化数据，建议在 Render 上创建一个 "Disk" 并挂载，或者使用 Render 的 PostgreSQL 数据库（需要修改代码适配 Postgres）。
6. 点击 Create Web Service。
7. 等待部署完成，复制分配给你的后端 URL (例如 https://velum-backend.onrender.com)。
