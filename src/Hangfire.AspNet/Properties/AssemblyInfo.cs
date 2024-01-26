using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[assembly: AssemblyTitle("Hangfire.AspNet")]
[assembly: AssemblyDescription("ASP.NET Integration for Hangfire.")]

// Allow the generation of mocks for internal types
[assembly: InternalsVisibleTo("Hangfire.AspNet.Tests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
