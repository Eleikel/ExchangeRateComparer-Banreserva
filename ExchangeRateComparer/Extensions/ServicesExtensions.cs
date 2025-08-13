using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using System.Reflection;

namespace ExchangeRateComparer.WebApi.Extensions
{
    public static class ServicesExtensions
    {
        public static void AddSwaggerExtensions(this IServiceCollection services)
        {
            services.AddSwaggerGen(opt =>
            {
                //List<string> xmlFiles = Directory.GetFiles(AppContext.BaseDirectory, "*.xml", SearchOption.TopDirectoryOnly).ToList();
                //xmlFiles.ForEach(xmlFiles => opt.IncludeXmlComments(xmlFiles));

                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                opt.IncludeXmlComments(xmlPath);

                opt.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "Prueba Tecnica BANRESERVAS",
                    Description = "API ASP.NET CORE 8",
                    Contact = new OpenApiContact
                    {
                        Name = "Esleiker Diaz",
                        Email = "victoresleikerdiazsantana@gmail.com",
                        Url = new Uri("https://www.linkedin.com/in/esleiker-diaz-34a636237/")
                    }
                });

                opt.EnableAnnotations();

                opt.DescribeAllParametersInCamelCase();              
            });
        }
        public static void AddApiVersioningExtension(this IServiceCollection services)
        {
            services.AddApiVersioning(config =>
            {
                config.DefaultApiVersion = new ApiVersion(1, 0);
                config.AssumeDefaultVersionWhenUnspecified = true;
                config.ReportApiVersions = true;
            });
        }
    }
}

