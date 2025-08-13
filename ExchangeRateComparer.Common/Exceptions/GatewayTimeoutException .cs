using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ExchangeRateComparer.Common.Exceptions
{
    public class GatewayTimeoutException : ExceptionBase
    {
        public GatewayTimeoutException() : base(HttpStatusCode.GatewayTimeout)
        {

        }
        public GatewayTimeoutException(string message) : base(HttpStatusCode.GatewayTimeout, message)
        {
        }
    }
}
