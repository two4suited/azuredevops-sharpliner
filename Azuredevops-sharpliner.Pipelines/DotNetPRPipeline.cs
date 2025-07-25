using Sharpliner.AzureDevOps;
using Sharpliner.AzureDevOps.Tasks;
using Sharpliner;

namespace Azuredevops_sharpliner.Pipelines;

/// <summary>
/// Defines a lightweight pipeline for pull request validation
/// Performs restore, build, and test operations only
/// </summary>
public class DotNetPRPipeline : SingleStagePipelineDefinition
{
    // Configuration for the pipeline
    private static readonly BuildPool BuildPool = BuildPool.UbuntuLatest;
    private readonly string _targetFile;
    
    public DotNetPRPipeline(string fileName = "dotnet-pr.yml", string folder = ".azdo")
    {
        _targetFile = $"{folder}/{fileName}";
    }
    
    public override string TargetFile => _targetFile;

    public override TargetPathType TargetPathType => TargetPathType.RelativeToGitRoot;

    public override SingleStagePipeline Pipeline => new()
    {
        Jobs =
        {
            new Job("PRValidation", "Pull Request Validation")
            {
                Pool = BuildPool.ToHostedPool(),
                
                Steps =
                {
                    // PR validation steps
                    new SelfCheckoutTask(),
                    DotNetTasks.Restore(),
                    DotNetTasks.Build(),
                    DotNetTasks.Test()
                }
            }
        }
    };
}
