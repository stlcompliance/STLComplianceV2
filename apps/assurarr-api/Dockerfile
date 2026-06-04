FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY STLCompliance.slnx ./
COPY packages/shared-dotnet/STLCompliance.Shared/STLCompliance.Shared.csproj packages/shared-dotnet/STLCompliance.Shared/
COPY apps/assurarr-api/AssurArr.Api/AssurArr.Api.csproj apps/assurarr-api/AssurArr.Api/

RUN dotnet restore apps/assurarr-api/AssurArr.Api/AssurArr.Api.csproj

COPY . .
RUN dotnet publish apps/assurarr-api/AssurArr.Api/AssurArr.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "AssurArr.Api.dll"]
