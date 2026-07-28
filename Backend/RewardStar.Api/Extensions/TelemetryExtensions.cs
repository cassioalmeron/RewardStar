using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace RewardStar.Api.Extensions;

public static class TelemetryExtensions
{
    private const string SERVICE_NAME = "rewardstar-api";

    public static WebApplicationBuilder AddApiTelemetry(this WebApplicationBuilder builder)
    {
        var databaseProvider = string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DATABASE_URL"))
            ? "sqlite"
            : "postgresql";

        var otel = builder.Services.AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .AddService(SERVICE_NAME, serviceVersion: typeof(TelemetryExtensions).Assembly.GetName().Version?.ToString())
                .AddAttributes([
                    new KeyValuePair<string, object>("deployment.environment", builder.Environment.EnvironmentName),
                    new KeyValuePair<string, object>("db.provider", databaseProvider)
                ]))
            .WithTracing(tracing => tracing
                .AddAspNetCoreInstrumentation(options => options.Filter = IsInteresting)
                .AddHttpClientInstrumentation()
                .AddEntityFrameworkCoreInstrumentation())
            .WithMetrics(metrics => metrics
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation());

        // Without a configured endpoint there is no exporter: avoids the endless OTLP retry
        // when the dashboard is not running.
        if (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT")))
            otel.UseOtlpExporter();

        return builder;
    }

    // /swagger and the health probe (polled every 30s by Docker) pollute the trace without informing anything.
    private static bool IsInteresting(HttpContext context) =>
        context.Request.Path.StartsWithSegments("/api")
        && !context.Request.Path.StartsWithSegments("/api/healthcheck");
}
