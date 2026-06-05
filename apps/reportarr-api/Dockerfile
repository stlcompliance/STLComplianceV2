FROM node:22-bookworm-slim AS frontend-build
WORKDIR /src
ARG VITE_APP_PUBLIC_BASE_URL=https://app.stlcompliance.com
ARG VITE_ROUTER_BASENAME=/reportarr
ARG VITE_SUITE_URL=https://app.stlcompliance.com
ARG VITE_REPORTARR_FRONTEND_BASE=https://app.stlcompliance.com/reportarr
ARG VITE_REPORTARR_API_BASE=https://reportarr-api.onrender.com
ENV VITE_APP_PUBLIC_BASE_URL=${VITE_APP_PUBLIC_BASE_URL} \
    VITE_ROUTER_BASENAME=${VITE_ROUTER_BASENAME} \
    VITE_SUITE_URL=${VITE_SUITE_URL} \
    VITE_REPORTARR_FRONTEND_BASE=${VITE_REPORTARR_FRONTEND_BASE} \
    VITE_REPORTARR_API_BASE=${VITE_REPORTARR_API_BASE}
COPY packages/shared-ui/package.json packages/shared-ui/package-lock.json packages/shared-ui/
RUN npm ci --prefix packages/shared-ui
COPY apps/reportarr-frontend/package.json apps/reportarr-frontend/package-lock.json apps/reportarr-frontend/
RUN npm ci --prefix apps/reportarr-frontend
COPY packages/shared-ui packages/shared-ui
COPY apps/reportarr-frontend apps/reportarr-frontend
RUN npm run build --prefix apps/reportarr-frontend

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish apps/reportarr-api/ReportArr.Api/ReportArr.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
RUN apt-get update && apt-get install -y --no-install-recommends libgssapi-krb5-2 && rm -rf /var/lib/apt/lists/*
WORKDIR /app
COPY --from=build /app/publish .
COPY --from=frontend-build /src/apps/reportarr-frontend/dist ./wwwroot
EXPOSE 8080
ENTRYPOINT ["sh", "-c", "ASPNETCORE_URLS=http://+:${PORT:-8080} dotnet ReportArr.Api.dll"]
