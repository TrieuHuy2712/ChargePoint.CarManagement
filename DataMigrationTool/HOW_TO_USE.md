# DataMigrationTool - How to Use

## Quick Start

### Run Migration:
```cmd
run-data-migration.cmd
```

Or:
```powershell
cd DataMigrationTool
dotnet run
```

---

## Configuration

Edit `DataMigrationTool/Program.cs`:

```csharp
// SQLite path
var sqliteDbPath = @"..\ChargePoint.CarManagement\CarManagement.db";

// MySQL connection
var mysqlConnectionString = "Server=localhost;Port=3306;Database=ChargeManagement;User=root;Password=123456;";
```

---

## What it does:

1. Test connections (SQLite & MySQL)
2. Clear sample data in MySQL
3. Migrate tables in order:
   - Cars
   - CarMedias
   - MaintenanceRecords
   - TireRecords
   - TrafficViolationChecks
   - SystemSettings
4. Verify data counts
5. Report results

---

## Troubleshooting

**"Unable to open database file"**
Check SQLite file exists: `ChargePoint.CarManagement\CarManagement.db`

**"Unable to connect to MySQL"**
Check MySQL Server is running

**"Access denied"**
Check username/password in connection string

---

**Tech**: .NET 8, ADO.NET, SQLite, MySQL
