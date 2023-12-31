#load nuget:?package=Chocolatey.Cake.Recipe&version=0.25.0


///////////////////////////////////////////////////////////////////////////////
// ADDINS
///////////////////////////////////////////////////////////////////////////////

// (None)

///////////////////////////////////////////////////////////////////////////////
// RECIPE SETUP
///////////////////////////////////////////////////////////////////////////////

Environment.SetVariableNames();


BuildParameters.SetParameters(
    context: Context,
    buildSystem: BuildSystem,
    sourceDirectoryPath: "./src",
    solutionFilePath: "./src/TimeStamper.sln",
    title: "TimeStamper",
    repositoryOwner: "corbob",
    repositoryName: "timestamper",
    productName: "TimeStamper",
    productDescription: "",
    productCopyright: string.Format("Copyright © 2023 - {0} Cory Knox.", DateTime.Now.Year),
    productCompany: "",
    productTrademark: "",
    shouldStrongNameOutputAssemblies: false,
    shouldObfuscateOutputAssemblies: false,
    shouldAuthenticodeSignOutputAssemblies: false,
    shouldStrongNameSignDependentAssemblies: false,
    shouldRunInspectCode: false,
    treatWarningsAsErrors: true,
    testDirectoryPath: "./test",
    shouldRunDotNetPack: true);

BuildParameters.PrintParameters(Context);

ToolSettings.SetToolSettings(context: Context);

///////////////////////////////////////////////////////////////////////////////
// PROJECT SPECIFIC TASKS
///////////////////////////////////////////////////////////////////////////////

Task("Prepare-Chocolatey-Packages")
    .IsDependeeOf("Create-Chocolatey-Packages")
    .WithCriteria(() => BuildParameters.BuildAgentOperatingSystem == PlatformFamily.Windows, "Skipping because not running on Windows")
    .WithCriteria(() => BuildParameters.ShouldRunChocolatey, "Skipping because execution of Chocolatey has been disabled")
    .Does(() =>
{
    // Copy legal documents
    CopyFile(BuildParameters.RootDirectoryPath + "/LICENSE", BuildParameters.Paths.Directories.ChocolateyNuspecDirectory + "/tools/LICENSE.txt");

    // Copy built executables
    var filesToCopy = GetFiles(BuildParameters.Paths.Directories.PublishedApplications + "/TimeStamper/**/*.*");
    var verificationFile = GetFiles(BuildParameters.Paths.Directories.ChocolateyNuspecDirectory + "/tools/VERIFICATION.txt").FirstOrDefault();

    foreach (var file in filesToCopy)
    {
        var fileName = file.Segments[file.Segments.Length - 1];
        var checksumLine = fileName + ": " + CalculateFileHash(file, HashAlgorithm.SHA256).ToHex();
        Information(checksumLine);
        FileAppendText(verificationFile, $"{checksumLine}\r\n");
    }
});

///////////////////////////////////////////////////////////////////////////////
// RUN IT!
///////////////////////////////////////////////////////////////////////////////

Build.RunDotNet();
