using ExchangeRateComparer.Core.Application;
using ExchangeRateComparer.Core.Application.Services;
using ExchangeRateComparer.WebApi.Extensions;
using System.Globalization;
using System.Text;
using ExchangeRateComparer.Core.Application.Providers;
using Microsoft.Extensions.Options;
using ExchangeRateComparer.Core.Application.Interfaces.Service;
using ExchangeRateComparer.Common.Extensions;
using ExchangeRateComparer.Core.Application.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.Configure<DataFixerApiOptions>(
    builder.Configuration.GetSection("Apis:Api3"));

builder.Services.AddHttpClient<DataFixerApiService>((sp, http) =>
{
    var opts = sp.GetRequiredService<IOptions<DataFixerApiOptions>>().Value;

    http.BaseAddress = new Uri(opts.BaseUrl.TrimEnd('/'));
});

builder.Services.AddTransient<IExchangeRateProvider, DataFixerApiService>();



builder.Services.Configure<CurrencyFreaksOptions>(
    builder.Configuration.GetSection("Apis:Api2"));

builder.Services.AddHttpClient<CurrencyFreakApiService>((sp, http) =>
{
    var opts = sp.GetRequiredService<IOptions<CurrencyFreaksOptions>>().Value;

    http.BaseAddress = new Uri(opts.BaseUrl.TrimEnd('/'));
});

builder.Services.AddTransient<IExchangeRateProvider, CurrencyFreakApiService>();


builder.Services.Configure<FreeCurrencyOptions>(
    builder.Configuration.GetSection("Apis:Api1"));

builder.Services.AddHttpClient<FreeCurrencyApiService>((sp, http) =>
{
    var opts = sp.GetRequiredService<IOptions<FreeCurrencyOptions>>().Value;

    http.BaseAddress = new Uri(opts.BaseUrl.TrimEnd('/'));
});

builder.Services.AddTransient<IExchangeRateProvider, FreeCurrencyApiService>();

builder.Services.AddScoped<IRateComparisonService, RateComparisonService>();
builder.Services.AddSwaggerExtensions();
builder.Services.AddApiVersioningExtension();

var app = builder.Build();


app.ConfigureCustomExceptionMiddleware();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseSwaggerExtensions();
app.MapControllers();

app.Run();
