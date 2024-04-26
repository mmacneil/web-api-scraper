using PuppeteerSharp;
using WebApiScraper.Core.Models;
using WebApiScraper.Infrastructure.Repositories;

namespace WebApiScraper.Infrastructure
{
    public class StatisticsCrawler
    {
        private readonly PlayerRepository _playerRepository;
        private readonly StatisticsRepository _statisticsRepository;

        public StatisticsCrawler(
            PlayerRepository playerRepository,
            StatisticsRepository statisticsRepository
        )
        {
            _playerRepository = playerRepository;
            _statisticsRepository = statisticsRepository;
        }

        private static async Task<string> GetInnerText(IElementHandle elementHandle) =>
            (await elementHandle.GetPropertyAsync("innerText")).RemoteObject.Value.ToString();

        private async Task CrawlPage(string url)
        {
            var options = new LaunchOptions { Headless = true };
            Console.WriteLine("Downloading chromium");
            using var browserFetcher = new BrowserFetcher();
            await browserFetcher.DownloadAsync();

            Console.WriteLine($"Navigating {url}");
            await using var browser = await Puppeteer.LaunchAsync(options);
            await using var page = await browser.NewPageAsync();

            await page.GoToAsync(url);

            var nameElement = await page.QuerySelectorAsync(
                "body > div > div:nth-child(1) > div > div > div > div.col-md-8 > div > h5"
            );
            var homeTownElement = await page.QuerySelectorAsync(
                "body > div > div:nth-child(1) > div > div > div > div.col-md-8 > div > table > tbody > tr:nth-child(2) > td"
            );

            var name = await GetInnerText(nameElement);
            var homeTown = await GetInnerText(homeTownElement);

            Console.WriteLine($"Attempting to create {name} in the database.");
            await _playerRepository.Create(new Player() { Name = name, HomeTown = homeTown });

            var player = await _playerRepository.GetByName(name);

            Console.WriteLine(
                $"{name} created or already exists: {player.PlayerId} | {player.Name} | {player.HomeTown}."
            );

            var statTableRowElements = await page.QuerySelectorAllAsync(
                "body > div > div:nth-child(2) > div > table > tbody > tr"
            );

            foreach (var e in statTableRowElements)
            {
                // select the row column elements
                var columns = await e.QuerySelectorAllAsync("th, td");

                var season = (
                    await columns[0].GetPropertyAsync("innerText")
                ).RemoteObject.Value.ToString();
                var team = (
                    await columns[1].GetPropertyAsync("innerText")
                ).RemoteObject.Value.ToString();
                var league = (
                    await columns[2].GetPropertyAsync("innerText")
                ).RemoteObject.Value.ToString();
                var gamesPlayed = (
                    (int)(await columns[3].GetPropertyAsync("innerText")).RemoteObject.Value
                );
                var goals = (
                    (int)(await columns[4].GetPropertyAsync("innerText")).RemoteObject.Value
                );
                var assists = (
                    (int)(await columns[5].GetPropertyAsync("innerText")).RemoteObject.Value
                );
                var points = (
                    (int)(await columns[6].GetPropertyAsync("innerText")).RemoteObject.Value
                );
                var penaltyMinutes = (
                    (int)(await columns[7].GetPropertyAsync("innerText")).RemoteObject.Value
                );

                // Special handling for +/- as that column text can be empty in the markup.
                var plusMinusText = (
                    await columns[8].GetPropertyAsync("innerText")
                ).RemoteObject.Value.ToString();

                if (!int.TryParse(plusMinusText, out int plusMinus))
                {
                    plusMinus = 0;
                }

                Console.WriteLine(
                    $"Creating stat record: {season} | {team} | {league} "
                        + $"| GP: {gamesPlayed} "
                        + $"| G: {goals} "
                        + $"| A: {assists} "
                        + $"| P: {points} "
                        + $"| PIMs: {penaltyMinutes} "
                        + $"| +/-: {plusMinus}"
                );

                await _statisticsRepository.Create(
                    new SeasonStatistic()
                    {
                        PlayerId = player.PlayerId,
                        Season = season,
                        Team = team,
                        League = league,
                        GamesPlayed = gamesPlayed,
                        Goals = goals,
                        Assists = assists,
                        Points = points,
                        PenaltyMinutes = penaltyMinutes,
                        PlusMinus = plusMinus == 0 ? null : plusMinus
                    }
                );
            }
        }
    }
}
