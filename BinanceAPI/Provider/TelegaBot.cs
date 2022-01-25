using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;

namespace BinanceAPI.Provider
{
    class TelegaBot
    {
        static string botToken = Properties.Settings.Default.TGtoken;
        private TelegramBotClient Bot;

         public TelegaBot()
        {
            Bot= new TelegramBotClient(botToken);
        }
        public async Task sendAlert(string name,string link, decimal percent)
        {
            string []chatIdList = Properties.Settings.Default.ChatId.Split(',');

            foreach(string chatId in chatIdList)
            {
                await Bot.SendTextMessageAsync(chatId.ToString(), $"Актив {name} достиг {percent} %.\nСсылка:{link}");
            }
            
        }
    }
}
