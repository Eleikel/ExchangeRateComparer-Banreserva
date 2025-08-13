using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExchangeRateComparer.Core.Application.Configuration
{
    public class CurrencyFreaksOptions
    {
        public required string BaseUrl { get; init; }
        public required string ApiKey { get; init; }
    }
}
