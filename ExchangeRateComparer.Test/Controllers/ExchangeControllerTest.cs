using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ExchangeRateComparer.Core.Application.Interfaces.Service;
using ExchangeRateComparer.Core.Domain.Entities;
using ExchangeRateComparer.WebApi.Controllers;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Xunit;


namespace YourApp.Tests.Controllers
{
    public class ExchangeControllerTests
    {
        private readonly IRateComparisonService _service = A.Fake<IRateComparisonService>();

        private static Exchange CreateRequest() => new Exchange
        {
            From = "USD",
            To = "EUR",
            Value = 100m
        };

        private static CompareResult CreateResult()
        {
            var offers = new[]
            {
                new Offer { Api = "A", Rate = 0.92m, ElapsedTime = 120 },
                new Offer { Api = "B", Rate = 0.91m, ElapsedTime = 145 },
                new Offer { Api = "C", Rate = 0.90m, ElapsedTime = 160 },
            }.ToList();

            var best = offers.OrderByDescending(o => o.Rate).First();
            return new CompareResult(best, offers);
        }

        [Fact]
        public async Task Post_ReturnsOk_WithCompareResultPayload()
        {
            // Arrange
            var req = CreateRequest();
            var expected = CreateResult();
            var token = new CancellationTokenSource().Token;

            A.CallTo(() => _service.CompareAsync(req, token)).Returns(expected);

            var controller = new ExchangeController(_service);

            // Act
            var action = await controller.Post(req, token);

            // Assert
            action.Result.Should().BeOfType<OkObjectResult>();
            var ok = (OkObjectResult)action.Result!;
            ok.StatusCode.Should().Be(200);
            ok.Value.Should().NotBeNull();

            var payload = ExtractCompareResult(ok.Value!);
            payload.Should().NotBeNull();
            payload!.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public async Task Post_CallsService_WithSameRequestAndToken()
        {
            // Arrange
            var req = CreateRequest();
            var res = CreateResult();
            using var cts = new CancellationTokenSource();
            var token = cts.Token;

            A.CallTo(() => _service.CompareAsync(A<Exchange>._, A<CancellationToken>._)).Returns(res);

            var controller = new ExchangeController(_service);

            // Act
            await controller.Post(req, token);

            // Assert
            A.CallTo(() => _service.CompareAsync(
                    A<Exchange>.That.Matches(x => ReferenceEquals(x, req)),
                    A<CancellationToken>.That.Matches(t => t.Equals(token))))
             .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task Post_Propagates_Exception()
        {
            // Arrange
            var req = CreateRequest();
            var controller = new ExchangeController(_service);

            A.CallTo(() => _service.CompareAsync(A<Exchange>._, A<CancellationToken>._))
                .ThrowsAsync(new Exception("eplota"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => controller.Post(req, CancellationToken.None));
        }

        [Fact]
        public async Task Post_Propagates_OperationCanceledException()
        {
            // Arrange
            var req = CreateRequest();
            var controller = new ExchangeController(_service);

            A.CallTo(() => _service.CompareAsync(A<Exchange>._, A<CancellationToken>._))
                .ThrowsAsync(new OperationCanceledException());

            // Act & Assert
            await Assert.ThrowsAsync<OperationCanceledException>(() => controller.Post(req, CancellationToken.None));
        }

        private static CompareResult? ExtractCompareResult(object wrapper)
        {
            if (wrapper is CompareResult cr)
                return cr;

            var type = wrapper.GetType();

            var prop = type.GetProperties()
                           .FirstOrDefault(p => p.PropertyType == typeof(CompareResult));
            if (prop != null)
                return prop.GetValue(wrapper) as CompareResult;

            var candidates = new[] { "Data", "Result", "Value", "Payload", "Content" };
            foreach (var name in candidates)
            {
                var p = type.GetProperty(name);
                if (p?.PropertyType == typeof(CompareResult))
                    return p.GetValue(wrapper) as CompareResult;
            }

            return null;
        }
    }
}
