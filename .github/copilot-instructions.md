# Copilot Instructions for Azure DevOps Sharpliner Project

## Overview
This project creates Azure DevOps pipelines using **Sharpliner** - a C# DSL (Domain Specific Language) that generates YAML pipeline definitions. The solution uses .NET 9.0 and follows a specific nested project structure.

## Project Architecture

### Solution Structure
```
repo-root/
├── Azuredevops-sharpliner.Pipelines.sln         # Main solution file
├── create-sharpliner-project.ps1                # Project scaffolding script
└── Azuredevops-sharpliner.Pipelines/
    └── Azuredevops-sharpliner.Pipelines/        # Actual project folder
        ├── Azuredevops-sharpliner.Pipelines.csproj
        └── Program.cs
```

**Critical**: The solution file lives at repository root, but the project has a nested folder structure (`ProjectName/ProjectName/`). This pattern is established by the scaffolding script.

### Key Dependencies
- **Sharpliner 1.8.1**: Core DSL for pipeline definitions
- **Microsoft.Build.Framework/Utilities.Core**: MSBuild integration
- **YamlDotNet 16.3.0**: YAML serialization
- **OneOf 3.0.271**: Union types for Sharpliner's API

## Development Workflow

### Build Commands
```bash
# From repository root
dotnet build                                      # Build entire solution
dotnet run --project Azuredevops-sharpliner.Pipelines/Azuredevops-sharpliner.Pipelines
```

### Project Creation
Use the PowerShell script to create new Sharpliner projects:
```powershell
./create-sharpliner-project.ps1 -ProjectName "MyPipeline" -OutputPath "."
```

The script automatically:
- Creates nested project structure
- Adds Sharpliner package reference
- Sets up .NET 9.0 targeting
- Configures root namespace with underscore replacement (`-` → `_`)

## Sharpliner Pipeline Definition Patterns

### Core Base Classes
- **`SimpleYamlPipeline`**: Single-stage pipelines with jobs and steps
- **`SingleStagePipeline`**: More explicit single-stage definition
- **`MultiStagePipeline`**: Complex multi-stage pipeline scenarios
- **`PipelineTemplate`**: Reusable pipeline templates
- **`StepLibrary`**: Collections of reusable steps
- **`JobTemplate`**: Reusable job definitions
- **`VariableTemplate`**: Shared variable definitions

### Basic Pipeline Example
```csharp
public class BuildPipeline : SimpleYamlPipeline
{
    public override string TargetFile => "pipelines/build.yml";

    public override TargetPathType TargetPathType => TargetPathType.RelativeToGitRoot;

    public override SingleStagePipeline Pipeline => new()
    {
        Jobs =
        {
            new Job("Build")
            {
                Pool = new HostedPool("ubuntu-latest"),
                Steps =
                {
                    Checkout.Self,
                    DotNet.Restore(),
                    DotNet.Build("MyProject.sln"),
                    DotNet.Test("MyProject.sln")
                        .DisplayAs("Run unit tests")
                        .WithCondition(Condition.SucceededOrFailed)
                }
            }
        }
    };
}
```

### Multi-Stage Pipeline Pattern
```csharp
public class CompletePipeline : MultiStagePipeline
{
    public override string TargetFile => "pipelines/complete.yml";

    public override List<Stage> Stages =>
    [
        new Stage("Build")
        {
            Jobs =
            {
                new Job("BuildJob")
                {
                    Steps = { DotNet.Build("Solution.sln") }
                }
            }
        },
        new Stage("Deploy")
        {
            Condition = Condition.And(Condition.Succeeded(), Condition.Eq(variables["Build.SourceBranch"], "refs/heads/main")),
            Jobs =
            {
                new Job("DeployJob")
                {
                    Environment = "production",
                    Steps = { /* deployment steps */ }
                }
            }
        }
    ];
}
```

### Definition Collections (Dynamic Generation)
Use collections to generate multiple similar pipelines programmatically:

