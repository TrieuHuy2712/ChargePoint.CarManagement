@echo off
chcp 65001 >nul
echo.
echo ============================================
echo   MIGRATE D? LI?U SQLITE SANG MYSQL (C#)
echo ============================================
echo.

cd /d "%~dp0DataMigrationTool"

echo ?ang ch?y DataMigrationTool...
echo.

dotnet run

echo.
pause
