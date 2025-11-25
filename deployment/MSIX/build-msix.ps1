# Build MSIX Package for El Mansour Syndic Manager
# Requires: Windows SDK, MSIX Packaging Tool, or Visual Studio

param(
    [string]$Configuration = "Release",
    [string]$Version = "1.0.0.0"
)

$ErrorActionPreference = "Stop"

Write-Host "Building MSIX Package for El Mansour Syndic Manager" -ForegroundColor Green

# Set paths
$ProjectRoot = "c:\Users\adamk\Desktop\raisidance application"
$PublishPath = Join-Path $ProjectRoot "publish"
$MSIXPath = Join-Path $ProjectRoot "deployment\MSIX"
$OutputPath = Join-Path $ProjectRoot "deployment\Output"
$ProjectPath = "c:\Users\adamk\Desktop\raisidance application\src\ElMansourSyndicManager\ElMansourSyndicManager.csproj"

# Clean previous builds
Write-Host "Cleaning previous builds..." -ForegroundColor Yellow
if (Test-Path $PublishPath) { Remove-Item $PublishPath -Recurse -Force }
if (Test-Path $OutputPath) { Remove-Item $OutputPath -Recurse -Force }
New-Item -ItemType Directory -Path $OutputPath -Force | Out-Null

# Build and publish application
Write-Host "Building application..." -ForegroundColor Yellow

dotnet publish $ProjectPath `
    -c $Configuration `
    -r win-x64 `
    --self-contained true `
    -p:PublishSingleFile=true `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -p:EnableCompressionInSingleFile=true `
    -o $PublishPath

if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed!" -ForegroundColor Red
    exit 1
}

# Copy assets
Write-Host "Copying assets..." -ForegroundColor Yellow
$AssetsPath = Join-Path $MSIXPath "Assets"
$PublishAssetsPath = Join-Path $PublishPath "Assets"
if (-not (Test-Path $PublishAssetsPath)) {
    New-Item -ItemType Directory -Path $PublishAssetsPath -Force | Out-Null
}
Copy-Item (Join-Path $AssetsPath "*") $PublishAssetsPath -Recurse -Force

# Copy manifest
Copy-Item (Join-Path $MSIXPath "Package.appxmanifest") (Join-Path $PublishPath "AppxManifest.xml") -Force

# Create MSIX package using MakeAppx
Write-Host "Creating MSIX package..." -ForegroundColor Yellow
$MSIXFile = Join-Path $OutputPath "ElMansourSyndicManager_$Version.msix"

# Check if MakeAppx is available
$MakeAppxPath = "${env:ProgramFiles(x86)}\Windows Kits\10\bin\10.0.26100.0\x64\makeappx.exe"
if (-not (Test-Path $MakeAppxPath)) {
    # Try alternative paths
    $MakeAppxPath = Get-ChildItem "${env:ProgramFiles(x86)}\Windows Kits\10\bin" -Recurse -Filter "makeappx.exe" | Select-Object -First 1 -ExpandProperty FullName
}

if ($MakeAppxPath -and (Test-Path $MakeAppxPath)) {
    & $MakeAppxPath pack /d $PublishPath /p $MSIXFile /o
    Write-Host "MSIX package created: $MSIXFile" -ForegroundColor Green
} else {
    Write-Host "MakeAppx not found. Please install Windows SDK or use Visual Studio to create MSIX package." -ForegroundColor Yellow
    Write-Host "Package files are ready in: $PublishPath" -ForegroundColor Yellow
}

# Sign package (optional - requires certificate)
$CertPath = Join-Path $MSIXPath "ElMansourSyndic.pfx"
if (Test-Path $CertPath) {
    Write-Host "Signing MSIX package..." -ForegroundColor Yellow
    $SignToolPath = $null
    $sdkPaths = @(
        "${env:ProgramFiles(x86)}\Windows Kits\10\bin\10.0.26100.0\x64\signtool.exe",
        "${env:ProgramFiles(x86)}\Windows Kits\10\bin\10.0.22621.0\x64\signtool.exe",
        "${env:ProgramFiles(x86)}\Windows Kits\10\bin\10.0.19041.0\x64\signtool.exe",
        "${env:ProgramFiles(x86)}\Windows Kits\10\bin\10.0.18362.0\x64\signtool.exe",
        "${env:ProgramFiles(x86)}\Windows Kits\10\bin\x64\signtool.exe" # Generic x64 path
    )

    foreach ($path in $sdkPaths) {
        if (Test-Path $path) {
            $SignToolPath = $path
            break
        }
    }

    if ($SignToolPath -and (Test-Path $SignToolPath)) {
        & $SignToolPath sign /f $CertPath /p "ChangeThisPassword123!" /fd SHA256 $MSIXFile
        Write-Host "Package signed successfully" -ForegroundColor Green
    } else {
        Write-Host "SignTool.exe not found. Please ensure Windows SDK is installed and signtool.exe is in one of the expected paths." -ForegroundColor Yellow
    }
}

Write-Host "`nBuild completed successfully!" -ForegroundColor Green
Write-Host "Output: $OutputPath" -ForegroundColor Cyan
