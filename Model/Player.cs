namespace WebApiScraper.Model
{
    public class Player
    {
        public int PlayerId { get; set; }
        public required string Name { get; set; }
        public required string HomeTown { get; set; }
        public List<SeasonStatistic>? SeasonStatistics { get; set; }
    }
}
