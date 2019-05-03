using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Newtonsoft.Json;
using TheDialgaTeam.Worktips.Discord.Bot.Services.Console;
using TheDialgaTeam.Worktips.Discord.Bot.Services.EntityFramework;
using TheDialgaTeam.Worktips.Discord.Bot.Services.Rpc;
using TheDialgaTeam.Worktips.Discord.Bot.Services.Setting;
using TheDialgaTeam.Worktips.Discord.Bot.TradeOgre.Json;

namespace TheDialgaTeam.Worktips.Discord.Bot.Discord.Modules
{
    [Name("Market")]
    public class MarketModule : ModuleHelper
    {
        public MarketModule(LoggerService loggerService, ConfigService configService, SqliteDatabaseService sqliteDatabaseService, RpcService rpcService) : base(loggerService, configService, sqliteDatabaseService, rpcService)
        {
        }

        [Command("LTCMarketPrice")]
        [Alias("Price", "LTCPrice")]
        [Summary("Retrieve the volume, high, and low are in the last 24 hours, initial price is the price from 24 hours ago.")]
        public async Task LTCMarketPriceAsync()
        {
            try
            {
                var httpClient = new HttpClient();
                var response = await httpClient.GetAsync($"https://tradeogre.com/api/v1/ticker/LTC-{ConfigService.CoinSymbol}").ConfigureAwait(false);
                response.EnsureSuccessStatusCode();

                using (var streamReader = new StreamReader(await response.Content.ReadAsStreamAsync().ConfigureAwait(false)))
                {
                    using (var jsonReader = new JsonTextReader(streamReader))
                    {
                        var serializer = new JsonSerializer();
                        var ticker = serializer.Deserialize<Ticker>(jsonReader);

                        if (!ticker.Success)
                        {
                            await ReplyAsync(":x: Unable to retrieve market information from tradeogre.\nThe coin may not be listed.").ConfigureAwait(false);
                            return;
                        }

                        var priceChange = ticker.Price - ticker.InitialPrice;
                        var priceChangeSign = "";

                        if (priceChange > 0)
                            priceChangeSign = "+";
                        else if (priceChange < 0)
                            priceChangeSign = "-";

                        var marketEmbed = new EmbedBuilder()
                            .WithColor(Color.Orange)
                            .WithTitle($":moneybag: TradeOgre LTC-{ConfigService.CoinSymbol} Price")
                            .WithUrl($"https://tradeogre.com/api/v1/ticker/LTC-{ConfigService.CoinSymbol}")
                            .AddField("Current Price", $"{ticker.Price} LTC", true)
                            .AddField("24h Chg.", $"{ticker.Price - ticker.InitialPrice} LTC ({priceChangeSign}{(ticker.Price - ticker.InitialPrice) / ticker.InitialPrice * 100:F2}%)", true)
                            .AddField("Volume", $"{ticker.Volume} LTC", true)
                            .AddField("24h High", $"{ticker.High} LTC", true)
                            .AddField("24h Low", $"{ticker.Low} LTC", true);

                        await ReplyAsync("", false, marketEmbed.Build()).ConfigureAwait(false);
                    }
                }
            }
            catch (Exception ex)
            {
                await CatchError(ex).ConfigureAwait(false);
            }
        }

        [Command("BTCMarketPrice")]
        [Alias("BTCPrice")]
        [Summary("Retrieve the volume, high, and low are in the last 24 hours, initial price is the price from 24 hours ago.")]
        public async Task BTCMarketPriceAsync()
        {
            try
            {
                var httpClient = new HttpClient();
                var response = await httpClient.GetAsync($"https://tradeogre.com/api/v1/ticker/BTC-{ConfigService.CoinSymbol}").ConfigureAwait(false);
                response.EnsureSuccessStatusCode();

                using (var streamReader = new StreamReader(await response.Content.ReadAsStreamAsync().ConfigureAwait(false)))
                {
                    using (var jsonReader = new JsonTextReader(streamReader))
                    {
                        var serializer = new JsonSerializer();
                        var ticker = serializer.Deserialize<Ticker>(jsonReader);

                        if (!ticker.Success)
                        {
                            await ReplyAsync(":x: Unable to retrieve market information from tradeogre.\nThe coin may not be listed.").ConfigureAwait(false);
                            return;
                        }

                        var priceChange = ticker.Price - ticker.InitialPrice;
                        var priceChangeSign = "";

                        if (priceChange > 0)
                            priceChangeSign = "+";
                        else if (priceChange < 0)
                            priceChangeSign = "-";

                        var marketEmbed = new EmbedBuilder()
                            .WithColor(Color.Orange)
                            .WithTitle($":moneybag: TradeOgre BTC-{ConfigService.CoinSymbol} Price")
                            .WithUrl($"https://tradeogre.com/api/v1/ticker/BTC-{ConfigService.CoinSymbol}")
                            .AddField("Current Price", $"{ticker.Price} BTC", true)
                            .AddField("24h Chg.", $"{ticker.Price - ticker.InitialPrice} BTC ({priceChangeSign}{(ticker.Price - ticker.InitialPrice) / ticker.InitialPrice * 100:F2}%)", true)
                            .AddField("Volume", $"{ticker.Volume} BTC", true)
                            .AddField("24h High", $"{ticker.High} BTC", true)
                            .AddField("24h Low", $"{ticker.Low} BTC", true);

                        await ReplyAsync("", false, marketEmbed.Build()).ConfigureAwait(false);
                    }
                }
            }
            catch (Exception ex)
            {
                await CatchError(ex).ConfigureAwait(false);
            }
        }
    }
}