using Sharpliner.AzureDevOps.Tasks;

namespace Azuredevops_sharpliner.Pipelines;

/// <summary>
/// Static class containing reusable .NET build task definitions
/// </summary>
public static class DotNetTasks
{
    /// <summary>
    /// Creates a restore task for all .csproj files
    /// </summary>
    public static DotNetCoreCliTask Restore() => new("restore")
    {
        DisplayName = "Restore NuGet packages",
        Projects = "**/*.csproj"
    };

    /// <summary>
    /// Creates a build task for all solution files in Release configuration
    /// </summary>
    public static DotNetCoreCliTask Build() => new("build")
    {
        DisplayName = "Build solution",
        Projects = "**/*.sln",
        Arguments = "--configuration Release --no-restore"
    };

    /// <summary>
    /// Creates a test task for all test projects
    /// </summary>
    public static DotNetCoreCliTask Test() => new("test")
    {
        DisplayName = "Run unit tests",
        Projects = "**/*Tests.csproj",
        Arguments = "--configuration Release --no-build --verbosity normal"
    };

    /// <summary>
    /// Creates a publish task for the main application
    /// </summary>
    public static DotNetCoreCliTask Publish() => new("publish")
    {
        DisplayName = "Publish application",
        Projects = "**/*.csproj",
        Arguments = "--configuration Release --no-build --output $(Build.ArtifactStagingDirectory)"
    };
}
