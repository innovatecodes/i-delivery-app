FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
WORKDIR /src
COPY ["apps/server/src/IDelivery.Api/IDelivery.Api.csproj", "apps/server/src/IDelivery.Api/"]
COPY ["apps/server/src/IDelivery.Application/IDelivery.Application.csproj", "apps/server/src/IDelivery.Application/"]
COPY ["apps/server/src/IDelivery.Domain/IDelivery.Domain.csproj", "apps/server/src/IDelivery.Domain/"]
COPY ["apps/server/src/IDelivery.Infrastructure/IDelivery.Infrastructure.csproj", "apps/server/src/IDelivery.Infrastructure/"]
RUN dotnet restore "apps/server/src/IDelivery.Api/IDelivery.Api.csproj"

COPY . .
WORKDIR "/src/apps/server/src/IDelivery.Api"
RUN dotnet build "IDelivery.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "IDelivery.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "IDelivery.Api.dll"]