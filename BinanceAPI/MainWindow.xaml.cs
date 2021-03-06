using BinanceAPI.Model;
using BinanceAPI.Provider;
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
            return (double)value > 0 ?
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
        bool flagStart = false;
        bool flagReload = false;
        Timer timer;
        //Timer timerReload;
        int indexReload = 0;
        List<DataBinance> baseDataBinance = new List<DataBinance>();



        public MainWindow()
        {
            InitializeComponent();
            
        }
        private void updateBaseDataBinance()
        {
            baseDataBinance.Clear();
            List<DataBinance> updateData=new List<DataBinance>(); 
            try
            {
                updateData = Binance.getDataBinance();
            }
            catch(Exception ex)
            {
                list.Items.Add($"ОШИБКА:{ex}");
                return;
            }

            foreach(DataBinanceView udb in dataForTable)
            {
                updateData.ForEach(f => { 
                    if (f.symbol==udb.symbol) 
                    {
                        baseDataBinance.Add(new DataBinance { symbol = udb.symbol, lastPrice = f.lastPrice });
                    } });
            }
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            updateBaseDataBinance();

            //renderTable();

            
            TimerCallback tm = new TimerCallback(updateData);
            //TimerCallback tm_reload = new TimerCallback(updateData_reload);
            //TimerPeriod = int.Parse(ttime.Text)*1000;
            timer = new Timer(tm, 0, 0, getTimePeriod());
            string treload ;
            if(cbReloadData.IsChecked.Value)
            {
                indexReload = getTimeReloadPeriod();
                //timerReload = new Timer(tm_reload, 0, timeReload, timeReload);
                 treload = "С перезагрузкой";
                flagReload = true;
            }
            else
            {
                flagReload = false;
                treload = "Без перезагрузки";
            }

            cbReloadData.IsEnabled = false;
            Start.IsEnabled = false;
            Stop.IsEnabled = true;
            btnChangePeriod.IsEnabled = true;
            Add.IsEnabled = false;
            btndelete.IsEnabled = false;
            ttime_restart.IsEnabled = false;

            list.Items.Add(DateTime.Now.ToString("HH:mm")+ $" Старт с таймером {ttime.Text}."+ treload); 
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

            timer.Change(Timeout.Infinite, Timeout.Infinite);
           // if(timerReload!=null)
                //timerReload.Change(Timeout.Infinite, Timeout.Infinite);
            userData = FileProvider.ReadFile();

            Start.IsEnabled = true;
            Stop.IsEnabled = false;
            btnChangePeriod.IsEnabled = false;
            Add.IsEnabled = true;
            btndelete.IsEnabled = true;
            cbReloadData.IsEnabled = true;
            ttime_restart.IsEnabled = true;
            list.Items.Add(DateTime.Now.ToString("HH:mm") + $" Остановка обновления");
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            updateBaseDataBinance();

            printTime();
            
            List<string> ListName = Binance.CurName();
            foreach(string s in ListName)
            {
                cbPair.Items.Add(s);
            }

            foreach(string s in userData)
            {
                dataForTable.Add(new DataBinanceView { symbol = s, percent = 0 }) ;
            }
        
            Table.ItemsSource = dataForTable;
            updatecbdelete();
            Stop.IsEnabled = false;
            btnChangePeriod.IsEnabled = false;
            ttime_restart.IsEnabled = true;
            //dataForTable.CollectionChanged+= Users_CollectionChanged;
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
        

            private void updateData_reload()
        {

            Dispatcher.Invoke(() => {
                updateBaseDataBinance();
                list.Items.Add(DateTime.Now.ToString("HH:mm")+" Рестарт");
            });

        }
            private async void updateData(object data)
        {
            if(indexReload<=0 && flagReload)
            {
                updateData_reload();
                Dispatcher.Invoke(() => indexReload = getTimeReloadPeriod());
            }
            else
            {
                await updateDataAsync();
                indexReload--;

                
            }

        }

        private async Task updateDataAsync()
        {
            List<DataBinance> UD = Binance.getDataBinance();


            for (int i = 0; i < dataForTable.Count; i++)
            {
                foreach (DataBinance db in UD)
                {
                    if (dataForTable[i].symbol.Contains(db.symbol))
                    {
                            Dispatcher.Invoke(() => dataForTable[i] = new DataBinanceView
                            {
                                symbol = db.symbol,
                                percent = Math.Round((db.lastPrice - baseDataBinance[i].lastPrice) * 100 / baseDataBinance[i].lastPrice, 3),
                                StartPrice = baseDataBinance[i].lastPrice,
                                link = Binance.getLink(db.symbol)
                            });
                        

                        break;
                    }
                }
            }
        }

        private void printTime()
        {
            int time = int.Parse(ttime_restart.Text) * int.Parse(ttime.Text);
            var ts = TimeSpan.FromSeconds(time);
            if(ts.Minutes==0)
            {
                lbReloadTime.Content = ts.Seconds + "c";
            }
            else
            {
                lbReloadTime.Content = ts.Minutes + ":" + ts.Seconds + "мин";
            }
            
        }
        private void NumericOnly(System.Object sender,TextCompositionEventArgs e)
        {
           
            e.Handled = IsTextNumeric(e.Text);
            try
            {
                printTime();
            }
            catch(Exception ex)
            {
                list.Items.Add("Поле не может быть пустым");
            }

            
        }
        private static bool IsTextNumeric(string str)
        {
            System.Text.RegularExpressions.Regex reg = new System.Text.RegularExpressions.Regex("[^0-9]");

            return reg.IsMatch(str);

        }
        private void btnChangePeriod_Click(object sender, RoutedEventArgs e)
        {
            int timeUpdate = getTimePeriod();
            timer.Change(0, getTimePeriod());
            list.Items.Add($"Время таймера изменено на {timeUpdate/1000}c");
        }

        private int getTimePeriod()
        {
            return int.Parse(ttime.Text)*1000;
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
    }


}

