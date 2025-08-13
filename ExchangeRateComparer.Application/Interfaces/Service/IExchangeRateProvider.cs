using ExchangeRateComparer.Core.Application.Services;
using ExchangeRateComparer.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExchangeRateComparer.Core.Application.Interfaces.Service
{
    public interface IExchangeRateProvider
    {
        Task<Offer?> TryGetQuoteAsync(Exchange request, CancellationToken cancellationToken);
    }
}
