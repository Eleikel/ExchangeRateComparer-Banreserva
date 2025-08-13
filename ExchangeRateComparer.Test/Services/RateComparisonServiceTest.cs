using FakeItEasy;
using FluentAssertions;
using ExchangeRateComparer.Common.Exceptions;
using ExchangeRateComparer.Core.Application.Interfaces.Service; 
using ExchangeRateComparer.Core.Application.Services;          
using ExchangeRateComparer.Core.Domain.Entities;              

namespace ExchangeRateComparer.Tests.Application.Services
{
    public class RateComparisonServiceTests
    {
        private static Exchange CreateRequest() => new Exchange
        {
            From = "USD",
            To = "EUR",
            Value = 100m
        };

        private static Offer MakeOffer(string provider, decimal rate, long elapsed = 100)
            => new Offer { Api = provider, Rate = rate, ElapsedTime = elapsed };

        private static RateComparisonService CreateService(params IExchangeRateProvider[] providers)
            => new RateComparisonService(providers);

        [Fact]
        public async Task CompareAsync_WhenProvidersReturnOffers_ReturnsBestAndAllNonNull()
        {
            // Arrange
            var req = CreateRequest();

            var p1 = A.Fake<IExchangeRateProvider>();
            var p2 = A.Fake<IExchangeRateProvider>();
            var p3 = A.Fake<IExchangeRateProvider>();

            A.CallTo(() => p1.TryGetQuoteAsync(req, A<CancellationToken>._))
                .Returns(Task.FromResult<Offer?>(MakeOffer("P1", 0.90m, 120)));

            A.CallTo(() => p2.TryGetQuoteAsync(req, A<CancellationToken>._))
                .Returns(Task.FromResult<Offer?>(MakeOffer("P2", 0.95m, 80)));

            A.CallTo(() => p3.TryGetQuoteAsync(req, A<CancellationToken>._))
                .Returns(Task.FromResult<Offer?>(null));

            var sut = CreateService(p1, p2, p3);

            // Act
            var result = await sut.CompareAsync(req, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.AllOffers.Should().HaveCount(2);
            result.AllOffers.Select(o => o.Api).Should().BeEquivalentTo(new[] { "P1", "P2" });

            result.BestOffer.Api.Should().Be("P2");
            result.BestOffer.Rate.Should().Be(0.95m);

            A.CallTo(() => p1.TryGetQuoteAsync(
                A<Exchange>.That.Matches(x => ReferenceEquals(x, req)), A<CancellationToken>._)).MustHaveHappenedOnceExactly();
            A.CallTo(() => p2.TryGetQuoteAsync(
                A<Exchange>.That.Matches(x => ReferenceEquals(x, req)), A<CancellationToken>._)).MustHaveHappenedOnceExactly();
            A.CallTo(() => p3.TryGetQuoteAsync(
                A<Exchange>.That.Matches(x => ReferenceEquals(x, req)), A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        }
        


        [Fact]
        public async Task CompareAsync_WhenProviderTaskFaultsWithGenericException_ThrowsInternalErrorException()
        {
            // Arrange
            var req = CreateRequest();
            var pOk = A.Fake<IExchangeRateProvider>();
            var pBoom = A.Fake<IExchangeRateProvider>();

            A.CallTo(() => pOk.TryGetQuoteAsync(req, A<CancellationToken>._))
                .Returns(Task.FromResult<Offer?>(MakeOffer("OK", 0.90m)));

            A.CallTo(() => pBoom.TryGetQuoteAsync(req, A<CancellationToken>._))
                .ReturnsLazily(() => Task.FromException<Offer?>(new Exception("eplota")));

            var sut = CreateService(pOk, pBoom);

            // Act
            Func<Task> act = async () => await sut.CompareAsync(req, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<InternalErrorException>()
                .WithMessage("*Error Interno*eplota*");
        }

        [Fact]
        public async Task CompareAsync_WhenRequestIsNull_ThrowsArgumentNullException()
        {
            // Arrange
            var p = A.Fake<IExchangeRateProvider>();
            var sut = CreateService(p);

            // Act
            Func<Task> act = async () => await sut.CompareAsync(null!, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<ArgumentNullException>()
                .WithParameterName("request");
        }

        [Fact]
        public async Task CompareAsync_OnlyNonNullOffersAreAggregated_AndBestIsMaxRate()
        {
            // Arrange
            var req = CreateRequest();
            var p1 = A.Fake<IExchangeRateProvider>();
            var p2 = A.Fake<IExchangeRateProvider>();
            var p3 = A.Fake<IExchangeRateProvider>();

            A.CallTo(() => p1.TryGetQuoteAsync(req, A<CancellationToken>._))
                .Returns(Task.FromResult<Offer?>(null));
            A.CallTo(() => p2.TryGetQuoteAsync(req, A<CancellationToken>._))
                .Returns(Task.FromResult<Offer?>(MakeOffer("PX", 0.88m)));
            A.CallTo(() => p3.TryGetQuoteAsync(req, A<CancellationToken>._))
                .Returns(Task.FromResult<Offer?>(MakeOffer("PY", 0.93m)));

            var sut = CreateService(p1, p2, p3);

            // Act
            var result = await sut.CompareAsync(req, CancellationToken.None);

            // Assert
            result.AllOffers.Should().HaveCount(2);
            result.BestOffer.Rate.Should().Be(0.93m);
            result.BestOffer.Api.Should().Be("PY");
        }

        [Fact]
        public async Task CompareAsync_PassesSameRequestInstanceToEachProvider()
        {
            // Arrange
            var req = CreateRequest();

            var p1 = A.Fake<IExchangeRateProvider>();
            var p2 = A.Fake<IExchangeRateProvider>();

            A.CallTo(() => p1.TryGetQuoteAsync(A<Exchange>._, A<CancellationToken>._))
                .Returns(Task.FromResult<Offer?>(MakeOffer("A", 0.9m)));
            A.CallTo(() => p2.TryGetQuoteAsync(A<Exchange>._, A<CancellationToken>._))
                .Returns(Task.FromResult<Offer?>(MakeOffer("B", 0.91m)));

            var sut = CreateService(p1, p2);

            // Act
            _ = await sut.CompareAsync(req, CancellationToken.None);

            // Assert
            A.CallTo(() => p1.TryGetQuoteAsync(
                A<Exchange>.That.Matches(x => ReferenceEquals(x, req)), A<CancellationToken>._)).MustHaveHappenedOnceExactly();
            A.CallTo(() => p2.TryGetQuoteAsync(
                A<Exchange>.That.Matches(x => ReferenceEquals(x, req)), A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task CompareAsync_WhenExternalTokenIsAlreadyCanceled_ProvidersReceiveACanceledToken()
        {
            // Arrange
            var req = CreateRequest();
            var p = A.Fake<IExchangeRateProvider>();

            CancellationToken captured = default;
            A.CallTo(() => p.TryGetQuoteAsync(A<Exchange>._, A<CancellationToken>._))
                .Invokes((Exchange _, CancellationToken t) => captured = t)
                .Returns(Task.FromResult<Offer?>(null));

            var sut = CreateService(p);

            using var cts = new CancellationTokenSource();
            cts.Cancel(); 

            // Act
            Func<Task> act = async () => await sut.CompareAsync(req, cts.Token);

            await act.Should().ThrowAsync<GatewayTimeoutException>();
            captured.IsCancellationRequested.Should().BeTrue();
        }
    }
}
