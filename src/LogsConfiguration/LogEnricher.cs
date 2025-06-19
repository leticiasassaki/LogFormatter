using System.Diagnostics;
using Serilog.Core;
using Serilog.Events;

namespace LogsFormatter.LogsConfiguration;

public class LogEnricher : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var activity = Activity.Current;
        if (activity == null)
            return;
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(
            "TraceId", activity.TraceId.ToString()));
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(
            "SpanId", activity.SpanId.ToString()));
    }
}