using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Serilog.Events;
using Serilog.Formatting;

namespace LogsFormatter.LogsConfiguration;

public class SnakeCaseJsonFormatter : ITextFormatter
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = new SnakeCaseNamingPolicy(),
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public void Format(LogEvent logEvent, TextWriter output)
    {
        var logObject = new
        {
            @timestamp = logEvent.Timestamp.UtcDateTime.ToString("O", CultureInfo.InvariantCulture),
            level = logEvent.Level.ToString(),
            message = logEvent.RenderMessage(),
            fields = logEvent.Properties.ToDictionary(
                p => ToSnakeCase(p.Key),
                p => SimplifyPropertyValue(p.Value)
            ),
            exception = logEvent.Exception?.ToString()
        };

        try
        {
            var json = JsonSerializer.Serialize(logObject, JsonOptions);
            output.WriteLine(json);
        }
        catch (Exception ex)
        {
            output.WriteLine($"{{\"error\": \"Failed to serialize log event: {ex.Message}\"}}");
        }
    }

    private static string ToSnakeCase(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;
        var sb = new System.Text.StringBuilder(input.Length + 5);
        for (var i = 0; i < input.Length; i++)
        {
            var c = input[i];
            if (char.IsUpper(c))
            {
                if (i > 0) sb.Append('_');
                sb.Append(char.ToLowerInvariant(c));
            }
            else
            {
                sb.Append(c);
            }
        }
        return sb.ToString();
    }

    private static object? SimplifyPropertyValue(LogEventPropertyValue value)
    {
        return value switch
        {
            ScalarValue scalar => scalar.Value,
            SequenceValue sequence => sequence.Elements.Select(SimplifyPropertyValue).ToList(),
            StructureValue structure => structure.Properties.ToDictionary(
                p => ToSnakeCase(p.Name), p => SimplifyPropertyValue(p.Value)),
            DictionaryValue dict => dict.Elements.ToDictionary(
                kvp => kvp.Key.Value?.ToString() ?? "", kvp => SimplifyPropertyValue(kvp.Value)),
            _ => value.ToString()
        };
    }
}

public class SnakeCaseNamingPolicy : JsonNamingPolicy
{
    public override string ConvertName(string name)
    {
        if (string.IsNullOrEmpty(name)) return name;
        var sb = new System.Text.StringBuilder(name.Length + 5);
        for (var i = 0; i < name.Length; i++)
        {
            var c = name[i];
            if (char.IsUpper(c))
            {
                if (i > 0) sb.Append('_');
                sb.Append(char.ToLowerInvariant(c));
            }
            else
            {
                sb.Append(c);
            }
        }
        return sb.ToString();
    }
}