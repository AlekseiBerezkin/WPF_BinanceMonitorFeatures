
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ButtonBinance.Models
{
    public class TimerControl
    {
        // устанавливаем метод обратного вызова
        static TimerCallback tm = new TimerCallback(updateData);
        // создаем таймер
        static Timer timer;

        static public void TimerStart()
        {
            timer = new Timer(tm, 0, 0, 2000);
        }

        private static async void updateData(object obj)
        {

        }
    }
}
