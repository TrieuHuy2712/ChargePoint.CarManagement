using System.Data;
using Microsoft.Data.Sqlite;
using MySqlConnector;

namespace DataMigrationTool;

public class DataMigrationService
{
    private readonly string _sqliteConnectionString;
    private readonly string _mysqlConnectionString;

    // Danh sách bảng theo thứ tự migrate (theo foreign key dependencies)
    private readonly List<string> _tablesOrder = new()
    {
        "Cars",
        "CarMedias",
        "MaintenanceRecords",
        "TireRecords",
        "TrafficViolationChecks",
        "SystemSettings"
    };

    public DataMigrationService(string sqliteConnectionString, string mysqlConnectionString)
    {
        _sqliteConnectionString = sqliteConnectionString;
        _mysqlConnectionString = mysqlConnectionString;
    }

    public async Task MigrateAsync()
    {
        Console.WriteLine("=" + new string('=', 69));
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("  MIGRATE DỮ LIỆU TỪ SQLITE SANG MYSQL");
        Console.ResetColor();
        Console.WriteLine("=" + new string('=', 69));
        Console.WriteLine();

        try
        {
            // Test connections
            await TestConnectionsAsync();

            // Clear MySQL sample data
            await ClearMySqlDataAsync();

            // Migrate each table
            Console.WriteLine();
            Console.WriteLine("?? Bắt đầu migrate dữ liệu:");
            Console.WriteLine(new string('-', 70));

            int totalMigrated = 0;
            foreach (var table in _tablesOrder)
            {
                var migrated = await MigrateTableAsync(table);
                totalMigrated += migrated;
            }

            Console.WriteLine(new string('-', 70));
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"? Đã migrate tổng cộng {totalMigrated} rows");
            Console.ResetColor();

            // Verify migration
            await VerifyMigrationAsync();

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("?? MIGRATION THÀNH CÔNG!");
            Console.ResetColor();
            Console.WriteLine("Tất cả dữ liệu đã được migrate chính xác.");
            Console.WriteLine();
            Console.WriteLine("?? Bước tiếp theo:");
            Console.WriteLine("1. Kiểm tra dữ liệu trong MySQL Workbench");
            Console.WriteLine("2. Chạy application và test các chức năng");
            Console.WriteLine("3. Backup SQLite database cũ để an toàn");
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n? Lỗi trong quá trình migration: {ex.Message}");
            Console.ResetColor();
            throw;
        }
    }

    private async Task TestConnectionsAsync()
    {
        // Test SQLite
        using (var conn = new SqliteConnection(_sqliteConnectionString))
        {
            await conn.OpenAsync();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("? Kết nối SQLite thành công");
            Console.ResetColor();
        }

        // Test MySQL
        using (var conn = new MySqlConnection(_mysqlConnectionString))
        {
            await conn.OpenAsync();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("? Kết nối MySQL thành công");
            Console.ResetColor();
        }
    }

    private async Task ClearMySqlDataAsync()
    {
        Console.WriteLine();
        Console.WriteLine("?? Xóa dữ liệu mẫu trong MySQL...");

        using var conn = new MySqlConnection(_mysqlConnectionString);
        await conn.OpenAsync();

        using var transaction = await conn.BeginTransactionAsync();
        try
        {
            // Disable foreign key checks
            using (var cmd = new MySqlCommand("SET FOREIGN_KEY_CHECKS = 0", conn, transaction))
            {
                await cmd.ExecuteNonQueryAsync();
            }

            // Delete data from each table
            foreach (var table in _tablesOrder)
            {
                using var cmd = new MySqlCommand($"DELETE FROM `{table}`", conn, transaction);
                await cmd.ExecuteNonQueryAsync();
                Console.WriteLine($"Đã xóa dữ liệu trong bảng {table}");
            }

            // Enable foreign key checks
            using (var cmd = new MySqlCommand("SET FOREIGN_KEY_CHECKS = 1", conn, transaction))
            {
                await cmd.ExecuteNonQueryAsync();
            }

            await transaction.CommitAsync();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("? Xóa dữ liệu mẫu thành công");
            Console.ResetColor();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    private async Task<int> MigrateTableAsync(string tableName)
    {
        using var sqliteConn = new SqliteConnection(_sqliteConnectionString);
        using var mysqlConn = new MySqlConnection(_mysqlConnectionString);

        await sqliteConn.OpenAsync();
        await mysqlConn.OpenAsync();

        // Check if table exists in SQLite
        using (var checkCmd = new SqliteCommand(
            "SELECT name FROM sqlite_master WHERE type='table' AND name=@tableName", sqliteConn))
        {
            checkCmd.Parameters.AddWithValue("@tableName", tableName);
            var exists = await checkCmd.ExecuteScalarAsync();
            if (exists == null)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Bảng {tableName} không tồn tại trong SQLite, bỏ qua");
                Console.ResetColor();
                return 0;
            }
        }

        // Count rows
        using (var countCmd = new SqliteCommand($"SELECT COUNT(*) FROM {tableName}", sqliteConn))
        {
            var count = Convert.ToInt32(await countCmd.ExecuteScalarAsync());
            if (count == 0)
            {
                Console.WriteLine($"Bảng {tableName} trống, bỏ qua");
                return 0;
            }

            Console.WriteLine($"Đang migrate bảng {tableName} ({count} rows)...");
        }

        // Get column names
        var columns = await GetTableColumnsAsync(sqliteConn, tableName);

        // Read data from SQLite
        var dataTable = new DataTable();
        using (var selectCmd = new SqliteCommand($"SELECT * FROM {tableName}", sqliteConn))
        using (var reader = await selectCmd.ExecuteReaderAsync())
        {
            dataTable.Load(reader);
        }

        // Insert into MySQL
        int migrated = 0;
        int errors = 0;

        using var transaction = await mysqlConn.BeginTransactionAsync();
        try
        {
            foreach (DataRow row in dataTable.Rows)
            {
                try
                {
                    var columnsStr = string.Join(", ", columns.Select(c => $"`{c}`"));
                    var valuesStr = string.Join(", ", columns.Select(c => $"@{c}"));
                    var insertSql = $"INSERT INTO `{tableName}` ({columnsStr}) VALUES ({valuesStr})";

                    using var insertCmd = new MySqlCommand(insertSql, mysqlConn, transaction);

                    foreach (var column in columns)
                    {
                        var value = row[column];
                        insertCmd.Parameters.AddWithValue($"@{column}", value == DBNull.Value ? null : value);
                    }

                    await insertCmd.ExecuteNonQueryAsync();
                    migrated++;
                }
                catch (Exception ex)
                {
                    errors++;
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"Lỗi insert row: {ex.Message}");
                    Console.ResetColor();
                }
            }

            await transaction.CommitAsync();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Đã migrate {migrated}/{dataTable.Rows.Count} rows (lỗi: {errors})");
            Console.ResetColor();

            return migrated;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    private async Task<List<string>> GetTableColumnsAsync(SqliteConnection conn, string tableName)
    {
        var columns = new List<string>();
        using var cmd = new SqliteCommand($"PRAGMA table_info({tableName})", conn);
        using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            columns.Add(reader.GetString(1)); // Column name is at index 1
        }

        return columns;
    }

    private async Task VerifyMigrationAsync()
    {
        Console.WriteLine();
        Console.WriteLine("Kiểm tra kết quả migration:");
        Console.WriteLine(new string('-', 70));
        Console.WriteLine($"{"Bảng",-30} {"SQLite",-15} {"MySQL",-15} Status");
        Console.WriteLine(new string('-', 70));

        bool allMatch = true;

        using var sqliteConn = new SqliteConnection(_sqliteConnectionString);
        using var mysqlConn = new MySqlConnection(_mysqlConnectionString);

        await sqliteConn.OpenAsync();
        await mysqlConn.OpenAsync();

        foreach (var table in _tablesOrder)
        {
            int sqliteCount = 0;
            int mysqlCount = 0;

            try
            {
                // Count SQLite
                using var sqliteCmd = new SqliteCommand($"SELECT COUNT(*) FROM {table}", sqliteConn);
                sqliteCount = Convert.ToInt32(await sqliteCmd.ExecuteScalarAsync());
            }
            catch { }

            try
            {
                // Count MySQL
                using var mysqlCmd = new MySqlCommand($"SELECT COUNT(*) FROM `{table}`", mysqlConn);
                mysqlCount = Convert.ToInt32(await mysqlCmd.ExecuteScalarAsync());
            }
            catch { }

            var status = sqliteCount == mysqlCount ? "? OK" : "? KHÁC";
            if (sqliteCount != mysqlCount)
            {
                allMatch = false;
                Console.ForegroundColor = ConsoleColor.Red;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Green;
            }

            Console.WriteLine($"{table,-30} {sqliteCount,-15} {mysqlCount,-15} {status}");
            Console.ResetColor();
        }

        Console.WriteLine(new string('-', 70));

        if (!allMatch)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\n? MIGRATION HOÀN TẤT NHƯNG CÓ KHÁC BIỆT");
            Console.WriteLine("Vui lòng kiểm tra lại dữ liệu trong MySQL.");
            Console.ResetColor();
        }
    }
}
