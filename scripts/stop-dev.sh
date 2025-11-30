#!/usr/bin/env bash
set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"

# 关闭 Analysis worker 端口（如17597）和 API 进程
PORTS=(17597)
for PORT in "${PORTS[@]}"; do
  PID=$(lsof -ti tcp:$PORT)
  if [ -n "$PID" ]; then
    echo "Killing process on port $PORT (PID: $PID)"
    kill -9 $PID || true
  fi
done

# 关闭 VBCSCompiler
pkill -f VBCSCompiler || true

# 关闭 dotnet 后台进程
pkill -f "dotnet run" || true

dotnet build-server shutdown || true

cd "$REPO_ROOT"
echo -e "\033[32m✅ All dev services stopped.\033[0m"
