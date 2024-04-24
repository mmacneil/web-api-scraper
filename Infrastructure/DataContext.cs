namespace WebApplication1.Helpers;

using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;

public class DataContext(IConfiguration configuration)
{
    private IDbConnection CreateDbConnection(string connectionString)
    {
        return new SqlConnection(connectionString);
    }

    public async Task Init()
    {
        await InitDatabase();
        await InitTables();
    }

    private async Task InitDatabase()
    {
        // create database if it doesn't exist
        using var connection = CreateDbConnection(
            configuration.GetConnectionString("MasterDb") ?? throw new InvalidOperationException()
        );

        await connection.ExecuteAsync(
            "IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'WebApiScraper') CREATE DATABASE [WebApiScraper];"
        );
    }

    private async Task InitTables()
    {
        // create tables if they don't exist
        using var connection = CreateDbConnection(
            configuration.GetConnectionString("AppDb") ?? throw new InvalidOperationException()
        );

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
