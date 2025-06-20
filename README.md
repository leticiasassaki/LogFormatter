# LogsFormatter

Sample ASP.NET Core project demonstrating structured log formatting with **Serilog** and **OpenTelemetry**.

---

## 🚀 Technologies Used

- **C# ASP.NET Core (.NET 8)**
- **Serilog** – Structured logging with enrichment and custom JSON formatting
- **OpenTelemetry** – Instrumentation and trace exporting
- **Swagger (Swashbuckle)** – Interactive API documentation for testing endpoints

---

## ✅ Features

- Exposes a simple **`/products`** endpoint that returns a list of random products
- Logs are output in **structured JSON format** to the console
- Includes controlled fields like:  
  ✅ `RequestId`  
  ✅ `TraceId`  
  ✅ `SpanId`
- **Custom console formatters**:
  - `ControlledFieldsJsonFormatter`: Filters which fields appear in the JSON log, pushing any unexpected fields into the message string
  - `SnakeCaseJsonFormatter`: Converts all field names in the JSON log to **snake_case**
- **OpenTelemetry Tracing** for distributed tracing (OTLP exporter ready)
- Automatic HTTP request instrumentation with trace correlation
- Interactive API documentation available via **Swagger UI**

---

## ▶️ How to Run Locally

### Prerequisites:

- .NET 8 SDK or higher installed

### Steps:

1. **Clone the repository:**

```bash
git clone <repository-url>
cd LogFormatter
````

2. **Restore dependencies:**

```bash
dotnet restore
```

3. **Run the application:**

```bash
dotnet run --project src/LogsFormatter.csproj
```

---

## 🌐 Accessing the API

* **Swagger UI:**
  [http://localhost:5276/swagger](http://localhost:5276/swagger)

* **Sample Endpoint:**
  `GET /products`

---

## 🗂️ Project Structure Highlights

| File/Folder                                          | Purpose                                                                           |
| ---------------------------------------------------- | --------------------------------------------------------------------------------- |
| `Program.cs`                                         | App startup, logging configuration, OpenTelemetry setup, and endpoint definitions |
| `LogsConfiguration/LogEnricher.cs`                   | Custom log enricher (for TraceId, SpanId, etc)                                    |
| `LogsConfiguration/ControlledFieldsJsonFormatter.cs` | Custom Serilog JSON formatter that filters specific fields                        |
| `LogsConfiguration/SnakeCaseJsonFormatter.cs`        | Custom Serilog JSON formatter that converts all property names to **snake\_case** |

---

## 🐍 Using SnakeCaseJsonFormatter

If you want your logs to output all field names in **snake\_case**, use the `SnakeCaseJsonFormatter` as your Serilog console sink formatter.

### Configuration Example in `Program.cs`:

```csharp
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builtConfig)
    .WriteTo.Console(new SnakeCaseJsonFormatter())
    .CreateLogger();
```

### Sample Output:

```json
{
  "@timestamp": "2025-06-19T15:00:00.123Z",
  "level": "Information",
  "message": "HTTP GET /products responded 200 in 12ms",
  "fields": {
    "request_id": "xyz",
    "trace_id": "abc",
    "span_id": "123"
  }
}
```

---

## 🎛️ Using ControlledFieldsJsonFormatter

If you want **full control over which fields appear in your structured logs**, use the `ControlledFieldsJsonFormatter`.

### What it does:

* Includes **only the fields you explicitly list** (like `TraceId`, `SpanId`, `RequestId`, etc.)
* **Any extra fields (not in your allowed list)** are automatically **moved into the log message**, appended as a string.

### Configuration Example in `Program.cs`:

```csharp
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builtConfig)
    .WriteTo.Console(new ControlledFieldsJsonFormatter(new[]
    {
        "TraceId",
        "SpanId",
        "RequestId"
    }))
    .CreateLogger();
```

### Example Output (when extra fields exist):

```json
{
  "@timestamp": "2025-06-19T15:10:00.123Z",
  "level": "Information",
  "message": "HTTP GET /products responded 200 in 18ms | ExtraFields: MachineName=PC123, Environment=Development",
  "fields": {
    "TraceId": "abc123",
    "SpanId": "def456",
    "RequestId": "req789"
  }
}
```

### Benefits:

* Keeps logs clean and focused
* Ensures that unexpected fields **don’t break downstream processing**
* Preserves all information

---

## 📝 Notes

* Logs are emitted in **JSON format**, making them easily parsable by observability tools like **ElasticSearch**, **Seq**, or **OpenTelemetry Collectors**.
* The project is **pre-configured for OTLP trace export**, ready to integrate with systems like **Jaeger**, **Zipkin**, or **Grafana Tempo**.
* Two customizable formatters are available:
  * ✅ **ControlledFieldsJsonFormatter** → Filters specific fields
  * ✅ **SnakeCaseJsonFormatter** → Converts all field names to **snake\_case**
