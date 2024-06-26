Include "packages\Hangfire.Build.0.4.3\tools\psake-common.ps1"

Task Default -Depends Pack

Task Test -Depends Compile -Description "Run unit and integration tests." {
    Exec { dotnet test --no-build -c release "tests\Hangfire.AspNet.Tests" }
}

Task Collect -Depends Test -Description "Copy all artifacts to the build folder." {
    Collect-Assembly "Hangfire.AspNet" "net45"
    Collect-File "README.md"
    Collect-File "LICENSE"
}

Task Pack -Depends Collect -Description "Create NuGet packages and archive files." {
    $version = Get-PackageVersion
    
    Create-Package "Hangfire.AspNet" $version
    Create-Archive "Hangfire.AspNet-$version"
}

Task Sign -Depends Pack -Description "Sign artifacts." {
    $version = Get-PackageVersion
    Sign-ArchiveContents "Hangfire.AspNet-$version" "hangfire"
}