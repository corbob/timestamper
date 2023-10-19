#load nuget:?package=Chocolatey.Cake.Recipe


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
    shouldStrongNameOutputAssemblies: false,
    shouldObfuscateOutputAssemblies: false,
    shouldAuthenticodeSignOutputAssemblies: false,
    shouldStrongNameSignDependentAssemblies: false,
    shouldRunInspectCode: false,
    treatWarningsAsErrors: true,
    testDirectoryPath: "./test",
    shouldRunDotNetPack: false);

BuildParameters.PrintParameters(Context);

ToolSettings.SetToolSettings(context: Context);

///////////////////////////////////////////////////////////////////////////////
// PROJECT SPECIFIC TASKS
///////////////////////////////////////////////////////////////////////////////

// (None)

///////////////////////////////////////////////////////////////////////////////
// RUN IT!
///////////////////////////////////////////////////////////////////////////////

Build.RunDotNet();
