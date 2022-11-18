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
COPY ["services/Scheduling/Scheduling.Api/Scheduling.Api.csproj", "services/Scheduling/Scheduling.Api/"]
COPY ["services/Scheduling/Scheduling.Domain/Scheduling.Domain.csproj", "services/Scheduling/Scheduling.Domain/"]
COPY ["services/Scheduling/Scheduling.Core/Scheduling.Core.csproj", "services/Scheduling/Scheduling.Core/"]
COPY ["services/Scheduling/Scheduling.Infrastructure/Scheduling.Infrastructure.csproj", "services/Scheduling/Scheduling.Infrastructure/"]
COPY ["nuget.config", "."]

RUN dotnet restore "services/Scheduling/Scheduling.Api/Scheduling.Api.csproj"
COPY . .
WORKDIR "/src/services/Scheduling/Scheduling.Api"

RUN dotnet build "Scheduling.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Scheduling.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Laso.Scheduling.Api.dll"]