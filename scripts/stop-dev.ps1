$ErrorActionPreference = "Stop"

# 获取脚本所在的绝对路径
$ScriptPath = $MyInvocation.MyCommand.Path
$ScriptDir = Split-Path -Parent $ScriptPath
# 假设脚本位于 /scripts，项目根目录在上一级
$RepoRoot = (Resolve-Path "$ScriptDir\.." ).Path

Write-Host "🛑 Stopping Velum Development Environment..." -ForegroundColor Red

# 关闭指定端口的进程（如 Analysis worker 占用的 17597 端口）
$ports = @(17597)
foreach ($port in $ports) {
    $pids = Get-NetTCPConnection -LocalPort $port -ErrorAction SilentlyContinue | Select-Object -ExpandProperty OwningProcess -Unique
    foreach ($pid in $pids) {
        if ($pid) {
            Write-Host "Killing process on port $port (PID: $pid)" -ForegroundColor Yellow
            Stop-Process -Id $pid -Force -ErrorAction SilentlyContinue
        }
    }
}

# 关闭 VBCSCompiler
Get-Process -Name "VBCSCompiler" -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue

# 关闭 dotnet run
Get-Process -Name "dotnet" -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue

dotnet build-server shutdown

Set-Location $RepoRoot
Write-Host "✅ All dev services stopped." -ForegroundColor Green
