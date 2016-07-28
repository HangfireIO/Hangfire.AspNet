# Hangfire.AspNet

[![GitHub release](https://img.shields.io/github/release/HangfireIO/Hangfire.AspNet.svg?maxAge=3600)](https://github.com/HangfireIO/Hangfire.IIS/releases)
[![license](https://img.shields.io/github/license/HangfireIO/Hangfire.AspNet.svg?maxAge=3600)](https://github.com/HangfireIO/Hangfire.IIS/blob/master/LICENSE)
[![Build status](https://ci.appveyor.com/api/projects/status/ywkl7xx1022odi7m?svg=true)](https://ci.appveyor.com/project/odinserj/hangfire-aspnet)

This package provides recommended way to install Hangfire to ASP.NET applications hosted in IIS with later transition to always-running mode in mind. It contains classes and methods that use `IRegisteredObject` and `IProcessHostPreloadClient` interfaces to plug in to the IIS and ASP.NET application lifecycle more tightly than regular OWIN methods available in the `Hangfire.Core` package. 

This package also includes a Powershell script to enable Always Running mode for your application that is based on Service Autostart Providers.

This package aims to replace the documentation article *Making ASP.NET application always running*.

Documentation is pending, please see https://github.com/HangfireIO/Hangfire.Highlighter.
