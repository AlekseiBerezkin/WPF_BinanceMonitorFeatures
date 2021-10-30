using Binance.Net;
using Binance.Net.Objects;
using Binance.Net.Objects.Spot.MarketData;
using BinanceAPI.Model;
using BinanceAPI.Provider;
using CryptoExchange.Net.Authentication;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


namespace BinanceAPI
{
    class percentToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Все проверки для краткости выкинул
            return (decimal)value > 0 ?
                new SolidColorBrush(Colors.Green)
                : new SolidColorBrush(Colors.Red);
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new Exception("The method or operation is not implemented.");
        }
    }
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Logger logger = LogManager.GetCurrentClassLogger();
        private SolidColorBrush more = new SolidColorBrush(Colors.Green);
        private SolidColorBrush smaller = new SolidColorBrush(Colors.Red);
         ObservableCollection<DataBinanceView> dataForTable = new ObservableCollection<DataBinanceView>();
        List<string> userData = FileProvider.ReadFile();
        List<DataBinanceView> dataBaseAlert = new List<DataBinanceView>();
        Timer timer;
        Timer timerAlert;

        int indexReload = 0;
        BinanceSocketClient socketClient = new BinanceSocketClient();
        int stateAlert = -1;
        List<string> alertSended = new List<string>();
        int countAlert = 0;
        int maxCountAlert = 0;


        public MainWindow()
        {
            InitializeComponent();
            
        }


        private async void updateBaseDataBinance()
        {
            try
            {
                List<BinancePrice> dataBinance = BinanceProvider.getDataBinance();

                foreach (DataBinanceView udb in dataForTable)
                {
                    var updateSymbol = dataBinance.FirstOrDefault(p => p.Symbol == udb.symbol);
                    if (updateSymbol != null)
                    {
                        udb.link = BinanceProvider.getLink(updateSymbol.Symbol);
                        udb.StartPrice = updateSymbol.Price;
                    }

                }
            }
            catch(Exception ex)
            {
                logger.Error(ex);
                MessageBox.Show("Ошибка при обновлении базовых данных"+ex,"ОШИБКА",MessageBoxButton.OK,MessageBoxImage.Error);
            }

        }


        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            logger.Info("Кнопка старт");
            try
            {
                int TimerPeriod = 0;
                string treload;
                if (cbReloadData.IsChecked.Value)
                {
                    indexReload = getTimeReloadPeriod();
                    treload = " c перезагрузкой";

                    Properties.Settings.Default.TimeReload = int.Parse(ttime_restart.Text);
                    Properties.Settings.Default.Save();

                    TimerCallback tm = new TimerCallback(updateData);
                    TimerPeriod = int.Parse(ttime_restart.Text) * 60000;
                    timer = new Timer(tm, 0, TimerPeriod, TimerPeriod);
                }
                else
                {
                    treload = " без перезагрузки";
                }

                if (cbAlert.IsChecked.Value)
                {
                    int timeAlert = Properties.Settings.Default.IntervalTime * 60000;
                    if (timeAlert == 0 || (timeAlert == TimerPeriod && cbReloadData.IsChecked.Value))
                    {
                        stateAlert = 0;
                        list.Items.Add($"Оповещение на {Properties.Settings.Default.ChangePercent}% без интервала времени");
                        dataBaseAlert = dataForTable.ToList();
                        TimerCallback tm = new TimerCallback(checkAlert);
                        timerAlert = new Timer(tm, 0, 5000, 5000);
                    }
                    else if (timeAlert > 0 && timeAlert < TimerPeriod && cbReloadData.IsChecked.Value)
                    {
                        maxCountAlert = timeAlert / 5000;
                        countAlert = 0;
                        stateAlert = 1;
                        list.Items.Add($"Оповещение на {Properties.Settings.Default.ChangePercent}% в отрезке времени {Properties.Settings.Default.IntervalTime} мин");
                        dataBaseAlert = dataForTable.ToList();
                        TimerCallback tm = new TimerCallback(checkAlert);
                        timerAlert = new Timer(tm, 0, 5000, 5000);
                    }
                    else if (timeAlert > TimerPeriod)
                    {

                        MessageBox.Show("Время оповещения не может быть больше времени перезагрузки");
                        return;
                    }
                    else
                    {
                        MessageBox.Show("Оповещение не может быть установлено,т.к не установлена перезагрузка");
                        return;
                    }


                }
                else
                {
                    list.Items.Add("Запуск без оповещения  в Telegram");
                }

                list.Items.Add("Получение начальных значений");
                updateBaseDataBinance();


                await StartDataStream();

                cbReloadData.IsEnabled = false;
                Start.IsEnabled = false;
                Stop.IsEnabled = true;
                //btnChangePeriod.IsEnabled = true;
                Add.IsEnabled = false;
                btndelete.IsEnabled = false;
                ttime_restart.IsEnabled = false;
                cbAlert.IsEnabled = false;
                btnSetingsAlert.IsEnabled = false;

                list.Items.Add(DateTime.Now.ToString("HH:mm") + $" Старт" + treload);
            }
            catch(Exception ex)
            {
                logger.Error(ex);
                MessageBox.Show("Ошибка при старте " + ex, "ОШИБКА", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            
        }

        private void updatecbdelete()
        {
            cbdelete.Items.Clear();
            foreach (DataBinanceView s in dataForTable)
            {
                cbdelete.Items.Add(s.symbol);
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                if (timer != null)
                    timer.Change(Timeout.Infinite, Timeout.Infinite);
                if (timerAlert != null)
                    timerAlert.Change(Timeout.Infinite, Timeout.Infinite);


                dataBaseAlert.Clear();
                alertSended.Clear();
                socketClient.UnsubscribeAllAsync();
                userData = FileProvider.ReadFile();
                stateAlert = -1;
                Start.IsEnabled = true;
                Stop.IsEnabled = false;
                //btnChangePeriod.IsEnabled = false;
                Add.IsEnabled = true;
                btndelete.IsEnabled = true;
                cbReloadData.IsEnabled = true;
                ttime_restart.IsEnabled = true;
                cbAlert.IsEnabled = true;
                btnSetingsAlert.IsEnabled = true;
                list.Items.Add(DateTime.Now.ToString("HH:mm") + $" Остановка обновления");
            }
            catch(Exception ex)
            {
                logger.Error(ex);
                MessageBox.Show("Ошибка при остановке " + ex, "ОШИБКА", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            
        }

        private void LogParamStart()
        {
            bool flagRestart=cbReloadData.IsChecked.Value;
            bool flagAlert = cbAlert.IsChecked.Value;
            string timeRestart= Properties.Settings.Default.TimeReload.ToString();
            string timeAlert= Properties.Settings.Default.IntervalTime.ToString();
            string tresholdAlert = Properties.Settings.Default.ChangePercent.ToString();

            logger.Info($"__________________________________________________________________________");
            logger.Info($"Старт таймера с параметрами:\n" +
                $"Флаг перезагрузки:{flagRestart}\n" +
                $"Флаг оповещения:{flagAlert}\n" +
                $"Время рестарта:{timeRestart}\n" +
                $"Время оповещения:{timeAlert}\n" +
                $"Порог оповещения:{tresholdAlert}");
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LogParamStart();
            try
            {
                foreach (string s in userData)
                {
                    dataForTable.Add(new DataBinanceView { symbol = s, percent = 0 });
                }
                updateBaseDataBinance();
                List<string> ListName = BinanceProvider.CurName();
                foreach (string s in ListName)
                {
                    cbPair.Items.Add(s);
                }
                Table.ItemsSource = dataForTable;
                updatecbdelete();
                Stop.IsEnabled = false;
                ttime_restart.IsEnabled = true;

                ttime_restart.Text = Properties.Settings.Default.TimeReload.ToString();

                if (Properties.Settings.Default.API == "")
                {
                    MessageBox.Show("API ключ отсутствует");
                }
            }
            catch(Exception ex)
            {
                logger.Error(ex);
                MessageBox.Show("Ошибка при загрузке программы " + ex, "ОШИБКА", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            
        }

    
    private void Add_Click(object sender, RoutedEventArgs e)
        {
            string NameCur = cbPair.Text;
            if(NameCur.Any())
            {
                
                int res=FileProvider.WriteFile(NameCur);

                if (res == -1)
                { MessageBox.Show("Валюта уже добавлена"); return; }
                
                dataForTable.Add(new DataBinanceView { symbol = NameCur, percent = 0 });
                updatecbdelete();
                list.Items.Add(DateTime.Now.ToString("HH:mm") + $" Пара {NameCur} добавлена в таблицу");
            }
            else
            {
                MessageBox.Show("Выберите валюту");
            }
        }


        public async Task StartDataStream()
        {
            logger.Info("Старт стрима");
            try
            {
                BinanceClient.SetDefaultOptions(new BinanceClientOptions()
                {
                    ApiCredentials = new ApiCredentials(Properties.Settings.Default.API, Properties.Settings.Default.API)
                    //LogLevel = LogLevel.Debug,
                    //LogWriters = new List<ILogger> { new ConsoleLogger() }
                });
                BinanceSocketClient.SetDefaultOptions(new BinanceSocketClientOptions()
                {
                    ApiCredentials = new ApiCredentials(Properties.Settings.Default.API, Properties.Settings.Default.API)
                    //LogLevel = LogLevel.Debug,
                    //LogWriters = new List<ILogger> { new ConsoleLogger() }
                });

                using (var client = new BinanceClient())
                {
                    await client.FuturesUsdt.UserStream.StartUserStreamAsync();
                }



                await socketClient.FuturesUsdt.SubscribeToAllSymbolTickerUpdatesAsync(data =>
                {

                    for (int i = 0; i < dataForTable.Count; i++)
                    {
                        var updateSymbol = data.Data.FirstOrDefault(d => d.Symbol == dataForTable[i].symbol);
                        if (updateSymbol != null)
                        {
                            Dispatcher.Invoke(() => {
                                dataForTable[i] = new DataBinanceView
                                {
                                    symbol = updateSymbol.Symbol,
                                    percent = Math.Round(((updateSymbol.LastPrice - dataForTable[i].StartPrice) / dataForTable[i].StartPrice) * 100, 3),
                                    link = dataForTable[i].link,
                                    StartPrice = dataForTable[i].StartPrice
                                };
                            });

                            //dataForTable[i].percent = 
                        }
                    }
                });
            }
            catch(Exception ex)
            {
                logger.Error(ex);
                MessageBox.Show("Ошибка при старте стрима " + ex, "ОШИБКА", MessageBoxButton.OK, MessageBoxImage.Error);
            }
           
            }


        private async void checkAlert(object data)
        {
            try
            {
                TelegaBot tb = new TelegaBot();
                decimal ThresholdPersent = Properties.Settings.Default.ChangePercent;

                for (int i = 0; i < dataForTable.Count; i++)
                {
                    decimal percent = dataForTable[i].percent - dataBaseAlert[i].percent;
                    if (percent >= ThresholdPersent)
                    {
                        if (!alertSended.Contains(dataForTable[i].symbol))
                        {
                            await tb.sendAlert(dataForTable[i].symbol, dataForTable[i].link, percent);
                            alertSended.Add(dataForTable[i].symbol);
                        }
                    }
                }

                switch (stateAlert)
                {
                    case 0:

                        break;
                    case 1:
                        if (countAlert == maxCountAlert)
                        {
                            timerAlert.Change(Timeout.Infinite, Timeout.Infinite);
                            Dispatcher.Invoke(() =>
                            {
                                list.Items.Add("Стоп таймера оповещения");
                            });
                        }
                        else
                        {
                            countAlert++;
                        }

                        break;
                }
            }
            catch(Exception ex)
            {
                logger.Error(ex);
                MessageBox.Show("Ошибка при проверке алерта " + ex, "ОШИБКА", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }
        private void updateData(object data)
        {
            try
            {
                updateBaseDataBinance();
                Dispatcher.Invoke(() => {
                    list.Items.Add(DateTime.Now.ToString("HH:mm") + " изменение стартового значения");
                    if (cbAlert.IsChecked.Value)
                    {
                        dataBaseAlert = dataForTable.ToList();
                        alertSended.Clear();
                        list.Items.Add(DateTime.Now.ToString("HH:mm") + " перезапись данных для оповещения");
                        if (stateAlert == 1)
                        {
                            list.Items.Add("Старт таймера оповещения");
                            timerAlert.Change(5000, 5000);
                            countAlert = 0;
                        }
                    }
                });
            }
            catch(Exception ex)
            {
                logger.Error(ex);
                MessageBox.Show("Ошибка при обновлении данных " + ex, "ОШИБКА", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        private void NumericOnly(System.Object sender,TextCompositionEventArgs e)
        {
            e.Handled = IsTextNumeric(e.Text);
        }
        private static bool IsTextNumeric(string str)
        {
            System.Text.RegularExpressions.Regex reg = new System.Text.RegularExpressions.Regex("[^0-9]");
            return reg.IsMatch(str);
        }

        private void DG_Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Hyperlink link = (Hyperlink)e.OriginalSource;
                
                Process.Start(new ProcessStartInfo { FileName = link.NavigateUri.ToString(), UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
        private int getTimeReloadPeriod()
        {
            return int.Parse(ttime_restart.Text);
        }

        private void btndelete_Click(object sender, RoutedEventArgs e)
        {
            string NameDel = cbdelete.Text;
            if (!NameDel.Any())
            {
                MessageBox.Show("Выберите валюту");
                return;
            }

            FileProvider.deleteFromFile(NameDel);
            for(int i=0;i< dataForTable.Count;i++)
            {
                if(dataForTable[i].symbol.Contains(NameDel))
                {
                    dataForTable.RemoveAt(i);
                }
            }
            list.Items.Add(DateTime.Now.ToString("HH:mm") + $" Пара {NameDel} удалена из таблицы");
            updatecbdelete();
        }

        private void Table_LoadingRow(object sender, DataGridRowEventArgs e)
        {

        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            new Settings().ShowDialog();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            logger.Info("Программа закрыта");
            socketClient.UnsubscribeAllAsync();
            
        }

        private void btnSetingsAlert_Click(object sender, RoutedEventArgs e)
        {
            logger.Info("Открытие окна оповещения");
            new SettingsAlert().ShowDialog();
        }
    }


}

