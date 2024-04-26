namespace WebApiScraper.Infrastructure.Repositories;

using Dapper;
using WebApiScraper.Core.Models;

public class PlayerRepository
{
    private readonly DatabaseContext _dbContext;

    public PlayerRepository(DatabaseContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Player?> GetByName(string name)
    {
        using var connection = _dbContext.CreateDbConnection();
        const string sql = """
                SELECT * FROM Players
                WHERE Name = @name
            """;
        return await connection.QuerySingleOrDefaultAsync<Player>(sql, new { name });
    }

    public async Task Create(Player player)
    {
        using var connection = _dbContext.CreateDbConnection();
        const string sql = """
            	INSERT INTO Players (Name, HomeTown)
            	SELECT @Name, @HomeTown
            	WHERE NOT EXISTS (
            	  SELECT 1 FROM Players WHERE LOWER(Name) = LOWER(@Name));
            """;
        await connection.ExecuteAsync(sql, player);
    }
}
