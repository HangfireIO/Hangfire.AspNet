using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[assembly: AssemblyTitle("Hangfire.AspNet")]
[assembly: AssemblyDescription("ASP.NET Integration for Hangfire.")]
[assembly: Guid("dc6f0777-e99d-49dc-8a7c-84b087c84738")]

// Allow the generation of mocks for internal types
[assembly: InternalsVisibleTo("Hangfire.AspNet.Tests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
