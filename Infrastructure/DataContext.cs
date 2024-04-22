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
}
