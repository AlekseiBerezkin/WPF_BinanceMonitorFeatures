using System;
using System.Collections.Generic;
using System.Text;

namespace BinanceAPI.Model
{
    class DataBinanceView
    {

        public string symbol { get; set; }
        public decimal percent { get; set; } = 0;
        public decimal StartPrice { get; set; }
        public string link { get; set; }

    }
}
