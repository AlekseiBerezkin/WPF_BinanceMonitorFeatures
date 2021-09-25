using BinanceAPI.Model;
using BinanceAPI.Provider;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
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
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ObservableCollection<DataBinance> dataForTable = new ObservableCollection<DataBinance>();
        List<string> userData = FileProvider.ReadFile();
        bool flagStart = false;
        Timer timer;
       

        public MainWindow()
        {
            InitializeComponent();
            
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            renderTable();

            updatecbdelete();
            TimerCallback tm = new TimerCallback(updateData);
            //TimerPeriod = int.Parse(ttime.Text)*1000;
            timer = new Timer(tm, 0, 0, getTimePeriod());


            Start.IsEnabled = false;
            Stop.IsEnabled = true;
        }

        private void updatecbdelete()
        {
            cbdelete.Items.Clear();
            foreach (DataBinance s in dataForTable)
            {
                
                cbdelete.Items.Add(s.symbol);
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

            timer.Change(Timeout.Infinite, Timeout.Infinite);
            dataForTable.Clear();
            userData = FileProvider.ReadFile();
            Start.IsEnabled = true;
            Stop.IsEnabled = false;
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
            Stop.IsEnabled = false;
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
                
                dataForTable.Add(new DataBinance { symbol = NameCur, priceChangePercent = 0 });
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
                        Dispatcher.Invoke(() => dataForTable[i]=db);
                    }
                }
            }
            
            /*Dispatcher.Invoke(() => dataForTable.Clear());
            foreach (DataBinance db in GetUpdateUserData())
            {
                Dispatcher.Invoke(() => dataForTable.Add(db));
            }*/

            //renderTable();
            //dataForTable = GetUpdateUserData();
            //Dispatcher.Invoke(() => Table.Items.Refresh());
        }

        private List<DataBinance> UpdateUserData()
        {

            List<DataBinance> LDB = Binance.getDataBinance();
            List<DataBinance> viewData = new List<DataBinance>();
            foreach (string s in userData)
            {
                foreach (DataBinance db in LDB)
                {
                    if (db.symbol.Contains(s))
                    {
                        viewData.Add(db);
                        break;
                    }
                }
            }
            return viewData;
        }

        private void renderTable()
        {
            // dataForTable = GetUpdateUserData();
            List<DataBinance> updateData = Binance.getDataBinance();
            if (userData != null)
            {
                foreach(string s in userData)
                {
                    foreach(DataBinance db in updateData)
                    {
                        if(db.symbol.Contains(s))
                        {
                            dataForTable.Add(db);
                            break;
                        }
                    }
                }
                Table.ItemsSource = dataForTable;
            }
            else
            {
                MessageBox.Show("Добавьте пару");
            }

        }

        private void btnChangePeriod_Click(object sender, RoutedEventArgs e)
        {
            timer.Change(0, getTimePeriod());
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
            updatecbdelete();
        }
    }
}
