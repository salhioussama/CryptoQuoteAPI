using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace CryptoQuote.HuobiAPI.DataObjectsModel
{
    /// <summary>
    /// Class to format data coming from GetAllSymbols request.
    /// </summary>
    [JsonObject(NamingStrategyType = typeof(Newtonsoft.Json.Serialization.CamelCaseNamingStrategy))]
    public class TradingSymbol
    {
        [JsonProperty("base-currency")]
        public string BaseCurrency { get; set; }

        [JsonProperty("quote-currency")]
        public string QuoteCurrency { get; set; }

        [JsonProperty("price-precision")]
        public int PricePrecision { get; set; }

        [JsonProperty("amount-precision")]
        public int AmountPrecision { get; set; }

        [JsonProperty("symbol-partition")]
        public string SymbolPartition { get; set; }

        public string Symbol { get; set; }
        public string State { get; set; }

        [JsonProperty("value-precision")]
        public int ValuePrecision { get; set; }

        [JsonProperty("min-order-amt")]
        public long MinOrderAmt { get; set; }

        [JsonProperty("max-order-amt")]
        public long MaxOrderAmt { get; set; }

        [JsonProperty("min-order-value")]
        public long MinOrderValue { get; set; }

        [JsonProperty("leverage-ratio")]
        public short? LeverageRatio { get; set; }
    }

    /// <summary>
    /// Class to format data coming from GetCcyReferenceInfo request.
    /// </summary>
    [JsonObject(NamingStrategyType = typeof(Newtonsoft.Json.Serialization.CamelCaseNamingStrategy))]
    public class CcyReferenceInfo
    {
        public string Currency { get; set; }
        public List<ChainReferenceInfo> Chains { get; set; }

        [JsonProperty("instStatus")]
        public string InstStatus { get; set; }
    }

    /// <summary>
    /// Definition of a chain info.
    /// </summary>
    public class ChainReferenceInfo
    {
        [JsonProperty("chain")]
        public string Chain { get; set; }

        [JsonProperty("depositStatus")]
        public string DepositStatus { get; set; }

        [JsonProperty("maxTransactFeeWithdraw")]
        public string MaxTransactFeeWithdraw { get; set; }

        [JsonProperty("maxWithdrawAmt")]
        public string MaxWithdrawAmt { get; set; }

        [JsonProperty("minDepositAmt")]
        public string MinDepositAmt { get; set; }

        [JsonProperty("minTransactFeeWithdraw")]
        public string MinTransactFeeWithdraw { get; set; }

        [JsonProperty("transactFeeWithdraw")]
        public string TransactFeeWithdraw { get; set; }

        [JsonProperty("transactFeeRateWithdraw")]
        public string TransactFeeRateWithdraw { get; set; }

        [JsonProperty("minWithdrawAmt")]
        public string MinWithdrawAmt { get; set; }

        [JsonProperty("numOfConfirmations")]
        public int NumOfConfirmations { get; set; }

        [JsonProperty("numOfFastConfirmations")]
        public int NumOfFastConfirmations { get; set; }

        [JsonProperty("withdrawFeeType")]
        public string WithdrawFeeType { get; set; }

        [JsonProperty("withdrawPrecision")]
        public int WithdrawPrecision { get; set; }

        [JsonProperty("withdrawQuotaPerDay")]
        public string WithdrawQuotaPerDay { get; set; }

        [JsonProperty("withdrawQuotaPerYear")]
        public string WithdrawQuotaPerYear { get; set; }

        [JsonProperty("withdrawQuotaTotal")]
        public string WithdrawQuotaTotal { get; set; }

        [JsonProperty("withdrawStatus")]
        public string WithdrawStatus { get; set; }
    }
}
