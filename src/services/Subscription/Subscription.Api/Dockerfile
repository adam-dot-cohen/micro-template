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
COPY ["services/Subscription/Subscription.Api/Subscription.Api.csproj", "services/Subscription/Subscription.Api/"]
COPY ["services/Subscription/Subscription.Domain/Subscription.Domain.csproj", "services/Subscription/Subscription.Domain/"]
COPY ["services/Subscription/Subscription.Core/Subscription.Core.csproj", "services/Subscription/Subscription.Core/"]
COPY ["services/Subscription/Subscription.Infrastructure/Subscription.Infrastructure.csproj", "services/Subscription/Subscription.Infrastructure/"]
COPY ["nuget.config", "."]

RUN dotnet restore "services/Subscription/Subscription.Api/Subscription.Api.csproj"
COPY . .
WORKDIR "/src/services/Subscription/Subscription.Api"

RUN dotnet build "Subscription.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Subscription.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Laso.Subscription.Api.dll"]