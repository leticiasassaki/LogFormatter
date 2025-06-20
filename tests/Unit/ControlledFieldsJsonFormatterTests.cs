using LogsFormatter.LogsConfiguration;
    using Serilog.Events;
    using Serilog.Parsing;

    namespace LogsFormatterTests.Unit;
    
    public class ControlledFieldsJsonFormatterTests
    {
        [Fact(DisplayName = "Should include only allowed fields and put extras in the message")]
        public void Formatter_ShouldIncludeOnlyAllowedFields()
        {
            // Arrange
            var allowedFields = new[] { "RequestId", "TraceId" };
            var formatter = new ControlledFieldsJsonFormatter(allowedFields);
    
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
    
            // Act
            formatter.Format(logEvent, output);
            var result = output.ToString();
    
            // Assert
            Assert.Contains("\"RequestId\":\"12345\"", result);
            Assert.Contains("\"TraceId\":\"abcde\"", result);
            Assert.Contains("ExtraFields: ExtraField=extra", result);
        }
    
        [Fact(DisplayName = "Should not include extra fields when all are allowed")]
        public void Formatter_ShouldNotIncludeExtraFields_WhenAllAreAllowed()
        {
            var allowedFields = new[] { "RequestId", "TraceId", "ExtraField" };
            var formatter = new ControlledFieldsJsonFormatter(allowedFields);
    
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
    
            Assert.Contains("\"RequestId\":\"12345\"", result);
            Assert.Contains("\"TraceId\":\"abcde\"", result);
            Assert.Contains("\"ExtraField\":\"extra\"", result);
            Assert.DoesNotContain("ExtraFields:", result);
        }
    
        [Fact(DisplayName = "Should handle empty allowed fields and put all as extras")]
        public void Formatter_ShouldHandleEmptyAllowedFields_AllAsExtras()
        {
            var allowedFields = Array.Empty<string>();
            var formatter = new ControlledFieldsJsonFormatter(allowedFields);
    
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
            var allowedFields = new[] { "RequestId" };
            var formatter = new ControlledFieldsJsonFormatter(allowedFields);
    
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
    
            Assert.Contains("\"RequestId\":\"12345\"", result);
            Assert.Contains("Test error", result);
            Assert.Contains("\"exception\"", result);
        }
    }