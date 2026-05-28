# Runtime, Frameworks, and Packages

## Backend Standard

- .NET 10
- ASP.NET Core 10
- Entity Framework Core 10
- C# 14 where supported by the .NET 10 SDK
- Npgsql
- PostgreSQL
- FluentValidation
- Serilog
- OpenAPI / Swagger
- JWT bearer validation
- Health checks
- Redis client
- Testcontainers
- xUnit or NUnit

## Frontend Standard

- React
- TypeScript
- Vite
- React Router
- TanStack Query or RTK Query
- Tailwind CSS
- shadcn/ui-style component system
- lucide-react
- lucide only for non-React icon metadata/utilities
- Zod
- React Hook Form
- Vitest
- Playwright

## Docker API Pattern

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "Product.Api.dll"]
```

## Docker Worker Pattern

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Product.Worker.dll"]
```

## Governance

- Same backend runtime across all APIs and workers.
- Same frontend stack across web apps.
- Shared packages can contain infrastructure contracts, generated clients, UI primitives, design tokens, and helpers.
- Shared packages cannot own product authority or product database models.
