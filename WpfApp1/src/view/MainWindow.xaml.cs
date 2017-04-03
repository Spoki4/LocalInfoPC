using System;
using System.Windows;
using LocalRadar;
using System.Net;

namespace WpfApp1
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Radar radar;

        public MainWindow()
        {
            InitializeComponent();
            RadarBuilder builder = new RadarBuilder();
            radar = builder.SetRange(IPAddress.Parse("192.168.1.1"),
                IPAddress.Parse("192.168.1.255"))
                .SetPort(3210)
                .Build();

            radar.Scan();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            radar.Stop();
        }
    }
}