```csharp
class TestPipelines : SingleStagePipelineCollection
{
    private static readonly string[] s_platforms = ["ubuntu-20.04", "windows-2019"];

    public override IEnumerable<PipelineDefinitionData<SingleStagePipeline>> Pipelines =>
        s_platforms.Select(platform => new PipelineDefinitionData<SingleStagePipeline>(
            TargetFile: $"tests/pipelines/{platform}.yml",
            Pipeline: Define(platform),
            Header: ["Generated pipeline for " + platform]));

    private static SingleStagePipeline Define(string platform) => new()
    {
        Jobs =
        {
            new Job("Build")
            {
                Pool = new HostedPool(name: platform),
                Steps =
                {
                    DotNet.Build("Sharpliner.sln", includeNuGetOrg: true),
                    DotNet.Test("Sharpliner.sln")
                }
            }
        }
    };
}
```

### Template and Library Patterns

#### Step Library Example
```csharp
public class CommonSteps : StepLibrary
{
    public override string TargetFile => "templates/steps/common.yml";

    public override List<Step> Steps =>
    [
        Checkout.Self,
        PowerShell.Inline("echo 'Setting up environment'")
            .DisplayAs("Environment setup"),
        DotNet.Restore()
            .WithCondition(Condition.Succeeded())
    ];
}
```

#### Job Template Example
```csharp
public class BuildJobTemplate : JobTemplate
{
    public override string TargetFile => "templates/jobs/build.yml";

    public override TemplateParameters Parameters =>
    [
        StringParameter("solution", "Solution file to build"),
        BooleanParameter("runTests", defaultValue: true)
    ];

    public override Job Job => new("BuildJob")
    {
        Pool = new HostedPool("ubuntu-latest"),
        Steps =
        {
            DotNet.Build(parameters["solution"]),
            If.IsTruthy(parameters["runTests"])
                .Step(DotNet.Test(parameters["solution"]))
        }
    };
}
```

### Advanced Features

#### Variables and Parameters
```csharp
public override VariableDefinition Variables =>
[
    Variable("BuildConfiguration", "Release"),
    Variable("Version", "1.0.0"),
    Group("SharedVariables")  // Variable group reference
];

public override ParameterDefinition Parameters =>
[
    StringParameter("environment", "target environment", allowedValues: ["dev", "staging", "prod"]),
    BooleanParameter("skipTests", defaultValue: false)
];
```

#### Conditional Logic
```csharp
Steps =
{
    If.IsBranch("main")
        .Step(PowerShell.Inline("echo 'Main branch deployment'"))
        .ElseStep(PowerShell.Inline("echo 'Feature branch build'")),
    
    If.And(
        Condition.Succeeded(),
        Condition.Eq(variables["Build.Reason"], "PullRequest")
    ).Step(/* PR-specific steps */)
};
```

#### Resource References
```csharp
public override ResourceDefinition Resources =>
[
    new Repository("self"),
    new Repository("templates") 
    { 
        Repository = "MyOrg/pipeline-templates",
        Reference = "refs/heads/main"
    },
    new Container("buildImage")
    {
        Image = "mcr.microsoft.com/dotnet/sdk:8.0",
        Options = "--rm"
    }
];
```

## Build Steps and Azure Tasks

### Common .NET Build Steps
```csharp
// Standard .NET workflow
DotNet.Restore("**/*.csproj"),
DotNet.Build("Solution.sln")
    .WithConfiguration("Release")
    .WithArguments("--no-restore"),
DotNet.Test("**/*Tests.csproj")
    .DisplayAs("Run unit tests")
    .WithContinueOnError(true),
DotNet.Publish("MyApp/MyApp.csproj")
    .WithOutput("$(Build.ArtifactStagingDirectory)")
```

### Azure-Specific Tasks
```csharp
// Azure deployments
AzureCLI.Inline("az webapp deploy --resource-group myRG --name myApp")
    .WithAzureSubscription("MySubscription"),

// NuGet operations
NuGet.Pack("MyPackage.nuspec")
    .WithOutputDirectory("$(Build.ArtifactStagingDirectory)"),

// Publishing artifacts
PublishPipelineArtifact.ToPath("drop")
    .WithArtifactName("application")
    .WithTargetPath("$(Build.ArtifactStagingDirectory)")
```

