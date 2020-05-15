using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Web;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using CryptoQuote.HuobiAPI.DataObjectsModel;


namespace CryptoQuote.HuobiAPI
{
    /// <summary>
    /// Rest API class handling communication with Huobi server to get access to all needed information.
    /// The requirements have been gathered from Huobi API documentation: https://huobiapi.github.io/docs/spot/v1/en/
    /// </summary>
    #region Rest API
    public class Rest : HuobiAPI
    {
        public Rest(bool UseAws = false, string read_key = null, string read_secret = null) : base(UseAws, read_key, read_secret)
        {
        }

        /// <summary>
        /// Format the uri to use to request data from the Exchange as specified in https://huobiapi.github.io/docs/spot/v1/en/#access-and-authentication.
        /// </summary>
        /// <param name="uri_path">Uri path</param>
        /// <param name="method">The method to use (GET or POST).</param>
        /// <param name="parameters">All the necessary uri parameters.</param>
        /// <param name="access_key">Personal access key to use to identify the user.</param>
        /// <param name="secret_key">Personal secret key to use to create the signature.</param>
        /// <returns></returns>
        private Uri FormatUri(string uri_path, HttpMethod method = null, SortedDictionary<string, string> parameters = null, string access_key = null, string secret_key = null)
        {
            bool signUri = string.IsNullOrEmpty(access_key) || string.IsNullOrEmpty(secret_key) ? false : true;

            if (signUri)
            {
                parameters = parameters ?? new SortedDictionary<string, string>();
                parameters.Add("AccessKeyId", access_key);
                parameters.Add("SignatureMethod", SignatureMethod);
                parameters.Add("SignatureVersion", SignatureVersion);
                parameters.Add("Timestamp", DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss"));
            }

            UriBuilder final_uri = new UriBuilder(global_uri);
            final_uri.Path = uri_path;

            // Add parameters
            if (parameters != null)
            {
                var query = HttpUtility.ParseQueryString(final_uri.Query);
                Array.ForEach(parameters.Keys.ToArray(), x => query[x] = parameters[x]);

                if (signUri) query["Signature"] = GetUriSignature(method ?? HttpMethod.Get, final_uri.Host, final_uri.Path, query.ToString(), secret_key);

                final_uri.Query = query.ToString();
            }

            return final_uri.Uri;
        }

        /// <summary>
        /// Sign the request as specified in https://huobiapi.github.io/docs/spot/v1/en/#access-and-authentication.
        /// </summary>
        /// <param name="method">The method to use (GET or POST).</param>
        /// <param name="host">Hotsname.</param>
        /// <param name="path">Uri path.</param>
        /// <param name="query">Uri parameters.</param>
        /// <param name="secret_key">Personal secret key to use to create the signature.</param>
        /// <returns></returns>
        private string GetUriSignature(HttpMethod method, string host, string path, string query, string secret_key)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(method.ToString().ToUpper()).Append("\n").Append(host).Append("\n").Append(path).Append("\n").Append(query);

            using (var hmacsha256 = new HMACSHA256(Encoding.UTF8.GetBytes(secret_key)))
            {
                var hash = hmacsha256.ComputeHash(Encoding.UTF8.GetBytes(sb.ToString()));
                return Convert.ToBase64String(hash);
            }
        }

        /// <summary>
        /// Common implementation of http get request to communicate with exchange servers.
        /// </summary>
        /// <param name="uri">The final Uri to use to query information from Exchange's servers.</param>
        /// <returns>Returns received json file.</returns>
        private async Task<string> HttpGetRequest(Uri uri)
        {
            string json_string = string.Empty;

            WebRequest request = HttpWebRequest.Create(uri);
            request.Method = "GET";

            using (var http_response = (HttpWebResponse)await request.GetResponseAsync())
            {
                if (http_response.StatusCode != HttpStatusCode.OK)
                {
                    throw new HttpRequestException($"Bad request status code '{http_response.StatusCode}' and status description '{http_response.StatusDescription}'");
                }

                using (Stream stream = http_response.GetResponseStream())
                {
                    using (StreamReader response_stream = new StreamReader(stream))
                    {
                        json_string = await response_stream.ReadToEndAsync();
                    }
                }
            }

            return json_string;
        }

