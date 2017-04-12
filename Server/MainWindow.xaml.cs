using PCInfo;
using Server.services;
using System;
using System.Collections.Generic;
using System.Timers;
using System.Windows;
using System.Windows.Controls;

namespace Server
{
    public partial class MainWindow : Window
    {
        Computers computers = new Computers();
        Timer refreshDataTimer = new Timer(5000);
        List<ComputerInfo> computersListData = new List<ComputerInfo>();

        public MainWindow()
        {
            InitializeComponent();
            refreshDataTimer.Elapsed += refreshData;
            refreshDataTimer.AutoReset = true;
            refreshDataTimer.Start();
        }

        private void refreshData(object sender, ElapsedEventArgs e)
        {
            Dispatcher.BeginInvoke((Action)delegate ()
            {
                listView.Items.Clear();
                foreach(var comp in computers.getInfo())
                {
                    if (comp == null)
                        continue;

                    listView.Items.Add(comp.computerName);
                    computersListData.Add(comp);
                }
            });            
        }

        private void SelectComputer(object sender, SelectionChangedEventArgs e)
        {
            if (listView.SelectedIndex == -1)
                return;

            var data = computersListData[listView.SelectedIndex];
            computerNameText.Text = data.computerName;
            processorNameText.Text = data.processorName;
            OSNameText.Text = data.OSName;
            RAMSizeText.Text = (data.ramSize/1024/1024).ToString();
        }
    }
}
