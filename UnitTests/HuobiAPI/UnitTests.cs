using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using CryptoQuote.HuobiAPI;
using CryptoQuote.HuobiAPI.DataObjectsModel;

namespace UnitTests.HuobiAPI
{
    public class UnitTestHuobiAPI
    {
        Rest RestApi = new Rest();

        Action<Task> ThrowsInnerException = delegate (Task async_task)
        {
            try
            {
                async_task.Wait();
            }

            catch (Exception ex)
            {
                throw ex.InnerException;
            }
        };

        [Fact]
        public void TestGetAllCurrencies()
        {
            var expected_ccy = new List<string>() { "btc", "eth", "dash", "xrp", "ltc", "eos", "usdt" };
            var allcurrencies = RestApi.GetAllCurrencies();
            Task.WaitAll(allcurrencies);

            Assert.Equal(TaskStatus.RanToCompletion, allcurrencies.Status);


            var missing_ccy = expected_ccy.Except(allcurrencies.Result);

            Assert.Empty(missing_ccy);
        }

        [Theory]
        [InlineData("btc", "eth")]
        public void TestGetAllSymbols(string quote_ccy, string base_ccy)
        {
            var allsymbols = RestApi.GetAllSymbols();
            Task.WaitAll(allsymbols);

            Assert.Equal(TaskStatus.RanToCompletion, allsymbols.Status);


            var FoundQuoteCcy = (from trading_symbol in allsymbols.Result
                                 where trading_symbol.BaseCurrency == base_ccy && trading_symbol.QuoteCurrency == quote_ccy
                                 select trading_symbol.QuoteCurrency).SingleOrDefault();

            Assert.Equal(quote_ccy, FoundQuoteCcy);
        }

        [Theory]
        [InlineData("usdt")]
        [InlineData("btc")]
        [InlineData("eth")]
        public void TestGetCcyReferenceInfo(string ccy)
        {
            var ccy_reference_info = RestApi.GetCcyReferenceInfo(ccy, true);
            var all_reference_info = RestApi.GetCcyReferenceInfo(null, true);
            var ccy_reference_info_false = RestApi.GetCcyReferenceInfo(ccy, true);
            var all_reference_info_false = RestApi.GetCcyReferenceInfo(null, true);

            List<Task> AllTasks = new List<Task>()
            {
                ccy_reference_info,
                all_reference_info,
                ccy_reference_info_false,
                all_reference_info_false
            };

            Task.WaitAll(AllTasks.ToArray());

            Assert.Equal(TaskStatus.RanToCompletion, ccy_reference_info.Status);
            Assert.Equal(TaskStatus.RanToCompletion, all_reference_info.Status);
            Assert.Equal(TaskStatus.RanToCompletion, ccy_reference_info_false.Status);
            Assert.Equal(TaskStatus.RanToCompletion, all_reference_info_false.Status);

            var ref_info = (from info in all_reference_info.Result
                            where info.Currency == ccy
                            select info).SingleOrDefault();

            var ref_info_false = (from info in all_reference_info_false.Result
                                  where info.Currency == ccy
                                  select info).SingleOrDefault();


            Assert.NotEmpty(ccy_reference_info.Result); Assert.NotEmpty(all_reference_info.Result);
            Assert.NotEmpty(ccy_reference_info_false.Result); Assert.NotEmpty(all_reference_info_false.Result);

            Assert.Equal(ccy, ref_info.Currency);
            Assert.Equal(ccy, ccy_reference_info.Result.SingleOrDefault().Currency);

            Assert.Equal(ccy, ref_info_false.Currency);
            Assert.Equal(ccy, ccy_reference_info_false.Result.SingleOrDefault().Currency);
        }

