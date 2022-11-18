FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base

WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build

WORKDIR /src

COPY ["components/Hosting/Hosting.csproj", "components/Hosting/"]
COPY ["components/Mediation/Mediation/Mediation.csproj", "components/Mediation/Mediation/"]
COPY ["components/IO/IO.Serialization.Newtonsoft/IO.Serialization.Newtonsoft.csproj", "components/IO.Serialization.Newtonsoft/"]
COPY ["components/IntegrationEvents/IntegrationEvents.AzureServiceBus/IntegrationEvents.AzureServiceBus.csproj", "components/IntegrationEvents/IntegrationEvents.AzureServiceBus/"]
COPY ["services/Catalog/Catalog.Api/Catalog.Api.csproj", "services/Catalog/Catalog.Api/"]
COPY ["services/Catalog/Catalog.Domain/Catalog.Domain.csproj", "services/Catalog/Catalog.Domain/"]
COPY ["services/Catalog/Catalog.Core/Catalog.Core.csproj", "services/Catalog/Catalog.Core/"]
COPY ["services/Catalog/Catalog.Infrastructure/Catalog.Infrastructure.csproj", "services/Catalog/Catalog.Infrastructure/"]
COPY ["nuget.config", "."]

RUN dotnet restore "services/Catalog/Catalog.Api/Catalog.Api.csproj"
COPY . .
WORKDIR "/src/services/Catalog/Catalog.Api"

RUN dotnet build "Catalog.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Catalog.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Laso.Catalog.Api.dll"]