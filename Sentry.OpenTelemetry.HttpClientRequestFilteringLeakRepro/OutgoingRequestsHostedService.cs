using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace Sentry.OpenTelemetry.HttpClientRequestFilteringLeakRepro;

public class OutgoingRequestsHostedService(IHttpClientFactory clientFactory) : IHostedService
{
	private CancellationTokenSource? _serviceStoppedCts;

	public Task StartAsync(CancellationToken cancellationToken)
	{
		_serviceStoppedCts = new CancellationTokenSource();
		StartSendingRequests(_serviceStoppedCts.Token);
		return Task.CompletedTask;
	}

	public Task StopAsync(CancellationToken cancellationToken)
	{
		_serviceStoppedCts?.Cancel();
		_serviceStoppedCts?.Dispose();
		_serviceStoppedCts = null;
		return Task.CompletedTask;
	}

	private void StartSendingRequests(CancellationToken cancellationToken)
	{
		Task.Run(async() =>
		{
			while (!cancellationToken.IsCancellationRequested)
			{
				await Task.Delay(TimeSpan.FromMilliseconds(500), cancellationToken).ConfigureAwait(false);
				await SendRequest(cancellationToken).ConfigureAwait(false);
			}
		}, cancellationToken);
	}

	private async Task SendRequest(CancellationToken cancellationToken)
	{
		using HttpClient client = clientFactory.CreateClient();

		try
		{
			await client.GetAsync("http://localhost:5000/", cancellationToken).ConfigureAwait(false);
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Unexpected exception: {ex.Message}");
			return;
		}

		Console.WriteLine("Request Completed");
	}
}
