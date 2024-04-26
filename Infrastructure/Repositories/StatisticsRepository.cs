namespace WebApiScraper.Infrastructure.Repositories;

using Dapper;
using WebApiScraper.Core.Models;

public class StatisticsRepository
{
    private readonly DatabaseContext _dbContext;

    public StatisticsRepository(DatabaseContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Create(SeasonStatistic stat)
    {
        using var connection = _dbContext.CreateDbConnection();
        const string sql = """
            	INSERT INTO SeasonStatistics (PlayerId, Season, Team, League, GamesPlayed, Goals, Assists, Points, PenaltyMinutes, PlusMinus)
            	SELECT @PlayerId, @Season, @Team, @League, @GamesPlayed, @Goals, @Assists, @Points, @PenaltyMinutes, @PlusMinus
            	WHERE NOT EXISTS (
            	  SELECT 1 FROM SeasonStatistics WHERE PlayerId = @PlayerId AND LOWER(Season) = LOWER(@Season) AND LOWER(Team) = LOWER(@Team) AND LOWER(League) = LOWER(@League));
            """;
        await connection.ExecuteAsync(sql, stat);
    }
}
