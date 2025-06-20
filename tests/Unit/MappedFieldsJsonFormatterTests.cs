using LogsFormatter.LogsConfiguration;
using Serilog.Events;
using Serilog.Parsing;
using Xunit;

namespace LogsFormatterTests.Unit;

public class MappedFieldsJsonFormatterTests
{
    [Fact(DisplayName = "Should map allowed fields and put extras in the message")]
    public void Formatter_ShouldMapFields_AndPutExtrasInMessage()
    {
        var mappings = new Dictionary<string, string>
        {
            { "RequestId", "request_id" },
            { "TraceId", "trace_id" }
        };
        var formatter = new MappedFieldsJsonFormatter(mappings);

        var logEvent = new LogEvent(
            DateTimeOffset.UtcNow,
            LogEventLevel.Information,
            null,
            new MessageTemplate("Test message", Enumerable.Empty<MessageTemplateToken>()),
            [
                new LogEventProperty("RequestId", new ScalarValue("12345")),
                new LogEventProperty("TraceId", new ScalarValue("abcde")),
                new LogEventProperty("ExtraField", new ScalarValue("extra"))
            ]
        );

        using var output = new StringWriter();
        formatter.Format(logEvent, output);
        var result = output.ToString();

        Assert.Contains("\"request_id\":\"12345\"", result);
        Assert.Contains("\"trace_id\":\"abcde\"", result);
        Assert.DoesNotContain("\"RequestId\"", result);
        Assert.DoesNotContain("\"TraceId\"", result);
        Assert.Contains("ExtraFields: ExtraField=extra", result);
    }

    [Fact(DisplayName = "Should not add extras when all fields are mapped")]
    public void Formatter_ShouldNotAddExtras_WhenAllFieldsMapped()
    {
        var mappings = new Dictionary<string, string>
        {
            { "RequestId", "request_id" },
            { "TraceId", "trace_id" },
            { "ExtraField", "extra_field" }
        };
        var formatter = new MappedFieldsJsonFormatter(mappings);

        var logEvent = new LogEvent(
            DateTimeOffset.UtcNow,
            LogEventLevel.Information,
            null,
            new MessageTemplate("Test message", Enumerable.Empty<MessageTemplateToken>()),
            [
                new LogEventProperty("RequestId", new ScalarValue("12345")),
                new LogEventProperty("TraceId", new ScalarValue("abcde")),
                new LogEventProperty("ExtraField", new ScalarValue("extra"))
            ]
        );

        using var output = new StringWriter();
        formatter.Format(logEvent, output);
        var result = output.ToString();

        Assert.Contains("\"request_id\":\"12345\"", result);
        Assert.Contains("\"trace_id\":\"abcde\"", result);
        Assert.Contains("\"extra_field\":\"extra\"", result);
        Assert.DoesNotContain("ExtraFields:", result);
    }

    [Fact(DisplayName = "Should handle empty mappings and put all as extras")]
    public void Formatter_ShouldHandleEmptyMappings_AllAsExtras()
    {
        var mappings = new Dictionary<string, string>();
        var formatter = new MappedFieldsJsonFormatter(mappings);

        var logEvent = new LogEvent(
            DateTimeOffset.UtcNow,
            LogEventLevel.Information,
            null,
            new MessageTemplate("Test message", Enumerable.Empty<MessageTemplateToken>()),
            [
                new LogEventProperty("RequestId", new ScalarValue("12345")),
                new LogEventProperty("TraceId", new ScalarValue("abcde"))
            ]
        );

        using var output = new StringWriter();
        formatter.Format(logEvent, output);
        var result = output.ToString();

        Assert.DoesNotContain("\"RequestId\"", result);
        Assert.DoesNotContain("\"TraceId\"", result);
        Assert.Contains("ExtraFields: RequestId=12345, TraceId=abcde", result);
    }

    [Fact(DisplayName = "Should include exception in log when present")]
    public void Formatter_ShouldIncludeException_WhenPresent()
    {
        var mappings = new Dictionary<string, string>
        {
            { "RequestId", "request_id" }
        };
        var formatter = new MappedFieldsJsonFormatter(mappings);

        var exception = new InvalidOperationException("Test error");

        var logEvent = new LogEvent(
            DateTimeOffset.UtcNow,
            LogEventLevel.Error,
            exception,
            new MessageTemplate("Test message", Enumerable.Empty<MessageTemplateToken>()),
            [
                new LogEventProperty("RequestId", new ScalarValue("12345"))
            ]
        );

        using var output = new StringWriter();
        formatter.Format(logEvent, output);
        var result = output.ToString();

        Assert.Contains("\"request_id\":\"12345\"", result);
        Assert.Contains("Test error", result);
        Assert.Contains("\"exception\"", result);
    }
}