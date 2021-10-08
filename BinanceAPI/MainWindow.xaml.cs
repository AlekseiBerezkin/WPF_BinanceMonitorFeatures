using Binance.Net;
using Binance.Net.Objects;
using Binance.Net.Objects.Spot.MarketData;
using BinanceAPI.Model;
using BinanceAPI.Provider;
using CryptoExchange.Net.Authentication;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
        private SolidColorBrush more = new SolidColorBrush(Colors.Green);
        private SolidColorBrush smaller = new SolidColorBrush(Colors.Red);
        ObservableCollection<DataBinanceView> dataForTable = new ObservableCollection<DataBinanceView>();
        List<string> userData = FileProvider.ReadFile();
        Timer timer;
        //Timer timerReload;
        int indexReload = 0;
        List<DataBinance> baseDataBinance = new List<DataBinance>();
        BinanceSocketClient socketClient = new BinanceSocketClient();


        public MainWindow()
        {
            InitializeComponent();
            
        }
        private async void updateBaseDataBinance()
        {


            List<BinancePrice> dataBinance = BinanceProvider.getDataBinance();

            foreach (DataBinanceView udb in dataForTable)
            {
                var updateSymbol = dataBinance.FirstOrDefault(p => p.Symbol == udb.symbol);
                if(updateSymbol!=null)
                {
                    udb.link = BinanceProvider.getLink(updateSymbol.Symbol);
                    udb.StartPrice = updateSymbol.Price;
                }
                
            }
        }
        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            updateBaseDataBinance();

            //renderTable();
            await StartDataStream();
            
            string treload ;
            if(cbReloadData.IsChecked.Value)
            {
                indexReload = getTimeReloadPeriod();
                //timerReload = new Timer(tm_reload, 0, timeReload, timeReload);
                 treload = " c перезагрузкой";
                int TimerPeriod;
                TimerCallback tm = new TimerCallback(updateData);
                //TimerCallback tm_reload = new TimerCallback(updateData_reload);
                TimerPeriod = int.Parse(ttime_restart.Text) * 60000;
                timer = new Timer(tm, 0, 0, TimerPeriod);
            }
            else
            {
                //flagReload = false;
                treload = " без перезагрузки";
            }

            cbReloadData.IsEnabled = false;
            Start.IsEnabled = false;
            Stop.IsEnabled = true;
            //btnChangePeriod.IsEnabled = true;
            Add.IsEnabled = false;
            btndelete.IsEnabled = false;
            ttime_restart.IsEnabled = false;

            list.Items.Add(DateTime.Now.ToString("HH:mm")+ $" Старт" + treload); 
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

            //timer.Change(Timeout.Infinite, Timeout.Infinite);
            // if(timerReload!=null)
            //timerReload.Change(Timeout.Infinite, Timeout.Infinite);
            socketClient.UnsubscribeAllAsync();
            userData = FileProvider.ReadFile();

            Start.IsEnabled = true;
            Stop.IsEnabled = false;
            //btnChangePeriod.IsEnabled = false;
            Add.IsEnabled = true;
            btndelete.IsEnabled = true;
            cbReloadData.IsEnabled = true;
            ttime_restart.IsEnabled = true;
            list.Items.Add(DateTime.Now.ToString("HH:mm") + $" Остановка обновления");
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            foreach (string s in userData)
            {
                dataForTable.Add(new DataBinanceView { symbol = s, percent = 0 });
            }
            updateBaseDataBinance();

            //printTime();
            
            List<string> ListName = BinanceProvider.CurName();
            foreach(string s in ListName)
            {
                cbPair.Items.Add(s);
            }


           
            Table.ItemsSource = dataForTable;
           // updatecbdelete();
            //Stop.IsEnabled = false;
            //btnChangePeriod.IsEnabled = false;
            ttime_restart.IsEnabled = true;
            //dataForTable.CollectionChanged+= Users_CollectionChanged;
           // test();

            if(Properties.Settings.Default.API=="" || Properties.Settings.Default.Secret == "")
            {
                MessageBox.Show("API ключ или SecretKey отсутствует");
            }

            //test();

        }

        public async Task test()
        {
            var client = new BinanceClient(new BinanceClientOptions()
        {
            // Specify options for the client
        });
            var callResult = await client.Spot.Market.GetPricesAsync();

            var btc = callResult.Data.Where(p => Regex.Match(p.Symbol, @"(.{3})\s*$").ToString() == "BTC") ;
        }

    
    private void Add_Click(object sender, RoutedEventArgs e)
        {
            string NameCur = cbPair.Text;
            if(NameCur.Any())
            {
                
                int res=FileProvider.WriteFile(NameCur);

                if (res == -1)
                { MessageBox.Show("Валюта уже добавлена"); return; }
                
                dataForTable.Add(new DataBinanceView { symbol = NameCur, percent = 0,link=BinanceProvider.getLink(NameCur) });
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
            BinanceClient.SetDefaultOptions(new BinanceClientOptions()
            {
                ApiCredentials = new ApiCredentials(Properties.Settings.Default.API, Properties.Settings.Default.Secret)
                //LogLevel = LogLevel.Debug,
                //LogWriters = new List<ILogger> { new ConsoleLogger() }
            });
            BinanceSocketClient.SetDefaultOptions(new BinanceSocketClientOptions()
            {
                ApiCredentials = new ApiCredentials(Properties.Settings.Default.API, Properties.Settings.Default.Secret)
                //LogLevel = LogLevel.Debug,
                //LogWriters = new List<ILogger> { new ConsoleLogger() }
            });

            /*using (var client = new BinanceClient())
            {
                await client.FuturesUsdt.UserStream.StartUserStreamAsync();
            }*/



            await socketClient.Spot.SubscribeToAllSymbolTickerUpdatesAsync(data =>
            {

                for(int i=0;i<dataForTable.Count;i++)
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
                        } ); 
                        
                        //dataForTable[i].percent = 
                    }
                }
            });
            }
    

            private void updateData(object data)
        {

            updateBaseDataBinance();
            Dispatcher.Invoke(() => list.Items.Add(DateTime.Now.ToString("HH:mm")+" изменение стартового значения"));
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
        private void btnChangePeriod_Click(object sender, RoutedEventArgs e)
        {
            //int timeUpdate = getTimePeriod();
            //timer.Change(0, getTimePeriod());
            //list.Items.Add($"Время таймера изменено на {timeUpdate/1000}c");
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
            socketClient.UnsubscribeAllAsync();
            
        }
    }


}

