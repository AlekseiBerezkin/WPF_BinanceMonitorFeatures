using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;

namespace BinanceAPI.Provider
{
    class TelegaBot
    {
        static string botToken = "2014832019:AAEiuyoqW16V8LSkgGi9h7PQM1-5dunas8w";
        private TelegramBotClient Bot;

         public TelegaBot()
        {
            Bot= new TelegramBotClient(botToken);
        }
        public async Task sendAlert(string name,string link, decimal percent)
        {
            await Bot.SendTextMessageAsync(Properties.Settings.Default.ChatId, $"Актив {name} увеличелся до {percent} процентов.\nСсылка:{link}");
        }
    }
}
