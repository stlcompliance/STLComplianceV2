FROM node:22-bookworm-slim AS frontend-build
WORKDIR /src
ARG VITE_APP_PUBLIC_BASE_URL=https://app.stlcompliance.com
ENV VITE_APP_PUBLIC_BASE_URL=${VITE_APP_PUBLIC_BASE_URL} \
    VITE_SUITE_URL=${VITE_APP_PUBLIC_BASE_URL} \
    VITE_STAFFARR_API_BASE=https://staffarr-api-6ki6.onrender.com \
    VITE_TRAINARR_API_BASE=https://trainarr-api-6eww.onrender.com \
    VITE_MAINTAINARR_API_BASE=https://maintainarr-api-mgel.onrender.com \
    VITE_ROUTARR_API_BASE=https://routarr-api-ua7l.onrender.com \
    VITE_SUPPLYARR_API_BASE=https://supplyarr-api-ytp0.onrender.com \
    VITE_COMPLIANCECORE_API_BASE=https://compliancecore-api-pqyl.onrender.com \
    VITE_LOADARR_API_BASE=https://loadarr-api.onrender.com \
    VITE_ASSURARR_API_BASE=https://assurarr-api.onrender.com \
    VITE_REPORTARR_API_BASE=https://reportarr-api.onrender.com \
    VITE_RECORDARR_API_BASE=https://recordarr-api.onrender.com
COPY . .
RUN npm ci --prefix packages/shared-ui
RUN npm ci --prefix apps/suite-frontend \
    && npm ci --prefix apps/staffarr-frontend \
    && npm ci --prefix apps/trainarr-frontend \
    && npm ci --prefix apps/maintainarr-frontend \
    && npm ci --prefix apps/routarr-frontend \
    && npm ci --prefix apps/supplyarr-frontend \
    && npm ci --prefix apps/compliancecore-frontend \
    && npm ci --prefix apps/loadarr-frontend \
    && npm ci --prefix apps/assurarr-frontend \
    && npm ci --prefix apps/reportarr-frontend \
    && npm ci --prefix apps/recordarr-frontend
RUN npm run build --prefix apps/suite-frontend \
    && VITE_ROUTER_BASENAME=/staffarr npm run build --prefix apps/staffarr-frontend \
    && VITE_ROUTER_BASENAME=/trainarr npm run build --prefix apps/trainarr-frontend \
    && VITE_ROUTER_BASENAME=/maintainarr npm run build --prefix apps/maintainarr-frontend \
    && VITE_ROUTER_BASENAME=/routarr npm run build --prefix apps/routarr-frontend \
    && VITE_ROUTER_BASENAME=/supplyarr npm run build --prefix apps/supplyarr-frontend \
    && VITE_ROUTER_BASENAME=/compliancecore npm run build --prefix apps/compliancecore-frontend \
    && VITE_ROUTER_BASENAME=/loadarr npm run build --prefix apps/loadarr-frontend \
    && VITE_ROUTER_BASENAME=/assurarr npm run build --prefix apps/assurarr-frontend \
    && VITE_ROUTER_BASENAME=/reportarr npm run build --prefix apps/reportarr-frontend \
    && VITE_ROUTER_BASENAME=/recordarr npm run build --prefix apps/recordarr-frontend

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish apps/nexarr-api/NexArr.Api/NexArr.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
RUN apt-get update && apt-get install -y --no-install-recommends libgssapi-krb5-2 && rm -rf /var/lib/apt/lists/*
WORKDIR /app
COPY --from=build /app/publish .
COPY --from=frontend-build /src/apps/suite-frontend/dist ./wwwroot
COPY --from=frontend-build /src/apps/staffarr-frontend/dist ./wwwroot/staffarr
COPY --from=frontend-build /src/apps/trainarr-frontend/dist ./wwwroot/trainarr
COPY --from=frontend-build /src/apps/maintainarr-frontend/dist ./wwwroot/maintainarr
COPY --from=frontend-build /src/apps/routarr-frontend/dist ./wwwroot/routarr
COPY --from=frontend-build /src/apps/supplyarr-frontend/dist ./wwwroot/supplyarr
COPY --from=frontend-build /src/apps/compliancecore-frontend/dist ./wwwroot/compliancecore
COPY --from=frontend-build /src/apps/loadarr-frontend/dist ./wwwroot/loadarr
COPY --from=frontend-build /src/apps/assurarr-frontend/dist ./wwwroot/assurarr
COPY --from=frontend-build /src/apps/reportarr-frontend/dist ./wwwroot/reportarr
COPY --from=frontend-build /src/apps/recordarr-frontend/dist ./wwwroot/recordarr
EXPOSE 8080
ENTRYPOINT ["sh", "-c", "ASPNETCORE_URLS=http://+:${PORT:-8080} dotnet NexArr.Api.dll"]
