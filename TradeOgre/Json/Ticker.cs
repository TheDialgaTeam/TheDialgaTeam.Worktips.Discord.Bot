using Newtonsoft.Json;

namespace TheDialgaTeam.Worktips.Discord.Bot.TradeOgre.Json
{
    public class Ticker
    {
        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("initialprice")]
        public decimal InitialPrice { get; set; }

        [JsonProperty("price")]
        public decimal Price { get; set; }

        [JsonProperty("high")]
        public decimal High { get; set; }

        [JsonProperty("low")]
        public decimal Low { get; set; }

        [JsonProperty("volume")]
        public decimal Volume { get; set; }

        [JsonProperty("bid")]
        public decimal Bid { get; set; }

        [JsonProperty("ask")]
        public decimal Ask { get; set; }
    }
}