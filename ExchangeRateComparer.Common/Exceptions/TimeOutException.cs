using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ExchangeRateComparer.Common.Exceptions
{
    public class TimeOutException : ExceptionBase
    {
        public TimeOutException() : base(HttpStatusCode.RequestTimeout)
        {

        }
        public TimeOutException(string message) : base(HttpStatusCode.RequestTimeout, message)
        {
        }
    }
}
