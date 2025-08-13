using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExchangeRateComparer.Common
{
    public enum ApiStatus
    {
        TransactionError = -1,
        TransactionSuccess = 200,
        InternalServerError = 500,
        BadRequest = 400
    }
}
