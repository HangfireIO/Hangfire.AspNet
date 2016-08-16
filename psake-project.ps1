Framework 4.5.1
Properties {
    $solution = "Hangfire.AspNet.sln"
    $xunit2 = "packages\xunit.runner.console.*\tools\xunit.console.exe"
}

Include "packages\Hangfire.Build.0.2.5\tools\psake-common.ps1"

Task Default -Depends Collect

Task Test -Depends Compile -Description "Run unit and integration tests." {
    Run-Xunit2Tests "Hangfire.AspNet.Tests"
}

Task Collect -Depends Test -Description "Copy all artifacts to the build folder." {
    Collect-Assembly "Hangfire.AspNet" "net45"
}

Task Pack -Depends Collect -Description "Create NuGet packages and archive files." {
    $version = Get-PackageVersion
    
    Create-Archive "Hangfire.AspNet-$version"
    Create-Package "Hangfire.AspNet" $version
}

function Run-Xunit2Tests($project, $target) {
    Write-Host "Running xUnit2 test runner for '$project'..." -ForegroundColor "Green"

    $assembly = (Get-TestsOutputDir $project $target) + "\$project.dll"
    $xunit2 = Resolve-Path $xunit2

    Exec { .$xunit2 $assembly -verbose }
}