        [Theory]
        [InlineData("eosbtc", TickerPeriod.Day1, 600)]
        [InlineData("ltcusdt", TickerPeriod.Hour4, 500)]
        [InlineData("btcusdt", TickerPeriod.Hour4, 2001)]
        [InlineData("", TickerPeriod.Hour4, 200)]
        [InlineData(null, TickerPeriod.Hour4, 200)]
        public void TestGetTickerHistory(string ccy, TickerPeriod period, int? size)
        {
            var ticker_history = RestApi.GetTickerHistory(ccy, period, size);

            if (!string.IsNullOrEmpty(ccy) && size <= 2000 && size > 0)
            {
                ticker_history.Wait();

                Assert.Equal(TaskStatus.RanToCompletion, ticker_history.Status);
                Assert.Equal(size, ticker_history.Result.Count);
            }

            // Test Exceptions
            else
            {
                if (size > 2000 || size < 1)
                {
                    var ex = Assert.Throws<ArgumentOutOfRangeException>(() => ThrowsInnerException(ticker_history));

                    Assert.Equal(TaskStatus.Faulted, ticker_history.Status);
                    Assert.Equal("size parameter should be in [1, 2000] range\r\nParameter name: size", ex.Message);
                }

                else if (string.IsNullOrEmpty(ccy))
                {
                    var ex = Assert.Throws<ArgumentException>(() => ThrowsInnerException(ticker_history));

                    Assert.Equal(TaskStatus.Faulted, ticker_history.Status);
                    Assert.Equal("symbol parameter is required\r\nParameter name: symbol", ex.Message);
                }
            }
        }

        [Theory]
        [InlineData("ltcbtc")]
        [InlineData("ethusdt")]
        [InlineData(null)]
        public void TestGetLatestAggregatedTicker(string ccy)
        {
            var latest_aggregated_ticker = RestApi.GetLatestAggregatedTicker(ccy);
            if (!string.IsNullOrEmpty(ccy))
            {
                latest_aggregated_ticker.Wait();

                Assert.Equal(TaskStatus.RanToCompletion, latest_aggregated_ticker.Status);
                Assert.True(latest_aggregated_ticker.Result.Open > 0);
            }

            // Test Exceptions
            else
            {
                var ex = Assert.Throws<ArgumentException>(() => ThrowsInnerException(latest_aggregated_ticker));

                Assert.Equal(TaskStatus.Faulted, latest_aggregated_ticker.Status);
                Assert.Equal("symbol parameter is required\r\nParameter name: symbol", ex.Message);
            }
        }

        [Fact]
        public void TestGetAllPairsLatestTickers()
        {
            var expected_ccy = new List<string>() { "btcusdt", "eoseth", "dashbtc", "xrpusdt", "ltcusdt", "eosbtc" };
            var all_pairs_ticker = RestApi.GetAllPairsLatestTickers();
            Task.WaitAll(all_pairs_ticker);

            Assert.Equal(TaskStatus.RanToCompletion, all_pairs_ticker.Status);
            Assert.NotEmpty(all_pairs_ticker.Result);

            var tickers = (from ticker in all_pairs_ticker.Result
                           where expected_ccy.Contains(ticker.Symbol)
                           select ticker).ToList();

            Assert.Equal(expected_ccy.Count, tickers.Count);
            Array.ForEach(tickers.ToArray(), delegate (Ticker x)
            {
                Assert.True(x.Open > 0);
            });
        }

        [Theory]
        [InlineData("ltcbtc", Depth.Ten)]
        [InlineData("ethusdt", Depth.Twenty)]
        [InlineData("", Depth.Ten)]
        [InlineData(null, Depth.Twenty)]
        public void TestGetMarketDepth(string ccy, Depth depth)
        {
            var market_depth = RestApi.GetMarketDepth(ccy, AggregationLevel.Step1, depth);

            if (!string.IsNullOrEmpty(ccy))
            {
                Task.WaitAll(market_depth);

                Assert.Equal(TaskStatus.RanToCompletion, market_depth.Status);
                Assert.NotNull(market_depth.Result);

                Assert.NotEmpty(market_depth.Result.Asks);
                Assert.NotEmpty(market_depth.Result.Bids);
            }
            // Test Exceptions
            else
            {
                var ex = Assert.Throws<ArgumentException>(() => ThrowsInnerException(market_depth));

                Assert.Equal(TaskStatus.Faulted, market_depth.Status);
                Assert.Equal("symbol parameter is required\r\nParameter name: symbol", ex.Message);
            }
        }

