using System;
using System.Collections.Generic;
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
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : Window
    {
        public Settings()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if(teAPI.Text=="" ||teSekret.Text=="")
            {
                MessageBox.Show("Заполните все поля.");
                return;
            }

            Properties.Settings.Default.API = teAPI.Text;
            Properties.Settings.Default.Secret = teSekret.Text;
            this.Close();
        }
    }
}
