﻿using Server.services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Server
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Computers computers = new Computers();
        Timer refreshDataTimer = new Timer(1000);

        public MainWindow()
        {
            InitializeComponent();
            refreshDataTimer.Elapsed += refreshData;
            refreshDataTimer.AutoReset = true;
            refreshDataTimer.Start();
            DataContext = computers;
        }

        private void refreshData(object sender, ElapsedEventArgs e)
        {
            Dispatcher.BeginInvoke((Action)delegate ()
            {
                listView.ItemsSource = computers.getInfo();
            });            
        }
    }
}
