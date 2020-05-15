using System;

namespace CryptoQuote.HuobiAPI.DataObjectsModel
{
    public enum TickerPeriod
    {
        Min1,
        Min5,
        Min15,
        Min30,
        Min60,
        Hour4,
        Day1,
        Mon1,
        Week1,
        Year1
    }

    public static class DataObjectsModelEnumExtensions
    {
        public static string ToParamValue(this TickerPeriod period)
        {
            switch (period)
            {
                case TickerPeriod.Min1:
                    return "1min";
                case TickerPeriod.Min5:
                    return "5min";
                case TickerPeriod.Min15:
                    return "15min";
                case TickerPeriod.Min30:
                    return "30min";
                case TickerPeriod.Min60:
                    return "60min";
                case TickerPeriod.Hour4:
                    return "4hour";
                case TickerPeriod.Day1:
                    return "1day";
                case TickerPeriod.Mon1:
                    return "1mon";
                case TickerPeriod.Week1:
                    return "1week";
                case TickerPeriod.Year1:
                    return "1year";

                default:
                    return period.ToString();
            }
        }

        public static string ToParamValue(this AggregationLevel level)
        {
            return level.ToString().ToLower();
        }

        public static string ToParamValue(this Depth? depth)
        {
            return ((int)depth).ToString();
        }
    }

    public enum AggregationLevel
    {
        Step0,
        Step1,
        Step2,
        Step3,
        Step4,
        Step5
    }

    public enum Depth
    {
        Five = 5,
        Ten = 10,
        Twenty = 20,
        OneFifty = 150
    }
}
