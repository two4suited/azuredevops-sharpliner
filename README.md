# Azure DevOps Pipeline Generator with Sharpliner

This solution uses **Sharpliner** - a C# DSL (Domain Specific Language) to generate Azure DevOps pipeline YAML files. All generated pipelines are placed in a `.azdo` directory at the repository root.

## Project Structure

```
repo-root/
├── Azuredevops-sharpliner.Pipelines.sln         # Main solution file
├── create-sharpliner-project.ps1                # Project scaffolding script
├── generate-pipelines.sh                        # Pipeline generation script
└── Azuredevops-sharpliner.Pipelines/            # Pipeline definitions library
    ├── DotNetBuildPipeline.cs                   # Multi-stage build pipeline
    ├── DotNetPRPipeline.cs                      # PR validation pipeline
    ├── DotNetTasks.cs                           # Reusable .NET tasks
    └── BuildPools.cs                            # Azure-hosted pool definitions
└── Azuredevops-sharpliner.Generator/            # Pipeline instances
    └── Program.cs                               # Concrete pipeline definitions
```

## Components

### Pipeline Library (`Azuredevops-sharpliner.Pipelines`)

**Base Pipeline Classes:**
- **DotNetBuildPipeline**: Multi-stage pipeline with Build and Publish stages
  - Constructor: `DotNetBuildPipeline(fileName = "azure-pipelines.yml", folder = ".azdo")`
  - Generates: `.azdo/azure-pipelines.yml` by default
  - Features: Flexible filename and folder configuration
  
- **DotNetPRPipeline**: Single-stage pull request validation pipeline
  - Constructor: `DotNetPRPipeline(fileName = "azure-pipelines-pr.yml", folder = ".azdo")`
  - Generates: `.azdo/azure-pipelines-pr.yml` by default
  - Features: Restore, build, and test operations only

**Supporting Components:**
- **DotNetTasks**: Reusable .NET task definitions (Restore, Build, Test, Publish)
- **BuildPools**: Type-safe enum for Azure DevOps hosted pools

### Generator Project (`Azuredevops-sharpliner.Generator`)

Contains concrete pipeline instances that use the base class defaults:
- **ProjectPipeline**: Main build pipeline → `.azdo/azure-pipelines.yml`
- **ProjectPRPipeline**: PR validation pipeline → `.azdo/azure-pipelines-pr.yml`

Both classes use simple inheritance with no customization:
```csharp
public class ProjectPipeline : DotNetBuildPipeline{}
public class ProjectPRPipeline : DotNetPRPipeline{}
```

## Usage

### Quick Start

```bash
# Build the Generator project to generate pipeline files
dotnet build Azuredevops-sharpliner.Generator

# Or build the entire solution
dotnet build

# Or use the convenience script
./generate-pipelines.sh
```

### Generated Output

Building the Generator project automatically creates:
- `.azdo/` directory at repository root
- `azure-pipelines.yml` - Multi-stage .NET build and publish pipeline
- `azure-pipelines-pr.yml` - Single-stage pull request validation pipeline

### Pipeline Configuration Examples

**Creating custom pipeline instances:**

```csharp
// Default configuration (recommended)
public class MyBuildPipeline : DotNetBuildPipeline 
{
    // Generates: .azdo/azure-pipelines.yml
}

// Custom filename, default folder
public class MyBuildPipeline : DotNetBuildPipeline
{
    public MyBuildPipeline() : base("my-custom-build.yml") { }
    // Generates: .azdo/my-custom-build.yml
}

// Custom folder, default filename
public class MyBuildPipeline : DotNetBuildPipeline
{
    public MyBuildPipeline() : base(folder: "pipelines") { }
    // Generates: pipelines/azure-pipelines.yml
}

// Both custom
public class MyBuildPipeline : DotNetBuildPipeline
{
    public MyBuildPipeline() : base("build.yml", "ci-cd") { }
    // Generates: ci-cd/build.yml
}
```

## Adding New Pipelines

### Option 1: Create New Pipeline Class (Recommended)

1. **In `Azuredevops-sharpliner.Pipelines` project**, create a new pipeline class:

```csharp
public class MyCustomPipeline : SimpleYamlPipeline
{
    public override string TargetFile => ".azdo/my-custom-pipeline.yml";
    public override TargetPathType TargetPathType => TargetPathType.RelativeToGitRoot;
    
    public override SingleStagePipeline Pipeline => new()
    {
        Jobs = 
        {
            new Job("MyJob")
            {
                Pool = new HostedPool("ubuntu-latest"),
                Steps = 
                {
                    Checkout.Self,
                    // Add your steps here
                }
            }
        }
    };
}
```

2. **In `Azuredevops-sharpliner.Generator` project**, create an instance:

```csharp
public class MyProjectPipeline : MyCustomPipeline { }
```

### Option 2: Extend Existing Base Classes

```csharp
// For .NET projects, extend the base classes
public class MySpecialBuildPipeline : DotNetBuildPipeline
{
    public MySpecialBuildPipeline() : base("special-build.yml", "custom-folder") { }
}
```

## Key Features

- **MSBuild Integration**: YAML files generated automatically during `dotnet build`
- **Git Root Placement**: All pipelines placed relative to repository root (not project folder)
- **Flexible Configuration**: Separate filename and folder parameters
- **Type Safety**: C# classes with compile-time validation
- **Reusable Components**: Shared tasks, pools, and pipeline patterns
- **Sharpliner DSL**: Full access to Sharpliner's Azure DevOps pipeline features

## Dependencies

- **.NET 9.0**: Target framework
- **Sharpliner 1.8.1**: Core DSL for pipeline definitions
- **Microsoft.Build.Framework/Utilities.Core**: MSBuild integration
- **YamlDotNet 16.3.0**: YAML serialization

## Development Workflow

```bash
# 1. Make changes to pipeline definitions
# 2. Build Generator project to generate YAML files
dotnet build Azuredevops-sharpliner.Generator

# 3. Commit both C# source and generated YAML files
git add .
git commit -m "Update pipeline definitions"
```

The generated YAML files should be committed to source control for transparency and Azure DevOps integration.
