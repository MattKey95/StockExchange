using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows;
using System.IO;
using StockExchangeServer;
using System.Data.OleDb;
using System.Data;
using System.Data.SqlClient;
using System.ComponentModel;

namespace StockExchangeServer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string path;
        List<string> Users = new List<string>();

        Thread listen;

        public MainWindow()
        {
            InitializeComponent();
            listen = new Thread(StartListening);
        }
        
        public void Window_Closing(object sender, CancelEventArgs e)
        {
            listen.Abort();
        }
        // Thread signal.
        public   ManualResetEvent allDone = new ManualResetEvent(false);

        public void StartListening()
        {
            // Data buffer for incoming data.
            byte[] bytes = new Byte[1024];

            // Establish the local endpoint for the socket.
            // The DNS name of the computer
            // running the listener is "host.contoso.com".
            //IPHostEntry ipHostInfo = Dns.Resolve(Dns.GetHostName());
            //IPAddress ipAddress = ipHostInfo.AddressList[0];
            string ip = "127.0.0.1";
            IPAddress ipt = IPAddress.Parse(ip);
            IPEndPoint localEndPoint = new IPEndPoint(ipt, 11000);

            // Create a TCP/IP socket.
            Socket listener = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp);

            // Bind the socket to the local endpoint and listen for incoming connections.
            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(100);
                while (true)
                {
                    // Set the event to nonsignaled state.
                    allDone.Reset();

                    // Start an asynchronous socket to listen for connections.
                    System.Diagnostics.Debug.WriteLine("Waiting for a connection...");
                    listener.BeginAccept( new AsyncCallback(AcceptCallback), listener);

                    // Wait until a connection is made before continuing.
                    allDone.WaitOne();
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            Console.WriteLine("\nPress ENTER to continue...");
            Console.Read();

        }

        public void AcceptCallback(IAsyncResult ar)
        {
            System.Diagnostics.Debug.WriteLine("accept");
            // Signal the main thread to continue.
            allDone.Set();

            // Get the socket that handles the client request.
            Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);

            // Create the state object.
            StateObject state = new StateObject();
            state.workSocket = handler;
            handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                new AsyncCallback(ReadCallback), state);
        }

        public void ReadCallback(IAsyncResult ar)
        {
            System.Diagnostics.Debug.WriteLine("Read");
            String content = String.Empty;

            // Retrieve the state object and the handler socket
            // from the asynchronous state object.
            StateObject state = (StateObject)ar.AsyncState;
            Socket handler = state.workSocket;

            // Read data from the client socket. 
            int bytesRead = handler.EndReceive(ar);

            if (bytesRead > 0)
            {
                System.Diagnostics.Debug.WriteLine("if");
                // There  might be more data, so store the data received so far.
                state.sb.Append(Encoding.ASCII.GetString(
                    state.buffer, 0, bytesRead));

                // Check for end-of-file tag. If it is not there, read 
                // more data.
                content = state.sb.ToString();
                using (OleDbConnection conn = new OleDbConnection("Classified"))
                {
                    if (content.Contains("#"))
                    {
                        try
                        {
                            string[] details = content.Split('#');
                            DataTable login = new DataTable();
                            string query = "select UserName from Users where Password='" + details[1] + "' and UserName='" + details[0] + "'";
                            OleDbCommand cmd = new OleDbCommand(query, conn);
                            conn.Open();

                            OleDbDataAdapter adepter = new OleDbDataAdapter(cmd);

                            adepter.Fill(login);
                            conn.Close();
                            adepter.Dispose();

                            if (login.Rows.Count == 1)
                            {
                                if (Users.Contains(details[0]))
                                {
                                    Send(handler, "No");
                                }
                                else
                                {
                                    Users.Add(details[0]);
                                    updatelist();
                                    Send(handler, "Yes");
                                }

                            }
                            else
                            {
                                Send(handler, "No");
                            }
                        }
                        catch (Exception)
                        {
                            MessageBox.Show("Error retrieving log in info");
                        }
                    }
                    else if (content.Contains("-"))
                    {
                        try
                        {
                            string[] details = content.Split('-');
                            DataTable login = new DataTable();

                            string query = "select * from Users where UserName='" + details[0] + "'";
                            OleDbCommand cmd = new OleDbCommand(query, conn);
                            conn.Open();

                            OleDbDataAdapter adepter = new OleDbDataAdapter(cmd);

                            adepter.Fill(login);
                            if (login.Rows.Count == 0)
                            {
                                cmd = new OleDbCommand("Insert into Users (UserName, Password, Funds) Values ('" + details[0] + "','" + details[1] + "','50000');", conn);
                                cmd.ExecuteNonQuery();
                                Users.Add(details[0]);
                                updatelist();
                                Send(handler, "Yes");
                            }
                            else
                            {
                                Send(handler, "No");
                            }
                            conn.Close();
                            adepter.Dispose();
                        }
                        catch (Exception)
                        {
                            MessageBox.Show("Error registering user");
                        }
                    }
                    else if (content == "GetStocks")
                    {
                        try
                        {
                            Retrieve retrievedata = new Retrieve();
                            string stocks = retrievedata.AllStocks();
                            Send(handler, stocks);
                        }
                        catch (Exception)
                        {
                            MessageBox.Show("Error sending stocks");
                        }
                    }
                    else if (content.Contains("GetMyStocks"))
                    {
                        try
                        {
                            Retrieve retrievedata = new Retrieve();
                            string stocks = retrievedata.UserStocks(content = content.Replace("GetMyStocks", ""));
                            Send(handler, stocks);
                        }
                        catch (Exception)
                        {
                            MessageBox.Show("Error sending users stocks");
                        }
                    }
                    else if (content.Contains("BuyStocks"))
                    {
                        try
                        {
                            Retrieve retrievedata = new Retrieve();
                            content = content.Replace("BuyStocks", "");
                            string[] details = content.Split('!');
                            int quantity = int.Parse(details[2]);
                            string bought = retrievedata.BuyStocks(details[0], details[1], quantity);
                            Send(handler, bought);
                        }
                        catch (Exception)
                        {
                            MessageBox.Show("Error sending buy conformation");
                        }
                    }
                    else if (content.Contains("SellStocks"))
                    {
                        try
                        {
                            Retrieve retrievedata = new Retrieve();
                            content = content.Replace("SellStocks", "");
                            string[] details = content.Split('!');
                            int quantity = int.Parse(details[2]);
                            string sold = retrievedata.SellStocks(details[0], details[1], quantity);
                            Send(handler, sold);
                        }
                        catch (Exception)
                        {
                            MessageBox.Show("Error sending sell conformation");
                        }
                    }
                    else if (content.Contains("MyFunds"))
                    {
                        try
                        {
                            Retrieve retrievedata = new Retrieve();
                            content = content.Replace("MyFunds", "");
                            string sold = retrievedata.MyFunds(content);
                            Send(handler, sold);
                        }
                        catch (Exception)
                        {
                            MessageBox.Show("Error sending users funds");
                        }
                    }
                    else if (content.Contains("LoggingOff"))
                    { 
                            content = content.Replace("LoggingOff", "");
                            Users.Remove(content);
                            updatelist();
                    }
                    else if(content == "GetLeaderBoard")
                    {
                        try
                        {
                            Retrieve retrievedata = new Retrieve();
                            string sold = retrievedata.LeaderBoard();
                            Send(handler, sold);
                        }
                        catch (Exception)
                        {
                            MessageBox.Show("Error sending leader board");
                        }
                    }
                }
            }
        }

        public void updatelist()
        {
            
            string names = "";
            foreach(string user in Users){
                names += user + "\n";
            }
            System.Diagnostics.Debug.WriteLine("call");
            this.Dispatcher.Invoke((Action)(() => {UserList.Content = names;}));
            System.Diagnostics.Debug.WriteLine("Here");
        }

        private void Send(Socket handler, String data)
        {
            // Convert the string data to byte data using ASCII encoding.
            byte[] byteData = Encoding.ASCII.GetBytes(data);

            // Begin sending the data to the remote device.
            handler.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallback), handler);
        }

        private   void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                Socket handler = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.
                int bytesSent = handler.EndSend(ar);
                Console.WriteLine("Sent {0} bytes to client.", bytesSent);

                handler.Shutdown(SocketShutdown.Both);
                handler.Close();

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private void Start_Button_Click(object sender, RoutedEventArgs e)
        {
            
            listen.Start();
            System.Diagnostics.Debug.WriteLine("endthread");
        }

        private async void button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                path = new FileInfo("stockprices.xml").Directory.FullName;
                string wpath = path + "\\stockprices.xml";
                HttpClient client = new HttpClient();
                
                HttpResponseMessage response = await client.GetAsync("https://query.yahooapis.com/v1/public/yql?q=select%20Symbol%2C%20Ask%2C%20Change%2C%20Name%20from%20yahoo.finance.quotes%20where%20symbol%20in%20(%22YHOO%22%2C%20%22AAPL%22%2C%20%22GOOG%22%2C%20%22MSFT%22%2C%20%22BAC%22%2C%20%22ETE%22%2C%20%22SIRI%22%2C%20%22PBR%22%2C%20%22FCX%22%2C%20%22CHK%22%2C%20%22PFE%22%2C%20%22WMB%22%2C%20%22WLL%22%2C%20%22F%22%2C%20%22NFLX%22%2C%20%22MS%22%2C%20%22MU%22%2C%20%22MRO%22%2C%20%22CPXX%22%2C%20%22C%22%2C%20%22VALE%22%2C%20%22ITUB%22)%0A%09%09&env=http%3A%2F%2Fdatatables.org%2Falltables.env");
                response.EnsureSuccessStatusCode();
                File.AppendAllText(wpath, await response.Content.ReadAsStringAsync());
                
            }
            catch(Exception)
            {

            }
        }

        private void Upload_Click(object sender, RoutedEventArgs e)
        {
            StreamReader reader = new StreamReader(path + "\\stockprices.xml");
            string result = reader.ReadLine();
            result = reader.ReadLine();
            reader.Close();
            result = result.Replace("><", ">" + Environment.NewLine + "<");
            StreamWriter writer = new StreamWriter(path + "\\test.txt");
            writer.WriteLine(result);
            writer.Close();
            StreamReader finalread = new StreamReader(path + "\\test.txt");
            StreamWriter finalwriter = new StreamWriter(path + "\\test2.txt");
            string line;
            while ((line = finalread.ReadLine()) != null)
            {
                line = line.Replace("<quote>", "");
                line = line.Replace("</quote>", "");
                line = line.Replace("<Change>", "");
                line = line.Replace("</Change>", "");
                line = line.Replace("<Name>", "");
                line = line.Replace("</Name>", "");
                line = line.Replace("<Symbol>", "");
                line = line.Replace("</Symbol>", "");
                line = line.Replace("<Ask>", "");
                line = line.Replace("</Ask>", "");
                if (line.Contains("<") == false && line != "")
                {
                    finalwriter.WriteLine(line);
                }
            }
            finalread.Close();
            finalwriter.Close();

            using (OleDbConnection conn = new OleDbConnection("Classified"))
            {
                conn.Open();
                StreamReader insertread = new StreamReader(path + "\\test2.txt");
                string change;
                while ((change = insertread.ReadLine()) != null)
                {
                    //change = insertread.ReadLine();
                    string name = insertread.ReadLine();
                    string symbol = insertread.ReadLine();
                    string sprice = insertread.ReadLine().ToString();
                    float price = float.Parse(sprice);
                    
                    OleDbCommand cmd = new OleDbCommand("Update Stocks Set Price='"+price+"', Change='"+change+"' Where Symbol='"+symbol+"'",conn);
                    cmd.ExecuteNonQuery();
                }
                insertread.Close();
                conn.Close();
                
                
            }
    }
                

        

    }
    public class StateObject
    {
        // Client  socket.
        public Socket workSocket = null;
        // Size of receive buffer.
        public const int BufferSize = 1024;
        // Receive buffer.
        public byte[] buffer = new byte[BufferSize];
        // Received data string.
        public StringBuilder sb = new StringBuilder();
    }
}

