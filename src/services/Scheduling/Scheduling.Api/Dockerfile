FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base

WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build

WORKDIR /src

COPY ["services/Scheduling/Scheduling.Api/Scheduling.Api.csproj", "services/Scheduling/Scheduling.Api/"]
COPY ["nuget.config", "services/Subscription/Subscription.Api/"]

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