#!/bin/bash
# Azure DevOps Pipeline Generator Script

echo "🔧 Building solution..."
dotnet build

if [ $? -eq 0 ]; then
    echo ""
    echo "🚀 Running pipeline generator..."
    if [ $# -eq 0 ]; then
        # No arguments - generate all pipelines
        dotnet run --project Azuredevops-sharpliner.Generator/Azuredevops-sharpliner.Generator.csproj
    else
        # Pass arguments to the generator
        dotnet run --project Azuredevops-sharpliner.Generator/Azuredevops-sharpliner.Generator.csproj -- "$@"
    fi
else
    echo "❌ Build failed. Please fix any compilation errors before running the generator."
    exit 1
fi
