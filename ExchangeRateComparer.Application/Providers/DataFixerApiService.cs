using ExchangeRateComparer.Core.Application.Interfaces.Service;
using ExchangeRateComparer.Core.Domain.Entities;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using System.Text.Json.Serialization;
using System.Net.Http.Json;
using ExchangeRateComparer.Core.Application.Configuration;

namespace ExchangeRateComparer.Core.Application.Providers
{
    public sealed class DataFixerApiService : IExchangeRateProvider
    {
        public string ApiName => "Api #3 -> Data Fixer Api";
        private readonly HttpClient _http;
        private readonly string _apiKey;
        private readonly string _baseUrl;

        public DataFixerApiService(HttpClient http, IOptions<DataFixerApiOptions> opts)
        {
            _http = http;
            var o = opts?.Value ?? new();
            _apiKey = o.ApiKey ?? "";
            _baseUrl = string.IsNullOrWhiteSpace(o.BaseUrl) ? "https://data.fixer.io/api" : o.BaseUrl.TrimEnd('/');
        }

        public async Task<Offer?> TryGetQuoteAsync(Exchange request, CancellationToken ct)
        {
            long elapsedTime = Environment.TickCount64;

            try
            {
                var from = (request.From ?? "").Trim().ToUpperInvariant();
                var toRaw = (request.To ?? "").Trim().ToUpperInvariant();
                if (from.Length != 3 || string.IsNullOrWhiteSpace(toRaw)) return null;

                var targets = toRaw.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                                   .Select(s => s.ToUpperInvariant()).Distinct().ToList();
                if (targets.Count == 0) return null;

                var symbols = string.Join(',', new[] { from }.Concat(targets));
                var url = QueryHelpers.AddQueryString($"{_baseUrl}/latest", new Dictionary<string, string?>
                {
                    ["access_key"] = _apiKey,
                    ["symbols"] = symbols
                });

                using var res = await _http.GetAsync(url, ct);
                if (!res.IsSuccessStatusCode) return null;

                var payload = await res.Content.ReadFromJsonAsync<FixerLatest>(cancellationToken: ct);
                var rates = (payload?.success == true) ? payload.rates : null;
                if (rates is null || rates.Count == 0) return null;

                var to = targets[0];

                bool hasFrom = rates.TryGetValue(from, out var rFrom);
                bool hasTo = rates.TryGetValue(to, out var rTo);

                decimal rate =
                    from == "EUR" && hasTo ? rTo :
                    to == "EUR" && hasFrom && rFrom != 0 ? 1m / rFrom :
                    hasFrom && hasTo && rFrom != 0 ? rTo / rFrom :
                    0m;

                if (rate <= 0) return null;

                var converted = request.Value * rate;

                return new Offer
                {
                    Api = ApiName,
                    Rate = decimal.Round(converted, 5, MidpointRounding.AwayFromZero),
                    ElapsedTime = (int)(Environment.TickCount64 - elapsedTime)
                };
            }
            catch (Exception ex)
            {
                return null;
            }
        }


    }

   

    public sealed class FixerLatest
    {
        public bool success { get; init; }
        public Dictionary<string, decimal>? rates { get; init; }
        [JsonPropertyName("base")] public string? Base { get; init; }
    }

}
