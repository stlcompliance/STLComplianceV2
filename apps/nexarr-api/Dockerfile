FROM node:22-bookworm-slim AS frontend-build
WORKDIR /src
ARG VITE_APP_PUBLIC_BASE_URL=https://app.stlcompliance.com
ENV VITE_APP_PUBLIC_BASE_URL=${VITE_APP_PUBLIC_BASE_URL}
COPY packages/shared-ui/package.json packages/shared-ui/package-lock.json packages/shared-ui/
RUN npm ci --prefix packages/shared-ui
COPY apps/suite-frontend/package.json apps/suite-frontend/package-lock.json apps/suite-frontend/
RUN npm ci --prefix apps/suite-frontend
COPY packages/shared-ui packages/shared-ui
COPY apps/suite-frontend apps/suite-frontend
RUN npm run build --prefix apps/suite-frontend

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish apps/nexarr-api/NexArr.Api/NexArr.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
RUN apt-get update && apt-get install -y --no-install-recommends libgssapi-krb5-2 && rm -rf /var/lib/apt/lists/*
WORKDIR /app
COPY --from=build /app/publish .
COPY --from=frontend-build /src/apps/suite-frontend/dist ./wwwroot
EXPOSE 8080
ENTRYPOINT ["sh", "-c", "ASPNETCORE_URLS=http://+:${PORT:-8080} dotnet NexArr.Api.dll"]