        /// <summary>
        /// Parse the json response into the correct HuobiAPI.DataObjectsModel <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">HuobiAPI.DataObjectsModel expected as output.</typeparam>
        /// <param name="json">The json response received from the Exchange server.</param>
        /// <param name="node_path">The node name where the final data are.</param>
        /// <returns>The requested DataObjectsModel <typeparamref name="T"/>.</returns>
        private async Task<T> JsonParse<T>(string json, string node_path = "$.data") where T : new()
        {
            T list_data = new T();

            if (!string.IsNullOrEmpty(json))
            {
                using (JsonTextReader reader = new JsonTextReader(new StringReader(json)))
                {
                    var parsed_json = await JObject.LoadAsync(reader);

                    if (parsed_json.ContainsKey("status") && string.Equals(parsed_json["status"].ToObject<string>(), "error", StringComparison.CurrentCultureIgnoreCase))
                    {
                        throw new Exception($"Bad json response with err-code '{parsed_json["err-code"].ToObject<string>()}' and err-msg '{parsed_json["err-msg"].ToObject<string>()}'");
                    }
                    else if (parsed_json.ContainsKey("code") && parsed_json["code"].ToObject<int>() != 200)
                    {
                        throw new Exception($"Bad json response with err-code '{parsed_json["code"].ToObject<int>()}' and message '{parsed_json["message"].ToObject<string>()}'");
                    }

                    var json_object = parsed_json.SelectToken(node_path);
                    if (json_object == null)
                    {
                        throw new KeyNotFoundException($"JsonPATH '{node_path}' return a null value");
                    }
                    else
                    {
                        list_data = json_object.ToObject<T>();
                    }
                }
            }

            return list_data;
        }

        /// <summary>
        /// Returns all Huobi's supported trading symbol.
        /// Implements end point https://api.huobi.pro/v1/common/symbols
        /// </summary>
        /// <returns>List of TradingSymbol objects representing all Huobi's supported trading symbol. </returns>
        public async Task<List<TradingSymbol>> GetAllSymbols()
        {
            string json = await HttpGetRequest(FormatUri(url_common + "symbols", null, null, read_access_key, read_secret_key));
            return await JsonParse<List<TradingSymbol>>(json);
        }

        /// <summary>
        /// Returns all Huobi's supported trading currencies.
        /// Implements end point https://api.huobi.pro/v1/common/currencys
        /// </summary>
        /// <returns>List of string representing all Huobi's supported trading currencies. </returns>
        public async Task<List<string>> GetAllCurrencies()
        {
            string json = await HttpGetRequest(FormatUri(url_common + "currencys", null, null, read_access_key, read_secret_key));
            return await JsonParse<List<string>>(json);
        }

        /// <summary>
        /// Query static reference information for each currency, as well as its corresponding chain(s).
        /// Implements end point https://api.huobi.pro/v2/reference/currencies
        /// </summary>
        /// <param name="ccy">Available currencies in Huobi Global: btc, ltc, bch, eth, etc ...</param>
        /// <param name="authorizedUser">true or false (if not filled, default value is true)</param>
        /// <returns>List of CcyReferenceInfo objects</returns>
        public async Task<List<CcyReferenceInfo>> GetCcyReferenceInfo(string ccy = null, bool? authorizedUser = null)
        {
            SortedDictionary<string, string> parameters = new SortedDictionary<string, string>();
            if (ccy != null) parameters.Add("currency", ccy);
            if (authorizedUser != null) parameters.Add("authorizedUser", authorizedUser.ToString().ToLower());

            string json = await HttpGetRequest(FormatUri(url_reference + "currencies", HttpMethod.Get, parameters, read_access_key, read_secret_key));
            var res = await JsonParse<List<CcyReferenceInfo>>(json);

            return res;
        }

