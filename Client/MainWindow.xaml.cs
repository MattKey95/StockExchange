using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Sockets;
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
using System.Text.RegularExpressions;
using System.ComponentModel;

namespace StockExchange
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string user;
        const int PORT_NO = 11000;
        const string SERVER_IP = "127.0.0.1";
        

        public MainWindow(string username)
        {
            InitializeComponent();
            user = username;
            AskForStocks();
            AskForMyStocks();
            AskForFunds();
            AskForLeaderBoard();
        }

        public void Window_Closing(object sender, CancelEventArgs e)
        {
            try
            {
                TcpClient client = new TcpClient(SERVER_IP, PORT_NO);
                NetworkStream nwStream = client.GetStream();
                byte[] bytesToSend = ASCIIEncoding.ASCII.GetBytes("LoggingOff" + user);
                nwStream.Write(bytesToSend, 0, bytesToSend.Length);
                byte[] data = new Byte[50000];
            
                client.Close();
            }
            catch (Exception)
            {

            }
            
        }
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void SymbolValidation(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^a-zA-Z0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void AskForFunds()
        {
            try
            {
                TcpClient client = new TcpClient(SERVER_IP, PORT_NO);
                NetworkStream nwStream = client.GetStream();
                byte[] bytesToSend = ASCIIEncoding.ASCII.GetBytes("MyFunds" + user);
                nwStream.Write(bytesToSend, 0, bytesToSend.Length);
                byte[] data = new Byte[50000];

                String responseData = String.Empty;
                Int32 bytes = nwStream.Read(data, 0, data.Length);
                responseData = System.Text.Encoding.ASCII.GetString(data, 0, bytes);
                client.Close();
                MyFundsLabel.Content = "$" + responseData;
            }
            catch (Exception)
            {
                MessageBox.Show("Could not retrieve funds");
            }

        }

        private void AskForMyStocks()
        {
            try
            {
                TcpClient client = new TcpClient(SERVER_IP, PORT_NO);
                NetworkStream nwStream = client.GetStream();
                byte[] bytesToSend = ASCIIEncoding.ASCII.GetBytes("GetMyStocks" + user);
                nwStream.Write(bytesToSend, 0, bytesToSend.Length);
                byte[] data = new Byte[50000];

                String responseData = String.Empty;
                Int32 bytes = nwStream.Read(data, 0, data.Length);
                responseData = System.Text.Encoding.ASCII.GetString(data, 0, bytes);
                client.Close();
                DataSet ds = new DataSet();
                DataTable MyStocksTable = new DataTable();
                MyStocksTable.Columns.Add("Name");
                MyStocksTable.Columns.Add("Price");
                MyStocksTable.Columns.Add("Symbol");
                MyStocksTable.Columns.Add("Change");
                MyStocksTable.Columns.Add("Quantity");
                string path = new FileInfo("MyStocks.xml").Directory.FullName;
                path = path + @"\MyStocks.xml";
                StreamWriter writer = new StreamWriter(path);
                writer.WriteLine(responseData);
                writer.Close();
                ds.Tables.Add(MyStocksTable);
                ds.ReadXml(path);
                MyStocks.ItemsSource = MyStocksTable.DefaultView;
            }
            catch (Exception)
            {
                MessageBox.Show("Could not retrieve your stocks");
            }
         }

        private void AskForStocks()
        {
            try
            {
                TcpClient client = new TcpClient(SERVER_IP, PORT_NO);
                NetworkStream nwStream = client.GetStream();
                byte[] bytesToSend = ASCIIEncoding.ASCII.GetBytes("GetStocks");
                nwStream.Write(bytesToSend, 0, bytesToSend.Length);
                byte[] data = new Byte[50000];

                String responseData = String.Empty;
                Int32 bytes = nwStream.Read(data, 0, data.Length);
                responseData = System.Text.Encoding.ASCII.GetString(data, 0, bytes);
                client.Close();
                DataSet ds = new DataSet();
                DataTable AllStocks = new DataTable();
                AllStocks.Columns.Add("Name");
                AllStocks.Columns.Add("Price");
                AllStocks.Columns.Add("Symbol");
                AllStocks.Columns.Add("Change");
                string path = new FileInfo("StockResults.xml").Directory.FullName;
                path = path + @"\StockResults.xml";
                StreamWriter writer = new StreamWriter(path);
                writer.WriteLine(responseData);
                writer.Close();
                ds.Tables.Add(AllStocks);
                ds.ReadXml(path);
                StockGrid.ItemsSource = AllStocks.DefaultView;
            }
            catch (Exception)
            {
                MessageBox.Show("Could not retieve stocks");
            }
        }

        public void AskForLeaderBoard()
        {
            try
            {
                TcpClient client = new TcpClient(SERVER_IP, PORT_NO);
                NetworkStream nwStream = client.GetStream();
                byte[] bytesToSend = ASCIIEncoding.ASCII.GetBytes("GetLeaderBoard");
                nwStream.Write(bytesToSend, 0, bytesToSend.Length);
                byte[] data = new Byte[50000];

                String responseData = String.Empty;
                Int32 bytes = nwStream.Read(data, 0, data.Length);
                responseData = System.Text.Encoding.ASCII.GetString(data, 0, bytes);
                client.Close();
                DataSet ds = new DataSet();
                DataTable LeaderBoard = new DataTable();
                LeaderBoard.Columns.Add("UserName");
                LeaderBoard.Columns.Add("Funds");
                string path = new FileInfo("LeaderBoards.xml").Directory.FullName;
                path = path + @"\LeaderBoards.xml";
                StreamWriter writer = new StreamWriter(path);
                writer.WriteLine(responseData);
                writer.Close();
                ds.Tables.Add(LeaderBoard);
                ds.ReadXml(path);
                LeaderBoardGrid.ItemsSource = LeaderBoard.DefaultView;
            }
            catch (Exception)
            {
                MessageBox.Show("Could not retrieve leader boards");
            }
        }
      
        public string User
        {
            get 
            {
                return user;
            }
            set
            {
                user = value;
            }
        }

        

        private void button_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void BuyButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                TcpClient client = new TcpClient(SERVER_IP, PORT_NO);
                NetworkStream nwStream = client.GetStream();
                byte[] bytesToSend = ASCIIEncoding.ASCII.GetBytes("BuyStocks" + user + "!" + BuySymbol.Text + "!" + BuyQuantity.Text);
                nwStream.Write(bytesToSend, 0, bytesToSend.Length);
                byte[] data = new Byte[50000];

                String responseData = String.Empty;
                Int32 bytes = nwStream.Read(data, 0, data.Length);
                responseData = System.Text.Encoding.ASCII.GetString(data, 0, bytes);
                client.Close();
                if(responseData == "Yes")
                {
                    AskForStocks();
                    AskForMyStocks();
                    AskForFunds();
                    AskForLeaderBoard();
                }
                else
                {
                    MessageBox.Show("insufficient Funds");
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Could not send buy order to the server");
            }
        }

        private void SellButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                TcpClient client = new TcpClient(SERVER_IP, PORT_NO);
                NetworkStream nwStream = client.GetStream();
                byte[] bytesToSend = ASCIIEncoding.ASCII.GetBytes("SellStocks" + user + "!" + SellSymbol.Text + "!" + SellQuantity.Text);
                nwStream.Write(bytesToSend, 0, bytesToSend.Length);
                byte[] data = new Byte[50000];

                String responseData = String.Empty;
                Int32 bytes = nwStream.Read(data, 0, data.Length);
                responseData = System.Text.Encoding.ASCII.GetString(data, 0, bytes);
                client.Close();
                if (responseData == "Yes")
                {
                    AskForStocks();
                    AskForMyStocks();
                    AskForFunds();
                    AskForLeaderBoard();
                }
                else
                {
                    MessageBox.Show("You don't own enough stocks to sell that many");
                }
            }
            catch(Exception)
            {
                MessageBox.Show("Could not send sell request to the server");
            }
        }

        private void BuyQuantity_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {

        }
    }

}
