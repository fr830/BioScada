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
using System.Windows.Navigation;
using System.Windows.Shapes;
using BioSCADA.WinClient.ServerSvc;

namespace BioSCADA.WinClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            client = new ClientWCFSvcClient("tcpClient");

        }


        private ServerSvc.ClientWCFSvcClient client;
        private void button1_Click(object sender, RoutedEventArgs e)
        {


            string ss = client.InnerChannel.SessionId;
            var dic = client.GetVariableExperiment("Exp1");
            if (dic != null)
                foreach (var d in dic)
                {
                    listBox1.Items.Add(d.Key + "    " + d.Value);
                }
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {

            client.Login("guille", "guillegg");
        }

        private bool flag = false;
        private void button3_Click(object sender, RoutedEventArgs e)
        {
            client.Login("guille", "guille");
//            if (!flag)
//            {
//                client.SetValueVariable(8, 1, "Exp1");
//                flag = true;
//            }
//            else
//            {
//                client.SetValueVariable(8, 0, "Exp1");
//                flag = false;
//            }

        }
    }
}
