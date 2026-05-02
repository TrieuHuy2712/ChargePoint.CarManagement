@echo off
cd %~dp0
dotnet ef migrations add InitialMySqlMigration --project ../ChargePoint.CarManagement.Infrastructure/ChargePoint.CarManagement.Infrastructure.csproj --startup-project ../ChargePoint.CarManagement/ChargePoint.CarManagement.csproj --output-dir Persistence/Migrations