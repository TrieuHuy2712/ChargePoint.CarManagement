using DataMigrationTool;

Console.OutputEncoding = System.Text.Encoding.UTF8;

// Configuration
var sqliteDbPath = @"..\ChargePoint.CarManagement\CarManagement.db";
var sqliteConnectionString = $"Data Source={sqliteDbPath}";

var mysqlConnectionString = "Server=localhost;Port=3306;Database=ChargeManagement;User=root;Password=123456;";

// Warning
Console.WriteLine();
Console.ForegroundColor = ConsoleColor.Yellow;
Console.WriteLine("⚠️  CẢNH BÁO:");
Console.ResetColor();
Console.WriteLine("Script này sẽ XÓA TẤT CẢ dữ liệu hiện có trong MySQL");
Console.WriteLine("và thay thế bằng dữ liệu từ SQLite.");
Console.WriteLine();
Console.WriteLine($"SQLite DB: {Path.GetFullPath(sqliteDbPath)}");
Console.WriteLine($"MySQL DB:  {mysqlConnectionString.Replace(";Password=123456;", ";Password=****;")}");
Console.WriteLine();

Console.Write("Bạn có chắc chắn muốn tiếp tục? (yes/no): ");
var response = Console.ReadLine()?.Trim().ToLower();

if (response != "yes" && response != "y")
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine("Đã hủy migration.");
    Console.ResetColor();
    return;
}

Console.WriteLine();

try
{
    var migrationService = new DataMigrationService(sqliteConnectionString, mysqlConnectionString);
    await migrationService.MigrateAsync();

    Console.WriteLine();
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("✓ Đã hoàn tất migration!");
    Console.ResetColor();
}
catch (Exception ex)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"\n✗ Lỗi: {ex.Message}");
    Console.WriteLine($"\nStack trace:\n{ex.StackTrace}");
    Console.ResetColor();
    Environment.Exit(1);
}

Console.WriteLine("\nNhấn phím bất kỳ để thoát...");
Console.ReadKey();
