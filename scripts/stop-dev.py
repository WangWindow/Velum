#!/usr/bin/env python3
import os
import subprocess

SCRIPT_DIR = os.path.dirname(os.path.abspath(__file__))
REPO_ROOT = os.path.abspath(os.path.join(SCRIPT_DIR, ".."))
PORTS = [17597]


def kill_port(port):
    try:
        pid = subprocess.check_output(["lsof", "-ti", f"tcp:{port}"]).decode().strip()
        if pid:
            print(f"Killing process on port {port} (PID: {pid})")
            subprocess.run(["kill", "-9", pid])
    except subprocess.CalledProcessError:
        pass


def main():
    for port in PORTS:
        kill_port(port)
    # 关闭 VBCSCompiler
    subprocess.run(["pkill", "-f", "VBCSCompiler"])
    # 关闭 dotnet run
    subprocess.run(["pkill", "-f", "dotnet run"])
    subprocess.run(["dotnet", "build-server", "shutdown"])
    os.chdir(REPO_ROOT)
    print("\033[32m✅ All dev services stopped.\033[0m")


if __name__ == "__main__":
    main()
