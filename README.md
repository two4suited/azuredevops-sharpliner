# Azure DevOps Pipeline Generator

This solution contains a Sharpliner-based Azure DevOps pipeline generator that creates YAML pipeline files in a `.azdo` directory.

## Structure

- **Azuredevops-sharpliner.Pipelines/**: Library project containing pipeline definitions using Sharpliner
- **Azuredevops-sharpliner.Generator/**: Console application that generates YAML files from the pipeline definitions

## Components

### Pipeline Library (`Azuredevops-sharpliner.Pipelines`)

- **DotNetBuildPipeline**: Multi-stage pipeline with Build and Publish stages
- **DotNetPRPipeline**: Single-stage pull request validation pipeline (restore, build, test)
- **DotNetTasks**: Reusable .NET task definitions (Restore, Build, Test, Publish)
- **BuildPools**: Type-safe enum for Azure DevOps hosted pools

### Generator Console App (`Azuredevops-sharpliner.Generator`)

The generator application:
1. Creates a `.azdo` directory in the current working directory
2. Supports generating all pipelines or specific pipelines by name
3. Allows custom target filenames for generated YAML files
4. Provides detailed logging and file size information

### Command Line Options

- **No arguments**: Generates all available pipelines with default filenames
- **Pipeline name**: Generates specific pipeline (build, pr)
- **Pipeline name + filename**: Generates specific pipeline with custom filename

Available pipeline names:
- `build` or `dotnetbuild` - Multi-stage build and publish pipeline
- `pr` or `dotnetpr` - Single-stage pull request validation pipeline

## Usage

### Run the Generator

```bash
# Build the solution
dotnet build

# Generate all pipelines (default)
dotnet run --project Azuredevops-sharpliner.Generator/Azuredevops-sharpliner.Generator.csproj

# Generate specific pipeline
dotnet run --project Azuredevops-sharpliner.Generator/Azuredevops-sharpliner.Generator.csproj -- build
dotnet run --project Azuredevops-sharpliner.Generator/Azuredevops-sharpliner.Generator.csproj -- pr

# Generate with custom filename
dotnet run --project Azuredevops-sharpliner.Generator/Azuredevops-sharpliner.Generator.csproj -- build my-build-pipeline.yml
dotnet run --project Azuredevops-sharpliner.Generator/Azuredevops-sharpliner.Generator.csproj -- pr my-pr-pipeline.yml

# Using the convenience script
./generate-pipelines.sh              # Generate all
./generate-pipelines.sh build        # Generate build pipeline only
./generate-pipelines.sh pr custom.yml # Generate PR pipeline with custom name
```

### Generated Output

The generator creates:
- `.azdo/` directory containing all generated pipeline YAML files
- `dotnet-build.yml` - Multi-stage .NET build and publish pipeline
- `dotnet-pr.yml` - Single-stage pull request validation pipeline

### Example Output

```
🚀 Azure DevOps Pipeline Generator
==================================
📁 Created directory: /path/to/project/.azdo
🔄 Generating pipeline definitions...
📋 Found 2 pipeline definition(s)
🔨 Processing: DotNetBuildPipeline
✅ Generated: .azdo/dotnet-build.yml
🔨 Processing: DotNetPRPipeline
✅ Generated: .azdo/dotnet-pr.yml

📊 Generation Summary
====================
📁 Output Directory: /path/to/project/.azdo
📄 Files Generated: 2

📋 Generated Files:
   • .azdo/dotnet-build.yml (1,486 bytes)
   • .azdo/dotnet-pr.yml (627 bytes)

🎉 Pipeline generation completed successfully!
```

## Adding New Pipelines

To add new pipeline definitions:

1. Create a new class in the `Azuredevops-sharpliner.Pipelines` project
2. Inherit from `PipelineDefinition` (multi-stage) or `SingleStagePipelineDefinition` (single stage)
3. Implement the required properties and methods
4. The generator will automatically discover and process the new pipeline

## Features

- **Automatic Discovery**: Finds all pipeline definitions using reflection
- **Type Safety**: Uses enums for build pools and standardized task definitions
- **Modular Design**: Separated concerns with reusable components
- **Error Handling**: Comprehensive error reporting and logging
- **File Management**: Automatic directory creation and file output management
