using System;
using Serilog.Core;
using Serilog.Events;

namespace Hangfire.AspNet.Sample
{
    internal class AppDomainIdEnricher : ILogEventEnricher
    {
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(
                "AppDomainId", AppDomain.CurrentDomain.Id));
        }
    }
}