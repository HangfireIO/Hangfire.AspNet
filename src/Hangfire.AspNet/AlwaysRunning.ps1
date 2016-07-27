# This script is used to enable always-running mode for your
# web applications that run under IIS, based on serviceAutoStartProviders
# feature. Don't use it if you have Application Initialization Module
# installed, as it uses another strategy for make applications always running.
# Turning the always-running feature automatically disables idle and

# You can see that it works by opening Operational event log.
# http://developers.de/blogs/damir_dobric/archive/2011/10/19/when-is-the-wcf-service-auto-started.aspx

# After running this script, ensure that your application is starting fine,
# you can see the errors in the Event Viewer under the Applications section.

# IIS 7.0 is required
# IIS 7.5 is required for startMode
# The <serviceAutoStartProviders> element was added in IIS 7.5.

### Configuration

# Application preload assembly-qualified type name.
# MyAutostartProvider, MyAutostartProvider, version=1.0.0.0, Culture=neutral, PublicKeyToken=426f62526f636b73
# This type should implement the `IProcessHostPreloadClient` interface.
$ApplicationPreloadType = "WebApplication1.ApplicationPreload, WebApplication1"

# Website name in IIS.
$WebSiteName = "websitename"

### Script

Add-PSSnapin WebAdministration -ErrorAction SilentlyContinue
Import-Module WebAdministration -ErrorAction SilentlyContinue

$ApplicationPreloadProvider = "ApplicationPreload"
$WebSiteFullName = "IIS:\Sites\" + $WebSiteName
$ApplicationPool = Get-Item $WebSiteFullName | Select-Object applicationPool
$ApplicationPoolFullName = "IIS:\AppPools\" + $ApplicationPool.applicationPool

# Disable IdleTime-out property for application pool
Set-ItemProperty $ApplicationPoolFullName -Name processModel.idleTimeout -Value ([TimeSpan]::FromMinutes(0))

# Indicates to the World Wide Web Publishing Service (W3SVC) that 
# the application pool should be automatically started when it is 
# created or when IIS is started.
# Required to start a worker process.
Set-ItemProperty $ApplicationPoolFullName -Name autoStart -Value True

# Specifies that the Windows Process Activation Service (WAS) will 
# always start the application pool. This behavior allows an application 
# to load the operating environment before any serving any HTTP requests, 
# which reduces the start-up processing for initial HTTP requests for 
# the application.
# Requried for serviceAutoStartProviders
Set-ItemProperty $ApplicationPoolFullName -Name startMode -Value 1 # 1 = AlwaysRunning, 0 = OnDemand

# Specifies a collection of managed assemblies that will be loaded when the AlwaysRunning is specifed for an applocation pool's startMode.
Set-WebConfiguration -Filter '/system.applicationHost/serviceAutoStartProviders' -Value (@{name=$ApplicationPreloadProvider;type=$ApplicationPreloadType})
Set-ItemProperty $WebSiteFullName -Name applicationDefaults.serviceAutoStartEnabled -Value True
Set-ItemProperty $WebSiteFullName -Name applicationDefaults.serviceAutoStartProvider -Value $ApplicationPreloadProvider
