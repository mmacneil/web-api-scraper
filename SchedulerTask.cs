using Hangfire;
using WebApiScraper.Infrastructure;

namespace WebApiScraper
{
    public class SchedulerTask : IHostedService
    {
        Task IHostedService.StartAsync(CancellationToken cancellationToken)
        {
            RecurringJob.AddOrUpdate<StatisticsCrawler>(
                "CrawlStats",
                x => x.CrawlPage("https://fullstackmark.com/static/posts/29/hockey-stats.html"),
                "0 * * ? * *" // Every minute
            );
            return Task.CompletedTask;
        }

        Task IHostedService.StopAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
