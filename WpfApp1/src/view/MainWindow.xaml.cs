using System;
using System.Windows;
using LocalRadar;
using System.Net;
using System.Collections.ObjectModel;

namespace WpfApp1
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Radar radar;
        private ObservableCollection<Computer> computers;

        public MainWindow()
        {
            InitializeComponent();
            computers = new ObservableCollection<Computer>();
            ComputersList.ItemsSource = computers;

            RadarBuilder builder = new RadarBuilder();
            radar = builder.SetRange(IPAddress.Parse("192.168.1.1"),
                IPAddress.Parse("192.168.255.255"))
                .SetPort(3210)
                .SetFindCallback(FindIPAddress)
                .Build();

            radar.Scan();
        }

        private void FindIPAddress(IPAddress address)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var computer = new Computer();
                computer.address = address;
                computer.name = address.ToString();
                foreach(var comp in computers)
                {
                    if (comp.address == address)
                        return;
                }
                computers.Add(computer);
            });
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            radar.Stop();
        }
    }

    internal class Computer
    {
        public String name;
        public IPAddress address;
    }
}
