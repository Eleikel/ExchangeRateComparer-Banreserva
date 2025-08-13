using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExchangeRateComparer.Core.Domain.Entities
{
    public class Offer
    {
        public string? Api { get; set; }
        public decimal Rate { get; set; }
        public long ElapsedTime { get; set; }

       
    }

    public record CompareResult(Offer BestOffer, List<Offer> AllOffers);


}
