using Serilog.Events;
using Serilog.Formatting;
using System.Globalization;
using System.Text.Json;

namespace LogsFormatter.LogsConfiguration;

public class ControlledFieldsJsonFormatter(IEnumerable<string> allowedFields) : ITextFormatter
{
    private readonly HashSet<string> _allowedFields = allowedFields.ToHashSet(StringComparer.OrdinalIgnoreCase);

    public void Format(LogEvent logEvent, TextWriter output)
    {
        var includedFields = new Dictionary<string, object?>();
        var extraFields = new Dictionary<string, object?>();

        foreach (var prop in logEvent.Properties)
        {
            if (_allowedFields.Contains(prop.Key))
            {
                includedFields[prop.Key] = SimplifyPropertyValue(prop.Value);
            }
            else
            {
                extraFields[prop.Key] = SimplifyPropertyValue(prop.Value);
            }
        }

        var logObject = new Dictionary<string, object?>
        {
            ["@timestamp"] = logEvent.Timestamp.UtcDateTime.ToString("O", CultureInfo.InvariantCulture),
            ["level"] = logEvent.Level.ToString(),
            ["message"] = EnrichMessageWithExtras(logEvent.RenderMessage(), extraFields),
            ["fields"] = includedFields
        };

        if (logEvent.Exception != null)
        {
            logObject["exception"] = logEvent.Exception.ToString();
        }

        var json = JsonSerializer.Serialize(logObject);
        output.WriteLine(json);
    }

    private static object? SimplifyPropertyValue(LogEventPropertyValue value)
    {
        return value switch
        {
            ScalarValue scalar => scalar.Value,
            SequenceValue sequence => sequence.Elements.Select(SimplifyPropertyValue).ToList(),
            StructureValue structure => structure.Properties.ToDictionary(
                p => p.Name,
                p => SimplifyPropertyValue(p.Value)
            ),
            DictionaryValue dict => dict.Elements.ToDictionary(
                kvp => kvp.Key.Value?.ToString() ?? "",
                kvp => SimplifyPropertyValue(kvp.Value)
            ),
            _ => value.ToString()
        };
    }

    private static string EnrichMessageWithExtras(string originalMessage, Dictionary<string, object?> extras)
    {
        if (extras.Count == 0) return originalMessage;

        var extrasAsText = string.Join(", ", extras.Select(kvp => $"{kvp.Key}={kvp.Value}"));
        return $"{originalMessage} | ExtraFields: {extrasAsText}";
    }
}