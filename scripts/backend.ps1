$ErrorActionPreference = "Stop"

# 获取脚本所在的绝对路径
$ScriptPath = $MyInvocation.MyCommand.Path
$ScriptDir = Split-Path -Parent $ScriptPath
# 假设脚本位于 /scripts，项目根目录在上一级
$RepoRoot = (Resolve-Path "$ScriptDir\..").Path

Write-Host "🚀 Starting Velum Development Environment..." -ForegroundColor Cyan
Write-Host "📂 Repo Root: $RepoRoot" -ForegroundColor Gray

# 清理残留进程和释放文件锁
Write-Host "🧹 Cleaning up previous build processes..." -ForegroundColor DarkGray
dotnet build-server shutdown
Get-Process -Name "VBCSCompiler" -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue

$ApiPath = Join-Path $RepoRoot "backend\api"

$processes = @()

try {
    # 启动 .NET 后端 API
    Write-Host "dotnet Launching Backend API (.NET)..." -ForegroundColor Blue
    $apiProcess = Start-Process dotnet -ArgumentList "run" -WorkingDirectory $ApiPath -NoNewWindow -PassThru
    $processes += $apiProcess

    # 等待进程结束，这样脚本不会立即退出
    if (-not $apiProcess.HasExited) {
        Wait-Process -Id $apiProcess.Id
    }
}
finally {
    Write-Host "`n🛑 Stopping background services..." -ForegroundColor Red
    foreach ($p in $processes) {
        if ($null -ne $p -and -not $p.HasExited) {
            Stop-Process -Id $p.Id -Force -ErrorAction SilentlyContinue
        }
    }

    # 再次清理构建进程，防止文件锁定
    Write-Host "🧹 Shutting down build servers..." -ForegroundColor DarkGray
    dotnet build-server shutdown
    Get-Process -Name "VBCSCompiler" -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue

    # 返回项目根目录
    Set-Location $RepoRoot
}
