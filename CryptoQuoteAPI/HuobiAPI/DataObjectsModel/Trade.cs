using System;
using System.Collections.Generic;
using System.Numerics;
using Newtonsoft.Json;

namespace CryptoQuote.HuobiAPI.DataObjectsModel
{
    /// <summary>
    /// Single definition of a Trade.
    /// </summary>
    [JsonObject(NamingStrategyType = typeof(Newtonsoft.Json.Serialization.CamelCaseNamingStrategy))]
    public class Trade
    {
        [JsonProperty("ts")]
        public UInt64? Timestamp { get; set; }

        [JsonProperty("trade-id")]
        public UInt64 TradeId { get; set; }

        public System.Numerics.BigInteger Id { get; set; }
        public float Amount { get; set; }
        public float Price { get; set; }
        public string Direction { get; set; }
    }

    /// <summary>
    /// Class to format data coming from GetLatestTrade and GetMostRecentTrades requests.
    /// </summary>
    [JsonObject(NamingStrategyType = typeof(Newtonsoft.Json.Serialization.CamelCaseNamingStrategy))]
    public class TradesAggregated
    {
        [JsonProperty("ts")]
        public UInt64? Timestamp { get; set; }

        [JsonProperty("data")]
        public List<Trade> Trades { get; set; }

        public System.Numerics.BigInteger Id { get; set; }
    }
}
