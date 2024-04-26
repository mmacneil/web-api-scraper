namespace WebApiScraper.Infrastructure;

using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;

public class DatabaseContext(IConfiguration configuration)
{
    public IDbConnection CreateDbConnection() => CreateDbConnection(AppDbConnectionString);

    public string AppDbConnectionString =>
        configuration.GetConnectionString("AppDb") ?? throw new InvalidOperationException();
    public string MasterDbConnectionString =>
        configuration.GetConnectionString("MasterDb") ?? throw new InvalidOperationException();

    public async Task Init()
    {
        await InitDatabase();
        await InitTables();
    }

    private static IDbConnection CreateDbConnection(string connectionString) =>
        new SqlConnection(connectionString);

    private async Task InitDatabase()
    {
        // create database if it doesn't exist
        using var connection = CreateDbConnection(MasterDbConnectionString);

        await connection.ExecuteAsync(
            "IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'WebApiScraper') CREATE DATABASE [WebApiScraper];"
        );
    }

    private async Task InitTables()
    {
        // create tables if they don't exist
        using var connection = CreateDbConnection(AppDbConnectionString);

        await connection.ExecuteAsync(
            """
            IF OBJECT_ID('SeasonStatistics', 'U') IS NULL
            CREATE TABLE SeasonStatistics (
                SeasonStatisticId INT NOT NULL PRIMARY KEY IDENTITY,
                 PlayerId INT, Season NVARCHAR(MAX), Team NVARCHAR(MAX), League NVARCHAR(MAX), GamesPlayed INT, Goals INT, Assists INT, Points INT, PenaltyMinutes INT, PlusMinus INT NULL, CreatedAtUtc DATETIME DEFAULT GETUTCDATE());

            IF OBJECT_ID('Players', 'U') IS NULL
            CREATE TABLE Players (
                PlayerId INT NOT NULL PRIMARY KEY IDENTITY,
                Name NVARCHAR(MAX),
                HomeTown NVARCHAR(MAX),	CreatedAtUtc DATETIME DEFAULT GETUTCDATE()
            );
            """
        );
    }
}
