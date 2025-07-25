using Sharpliner.AzureDevOps;
using Sharpliner.AzureDevOps.Tasks;
using Sharpliner;

namespace Azuredevops_sharpliner.Pipelines;

/// <summary>
/// Defines a multi-stage .NET pipeline with separate build and publish stages
/// </summary>
public class DotNetBuildPipeline : PipelineDefinition
{
    // Configuration for the pipeline
    private static readonly BuildPool BuildPool = BuildPool.UbuntuLatest;
    private static readonly BuildPool PublishPool = BuildPool.UbuntuLatest;
    private readonly string _targetFile;
    
    public DotNetBuildPipeline(string fileName = "dotnet-build.yml", string folder = ".azdo")
    {
        _targetFile = $"{folder}/{fileName}";
    }
    
    public override string TargetFile => _targetFile;

    public override TargetPathType TargetPathType => TargetPathType.RelativeToGitRoot;

    public override Pipeline Pipeline => new()
    {
        Stages =
        {
            new Stage("Build", "Build and Test")
            {
                Jobs =
                {
                    new Job("BuildJob", "Build and Test .NET Application")
                    {
                        Pool = BuildPool.ToHostedPool(),
                        
                        Steps =
                        {
                            // Build and test steps
                            new SelfCheckoutTask(),
                            DotNetTasks.Restore(),
                            DotNetTasks.Build(),
                            DotNetTasks.Test()
                        }
                    }
                }
            },
            
            new Stage("Publish", "Publish Application")
            {
                Jobs =
                {
                    new Job("PublishJob", "Publish .NET Application")
                    {
                        Pool = PublishPool.ToHostedPool(),
                        
                        Steps =
                        {
                            // Publish steps
                            new SelfCheckoutTask(),
                            DotNetTasks.Restore(),
                            DotNetTasks.Build(),
                            DotNetTasks.Publish()
                        }
                    }
                }
            }
        }
    };
}
