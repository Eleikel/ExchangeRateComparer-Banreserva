using ExchangeRateComparer.Core.Application.Configuration;
using ExchangeRateComparer.Core.Application.Interfaces.Service;
using ExchangeRateComparer.Core.Domain.Entities;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;

namespace ExchangeRateComparer.Core.Application.Providers
{
    public class FreeCurrencyApiService : IExchangeRateProvider
    {
        public string ApiName => "Api #1 ---> Freecurrency Api";
        private readonly HttpClient _client;
        private readonly string _apiKey;
        private readonly string _baseUrl;

        public FreeCurrencyApiService(HttpClient http, IOptions<FreeCurrencyOptions> opts)
        {
            _client = http ?? throw new ArgumentNullException(nameof(http));
            var o = opts?.Value ?? new();
            _apiKey = o.ApiKey ?? "";
            _baseUrl = string.IsNullOrWhiteSpace(o.BaseUrl) ? "https://api.freecurrencyapi.com" : o.BaseUrl.TrimEnd('/');
        }

        public async Task<Offer?> TryGetQuoteAsync(Exchange request, CancellationToken cancellationToken)
        {
            long elapsedTime = Environment.TickCount64;

            try
            {
                var from = (request.From ?? "").Trim().ToUpperInvariant();
                var to = (request.To ?? "").Trim().ToUpperInvariant();
                if (string.IsNullOrWhiteSpace(from) || string.IsNullOrWhiteSpace(to))
                    return null;

                if (from == to)
                {
                    return new Offer
                    {
                        Api = ApiName,
                        Rate = 1m,
                        ElapsedTime = (int)(Environment.TickCount64 - elapsedTime)
                    };
                }

                var baseUri = new Uri($"{_baseUrl}/v1/latest");
                var path = baseUri.GetLeftPart(UriPartial.Path);

                var url = QueryHelpers.AddQueryString(path, new Dictionary<string, string?>
                {
                    ["apikey"] = _apiKey,
                    ["base_currency"] = from,
                    ["currencies"] = to
                });

                using var response = await _client.GetAsync(url, cancellationToken);
                if (!response.IsSuccessStatusCode)
                    return null;

                var payload = await response.Content.ReadFromJsonAsync<ExchangeRateResponse>(cancellationToken: cancellationToken);
                if (payload?.data is null || payload.data.Count == 0)
                    return null;

                if (!payload.data.TryGetValue(to, out var rate))
                    return null;

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

   

    public class ExchangeRateResponse
    {
        public required Dictionary<string, decimal> data { get; init; }
    }

}



