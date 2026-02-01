# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["ProductsSales.Api/ProductsSales.Api.csproj", "ProductsSales.Api/"]
COPY ["ProductsSales.Application/ProductsSales.Application.csproj", "ProductsSales.Application/"]
COPY ["ProductsSales.Infrastructure/ProductsSales.Infrastructure.csproj", "ProductsSales.Infrastructure/"]
COPY ["ProductsSales.Domain/ProductsSales.Domain.csproj", "ProductsSales.Domain/"]

RUN dotnet restore "ProductsSales.Api/ProductsSales.Api.csproj"

# Copy all source files
COPY . .

# Build the application
WORKDIR "/src/ProductsSales.Api"
RUN dotnet build "ProductsSales.Api.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "ProductsSales.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ProductsSales.Api.dll"]
