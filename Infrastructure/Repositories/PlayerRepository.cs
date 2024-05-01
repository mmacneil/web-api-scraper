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

        return await connection.QuerySingleOrDefaultAsync<Player>("""
                SELECT * FROM Players
                WHERE Name = @name
            """, new { name });
    }

    public async Task<Player?> GetById(int id)
    {
        using var connection1 = _dbContext.CreateDbConnection();

        var lookup = new Dictionary<int, Player>();

        await connection1.QueryAsync<Player, SeasonStatistic, Player>("""
			select p.*, s.* from Players p LEFT JOIN SeasonStatistics s on p.PlayerId = s.PlayerId
			WHERE p.PlayerId = @id
			""", (player, stat) =>
        {

            if (!lookup.TryGetValue(player.PlayerId, out Player? currentPlayer))
            {
                currentPlayer = player;
                currentPlayer.SeasonStatistics = [];
                lookup.Add(currentPlayer.PlayerId, currentPlayer);
            }
            currentPlayer.SeasonStatistics!.Add(stat);
            return currentPlayer;
        }, param: new { id }, splitOn: "SeasonStatisticId");


        return lookup.Count > 0 ? lookup.Values.First() : null;
    }

    public async Task Create(Player player)
    {
        using var connection = _dbContext.CreateDbConnection();

        await connection.ExecuteAsync("""
            	INSERT INTO Players (Name, HomeTown)
            	SELECT @Name, @HomeTown
            	WHERE NOT EXISTS (
            	  SELECT 1 FROM Players WHERE LOWER(Name) = LOWER(@Name));
            """, player);
    }
}
