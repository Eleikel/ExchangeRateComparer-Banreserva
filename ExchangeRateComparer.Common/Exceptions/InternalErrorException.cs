using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ExchangeRateComparer.Common.Exceptions
{
    public class InternalErrorException : ExceptionBase
    {
        public InternalErrorException() : base(HttpStatusCode.InternalServerError)
        {

        }
        public InternalErrorException(string message) : base(HttpStatusCode.InternalServerError, message)
        {
        }
    }
}
