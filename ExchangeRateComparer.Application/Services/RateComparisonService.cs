using ExchangeRateComparer.Common.Exceptions;
using ExchangeRateComparer.Core.Application.Interfaces.Service;
using ExchangeRateComparer.Core.Domain.Entities;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Xml.Linq;
using Exchange = ExchangeRateComparer.Core.Domain.Entities.Exchange;
namespace ExchangeRateComparer.Core.Application.Services
{
    public sealed class RateComparisonService : IRateComparisonService
    {
        private readonly IReadOnlyList<IExchangeRateProvider> _exchangeRateProviders;
        private readonly ILogger<RateComparisonService> _logger;

        public RateComparisonService(
            IEnumerable<IExchangeRateProvider> exchangeRateProviders,
             ILogger<RateComparisonService> logger
           )
        {
            _exchangeRateProviders = exchangeRateProviders?.ToList() ?? throw new ArgumentNullException(nameof(exchangeRateProviders));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<CompareResult> CompareAsync(Exchange request, CancellationToken ct)
        {
            if (request is null) throw new ArgumentNullException(nameof(request));
            var timeout = TimeSpan.FromSeconds(10);
            var cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cancellationToken.CancelAfter(timeout);
            var token = cancellationToken.Token;

            _logger.LogInformation(
           "Comparando {From}->{To} por {Amount}. Proveedores: {Count}. Timeout: {Timeout}s",
           request.From, request.To, request.Value, _exchangeRateProviders.Count, timeout.TotalSeconds);

            var providers = new Dictionary<Task<Offer?>, IExchangeRateProvider>(_exchangeRateProviders.Count);
            foreach (var provider in _exchangeRateProviders)
            {
                var name = provider.GetType().Name;
                _logger.LogDebug("Consultando proveedor {Provider}", name);

                var t = provider.TryGetQuoteAsync(request, token);
                providers.Add(t, provider);
            }

            var offers = new List<Offer>(_exchangeRateProviders.Count);

            while (providers.Count > 0)
            {
                var completed = await Task.WhenAny(providers.Keys).ConfigureAwait(false);
                var provider = providers[completed];
                var name = provider.GetType().Name;
                providers.Remove(completed);

                try
                {
                    var offer = await completed.ConfigureAwait(false);
                    if (offer is not null)
                    {
                        offers.Add(offer);
                        _logger.LogInformation(
                       "Oferta recibida de {Provider}: Rate={Rate} Latency={Latency}ms",
                       offer.Api ?? name, offer.Rate, offer.ElapsedTime);
                    }
                    else
                    {
                        _logger.LogWarning("Proveedor {Provider} no devolvió oferta.", name);
                    }
                }                
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error consultando proveedor {Provider}.", name);
                    throw new InternalErrorException($"Error Interno en el servidor en el Servicio: {ex.Message}");

                }
            }

            if (offers.Count == 0)
            {
                _logger.LogWarning("No hubo ofertas dentro del tiempo estimado ({Timeout}s).", timeout.TotalSeconds);
                throw new GatewayTimeoutException("No hubo ofertas encontradas durante el tiempo de espera estimado");


            }

            var bestOffer = offers
                .OrderByDescending(o => o.Rate)
                .First();

            _logger.LogInformation(
            "Mejor oferta: {Rate} de {Provider}. Total de ofertas: {Count}",
            bestOffer.Rate, bestOffer.Api, offers.Count);

            return new CompareResult(bestOffer, offers);
        }
    }


}
