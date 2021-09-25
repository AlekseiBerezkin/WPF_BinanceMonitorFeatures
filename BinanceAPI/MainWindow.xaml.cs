using BinanceAPI.Model;
using BinanceAPI.Provider;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
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
        Timer timer;
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
            //TimerPeriod = int.Parse(ttime.Text)*1000;
            timer = new Timer(tm, 0, 0, getTimePeriod());


            Start.IsEnabled = false;
            Stop.IsEnabled = true;
            btnChangePeriod.IsEnabled = true;
            Add.IsEnabled = false;
            btndelete.IsEnabled = false;

            list.Items.Add(DateTime.Now.ToString("HH:mm")+ $" Старт с таймером {ttime.Text}с"); 
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
            //dataForTable.Clear();
            userData = FileProvider.ReadFile();
            Start.IsEnabled = true;
            Stop.IsEnabled = false;
            btnChangePeriod.IsEnabled = false;
            Add.IsEnabled = true;
            btndelete.IsEnabled = true;
            list.Items.Add(DateTime.Now.ToString("HH:mm") + $" Остановка обновления");
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //Table.ItemsSource = dataForTable;
            //dataForTable.Add(new DataBinance {symbol="zbs",priceChangePercent=3378 });
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
            //dataForTable.CollectionChanged+= Users_CollectionChanged;
        }

       /* private static void Users_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add: // если добавление
                    //User newUser = e.NewItems[0] as User;
                    //Console.WriteLine($"Добавлен новый объект: {newUser.Name}");
                    break;
                case NotifyCollectionChangedAction.Remove: // если удаление
                    //User oldUser = e.OldItems[0] as User;
                    //Console.WriteLine($"Удален объект: {oldUser.Name}");
                    break;
                case NotifyCollectionChangedAction.Replace: // если замена
                    //User replacedUser = e.OldItems[0] as User;
                    //User replacingUser = e.NewItems[0] as User;
                    //xConsole.WriteLine($"Объект {replacedUser.Name} заменен объектом {replacingUser.Name}");
                    break;
            }
        }*/
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

        private void updateData(object data)
        {
            List<DataBinance> UD = Binance.getDataBinance();
            

            for (int i=0;i<dataForTable.Count;i++)
            {
                foreach (DataBinance db in UD)
                {
                    if(dataForTable[i].symbol.Contains(db.symbol))
                    {
                        Dispatcher.Invoke(() => dataForTable[i]=new DataBinanceView {symbol=db.symbol,
                            percent=Math.Round((db.lastPrice - baseDataBinance[i].lastPrice) *100/ baseDataBinance[i].lastPrice,3),
                            StartPrice= baseDataBinance[i].lastPrice
                        } );
                        break;
                    }
                }
            }
            
        }


        private void NumericOnly(System.Object sender, System.Windows.Input.TextCompositionEventArgs e)
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
            int timeUpdate = getTimePeriod();
            timer.Change(0, getTimePeriod());
            list.Items.Add($"Время таймера изменено на {timeUpdate/1000}c");
        }

        private int getTimePeriod()
        {
            return int.Parse(ttime.Text)*1000;
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
            /*try
            {
                var s = (DataBinanceView)e.Row.DataContext;

                var f=e.Row.ItemsPanel;

                if (s.percent > 0)
                    e.Row = more;
                if (s.percent < 0)
                    e.Row.Background = smaller;
            }
            catch(Exception ex)
            {

            }*/
        }
    }


}

