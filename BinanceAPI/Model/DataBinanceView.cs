using System;
using System.Collections.Generic;
using System.Text;

namespace BinanceAPI.Model
{
    class DataBinanceView
    {

        public string symbol { get; set; }
        public double percent { get; set; } = 0;
        public double StartPrice { get; set; }
        //public string lastPrice { get; set; }
        //public string highPrice { get; set; }
        // public string lowPrice { get; set; }
    }
}
