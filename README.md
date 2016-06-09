# Hangfire.IIS

This package provides recommended way to install Hangfire to applications hosted in IIS with later transition to always-running mode in mind. It contains classes and methods that use `IRegisteredObject` and `IProcessHostPreloadClient` interfaces to plug in to the IIS and ASP.NET application lifecycle more tightly than regular OWIN methods available in the `Hangfire.Core` package. 

This package also includes a Powershell script to enable Always Running mode for your application that is based on Service Autostart Providers.

This package aims to replace the documentation article *Making ASP.NET application always running*.

