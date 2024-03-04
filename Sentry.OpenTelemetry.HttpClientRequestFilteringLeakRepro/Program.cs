using System.Diagnostics;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Sentry;
using Sentry.OpenTelemetry;
using Sentry.OpenTelemetry.HttpClientRequestFilteringLeakRepro;

const string ServiceName = "my.service";
const string ServiceVersion = "1.0";

Activity.DefaultIdFormat = ActivityIdFormat.W3C;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient();
builder.Services.AddHostedService<OutgoingRequestsHostedService>();

builder.Services
	.AddOpenTelemetry()
		.ConfigureResource(resource => resource.AddService(ServiceName, serviceVersion: ServiceVersion))
		.WithTracing(tracing =>
		{
			tracing.AddHttpClientInstrumentation(options => options.FilterHttpRequestMessage = HttpRequestFiltering.ShouldCollectTelemetry);
			tracing.AddSentry();
		});

builder.WebHost.UseSentry(static o =>
{
	o.Dsn = "http://your.dsn.here";
	o.Release = $"{ServiceName}@{ServiceVersion}";

	o.Debug = true;
	o.DiagnosticLevel = SentryLevel.Debug;

	o.UseOpenTelemetry();
	o.TracesSampleRate = 1.0;
});

// Start app
WebApplication app = builder.Build();
app.MapGet("/", () => "Hello World!");
app.Run();
