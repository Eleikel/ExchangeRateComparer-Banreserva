using ExchangeRateComparer.Core.Application.Interfaces.Service;
using ExchangeRateComparer.Core.Domain.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Options;
using System.Xml;
using System.Text.Json;
using ExchangeRateComparer.Core.Application.Configuration;
namespace ExchangeRateComparer.Core.Application.Providers
{
    public sealed class CurrencyFreakApiService : IExchangeRateProvider
    {
        private readonly HttpClient _http;
        private readonly string _apiKey;
        private readonly string _baseUrl;
        public string ApiName => "Api #2 ---> CurrencyFreak Api";

        public CurrencyFreakApiService(HttpClient http, IOptions<CurrencyFreaksOptions> opts)
        {
            _http = http ?? throw new ArgumentNullException(nameof(http));
            var o = opts?.Value ?? throw new ArgumentNullException(nameof(opts));
            _apiKey = o.ApiKey ?? throw new ArgumentNullException(nameof(o.ApiKey));
            _baseUrl = string.IsNullOrWhiteSpace(o.BaseUrl) ? "https://api.currencyfreaks.com" : o.BaseUrl.TrimEnd('/');
        }

        public async Task<Offer?> TryGetQuoteAsync(Exchange request, CancellationToken ct)
        {
            var t0 = Environment.TickCount64;

            var from = (request.From ?? "").Trim().ToUpperInvariant();
            var to = (request.To ?? "").Trim().ToUpperInvariant();
            if (from.Length == 0 || to.Length == 0) return null;

            var url = $"{_baseUrl}/v2.0/rates/latest?apikey={_apiKey}";

            try
            {
                using var resp = await _http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, ct);
                if (!resp.IsSuccessStatusCode) return null;

                await using var stream = await resp.Content.ReadAsStreamAsync(ct);
                var contentType = resp.Content.Headers.ContentType?.MediaType?.ToLowerInvariant();

                var rates = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
                string? baseCode = null;

                if (contentType is not null && contentType.Contains("xml"))
                {
                    // Parseo a xml
                    using var xr = XmlReader.Create(stream, new XmlReaderSettings { Async = true, IgnoreWhitespace = true });
                    while (await xr.ReadAsync())
                    {
                        if (xr.NodeType != XmlNodeType.Element) continue;

                        if (xr.Name.Equals("base", StringComparison.OrdinalIgnoreCase))
                        {
                            baseCode = (await xr.ReadElementContentAsStringAsync())?.Trim().ToUpperInvariant();
                            continue;
                        }

                        if (xr.Name.Equals("rates", StringComparison.OrdinalIgnoreCase))
                        {
                            while (await xr.ReadAsync() && !(xr.NodeType == XmlNodeType.EndElement && xr.Name.Equals("rates", StringComparison.OrdinalIgnoreCase)))
                            {
                                if (xr.NodeType == XmlNodeType.Element)
                                {
                                    var code = xr.Name.Trim().ToUpperInvariant();
                                    var text = await xr.ReadElementContentAsStringAsync();
                                    if (code.Length > 0 && decimal.TryParse(text?.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var val))
                                        rates[code] = val;
                                }
                            }
                        }
                    }
                }
                else
                {
                   // Aqui paseo a json
                    using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: ct);
                    var root = doc.RootElement;

                    if (root.TryGetProperty("base", out var baseProp))
                        baseCode = baseProp.GetString()?.Trim().ToUpperInvariant();

                    if (root.TryGetProperty("rates", out var ratesProp) && ratesProp.ValueKind == JsonValueKind.Object)
                    {
                        foreach (var kv in ratesProp.EnumerateObject())
                        {
                            var code = kv.Name.ToUpperInvariant();
                            decimal val = 0m;

                            if (kv.Value.ValueKind == JsonValueKind.String)
                            {
                                var s = kv.Value.GetString();
                                if (decimal.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out var d)) val = d;
                            }
                            else if (kv.Value.ValueKind is JsonValueKind.Number && kv.Value.TryGetDecimal(out var d2))
                            {
                                val = d2;
                            }

                            if (val != 0m || code == baseCode) 
                                rates[code] = val;
                        }
                    }
                }

                if (!string.IsNullOrWhiteSpace(baseCode) && !rates.ContainsKey(baseCode!))
                    rates[baseCode!] = 1m;

                if (!rates.TryGetValue(from, out var rFrom)) return null;
                if (!rates.TryGetValue(to, out var rTo)) return null;

                var quote = rTo / rFrom;
                var converted = request.Value * quote;

                return new Offer
                {
                    Api = ApiName,
                    Rate = decimal.Round(converted, 5, MidpointRounding.AwayFromZero),
                    ElapsedTime = (int)(Environment.TickCount64 - t0)
                };
            }
            catch
            {
                return null;
            }
        }
    }

}
