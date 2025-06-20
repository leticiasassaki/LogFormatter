using Serilog.Events;
using Serilog.Formatting;
using System.Globalization;
using System.Text.Json;

namespace LogsFormatter.LogsConfiguration;

public class MappedFieldsJsonFormatter(Dictionary<string, string> fieldMappings) : ITextFormatter
{
    public void Format(LogEvent logEvent, TextWriter output)
    {
        var filteredFields = new Dictionary<string, object?>();
        var extraFields = new Dictionary<string, object?>();

        foreach (var property in logEvent.Properties)
        {
            if (fieldMappings.TryGetValue(property.Key, out var mappedName))
            {
                filteredFields[mappedName] = SimplifyPropertyValue(property.Value);
            }
            else
            {
                extraFields[property.Key] = SimplifyPropertyValue(property.Value);
            }
        }

        var logObject = new Dictionary<string, object?>
        {
            ["@timestamp"] = logEvent.Timestamp.UtcDateTime.ToString("O", CultureInfo.InvariantCulture),
            ["level"] = logEvent.Level.ToString(),
            ["message"] = AppendExtraFieldsToMessage(logEvent.RenderMessage(), extraFields),
            ["fields"] = filteredFields
        };

        if (logEvent.Exception != null)
        {
            logObject["exception"] = logEvent.Exception.ToString();
        }

        output.WriteLine(JsonSerializer.Serialize(logObject));
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

    private static string AppendExtraFieldsToMessage(string originalMessage, Dictionary<string, object?> extras)
    {
        if (extras.Count == 0) return originalMessage;

        var extraText = string.Join(", ", extras.Select(kvp => $"{kvp.Key}={kvp.Value}"));
        return $"{originalMessage} | ExtraFields: {extraText}";
    }
}
