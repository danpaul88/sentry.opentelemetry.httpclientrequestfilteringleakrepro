using System;
using System.Net.Http;

namespace Sentry.OpenTelemetry.HttpClientRequestFilteringLeakRepro;

/// <summary>
///  Static class to encapsulate decision making for whether to filter out the telemetry for a HTTP request and track
///  statistics about how many requests have been filtered vs collected.
/// </summary>
internal static class HttpRequestFiltering
{
	private static readonly object s_countLock = new();
	private static int s_filteredCount;
	private static int s_collectedCount;

	private static readonly Random s_filterRandom = new();

	public static bool ShouldCollectTelemetry(HttpRequestMessage random)
	{
		// Note: Changing `shouldCollect` to always be true results in no memory leak
		bool shouldCollect = s_filterRandom.Next(0, 2) == 1;
		lock (s_countLock)
		{
			if (shouldCollect)
				s_collectedCount++;
			else
				s_filteredCount++;
		}

		Console.WriteLine($"Telemetry for request to {random.RequestUri} was {(shouldCollect ? "collected" : "filtered")}. Total filtered: {s_filteredCount}, total collected: {s_collectedCount}");
		return shouldCollect;
	}
}
