FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base

WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build

WORKDIR /src

COPY ["services/Catalog/Catalog.Api/Catalog.Api.csproj", "services/Catalog/Catalog.Api/"]
COPY ["nuget.config", "services/Catalog/Catalog.Api/"]

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