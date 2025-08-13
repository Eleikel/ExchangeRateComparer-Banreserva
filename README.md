# ExchangeRateComparer.WebApi

![.NET 8](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)
![Docker](https://img.shields.io/badge/Docker-ready-2496ED?logo=docker)
![Status](https://img.shields.io/badge/status-active-success)

API para comparar y obtener **tipos de cambio** desde múltiples proveedores.  
Construida con **ASP.NET Core (.NET 8)** y lista para **Docker**.

---

## 🧭 Tabla de contenidos
- [Descripción del proyecto](#-descripción-del-proyecto)
- [Arquitectura / Capas](#-arquitectura--capas)
- [Build y Run con Docker](#-build-y-run-con-docker)
- [Swagger y Endpoints útiles](#-swagger-y-endpoints-útiles)

---

## 📝 Descripción del proyecto
> Permite consultar 
ExchangeRateComparer permite consultar distintos proveedores de tipos de cambio, comparar cotizaciones y devolver la mejor oferta considerando la disponibilidad.

**Objetivos:**
- Unificar acceso a múltiples APIs de tipos de cambio.
- Exponer endpoints REST simples y documentados con Swagger.
- Facilitar despliegue en contenedores (Docker).

---

## 🧩 Arquitectura / Capas
- **ExchangeRateComparer.WebApi**: capa API (endpoints, Swagger, DI, middlewares).
- **ExchangeRateComparer.Core.Domain**: entidades de dominio y contratos base.
- **ExchangeRateComparer.Core.Application**: casos de uso/servicios (comparación, orquestación).
- **ExchangeRateComparer.Common**: utilidades compartidas, excepciones, helpers.

---

## 🛠 Build y Run con Docker

**Build:**
```xml
docker build -t exchangerc-api:1.0 .
```
**Build:**
```xml
docker run -d -p 8000:8080 exchangerc-api:1.0
```


## 📚 Build y Run con Docker

**Swagger UI:**
```xml
http://localhost:8080/swagger/index.html
```
**Payload:**
```xml
{
  "from": "usd",
  "to": "eur",
  "value": 100
}

```
**Ejemplo:**

<img width="2144" height="1165" alt="image" src="https://github.com/user-attachments/assets/fd080576-7d8d-4b1e-a16b-50266ddc3dfc" />
<img width="1769" height="1106" alt="image" src="https://github.com/user-attachments/assets/baba6116-e6a0-47c8-86a2-92010bbb6a8c" />

