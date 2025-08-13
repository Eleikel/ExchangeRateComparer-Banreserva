using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ExchangeRateComparer.Common.Exceptions
{
    public abstract class ExceptionBase : Exception
    {

        private HttpStatusCode internalServerError;

        public HttpStatusCode ErrorCode { get; }

        protected ExceptionBase(HttpStatusCode errorCode, string message) : base(message)
        {
            ErrorCode = errorCode;
        }

        protected ExceptionBase(HttpStatusCode internalServerError)
        {
            this.internalServerError = internalServerError;
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
