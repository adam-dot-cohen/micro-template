#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

RUN curl -sL https://deb.nodesource.com/setup_13.x | bash -
RUN apt-get install -y nodejs

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build

ARG buildVersion=1.0.0.0-alpha
ARG buildConfiguration=Release

RUN curl -sL https://deb.nodesource.com/setup_13.x | bash -
RUN apt-get install -y nodejs
WORKDIR /src

COPY ["web/AdminPortal/AdminPortal.Web/AdminPortal.Web.csproj", "web/AdminPortal/AdminPortal.Web/"]
COPY ["nuget.config", "web/AdminPortal/AdminPortal.Web/"]
RUN dotnet restore "web/AdminPortal/AdminPortal.Web/AdminPortal.Web.csproj"
COPY . .
WORKDIR "/src/web/AdminPortal/AdminPortal.Web"
RUN ["sh", "-c", "dotnet build \"AdminPortal.Web.csproj\" -c ${buildConfiguration} -o /app/build /property:Version=\"${buildVersion}\""]

FROM build AS publish
ARG buildVersion=1.0.0.0-alpha
ARG buildConfiguration=Release

RUN ["sh", "-c", "dotnet publish \"AdminPortal.Web.csproj\" -f netcoreapp3.1 -c ${buildConfiguration} -o /app/publish"]

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Laso.AdminPortal.Web.dll"]