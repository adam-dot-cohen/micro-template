FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base

WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build

WORKDIR /src

COPY ["services/Subscription/Subscription.Api/Subscription.Api.csproj", "services/Subscription/Subscription.Api/"]
COPY ["nuget.config", "services/Subscription/Subscription.Api/"]

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