using Sharpliner.AzureDevOps;

namespace Azuredevops_sharpliner.Pipelines;

/// <summary>
/// Enumeration of available Azure DevOps hosted pools
/// </summary>
public enum BuildPool
{
    UbuntuLatest,
    WindowsLatest,
    MacOSLatest,
    Ubuntu2004,
    Ubuntu2204,
    Windows2019,
    Windows2022,
    MacOS11,
    MacOS12
}

/// <summary>
/// Extension methods for converting BuildPool enum to HostedPool instances
/// </summary>
public static class BuildPoolExtensions
{
    /// <summary>
    /// Converts a BuildPool enum value to a HostedPool instance
    /// </summary>
    public static HostedPool ToHostedPool(this BuildPool pool) => pool switch
    {
        BuildPool.UbuntuLatest => new HostedPool("ubuntu-latest"),
        BuildPool.WindowsLatest => new HostedPool("windows-latest"),
        BuildPool.MacOSLatest => new HostedPool("macos-latest"),
        BuildPool.Ubuntu2004 => new HostedPool("ubuntu-20.04"),
        BuildPool.Ubuntu2204 => new HostedPool("ubuntu-22.04"),
        BuildPool.Windows2019 => new HostedPool("windows-2019"),
        BuildPool.Windows2022 => new HostedPool("windows-2022"),
        BuildPool.MacOS11 => new HostedPool("macos-11"),
        BuildPool.MacOS12 => new HostedPool("macos-12"),
        _ => new HostedPool("ubuntu-latest")
    };
}
