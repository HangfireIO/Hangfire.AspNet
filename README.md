# Hangfire.AspNet

[![GitHub release](https://img.shields.io/github/release/HangfireIO/Hangfire.AspNet.svg)](https://github.com/HangfireIO/Hangfire.AspNet/releases)
[![Build status](https://ci.appveyor.com/api/projects/status/ywkl7xx1022odi7m?svg=true)](https://ci.appveyor.com/project/hangfireio/hangfire-aspnet)

This package provides recommended way to install Hangfire to ASP.NET applications hosted in IIS with later transition to always-running mode in mind. It contains classes and methods that use `IRegisteredObject` and `IProcessHostPreloadClient` interfaces to plug in to the IIS and ASP.NET application lifecycle more tightly than regular OWIN methods available in the `Hangfire.Core` package. 

The package also includes a Powershell script to enable Always Running mode for your application that is based on Service Autostart Providers.

The package aims to replace the documentation article [Making ASP.NET application always running](https://docs.hangfire.io/en/latest/deployment-to-production/making-aspnet-app-always-running.html).

## Installation

This project is available as a NuGet Package:

```powershell
Install-Package Hangfire.AspNet
```

## Usage

The package simplifies Hangfire configuration when an application has multiple startup paths, e.g. when using the autostart providers feature to make a web application always running as [described here](https://docs.hangfire.io/en/latest/deployment-to-production/making-aspnet-app-always-running.html).

We define a configuration method, and point Hangfire.AspNet to it from each startup point. Hangfire.AspNet will ensure it is called only once, so we have Hangfire initialized if any startup point is triggered.

```csharp
public class Startup
{
    public static IEnumerable<IDisposable> GetHangfireConfiguration()
    {
        // Calling configuration as a first step
        GlobalConfiguration.Configuration
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UseSqlServerStorage("connection_string");

        // And then creating background job servers, either one or multiple
        yield return new BackgroundJobServer();
    }

    public void Configuration(IAppBuilder app)
    {
        // Initializing from a regular web application startup, including
        // the developer workflow.
        app.UseHangfireAspNet(GetHangfireConfiguration);
        app.UseHangfireDashboard();
    }
}
```

```csharp
public class ApplicationPreload : IProcessHostPreloadClient
{
    public void Preload(string[] parameters)
    {
        // Pointing the same configuration from the Startup class,
        // and Hangfire.AspNet will ensure that it is called only
        // once.
        // This method will be triggered by autostart providers
        // feature as described in the article referenced above.
        HangfireAspNet.Use(Startup.GetHangfireConfiguration);
    }
}
```