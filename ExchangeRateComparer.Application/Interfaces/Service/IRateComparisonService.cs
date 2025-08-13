using ExchangeRateComparer.Core.Domain.Entities;
using System;
using System.Text;

namespace ExchangeRateComparer.Core.Application.Interfaces.Service
{
    public interface IRateComparisonService
    {
        Task<CompareResult> CompareAsync(Exchange request, CancellationToken ct);
    }
}
