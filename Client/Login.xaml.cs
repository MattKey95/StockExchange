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
using System.Windows.Shapes;
using System.Data.OleDb;
using System.Data;
using System.Net.Sockets;
using System.IO;
using System.Text.RegularExpressions;

namespace StockExchange
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class Login : Window
    {
        const int PORT_NO = 11000;
        const string SERVER_IP = "127.0.0.1";
        public Login()
        {
            InitializeComponent();
        }
        

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            if (RegisterUserName.Text != "" && RegisterPassword.Text != "" && ConfirmPassword.Text != "")
            {
                if (RegisterPassword.Text == ConfirmPassword.Text)
                {
                    //---data to send to the server---
                    string textToSend = RegisterUserName.Text + "-" + RegisterPassword.Text;
                    try
                    {
                        //---create a TCPClient object at the IP and port no.---
                        TcpClient client = new TcpClient(SERVER_IP, PORT_NO);
                        NetworkStream nwStream = client.GetStream();
                        byte[] bytesToSend = ASCIIEncoding.ASCII.GetBytes(textToSend);

                        //---send the text---
                        Console.WriteLine("Sending : " + textToSend);
                        nwStream.Write(bytesToSend, 0, bytesToSend.Length);

                        byte[] data = new Byte[256];

                        String responseData = String.Empty;
                        Int32 bytes = nwStream.Read(data, 0, data.Length);
                        responseData = System.Text.Encoding.ASCII.GetString(data, 0, bytes);
                        client.Close();

                        if (responseData == "Yes")
                        {
                            MainWindow obj = new MainWindow(RegisterUserName.Text);

                            App.Current.MainWindow = obj;
                            obj.Show(); //after login Redirect to second window  
                            this.Close();//after login hide the  Login window  
                        }
                        else
                        {
                            MessageBox.Show("UserName not Avaiable");
                        }
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("Could not connect to the server");
                    }
                }
            }
            else
            {

                MessageBox.Show("Invalid User Name or Password");

            }
        }

        private void LoginAccepter(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^a-zA-Z0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            if (userNameTextBox.Text != "" && passwordTextBox.Text != "")
            {

                //---data to send to the server---
                string textToSend = userNameTextBox.Text + "#" + passwordTextBox.Text;
                try
                {
                    //---create a TCPClient object at the IP and port no.---
                    TcpClient client = new TcpClient(SERVER_IP, PORT_NO);
                    NetworkStream nwStream = client.GetStream();
                    byte[] bytesToSend = ASCIIEncoding.ASCII.GetBytes(textToSend);

                    //---send the text---
                    Console.WriteLine("Sending : " + textToSend);
                    nwStream.Write(bytesToSend, 0, bytesToSend.Length);

                    byte[] data = new Byte[256];

                    String responseData = String.Empty;
                    Int32 bytes = nwStream.Read(data, 0, data.Length);
                    responseData = System.Text.Encoding.ASCII.GetString(data, 0, bytes);
                    client.Close();

                    if (responseData == "Yes")
                    {
                        MainWindow obj = new MainWindow(userNameTextBox.Text);

                        App.Current.MainWindow = obj;
                        obj.Show(); //after login Redirect to second window  
                        this.Close();//after login hide the  Login window  


                    }
                    else
                    {

                        MessageBox.Show("Invalid User Name or Password or the account is already logged in");
                    }
                }
                catch (Exception)
                {
                    MessageBox.Show("Could note connect to the server");
                }
            }
            else
            {

                MessageBox.Show("Invalid User Name or Password");

            }
        }
        
    }
}
