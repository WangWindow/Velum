#!/usr/bin/env python3
import os
import subprocess
import signal
import sys

# 获取脚本所在目录和项目根目录
SCRIPT_DIR = os.path.dirname(os.path.abspath(__file__))
REPO_ROOT = os.path.abspath(os.path.join(SCRIPT_DIR, ".."))
API_PATH = os.path.join(REPO_ROOT, "backend", "api")
WEB_PATH = os.path.join(REPO_ROOT, "frontend")

api_process = None


def cleanup(signum=None, frame=None):
    print("\n\033[31m🛑 Stopping background services...\033[0m")
    if api_process and api_process.poll() is None:
        api_process.terminate()
        try:
            api_process.wait(timeout=5)
        except subprocess.TimeoutExpired:
            api_process.kill()
    # 清理构建进程
    subprocess.run(["dotnet", "build-server", "shutdown"])
    subprocess.run(["pkill", "-f", "VBCSCompiler"])
    os.chdir(REPO_ROOT)
    sys.exit(0)


def main():
    global api_process
    print("\033[36m🚀 Starting Velum Development Environment...\033[0m")
    print(f"\033[90m📂 Repo Root: {REPO_ROOT}\033[0m")
    print("\033[90m🧹 Cleaning up previous build processes...\033[0m")
    subprocess.run(["dotnet", "build-server", "shutdown"])
    subprocess.run(["pkill", "-f", "VBCSCompiler"])

    print("\033[34mdotnet Launching Backend API (.NET)...\033[0m")
    api_process = subprocess.Popen(["dotnet", "run"], cwd=API_PATH)

    print("\033[33m⚡ Launching Web Frontend (Vue)...\033[0m")
    print("\033[33mPress Ctrl+C to stop all services.\033[0m")
    try:
        subprocess.run(["bun", "dev"], cwd=WEB_PATH)
    except KeyboardInterrupt:
        pass
    finally:
        cleanup()


if __name__ == "__main__":
    signal.signal(signal.SIGINT, cleanup)
    main()
