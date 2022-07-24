using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

namespace Replicant.ICS
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void loadrackButton_Click(object sender, RoutedEventArgs e)
        {

            // Read SID from all SID text box and send gRPC request to Edge Node.


        }

        private void loadrack10RacksButton_Click(object sender, RoutedEventArgs e)
        {
            // Send 10 calls to proxy each with 5 sample ID from the text box 
        }
    }
}
