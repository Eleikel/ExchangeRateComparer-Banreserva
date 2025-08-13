using Swashbuckle.AspNetCore.SwaggerUI;

namespace ExchangeRateComparer.WebApi.Extensions
{
    public static class AppExtensions
    {
        public static void UseSwaggerExtensions(this IApplicationBuilder app)
        {
            app.UseSwagger();
            app.UseSwaggerUI(opt =>
            {
                opt.SwaggerEndpoint("/swagger/v1/swagger.json", "Exchange Rate Offers");
                opt.DefaultModelRendering(ModelRendering.Model);
            });
        }
    }
}
