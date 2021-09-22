using BinanceAPI.Model;
using BinanceAPI.Provider;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
        
        bool flagStart = false;
        Timer timer;
        int TimerPeriod;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            flagStart = true;

            TimerCallback tm = new TimerCallback(updateData);
            TimerPeriod = int.Parse(ttime.Text);
            timer = new Timer(tm, 0, 0, TimerPeriod);

                /*List<DataBinance> dataBin = b.getDataBinance();
                //var list = stringCollection.Cast<string>().ToList();
                StringCollection curSettings = Properties.Settings.Default.ListCur;
                List<string> cur= curSettings.Cast<string>().ToList();
                List<DataBinance> source = new List<DataBinance>();
                foreach(string s in cur)
                {
                    foreach(DataBinance db in dataBin)
                    {
                        if(db.symbol.Contains(s))
                        {
                            source.Add(db);
                        }
                    }
                }
                Table.ItemsSource = source;*/

            }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Binance binance = new Binance();
            List<string> ListName = binance.CurName();
            foreach(string s in ListName)
            {
                cbPair.Items.Add(s);
            }
            
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            //timer.Change(0, 60000);
        }

        private void updateData(object data)
        {
            if(TimerPeriod!= int.Parse(ttime.Text))
            {
                TimerPeriod = int.Parse(ttime.Text);
                timer.Change(TimerPeriod, TimerPeriod);
            }
            Binance binance = new Binance();


        }
    }
}
