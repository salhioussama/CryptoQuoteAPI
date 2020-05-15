using System;
using Newtonsoft.Json;
using System.Numerics;

namespace CryptoQuote.HuobiAPI.DataObjectsModel
{
    [JsonObject(NamingStrategyType = typeof(Newtonsoft.Json.Serialization.CamelCaseNamingStrategy))]
    public class Ticker
    {
        [JsonProperty("id")]
        public BigInteger? Timestamp { get; set; }
        public double Amount { get; set; }
        public UInt32 Count { get; set; }
        public float Open { get; set; }
        public float Close { get; set; }
        public float Low { get; set; }
        public float High { get; set; }
        public float Vol { get; set; }
        public string Symbol { get; set; }
    }

    [JsonObject(NamingStrategyType = typeof(Newtonsoft.Json.Serialization.CamelCaseNamingStrategy))]
    public class TickerAggregated : Ticker
    {
        public BigInteger Id { get; set; }
        public float[] Bid { get; set; }
        public float[] Ask { get; set; }
        public BigInteger Version { get; set; }
    }
}
