# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["ChargePoint.CarManagement.sln", "."]
COPY ["ChargePoint.CarManagement/ChargePoint.CarManagement.csproj", "ChargePoint.CarManagement/"]
COPY ["ChargePoint.CarManagement.Application/ChargePoint.CarManagement.Application.csproj", "ChargePoint.CarManagement.Application/"]
COPY ["ChargePoint.CarManagement.Domain/ChargePoint.CarManagement.Domain.csproj", "ChargePoint.CarManagement.Domain/"]
COPY ["ChargePoint.CarManagement.Infrastructure/ChargePoint.CarManagement.Infrastructure.csproj", "ChargePoint.CarManagement.Infrastructure/"]

RUN dotnet restore "ChargePoint.CarManagement/ChargePoint.CarManagement.csproj"

COPY . .
WORKDIR /src/ChargePoint.CarManagement
RUN dotnet publish "ChargePoint.CarManagement.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "ChargePoint.CarManagement.dll"]
