FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
RUN apt-get update \
    && apt-get upgrade -y \
    && rm -rf /var/lib/apt/lists/*
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Directory.Build.props", "./"]
COPY ["Directory.Packages.props", "./"]
COPY ["src/FCG.Payments.Application/FCG.Payments.Application.csproj", "src/FCG.Payments.Application/"]
COPY ["src/FCG.Payments.Domain/FCG.Payments.Domain.csproj", "src/FCG.Payments.Domain/"]
COPY ["src/FCG.Payments.Infrastructure.Auth/FCG.Payments.Infrastructure.Auth.csproj", "src/FCG.Payments.Infrastructure.Auth/"]
COPY ["src/FCG.Payments.Infrastructure.Kafka/FCG.Payments.Infrastructure.Kafka.csproj", "src/FCG.Payments.Infrastructure.Kafka/"]
COPY ["src/FCG.Payments.Infrastructure.SqlServer/FCG.Payments.Infrastructure.SqlServer.csproj", "src/FCG.Payments.Infrastructure.SqlServer/"]
COPY ["src/FCG.Payments.WebApi/FCG.Payments.WebApi.csproj", "src/FCG.Payments.WebApi/"]
RUN dotnet restore "./src/FCG.Payments.WebApi/FCG.Payments.WebApi.csproj"
COPY . .
WORKDIR "/src/src/FCG.Payments.WebApi"
RUN dotnet build "./FCG.Payments.WebApi.csproj" -c "$BUILD_CONFIGURATION" -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./FCG.Payments.WebApi.csproj" -c "$BUILD_CONFIGURATION" -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FCG.Payments.WebApi.dll"]
