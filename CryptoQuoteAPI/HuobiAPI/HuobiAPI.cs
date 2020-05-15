using System;

namespace CryptoQuote.HuobiAPI
{
    /// <summary>
    /// Abstract class serving as a base class for Rest and WebSocket API.
    /// It contains all the necessary constants to communicate with Huobi servers 
    /// </summary>
    public abstract class HuobiAPI
    {
        #region constant configurations
        protected const string SignatureMethod = "HmacSHA256";
        protected const string SignatureVersion = "2";

        // Choose the url with the lowest latency
        // {0} = 'https' or 'wss' depending on the protocol we want to use
        // {1} = nothing or '/ws' depending on the protocol we want to use
        private const string url1 = @"{0}://api.huobi.pro{1}";
        private const string url2 = @"{0}://api-aws.huobi.pro{1}";

        // API url's paths
        protected const string url_common = @"/v1/common/";
        protected const string url_market_data = @"/market/";
        protected const string url_reference = @"/v2/reference/";
        #endregion

        #region variables section
        // Secret keys
        protected readonly string read_access_key;
        protected readonly string read_secret_key;

        protected string global_uri;

        private bool _UseAwsUri;

        /// <value>Define if Huobi's aws server should be used.</value>
        public bool UseAwsUri
        {
            get => _UseAwsUri;
            set
            {
                _UseAwsUri = value;
                set_global_uri();
            }
        }
        #endregion

        /// <summary>
        /// Initialize the API Object.
        /// </summary>
        /// <param name="UseAws">Specify whether Huobi's aws server should be used.</param>
        /// <param name="read_key">Personal access key generated in Huobi API management.</param>
        /// <param name="read_secret">Personal secret key generated in Huobi API management.</param>
        public HuobiAPI(bool UseAws = false, string read_key = null, string read_secret = null)
        {
            UseAwsUri = UseAws;
            read_access_key = read_key;
            read_secret_key = read_secret;
        }

        private void set_global_uri()
        {
            global_uri = string.Format((UseAwsUri ? url2 : url1), "https", string.Empty);
        }
    }
}
