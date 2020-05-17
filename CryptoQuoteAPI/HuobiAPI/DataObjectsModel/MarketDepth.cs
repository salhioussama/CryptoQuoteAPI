using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace CryptoQuote.HuobiAPI.DataObjectsModel
{
    /// <summary>
    /// Class to format data coming from GetMarketDepth request.
    /// </summary>
    [JsonObject(NamingStrategyType = typeof(Newtonsoft.Json.Serialization.CamelCaseNamingStrategy))]
    public class MarketDepth
    {
        [JsonProperty("ts")]
        public UInt64? Timestamp { get; set; }
        public List<float[]> Bids { get; set; }
        public List<float[]> Asks { get; set; }
        public System.Numerics.BigInteger Version { get; set; }
    }
}