        /// <summary>
        /// Retrieves the current timestamp, i.e. the number of milliseconds that have elapsed since 00:00:00 UTC on 1 January 1970.
        /// Implements end point https://api.huobi.pro/v1/common/timestamp
        /// </summary>
        /// <returns>The timestamp as int64</returns>
        public async Task<Int64?> GetTimestamp()
        {
            string json = await HttpGetRequest(FormatUri(url_common + "timestamp", null, null, read_access_key, read_secret_key));
            return await JsonParse<Int64?>(json);
        }

        /// <summary>
        /// Retrieves all klines in a specific range.
        /// Implements end point https://api.huobi.pro/market/history/kline
        /// </summary>
        /// <param name="symbol">The trading symbol to query</param>
        /// <param name="period">The period of each candle</param>
        /// <param name="size">The number of data returns. It should be in the range [1, 2000]</param>
        /// <returns>List of Ticker objects</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when size is not in range [1, 2000]</exception>
        /// <exception cref="System.ArgumentException">Thrown when symbol is empty or null</exception>
        public async Task<List<Ticker>> GetTickerHistory(string symbol, TickerPeriod period, int? size)
        {
            if (size != null && (size < 1 || size > 2000))
                throw new ArgumentOutOfRangeException("size", "size parameter should be in [1, 2000] range");

            if (string.IsNullOrEmpty(symbol))
                throw new ArgumentException("symbol parameter is required", "symbol");

            SortedDictionary<string, string> parameters = new SortedDictionary<string, string>() { { "symbol", symbol }, { "period", period.ToParamValue() } };
            if (size != null) parameters.Add("size", size.ToString());

            string json = await HttpGetRequest(FormatUri(url_market_data + "history/kline", HttpMethod.Get, parameters, read_access_key, read_secret_key));
            var res = JsonParse<List<Ticker>>(json);

            return await res;
        }

        /// <summary>
        /// Retrieves the latest ticker with some important 24h aggregated market data.
        /// Implements end point https://api.huobi.pro/market/detail/merged
        /// </summary>
        /// <param name="symbol">The trading symbol to query</param>
        /// <returns>TickerAggregated object</returns>
        /// <exception cref="System.ArgumentException">Thrown when symbol is empty or null</exception>
        public async Task<TickerAggregated> GetLatestAggregatedTicker(string symbol)
        {
            if (string.IsNullOrEmpty(symbol))
                throw new ArgumentException("symbol parameter is required", "symbol");

            SortedDictionary<string, string> parameters = new SortedDictionary<string, string>() { { "symbol", symbol } };

            string json = await HttpGetRequest(FormatUri(url_market_data + "detail/merged", HttpMethod.Get, parameters, read_access_key, read_secret_key));
            var res = JsonParse<TickerAggregated>(json, "$.tick");

            return await res;
        }

        /// <summary>
        /// Retrieves the latest tickers for all supported pairs.
        /// Implements end point https://api.huobi.pro/market/tickers"
        /// </summary>
        /// <returns>List of Ticker objects</returns>
        public async Task<List<Ticker>> GetAllPairsLatestTickers()
        {
            string json = await HttpGetRequest(FormatUri(url_market_data + "tickers", HttpMethod.Get, null, read_access_key, read_secret_key));
            var res = JsonParse<List<Ticker>>(json);

            return await res;
        }

