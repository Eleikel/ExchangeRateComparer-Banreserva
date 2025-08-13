using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExchangeRateComparer.Core.Application.Configuration
{
    public class DataFixerApiOptions
    {
        public string BaseUrl { get; init; }
        public string ApiKey { get; init; }
    }
}
