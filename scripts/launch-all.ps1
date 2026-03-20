# Launch all 4 web apps and open browser windows in a 2x2 grid

# Add Win32 API for window positioning (guard against re-definition)
if (-not ([System.Management.Automation.PSTypeName]'Win32Grid').Type) {
    Add-Type @"
using System;
using System.Runtime.InteropServices;
using System.Text;

public class Win32Grid {
    [DllImport("user32.dll")]
    public static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

    [DllImport("user32.dll")]
    public static extern int GetSystemMetrics(int nIndex);

    public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

    [DllImport("user32.dll")]
    public static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

    [DllImport("user32.dll")]
    public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

    [DllImport("user32.dll")]
    public static extern bool IsWindowVisible(IntPtr hWnd);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

    [DllImport("user32.dll")]
    public static extern int GetWindowTextLength(IntPtr hWnd);
}
"@
}

$repoRoot = Split-Path -Parent $PSScriptRoot

# App definitions: name, path, URL
$apps = @(
    @{ Name = "news-feed";           Path = "$repoRoot\src\news-feed";           Url = "http://localhost:5142" },
    @{ Name = "research-analytics";  Path = "$repoRoot\src\research-analytics";  Url = "http://localhost:5003" },
    @{ Name = "broker-backoffice";   Path = "$repoRoot\src\broker-backoffice";   Url = "http://localhost:5269" },
    @{ Name = "trading-platform";    Path = "$repoRoot\src\trading-platform";    Url = "http://localhost:5249" }
)

$dotnetProcesses = @()

Write-Host "Starting all 4 web apps..." -ForegroundColor Cyan

foreach ($app in $apps) {
    Write-Host "  Starting $($app.Name) at $($app.Url)..." -ForegroundColor Yellow
    $proc = Start-Process -FilePath "dotnet" `
        -ArgumentList "run", "--project", $app.Path `
        -PassThru -WindowStyle Minimized
    $dotnetProcesses += $proc
}

# Wait for apps to be ready (TCP port check — avoids triggering slow page renders)
Write-Host "`nWaiting for apps to start..." -ForegroundColor Cyan
$maxWait = 60
foreach ($app in $apps) {
    $uri = [System.Uri]$app.Url
    $ready = $false
    for ($i = 0; $i -lt $maxWait; $i++) {
        try {
            $tcp = New-Object System.Net.Sockets.TcpClient
            $tcp.Connect($uri.Host, $uri.Port)
            $tcp.Close()
            $ready = $true
            break
        } catch {
            Start-Sleep -Seconds 1
        }
    }
    if ($ready) {
        Write-Host "  $($app.Name) is ready" -ForegroundColor Green
    } else {
        Write-Host "  $($app.Name) did not respond within ${maxWait}s (may still be starting)" -ForegroundColor Red
    }
}

# Detect Chrome path
$chromePath = (Get-Command chrome -ErrorAction SilentlyContinue).Source
if (-not $chromePath) {
    $chromePath = "$env:ProgramFiles\Google\Chrome\Application\chrome.exe"
    if (-not (Test-Path $chromePath)) {
        $chromePath = "${env:ProgramFiles(x86)}\Google\Chrome\Application\chrome.exe"
    }
    if (-not (Test-Path $chromePath)) {
        $chromePath = "$env:LOCALAPPDATA\Google\Chrome\Application\chrome.exe"
    }
}

