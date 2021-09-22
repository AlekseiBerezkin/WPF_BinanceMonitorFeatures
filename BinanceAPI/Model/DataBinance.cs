using System;
using System.Collections.Generic;
using System.Text;

namespace BinanceAPI.Model
{
    class DataBinance
    {

        public string symbol { get; set; }
        public string priceChangePercent { get; set; }
        public string lastPrice { get; set; }
        public string highPrice { get; set; }
        public string lowPrice { get; set; }
    }
}