        /// <summary>
        /// Retrieves the current order book of a specific pair.
        /// Implements end point https://api.huobi.pro/market/depth
        /// </summary>
        /// <param name="symbol">The trading symbol to query</param>
        /// <param name="level">Market depth aggregation level.
        /// <para>step0: No market depth aggregation</para>
        /// <para>step1: Aggregation level = precision*10</para>
        /// <para>step2: Aggregation level = precision*100</para>
        /// <para>step3: Aggregation level = precision*1000</para>
        /// <para>step4: Aggregation level = precision*10000</para>
        /// <para>step5: Aggregation level = precision*100000</para>
        /// </param>
        /// <param name="depth">The number of market depth to return on each side</param>
        /// <returns>MarketDepth object</returns>
        /// <exception cref="System.ArgumentException">Thrown when symbol is empty or null</exception>
        public async Task<MarketDepth> GetMarketDepth(string symbol, AggregationLevel level, Depth? depth = null)
        {
            if (string.IsNullOrEmpty(symbol))
                throw new ArgumentException("symbol parameter is required", "symbol");

            SortedDictionary<string, string> parameters = new SortedDictionary<string, string>() { { "symbol", symbol }, { "type", level.ToParamValue() } };
            if (depth != null) parameters.Add("depth", depth.ToParamValue());

            string json = await HttpGetRequest(FormatUri(url_market_data + "depth", HttpMethod.Get, parameters, read_access_key, read_secret_key));
            var res = JsonParse<MarketDepth>(json, "$.tick");

            return await res;
        }

        /// <summary>
        /// Retrieves the latest trade with its price, volume, and direction.
        /// Implements end point https://api.huobi.pro/market/trade
        /// </summary>
        /// <param name="symbol">The trading symbol to query</param>
        /// <returns>Trade object</returns>
        /// /// <exception cref="System.ArgumentException">Thrown when symbol is empty or null</exception>
        public async Task<TradesAggregated> GetLatestTrade(string symbol)
        {
            if (string.IsNullOrEmpty(symbol))
                throw new ArgumentException("symbol parameter is required", "symbol");

            SortedDictionary<string, string> parameters = new SortedDictionary<string, string>() { { "symbol", symbol } };

            string json = await HttpGetRequest(FormatUri(url_market_data + "trade", HttpMethod.Get, parameters, read_access_key, read_secret_key));
            var res = JsonParse<TradesAggregated>(json, "$.tick");

            return await res;
        }

        /// <summary>
        /// Retrieves the most recent trades with their price, volume, and direction.
        /// Implements end point https://api.huobi.pro/market/history/trade
        /// </summary>
        /// <param name="symbol">The trading symbol to query</param>
        /// <param name="size">The number of data returns. It should be in the range [1, 2000]</param>
        /// <returns>List of Ticker objects</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when size is not in range [1, 2000]</exception>
        /// <exception cref="System.ArgumentException">Thrown when symbol is empty or null</exception>
        public async Task<List<TradesAggregated>> GetMostRecentTrades(string symbol, int? size)
        {
            if (size != null && (size < 1 || size > 2000))
                throw new ArgumentOutOfRangeException("size", "size parameter should be in [1, 2000] range");

            if (string.IsNullOrEmpty(symbol))
                throw new ArgumentException("symbol parameter is required", "symbol");

            SortedDictionary<string, string> parameters = new SortedDictionary<string, string>() { { "symbol", symbol } };
            if (size != null) parameters.Add("size", size.ToString());

            string json = await HttpGetRequest(FormatUri(url_market_data + "history/trade", HttpMethod.Get, parameters, read_access_key, read_secret_key));
            var res = JsonParse<List<TradesAggregated>>(json, "$.data");

            return await res;
        }

        /// <summary>
        /// Retrieves the summary of trading in the market for the last 24 hours.
        /// Implements end point https://api.huobi.pro/market/detail/
        /// </summary>
        /// <param name="symbol">The trading symbol to query</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException">Thrown when symbol is empty or null</exception>
        public async Task<Ticker> GetLastMarketSummary(string symbol)
        {
            if (string.IsNullOrEmpty(symbol))
                throw new ArgumentException("symbol parameter is required", "symbol");

            SortedDictionary<string, string> parameters = new SortedDictionary<string, string>() { { "symbol", symbol } };

            string json = await HttpGetRequest(FormatUri(url_market_data + "detail", HttpMethod.Get, parameters, read_access_key, read_secret_key));
            var res = JsonParse<Ticker>(json, "$.tick");

            return await res;
        }

    }
    #endregion
}
