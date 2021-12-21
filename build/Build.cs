using System;
using System.Linq;
using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.Execution;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.GitVersion;
using Nuke.Common.Utilities;
using Nuke.Common.Utilities.Collections;
using static Nuke.Common.EnvironmentInfo;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.PathConstruction;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

[CheckBuildProjectConfigurations]
[ShutdownDotNetAfterServerBuild]
class Build : NukeBuild
{
    /// Support plugins are available for:
    ///   - JetBrains ReSharper        https://nuke.build/resharper
    ///   - JetBrains Rider            https://nuke.build/rider
    ///   - Microsoft VisualStudio     https://nuke.build/visualstudio
    ///   - Microsoft VSCode           https://nuke.build/vscode

    public static int Main () => Execute<Build>(x => x.Compile);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;
    
    [Parameter] readonly string NuGetToken;
    [Parameter] readonly string MygetPrivateToken;
    [Parameter] readonly string MygetPrivateUrl = "";
    
    const string NugetOrgUrl = "https://api.nuget.org/v3/index.json";

    [Solution] readonly Solution Solution;
    [GitRepository] readonly GitRepository GitRepository;
    [GitVersion] readonly GitVersion GitVersion;

    AbsolutePath SourceDirectory => RootDirectory / "src";
    AbsolutePath ArtifactsDirectory => RootDirectory / "artifacts";

    Target Clean => _ => _
        .Before(Restore)
        .Executes(() =>
        {
            SourceDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);
            EnsureCleanDirectory(ArtifactsDirectory);
        });

    Target Restore => _ => _
        .Executes(() =>
        {
            DotNetRestore(s => s
                .SetProjectFile(Solution));
        });

    Target Compile => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            DotNetBuild(s => s
                .SetProjectFile(Solution)
                .SetConfiguration(Configuration)
                .SetAssemblyVersion(GitVersion.AssemblySemVer)
                .SetFileVersion(GitVersion.AssemblySemFileVer)
                .SetInformationalVersion(GitVersion.InformationalVersion)
                .SetVersion(GitVersion.AssemblySemVer)
                .EnableNoRestore());
        });

    Target Pack => _ => _
        .DependsOn(Compile)
        .Executes(() =>
        {
            DeleteDirectory(ArtifactsDirectory);
            
            var project = Solution.GetProject("Rogero.ReactiveSourceGenerator");
            DotNetPack(s =>
                           s.SetProject(project.Path)
                               .SetVersion(GitVersion.NuGetVersionV2)
                               .SetOutputDirectory(ArtifactsDirectory)
            );
        });
    
    Target PushToMyget => _ => _
        .DependsOn(Pack)
        .Requires(() => !MygetPrivateUrl.IsNullOrWhiteSpace())
        .Executes(() =>
        {
            Console.WriteLine(MygetPrivateToken);
            Console.WriteLine(MygetPrivateUrl);
            var package = ArtifactsDirectory.GlobFiles("*.nupkg").Single();
            DotNetNuGetPush(s => s
                                .SetApiKey(MygetPrivateToken)
                                .SetSource(MygetPrivateUrl)
                                .SetTargetPath(package)
            );
        });
    

}
