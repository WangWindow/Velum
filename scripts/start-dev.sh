#!/usr/bin/env bash
set -e

# 获取脚本所在目录和项目根目录
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"

echo -e "\033[36m🚀 Starting Velum Development Environment...\033[0m"
echo -e "\033[90m📂 Repo Root: $REPO_ROOT\033[0m"

# 清理残留进程和释放文件锁
echo -e "\033[90m🧹 Cleaning up previous build processes...\033[0m"
dotnet build-server shutdown || true
pkill -f VBCSCompiler || true

API_PATH="$REPO_ROOT/backend/api"
WEB_PATH="$REPO_ROOT/frontend"

# 启动 .NET 后端 API
echo -e "\033[34mdotnet Launching Backend API (.NET)...\033[0m"
(cd "$API_PATH" && dotnet run &)
API_PID=$!

trap 'echo -e "\n\033[31m🛑 Stopping background services...\033[0m"; kill $API_PID 2>/dev/null; dotnet build-server shutdown; pkill -f VBCSCompiler || true; cd "$REPO_ROOT"; exit 0' SIGINT

# 启动 Web 前端
echo -e "\033[33m⚡ Launching Web Frontend (Vue)...\033[0m"
echo -e "\033[33mPress Ctrl+C to stop all services.\033[0m"
cd "$WEB_PATH"
npm run dev
