using System;
using System.Collections.Generic;
using System.Numerics;
using Newtonsoft.Json;

namespace CryptoQuote.HuobiAPI.DataObjectsModel
{
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
