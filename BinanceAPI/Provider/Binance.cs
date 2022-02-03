using Binance.Net;
using Binance.Net.Objects;
using Binance.Net.Objects.Spot.MarketData;
using BinanceAPI.Model;
using CryptoExchange.Net.Authentication;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Threading;

namespace BinanceAPI.Provider
{
    class BinanceProvider
    {



        static string baseLinkBinance = "https://www.binance.com/ru/futures/";

        public static List<BinancePrice> getDataBinance()
        {
            RequestDataBinance();
            return DB;
        }

        /*public  Binance()
        {
            RequestDataBinance();
        }*/
        static List<BinancePrice> DB = new List<BinancePrice>();
        private static void RequestDataBinance()
        {
            DB.Clear();

            try
            {

                var client = new BinanceClient(new BinanceClientOptions());
                var callResult = client.FuturesUsdt.Market.GetPricesAsync();

                foreach (BinancePrice bp in callResult.Result.Data)
                {
                    DB.Add(bp);
                }

            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public static string getLink(string name)
        {
           
            string currency = name.Replace("0","");
            currency = currency.Replace("1", "");
            return baseLinkBinance + currency + "_perpetual";
        }

        public static List<string> CurName()
        {
            RequestDataBinance();
            List<string> ListName = new List<string>();
            foreach (BinancePrice bp in DB)
            {
                ListName.Add(bp.Symbol);
            }
            return ListName;
        }
    }
}