if (-not (Test-Path $chromePath)) {
    Write-Host "Google Chrome not found. Falling back to default browser." -ForegroundColor Yellow
    foreach ($app in $apps) {
        Start-Process $app.Url
    }
} else {
    # Get screen dimensions
    $screenWidth  = [Win32Grid]::GetSystemMetrics(0)  # SM_CXSCREEN
    $screenHeight = [Win32Grid]::GetSystemMetrics(1)  # SM_CYSCREEN

    $halfWidth  = [math]::Floor($screenWidth / 2)
    $halfHeight = [math]::Floor($screenHeight / 2)

    # Grid positions: top-left, top-right, bottom-left, bottom-right
    $positions = @(
        @{ X = 0;          Y = 0 },
        @{ X = $halfWidth;  Y = 0 },
        @{ X = 0;          Y = $halfHeight },
        @{ X = $halfWidth;  Y = $halfHeight }
    )

    Write-Host "`nOpening browsers in 2x2 grid (${screenWidth}x${screenHeight})..." -ForegroundColor Cyan

    # Create temp user data dirs so each Chrome instance is a separate process
    # (otherwise Chrome merges into the existing process and ignores position/size flags)
    $tempDirs = @()
    $browserProcesses = @()

    for ($i = 0; $i -lt $apps.Count; $i++) {
        $app = $apps[$i]
        $pos = $positions[$i]
        $tempDir = Join-Path $env:TEMP "fx-agent-chrome-$i"
        $tempDirs += $tempDir

        $arguments = @(
            "--user-data-dir=$tempDir"
            "--new-window"
            "--window-size=$halfWidth,$halfHeight"
            "--window-position=$($pos.X),$($pos.Y)"
            "--no-first-run"
            "--no-default-browser-check"
            $app.Url
        )

        $proc = Start-Process -FilePath $chromePath -ArgumentList $arguments -PassThru
        $browserProcesses += $proc
        Write-Host "  Opened $($app.Name) -> $($app.Url)" -ForegroundColor Green
        Start-Sleep -Milliseconds 800
    }

    # Give windows time to fully render, then reposition with Win32 as a fallback
    Start-Sleep -Seconds 2
    Write-Host "  Repositioning windows..." -ForegroundColor Gray

    for ($i = 0; $i -lt $browserProcesses.Count; $i++) {
        $bProc = $browserProcesses[$i]
        $pos = $positions[$i]
        if ($bProc -and -not $bProc.HasExited) {
            # Find all visible windows belonging to this process tree
            $targetPid = [uint32]$bProc.Id
            $handles = [System.Collections.Generic.List[IntPtr]]::new()
            $callback = [Win32Grid+EnumWindowsProc]{
                param($hWnd, $lParam)
                [uint32]$pid = 0
                [Win32Grid]::GetWindowThreadProcessId($hWnd, [ref]$pid) | Out-Null
                if ($pid -eq $targetPid -and [Win32Grid]::IsWindowVisible($hWnd)) {
                    $len = [Win32Grid]::GetWindowTextLength($hWnd)
                    if ($len -gt 0) {
                        $handles.Add($hWnd)
                    }
                }
                return $true
            }
            [Win32Grid]::EnumWindows($callback, [IntPtr]::Zero) | Out-Null

            foreach ($hwnd in $handles) {
                [Win32Grid]::MoveWindow($hwnd, $pos.X, $pos.Y, $halfWidth, $halfHeight, $true) | Out-Null
            }
        }
    }
}

Write-Host "`nAll apps launched! Press Ctrl+C to stop." -ForegroundColor Cyan
Write-Host "Process IDs:" -ForegroundColor Gray
for ($i = 0; $i -lt $apps.Count; $i++) {
    Write-Host "  $($apps[$i].Name): PID $($dotnetProcesses[$i].Id)" -ForegroundColor Gray
}

# Wait and clean up on Ctrl+C
try {
    Write-Host "`nWaiting... (Ctrl+C to stop all)" -ForegroundColor Yellow
    while ($true) {
        Start-Sleep -Seconds 5
        # Check if any process exited unexpectedly
        foreach ($proc in $dotnetProcesses) {
            if ($proc.HasExited) {
                Write-Host "  Warning: Process $($proc.Id) exited with code $($proc.ExitCode)" -ForegroundColor Red
            }
        }
    }
} finally {
    Write-Host "`nStopping all apps..." -ForegroundColor Cyan
    foreach ($proc in $dotnetProcesses) {
        if (-not $proc.HasExited) {
            Stop-Process -Id $proc.Id -Force -ErrorAction SilentlyContinue
            Write-Host "  Stopped dotnet PID $($proc.Id)" -ForegroundColor Yellow
        }
    }
    # Close browser windows
    foreach ($proc in $browserProcesses) {
        if ($proc -and -not $proc.HasExited) {
            Stop-Process -Id $proc.Id -Force -ErrorAction SilentlyContinue
        }
    }
    # Clean up temp Edge profile dirs
    foreach ($dir in $tempDirs) {
        Remove-Item -Path $dir -Recurse -Force -ErrorAction SilentlyContinue
    }
    Write-Host "All apps stopped." -ForegroundColor Green
}
