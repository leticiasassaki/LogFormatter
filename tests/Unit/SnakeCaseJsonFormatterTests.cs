using LogsFormatter.LogsConfiguration;
using Serilog.Events;
using Serilog.Parsing;

namespace LogsFormatterTests.Unit;

public class SnakeCaseJsonFormatterTests
{
    [Fact(DisplayName = "Should include exception when present")]
    public void Formatter_ShouldIncludeException_WhenPresent()
    {
        var formatter = new SnakeCaseJsonFormatter();
        var exception = new InvalidOperationException("Test error");
        var logEvent = new LogEvent(
            DateTimeOffset.UtcNow,
            LogEventLevel.Error,
            exception,
            new MessageTemplate("Error occurred", Enumerable.Empty<MessageTemplateToken>()),
            []
        );

        using var output = new StringWriter();
        formatter.Format(logEvent, output);
        var result = output.ToString();

        Assert.Contains("\"exception\"", result);
        Assert.Contains("Test error", result);
    }

    [Fact(DisplayName = "Should serialize complex property values")]
    public void Formatter_ShouldSerializeComplexPropertyValues()
    {
        var formatter = new SnakeCaseJsonFormatter();
        var structure = new StructureValue(new[]
        {
            new LogEventProperty("InnerValue", new ScalarValue(42))
        });
        var logEvent = new LogEvent(
            DateTimeOffset.UtcNow,
            LogEventLevel.Information,
            null,
            new MessageTemplate("Complex value", Enumerable.Empty<MessageTemplateToken>()),
            [
                new LogEventProperty("ComplexProperty", structure)
            ]
        );

        using var output = new StringWriter();
        formatter.Format(logEvent, output);
        var result = output.ToString();

        Assert.Contains("\"complex_property\"", result);
        Assert.Contains("\"inner_value\":42", result);
    }
}

internal class FaultyLogEventPropertyValue : LogEventPropertyValue
{
    public override void Render(TextWriter output, string? format = null, IFormatProvider? formatProvider = null)
    {
        throw new NotImplementedException();
    }

    public override string ToString() => throw new Exception("Serialization error");
}