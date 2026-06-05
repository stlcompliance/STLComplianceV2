FROM node:26-bookworm-slim AS frontend-build
WORKDIR /src
ARG VITE_APP_PUBLIC_BASE_URL=https://app.stlcompliance.com
ARG VITE_SUITE_URL=https://app.stlcompliance.com
ARG VITE_STAFFARR_FRONTEND_BASE=https://staffarr-api-r1kz.onrender.com
ARG VITE_TRAINARR_FRONTEND_BASE=https://trainarr-api-hq62.onrender.com
ARG VITE_MAINTAINARR_FRONTEND_BASE=https://maintainarr-api-uzph.onrender.com
ARG VITE_ROUTARR_FRONTEND_BASE=https://routarr-api-8oc7.onrender.com
ARG VITE_SUPPLYARR_FRONTEND_BASE=https://supplyarr-api-te1h.onrender.com
ARG VITE_SUPPLYARR_API_BASE=https://supplyarr-api-te1h.onrender.com
ARG VITE_COMPLIANCECORE_FRONTEND_BASE=https://compliancecore-api-b6fs.onrender.com
ARG VITE_LOADARR_FRONTEND_BASE=https://loadarr-api.onrender.com
ARG VITE_COMPANION_FRONTEND_BASE=https://fieldcompanion-frontend-vf7o.onrender.com
ENV VITE_APP_PUBLIC_BASE_URL=${VITE_APP_PUBLIC_BASE_URL} \
    VITE_SUITE_URL=${VITE_SUITE_URL} \
    VITE_STAFFARR_FRONTEND_BASE=${VITE_STAFFARR_FRONTEND_BASE} \
    VITE_TRAINARR_FRONTEND_BASE=${VITE_TRAINARR_FRONTEND_BASE} \
    VITE_MAINTAINARR_FRONTEND_BASE=${VITE_MAINTAINARR_FRONTEND_BASE} \
    VITE_ROUTARR_FRONTEND_BASE=${VITE_ROUTARR_FRONTEND_BASE} \
    VITE_SUPPLYARR_FRONTEND_BASE=${VITE_SUPPLYARR_FRONTEND_BASE} \
    VITE_SUPPLYARR_API_BASE=${VITE_SUPPLYARR_API_BASE} \
    VITE_COMPLIANCECORE_FRONTEND_BASE=${VITE_COMPLIANCECORE_FRONTEND_BASE} \
    VITE_LOADARR_FRONTEND_BASE=${VITE_LOADARR_FRONTEND_BASE} \
    VITE_COMPANION_FRONTEND_BASE=${VITE_COMPANION_FRONTEND_BASE}
COPY packages/shared-ui/package.json packages/shared-ui/package-lock.json packages/shared-ui/
RUN npm ci --prefix packages/shared-ui
COPY apps/supplyarr-frontend/package.json apps/supplyarr-frontend/package-lock.json apps/supplyarr-frontend/
RUN npm ci --prefix apps/supplyarr-frontend
COPY packages/shared-ui packages/shared-ui
COPY apps/supplyarr-frontend apps/supplyarr-frontend
RUN npm run build --prefix apps/supplyarr-frontend

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish apps/supplyarr-api/SupplyArr.Api/SupplyArr.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
RUN apt-get update && apt-get install -y --no-install-recommends libgssapi-krb5-2 && rm -rf /var/lib/apt/lists/*
WORKDIR /app
COPY --from=build /app/publish .
COPY --from=frontend-build /src/apps/supplyarr-frontend/dist ./wwwroot
EXPOSE 8080
ENTRYPOINT ["sh", "-c", "ASPNETCORE_URLS=http://+:${PORT:-8080} dotnet SupplyArr.Api.dll"]
