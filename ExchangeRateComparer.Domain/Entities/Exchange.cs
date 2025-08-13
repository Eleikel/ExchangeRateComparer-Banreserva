using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExchangeRateComparer.Core.Domain.Entities
{
    public sealed class Exchange
    {
        public string From { get; set; }
        public string To { get; set; }
        public decimal Value { get; set; }
    }
}
