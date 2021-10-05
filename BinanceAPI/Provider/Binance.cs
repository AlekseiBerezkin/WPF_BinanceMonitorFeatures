using Binance.Net;
using Binance.Net.Objects;
using BinanceAPI.Model;
using CryptoExchange.Net.Authentication;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Threading;

namespace BinanceAPI.Provider
{
     class BinanceProvider
    {
        
        

        static string baseLinkBinance = "https://www.binance.com/ru/trade/";

        public static List<DataBinance> getDataBinance()
        {
            RequestDataBinance();
            return DB;
        }

        /*public  Binance()
        {
            RequestDataBinance();
        }*/
        static List<DataBinance> DB = new List<DataBinance>();
        private static void RequestDataBinance()
        {
            DB.Clear();
            string request = $"https://www.binance.com/fapi/v1/ticker/24hr";

            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(request);
            try
            {

                string response;
                HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();

                using (StreamReader streamReader = new StreamReader(httpWebResponse.GetResponseStream()))
                {
                    response = streamReader.ReadToEnd();
                }

                DB = JsonConvert.DeserializeObject<List<DataBinance>>(response);

            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public static string getLink(string name)
            {
            string currency = name.Replace("USDT", "");
            return baseLinkBinance + currency + "_USDT";
            }

        public static List<string> CurName()
        {
            RequestDataBinance();
            List<string> ListName=new List<string>();
            foreach(DataBinance db in DB)
            {
                ListName.Add(db.symbol);
            }
            return ListName;
        }
    }
}