### Custom PowerShell and Bash
```csharp
PowerShell.Inline("Get-ChildItem -Recurse *.dll")
    .DisplayAs("List assemblies")
    .WithWorkingDirectory("bin/Release"),

Bash.Inline("find . -name '*.log' -delete")
    .WithCondition(Condition.Always())
```

## Namespace Convention
Projects use underscores in root namespace (e.g., `Azuredevops_sharpliner.Pipelines`) due to .NET naming restrictions, even when project names contain hyphens.

## MSBuild Integration
Sharpliner includes MSBuild targets (`Sharpliner.props`, `Sharpliner.targets`) that automatically:
- Generate YAML files during build
- Integrate with dotnet CLI workflow
- Handle source generation
- Export definitions to specified target paths

## Code Generation Best Practices

### Pipeline Definition Guidelines
1. **Use appropriate base classes** based on complexity:
   - `SimpleYamlPipeline` for straightforward single-stage scenarios
   - `MultiStagePipeline` for complex workflows with multiple stages
   - Collections for generating multiple similar pipelines

2. **Leverage C# language features**:
   - Constants and enums for repeated values
   - Methods for complex logic and reusable components
   - Inheritance for sharing common pipeline patterns
   - LINQ for dynamic pipeline generation

3. **Template and Library Strategy**:
   - Create `StepLibrary` classes for commonly used step sequences
   - Use `JobTemplate` for reusable job patterns
   - Implement `VariableTemplate` for shared configuration

4. **Conditional Logic Patterns**:
   - Use `If.IsBranch()`, `If.IsPR()`, `If.IsVariableSet()` for branch-specific behavior
   - Combine conditions with `Condition.And()`, `Condition.Or()` for complex scenarios
   - Apply conditions at step, job, or stage level as appropriate

### File Organization Best Practices
- Separate pipeline definitions into focused classes
- Use meaningful class names that reflect pipeline purpose
- Organize templates and libraries in dedicated folders/namespaces
- Group related pipelines using definition collections

### Template Parameterization
- Define clear, typed parameters using `StringParameter`, `BooleanParameter`, etc.
- Provide sensible default values where possible
- Use `allowedValues` for constrained choices
- Document parameter purposes in class comments

## Integration Points

### Azure DevOps Integration
- Generated YAML files are standard Azure DevOps pipeline format
- Sharpliner handles serialization and validation automatically
- Support for all Azure DevOps pipeline features (stages, jobs, steps, conditions, etc.)
- Built-in support for Azure-specific tasks and marketplace extensions

### Source Control Patterns
- Include generated YAML files in source control for transparency and debugging
- Use Sharpliner source files (.cs) as the source of truth
- Consider pre-commit hooks to ensure YAML is regenerated when C# definitions change
- Set `TargetPathType.RelativeToGitRoot` for predictable file placement

### Continuous Integration
- Run `dotnet build` to generate updated YAML files
- Validate generated YAML in CI using Azure DevOps validation APIs
- Use definition collections to maintain multiple environment-specific pipelines

## Common Troubleshooting

### Build Issues
- Ensure .NET 9.0 SDK is installed and accessible
- Verify Sharpliner package references are restored (`dotnet restore`)
- Check for MSBuild errors in Sharpliner targets during build
- Validate that `TargetFile` paths are accessible and correctly formatted

### Project Structure Issues
- Maintain the nested folder structure (`ProjectName/ProjectName/`) established by scaffolding script
- Ensure solution file references correct project path
- Use underscores in root namespace for project names containing hyphens
- Verify MSBuild can resolve Sharpliner targets and props files

### YAML Generation Issues
- Check that pipeline classes are public and properly inherit from base types
- Ensure `TargetFile` property returns valid file path
- Verify that complex conditional logic translates correctly to YAML
- Use Sharpliner's built-in validation to catch common errors

### Template and Parameter Issues
- Ensure template parameters match usage in pipeline definitions
- Validate that parameter types align with Azure DevOps expectations
- Check that template references use correct relative paths
- Verify that variable and parameter names follow Azure DevOps naming conventions

When working with this codebase, prioritize understanding the Sharpliner DSL patterns, leverage the extensive built-in Azure DevOps task support, and utilize C# language features to create maintainable, reusable pipeline definitions.
