#load nuget:?package=Cake.Recipe&version=3.1.1
#tool dotnet:?package=dotnet-t4&version=2.2.1
#addin nuget:?package=Cake.Git&version=1.0.0
#addin nuget:?package=Cake.FileHelpers&version=4.0.1

Environment.SetVariableNames(githubTokenVariable: "GITTOOLS_GITHUB_TOKEN");

var standardNotificationMessage = "A new version, {0} of {1} has just been released.  Get it from Chocolatey, NuGet, or as a .Net Global Tool.";

BuildParameters.SetParameters(context: Context,
                            buildSystem: BuildSystem,
                            sourceDirectoryPath: "./src",
                            title: "TimeStamper",
                            repositoryOwner: "corbob",
                            repositoryName: "timestamper",
                            shouldRunDotNetCorePack: true,
                            shouldRunIntegrationTests: true,
                            preferredBuildProviderType: BuildProviderType.GitHubActions);

BuildParameters.PrintParameters(Context);

ToolSettings.SetToolSettings(context: Context);

///////////////////////////////////////////////////////////////////////////////
// PROJECT SPECIFIC TASKS
///////////////////////////////////////////////////////////////////////////////

Task("Prepare-Chocolatey-Packages")
    .IsDependeeOf("Create-Chocolatey-Packages")
    .WithCriteria(() => BuildParameters.BuildAgentOperatingSystem == PlatformFamily.Windows, "Skipping because not running on Windows")
    .WithCriteria(() => BuildParameters.ShouldRunChocolatey, "Skipping because execution of Chocolatey has been disabled")
    .Does((context) =>
{
    Information(context);
    // Copy legal documents
    CopyFile(BuildParameters.RootDirectoryPath + "/LICENSE", BuildParameters.Paths.Directories.ChocolateyNuspecDirectory + "/tools/LICENSE.txt");

    // Copy built executables
    var filesToCopy = GetFiles(BuildParameters.Paths.Directories.PublishedApplications + "/TimeStamper/**/*.*");
    var verificationFile = GetFiles(BuildParameters.Paths.Directories.ChocolateyNuspecDirectory + "/tools/VERIFICATION.txt").FirstOrDefault();
    Information("Chocolatey package file checksums:");

    foreach (var file in filesToCopy)
    {
        var fileName = file.Segments[file.Segments.Length - 1];
        var checksumLine = fileName + ": " + CalculateFileHash(file, HashAlgorithm.SHA256).ToHex();
        Information(checksumLine);
        context.FileAppendLines(verificationFile, new [] { checksumLine });
    }
});


ToolSettings.SetToolPreprocessorDirectives(gitVersionGlobalTool: "#tool dotnet:?package=GitVersion.Tool&version=5.12.0");

((CakeTask)BuildParameters.Tasks.ExportReleaseNotesTask.Task).ErrorHandler = null;
((CakeTask)BuildParameters.Tasks.PublishGitHubReleaseTask.Task).ErrorHandler = null;
BuildParameters.Tasks.PublishPreReleasePackagesTask.IsDependentOn(BuildParameters.Tasks.PublishGitHubReleaseTask);
BuildParameters.Tasks.PublishReleasePackagesTask.IsDependentOn(BuildParameters.Tasks.PublishGitHubReleaseTask);

Build.RunDotNetCore();
