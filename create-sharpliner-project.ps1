#!/usr/bin/env pwsh

<#
.SYNOPSIS
    Creates a new .NET console application with solution and adds Sharpliner package reference.

.DESCRIPTION
    This script creates a new .NET console application, creates a solution file, 
    adds the console project to the solution, and adds a package reference to Sharpliner.

.PARAMETER ProjectName
    The name of the project and solution to create. Defaults to "SharplinerConsoleApp".

.PARAMETER OutputPath
    The path where the project should be created. Defaults to current directory.

.PARAMETER SharplinerVersion
    The version of Sharpliner package to add. If not specified, uses the latest version.

.EXAMPLE
    .\create-sharpliner-project.ps1
    
    Creates a project with default name "SharplinerConsoleApp" in current directory.

.EXAMPLE
    .\create-sharpliner-project.ps1 -ProjectName "MyPipeline" -OutputPath "C:\Projects"
    
    Creates a project named "MyPipeline" in the "C:\Projects" directory.

.EXAMPLE
    .\create-sharpliner-project.ps1 -ProjectName "MyPipeline" -SharplinerVersion "2.0.0"
    
    Creates a project and adds Sharpliner version 2.0.0.
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory = $false)]
    [string]$ProjectName = "SharplinerConsoleApp",
    
    [Parameter(Mandatory = $false)]
    [string]$OutputPath = ".",
    
    [Parameter(Mandatory = $false)]
    [string]$SharplinerVersion = $null
)

# Function to write colored output
function Write-Step {
    param([string]$Message)
    Write-Host "➤ $Message" -ForegroundColor Cyan
}

function Write-Success {
    param([string]$Message)
    Write-Host "✓ $Message" -ForegroundColor Green
}

function Write-Error {
    param([string]$Message)
    Write-Host "✗ $Message" -ForegroundColor Red
}

function Write-Warning {
    param([string]$Message)
    Write-Host "⚠ $Message" -ForegroundColor Yellow
}

# Validate prerequisites
Write-Step "Checking prerequisites..."

# Check if dotnet CLI is installed
try {
    $dotnetVersion = dotnet --version 2>$null
    if ($LASTEXITCODE -eq 0) {
        Write-Success "Found .NET CLI version: $dotnetVersion"
    } else {
        Write-Error ".NET CLI not found. Please install .NET SDK from https://dotnet.microsoft.com/download"
        exit 1
    }
} catch {
    Write-Error ".NET CLI not found. Please install .NET SDK from https://dotnet.microsoft.com/download"
    exit 1
}

# Resolve full output path
$FullOutputPath = Resolve-Path -Path $OutputPath -ErrorAction SilentlyContinue
if (-not $FullOutputPath) {
    $FullOutputPath = $OutputPath
}

$ProjectPath = Join-Path $FullOutputPath $ProjectName

# Check if project directory already exists
if (Test-Path $ProjectPath) {
    Write-Warning "Directory '$ProjectPath' already exists."
    $response = Read-Host "Do you want to continue and potentially overwrite existing files? (y/N)"
    if ($response -ne 'y' -and $response -ne 'Y') {
        Write-Host "Operation cancelled."
        exit 0
    }
}

try {
    # Create solution at the output path first
    Write-Step "Creating solution..."
    Set-Location $FullOutputPath
    dotnet new sln -n $ProjectName
    if ($LASTEXITCODE -eq 0) {
        Write-Success "Created solution: $ProjectName.sln at $FullOutputPath"
    } else {
        throw "Failed to create solution"
    }

    # Create project directory
    Write-Step "Creating project directory..."
    New-Item -ItemType Directory -Path $ProjectPath -Force | Out-Null
    Set-Location $ProjectPath
    Write-Success "Created directory: $ProjectPath"

    # Create console application
    Write-Step "Creating console application..."
    dotnet new console --framework net9.0
    if ($LASTEXITCODE -eq 0) {
        Write-Success "Created console application: $ProjectName"
    } else {
        throw "Failed to create console application"
    }

    # Add project to solution (go back to solution directory)
    Write-Step "Adding project to solution..."
    Set-Location $FullOutputPath
    dotnet sln add "$ProjectName/$ProjectName.csproj"
    if ($LASTEXITCODE -eq 0) {
        Write-Success "Added project to solution"
    } else {
        throw "Failed to add project to solution"
    }

    # Add Sharpliner package reference
    Write-Step "Adding Sharpliner package reference..."
    Set-Location "$ProjectPath"
    
    if ($SharplinerVersion) {
        dotnet add package Sharpliner --version $SharplinerVersion
        $versionText = "version $SharplinerVersion"
    } else {
        dotnet add package Sharpliner
        $versionText = "latest version"
    }
    
    if ($LASTEXITCODE -eq 0) {
        Write-Success "Added Sharpliner package reference ($versionText)"
    } else {
        Write-Warning "Failed to add Sharpliner package. The package might not exist or version might be invalid."
        Write-Host "You can manually add it later using: dotnet add package Sharpliner"
    }

    # Restore packages
    Write-Step "Restoring packages..."
    dotnet restore
    if ($LASTEXITCODE -eq 0) {
        Write-Success "Packages restored successfully"
    } else {
        Write-Warning "Package restore had issues, but project was created"
    }

    # Return to original directory
    Set-Location $FullOutputPath

    # Display project structure
    Write-Host ""
    Write-Host "Project structure created:" -ForegroundColor Yellow
    Write-Host "� $ProjectName.sln" -ForegroundColor Gray
    Write-Host "� $ProjectName/" -ForegroundColor Blue
    Write-Host "  📁 $ProjectName/" -ForegroundColor Blue
    Write-Host "    📄 $ProjectName.csproj" -ForegroundColor Gray
    Write-Host "    📄 Program.cs" -ForegroundColor Gray
    Write-Host ""

    # Show next steps
    Write-Host "Next steps:" -ForegroundColor Yellow
    Write-Host "1. cd '$FullOutputPath'" -ForegroundColor White
    Write-Host "2. code . # Open in VS Code" -ForegroundColor White
    Write-Host "3. dotnet run --project $ProjectName/$ProjectName # Run the application" -ForegroundColor White
    Write-Host ""
    Write-Host "To build and run:" -ForegroundColor Yellow
    Write-Host "dotnet build && dotnet run --project $ProjectName/$ProjectName" -ForegroundColor White

    Write-Success "Project created successfully!"

} catch {
    Write-Error "An error occurred: $($_.Exception.Message)"
    exit 1
}
