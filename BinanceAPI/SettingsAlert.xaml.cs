using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace BinanceAPI
{
    /// <summary>
    /// Interaction logic for SettingsAlert.xaml
    /// </summary>
    public partial class SettingsAlert : Window
    {
        public SettingsAlert()
        {
            InitializeComponent();
        }

        private void NumericOnly(System.Object sender,TextCompositionEventArgs e)
        {
            e.Handled = IsTextNumeric(e.Text);
        }
        private static bool IsTextNumeric(string str)
        {
            if(str=="," || str == "-")
            {
                return false;
            }
            else
            {
                System.Text.RegularExpressions.Regex reg = new System.Text.RegularExpressions.Regex("[^0-9]");
                return reg.IsMatch(str);
            }

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            tbNameTGalert.Text = Properties.Settings.Default.ChatId;
            tbAlertPercent.Text = Properties.Settings.Default.ChangePercent.ToString();
            tbTimeInterval.Text = Properties.Settings.Default.IntervalTime.ToString();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Properties.Settings.Default.ChatId = tbNameTGalert.Text;

                string dataPercent = tbAlertPercent.Text;
                if((dataPercent.Count(x => x == '.' || x == ','))>=2)
                {
                    MessageBox.Show("Ошибка ввода данных в пункт процента");
                    return;
                }

                if (dataPercent.Contains("."))
                {
                    dataPercent = dataPercent.Replace(".", ",");
                }

                Properties.Settings.Default.ChangePercent = decimal.Parse(tbAlertPercent.Text);


                Properties.Settings.Default.IntervalTime = int.Parse(tbTimeInterval.Text);

                Properties.Settings.Default.Save();
                this.Close();

            }
            catch(Exception ex)
            {
                MessageBox.Show("Ошибка ввода данных");
            }
            
        }
    }
}