        [Theory]
        [InlineData(null)] // Test Exception
        [InlineData("ethusdt")]
        [InlineData("ltcbtc")]
        [InlineData("eosbtc")]
        public void TestGetLatestTrades(string ccy)
        {
            var latest_trade = RestApi.GetLatestTrade(ccy);
            if (!string.IsNullOrEmpty(ccy))
            {
                latest_trade.Wait();

                Assert.Equal(TaskStatus.RanToCompletion, latest_trade.Status);
                Assert.True(latest_trade.Result.Timestamp > 0 && latest_trade.Result.Id > 0);
                Assert.NotEmpty(latest_trade.Result.Trades);
                Assert.True(latest_trade.Result.Trades.First().Price > 0 &&
                    (latest_trade.Result.Trades.First().Direction.ToLower() == "buy" || latest_trade.Result.Trades.First().Direction.ToLower() == "sell"));
            }

            // Test Exceptions
            else
            {
                var ex = Assert.Throws<ArgumentException>(() => ThrowsInnerException(latest_trade));

                Assert.Equal(TaskStatus.Faulted, latest_trade.Status);
                Assert.Equal("symbol parameter is required\r\nParameter name: symbol", ex.Message);
            }
        }

        [Theory]
        [InlineData("eosbtc", 200)]
        [InlineData("btcusdt", 5)]
        [InlineData(null, 0)] // Test ArgumentException
        [InlineData("ethusdt", 2001)] // Test ArgumentOutOfRangeException
        [InlineData("ltcbtc", 0)] // Test ArgumentOutOfRangeException
        public void TestGetMostRecentTrades(string ccy, int? size)
        {
            var recent_trades = RestApi.GetMostRecentTrades(ccy, size);
            if (!string.IsNullOrEmpty(ccy) && size <= 2000 && size > 0)
            {
                recent_trades.Wait();

                Assert.Equal(TaskStatus.RanToCompletion, recent_trades.Status);
                Assert.NotEmpty(recent_trades.Result);
                Assert.True(recent_trades.Result.First().Timestamp > 0 && recent_trades.Result.First().Id > 0);
                Assert.NotEmpty(recent_trades.Result.First().Trades);
            }

            // Test Exceptions
            else
            {
                if (size > 2000 || size < 1)
                {
                    var ex = Assert.Throws<ArgumentOutOfRangeException>(() => ThrowsInnerException(recent_trades));

                    Assert.Equal(TaskStatus.Faulted, recent_trades.Status);
                    Assert.Equal("size parameter should be in [1, 2000] range\r\nParameter name: size", ex.Message);
                }

                else if (string.IsNullOrEmpty(ccy))
                {
                    var ex = Assert.Throws<ArgumentException>(() => ThrowsInnerException(recent_trades));

                    Assert.Equal(TaskStatus.Faulted, recent_trades.Status);
                    Assert.Equal("symbol parameter is required\r\nParameter name: symbol", ex.Message);
                }
            }
        }

        [Theory]
        [InlineData(null)] // Test Exception
        [InlineData("ethusdt")]
        [InlineData("ltcbtc")]
        [InlineData("eosbtc")]
        public void TestGetLastMarketSummary(string ccy)
        {
            var last_market_summary = RestApi.GetLastMarketSummary(ccy);
            if (!string.IsNullOrEmpty(ccy))
            {
                last_market_summary.Wait();

                Assert.Equal(TaskStatus.RanToCompletion, last_market_summary.Status);
                Assert.True(last_market_summary.Result.Amount > 0);
            }

            // Test Exceptions
            else
            {
                var ex = Assert.Throws<ArgumentException>(() => ThrowsInnerException(last_market_summary));

                Assert.Equal(TaskStatus.Faulted, last_market_summary.Status);
                Assert.Equal("symbol parameter is required\r\nParameter name: symbol", ex.Message);
            }
        }
    }
}
