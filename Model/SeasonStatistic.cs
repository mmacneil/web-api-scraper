namespace WebApiScraper.Model
{
    public class SeasonStatistic
    {
        public int SeasonStatisticId { get; set; }
        public required int PlayerId { get; set; }
        public required string Season { get; set; }
        public required string Team { get; set; }
        public required string League { get; set; }
        public required int GamesPlayed { get; set; }
        public required int Goals { get; set; }
        public required int Assists { get; set; }
        public required int Points { get; set; }
        public required int PenaltyMinutes { get; set; }
        public int? PlusMinus { get; set; }
    }
}
