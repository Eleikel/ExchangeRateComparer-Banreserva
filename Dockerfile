
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia el .sln y los .csproj para aprovechar la caché en el restore
COPY ExchangeRateComparer.sln ./
COPY ExchangeRateComparer/ExchangeRateComparer.WebApi.csproj ExchangeRateComparer/
COPY ExchangeRateComparer.Application/ExchangeRateComparer.Core.Application.csproj ExchangeRateComparer.Application/
COPY ExchangeRateComparer.Common/ExchangeRateComparer.Common.csproj ExchangeRateComparer.Common/
COPY ExchangeRateComparer.Domain/ExchangeRateComparer.Core.Domain.csproj ExchangeRateComparer.Domain/

# Restaura solo con lo necesario
RUN dotnet restore "ExchangeRateComparer/ExchangeRateComparer.WebApi.csproj"

# Aqui se copia el resto del código y publica
COPY . .
WORKDIR /src/ExchangeRateComparer
RUN dotnet publish "ExchangeRateComparer.WebApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Escucha en 8080 (puerto típico en contenedores)
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .

# El ensamblado de arranque de la WebApi
ENTRYPOINT ["dotnet", "ExchangeRateComparer.WebApi.dll"]
