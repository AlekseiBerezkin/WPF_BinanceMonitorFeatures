using BinanceAPI.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace BinanceAPI.Provider
{
    static class  Binance
    {
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
