Framework 4.5.1
Properties {
    $solution = "Hangfire.AspNet.sln"
}

Include "packages\Hangfire.Build.0.2.5\tools\psake-common.ps1"

Task Default -Depends Collect

Task Collect -Depends Compile -Description "Copy all artifacts to the build folder." {
    Collect-Assembly "Hangfire.AspNet" "net45"
}

Task Pack -Depends Collect -Description "Create NuGet packages and archive files." {
    $version = Get-PackageVersion
    
    Create-Archive "Hangfire.AspNet-$version"
    Create-Package "Hangfire.AspNet" $version
}
