using LogsFormatter.LogsConfiguration;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Serilog Configuration
builder.Host.UseSerilog((context, services, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.With<LogEnricher>()
        .WriteTo.Console(new ControlledFieldsJsonFormatter([
            "RequestId",
            "TraceId",
            "SpanId"
        ]));
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddOpenTelemetry()
    .ConfigureResource(r => r.AddEnvironmentVariableDetector())
    .WithTracing(tracing =>
    {
        tracing.AddAspNetCoreInstrumentation();
        tracing.AddHttpClientInstrumentation();
        tracing.AddOtlpExporter();
    });

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();
app.UseHttpsRedirection();

var productNames = new[]
{
    "Laptop", "Mouse", "Keyboard", "Monitor", "Printer", "Headset"
};

app.MapGet("/products", () =>
    {
        var products = Enumerable.Range(1, 3).Select(index =>
                new Product
                (
                    index,
                    productNames[Random.Shared.Next(productNames.Length)],
                    Math.Round(Random.Shared.NextDouble() * 1000, 2)
                ))
            .ToArray();
        Log.Information("Retrieved {@ProductCount} products", products.Length);
        
        return products;
    })
    .WithName("GetProducts")
    .WithOpenApi();

app.Run();

internal record Product(int Id, string Name, double Price);