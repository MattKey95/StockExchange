using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace StockExchangeServer
{
    class Retrieve
    {
        public string AllStocks()
        {
            DataTable stocks = new DataTable();
            string result = "";
            try
            {
                using (OleDbConnection conn = new OleDbConnection("Classified"))
                {
                    string query = "select * from Stocks";
                    OleDbCommand cmd = new OleDbCommand(query, conn);
                    conn.Open();

                    OleDbDataAdapter adepter = new OleDbDataAdapter(cmd);

                    adepter.Fill(stocks);
                    conn.Close();
                    adepter.Dispose();
                }
                DataSet ds = new DataSet();
                ds.Tables.Add(stocks);
                StringWriter writer = new StringWriter();
                ds.WriteXml(writer);
                result = writer.ToString();
            }
            catch (Exception)
            {
                MessageBox.Show("Error processing stocks");
                result = "Error";
            }
            return result;
        }

        public string UserStocks(string username)
        {
            DataTable stocks = new DataTable();
            DataTable MyStocks = new DataTable();
            string result = "";
            try
            {
                using (OleDbConnection conn = new OleDbConnection("Classified"))
                {
                    string query = "select * from Portfolio Where UserName='" + username + "'";
                    OleDbCommand cmd = new OleDbCommand(query, conn);
                    conn.Open();

                    OleDbDataAdapter adepter = new OleDbDataAdapter(cmd);
                    MyStocks.Columns.Add("Quantity");
                    adepter.Fill(stocks);
                    int i = 0;
                    foreach (DataRow row in stocks.Rows)
                    {
                        query = "select * from Stocks where Symbol='" + row[1] + "'";
                        cmd = new OleDbCommand(query, conn);
                        adepter = new OleDbDataAdapter(cmd);
                        adepter.Fill(MyStocks);
                        MyStocks.Rows[i]["Quantity"] = row[2];
                        i++;
                    }
                    conn.Close();
                    adepter.Dispose();
                }
                DataSet ds = new DataSet();
                ds.Tables.Add(MyStocks);
                StringWriter writer = new StringWriter();
                ds.WriteXml(writer);
                result = writer.ToString();
            }
            catch (Exception)
            {
                MessageBox.Show("Error processing users stocks");
                result = "Error";
            }
            return result;
        }

        public string BuyStocks(string user, string symbol, int quantity)
        {
            string result = "";
            DataTable funds = new DataTable();
            DataTable price = new DataTable();
            try
            {
                using (OleDbConnection conn = new OleDbConnection("Classified"))
                {
                    string query = "select Funds from Users where UserName='" + user + "'";
                    OleDbCommand cmd = new OleDbCommand(query, conn);
                    conn.Open();

                    OleDbDataAdapter adepter = new OleDbDataAdapter(cmd);

                    adepter.Fill(funds);

                    query = "select Price from Stocks where Symbol='" + symbol + "'";
                    cmd = new OleDbCommand(query, conn);
                    adepter = new OleDbDataAdapter(cmd);
                    adepter.Fill(price);

                    string sprice = price.Rows[0]["Price"].ToString();
                    string sfunds = funds.Rows[0]["Funds"].ToString();
                    float finalprice = float.Parse(sprice);
                    finalprice *= quantity;
                    float initfunds = float.Parse(sfunds);
                    float aftertransfunds = initfunds - finalprice;
                    if (aftertransfunds > 0)
                    {
                        result = "Yes";
                        DataTable before = new DataTable();
                        query = "select Quantity from Portfolio where Symbol='" + symbol + "' and UserName='" + user + "'";
                        cmd = new OleDbCommand(query, conn);
                        adepter = new OleDbDataAdapter(cmd);
                        adepter.Fill(before);
                        if (before.Rows.Count == 0)
                        {
                            cmd = new OleDbCommand("Insert into Portfolio (UserName, Symbol, Quantity) Values ('" + user + "','" + symbol + "','" + quantity + "');", conn);
                            cmd.ExecuteNonQuery();
                            cmd = new OleDbCommand("Update Users Set Funds='" + aftertransfunds + "' where UserName='" + user + "'", conn);
                            cmd.ExecuteNonQuery();
                        }
                        else
                        {
                            string beforequant = before.Rows[0]["Quantity"].ToString();
                            int pastquant = int.Parse(beforequant);
                            pastquant += quantity;
                            cmd = new OleDbCommand("Update Portfolio Set Quantity='" + pastquant + "' Where UserName='" + user + "' and Symbol='" + symbol + "'", conn);
                            cmd.ExecuteNonQuery();
                            cmd = new OleDbCommand("Update Users Set Funds='" + aftertransfunds + "' where UserName='" + user + "'", conn);
                            cmd.ExecuteNonQuery();
                        }
                    }
                    else
                    {
                        result = "no";
                    }
                    conn.Close();
                    adepter.Dispose();
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Error processing buy request");
                result = "No";
            }
            return result;
        }

        public string SellStocks(string user, string symbol, int quantity)
        {
            string result = "";
            DataTable owned = new DataTable();
            try
            {
                using (OleDbConnection conn = new OleDbConnection("Classified"))
                {
                    string query = "select Quantity from Portfolio where Symbol='" + symbol + "' and UserName='" + user + "'";
                    OleDbCommand cmd = new OleDbCommand(query, conn);
                    conn.Open();

                    OleDbDataAdapter adepter = new OleDbDataAdapter(cmd);

                    adepter.Fill(owned);
                    if (owned.Rows.Count > 0)
                    {
                        int ownedquant = int.Parse(owned.Rows[0]["Quantity"].ToString());
                        int havequant = ownedquant - quantity;
                        if (havequant >= 0 && havequant < ownedquant)
                        {
                            result = "Yes";
                            DataTable price = new DataTable();
                            query = "select Price from Stocks where Symbol='" + symbol + "'";
                            cmd = new OleDbCommand(query, conn);
                            adepter = new OleDbDataAdapter(cmd);
                            adepter.Fill(price);
                            float profit = float.Parse(price.Rows[0]["Price"].ToString()) * quantity;
                            DataTable initfunds = new DataTable();
                            query = "select Funds from Users where UserName='" + user + "'";
                            cmd = new OleDbCommand(query, conn);
                            adepter = new OleDbDataAdapter(cmd);
                            adepter.Fill(initfunds);
                            float finalfunds = float.Parse(initfunds.Rows[0]["Funds"].ToString()) + profit;
                            query = "Update Users Set Funds='" + finalfunds + "' where UserName='" + user + "'";
                            cmd = new OleDbCommand(query, conn);
                            cmd.ExecuteNonQuery();
                            if (havequant > 0)
                            {
                                query = "Update Portfolio Set Quantity='" + havequant + "' where Symbol='" + symbol + "' and UserName='" + user + "'";
                                cmd = new OleDbCommand(query, conn);
                                cmd.ExecuteNonQuery();
                            }
                            else if (havequant == 0)
                            {
                                query = "Delete From Portfolio where Symbol='" + symbol + "' and UserName='" + user + "'";
                                cmd = new OleDbCommand(query, conn);
                                cmd.ExecuteNonQuery();
                            }

                        }
                        else
                        {
                            result = "No";
                        }
                    }
                    else
                    {
                        result = "No";
                    }
                    conn.Close();
                    adepter.Dispose();
                }
            }
            catch
            {
                MessageBox.Show("Error processing sell request");
                result = "No";
            }

            return result;
        }

        public string MyFunds(string username)
        {
            string result = "";
            DataTable Funds = new DataTable();
            try
            {
                using (OleDbConnection conn = new OleDbConnection("Classified"))
                {
                    string query = "select Funds from Users where UserName='" + username + "'";
                    OleDbCommand cmd = new OleDbCommand(query, conn);
                    conn.Open();

                    OleDbDataAdapter adepter = new OleDbDataAdapter(cmd);

                    adepter.Fill(Funds);
                    conn.Close();
                    adepter.Dispose();
                }
                result = Funds.Rows[0]["Funds"].ToString();
            }
            catch (Exception)
            {
                MessageBox.Show("Error sending funds");
                result = "Error";
            }
            return result;
        }

        public string LeaderBoard()
        {
            string result = "";
            DataTable LeaderBoard = new DataTable();
            try
            {
                using (OleDbConnection conn = new OleDbConnection("Classified"))
                {
                    string query = "select UserName, Funds from Users Order by Funds DESC";
                    OleDbCommand cmd = new OleDbCommand(query, conn);
                    conn.Open();

                    OleDbDataAdapter adepter = new OleDbDataAdapter(cmd);

                    adepter.Fill(LeaderBoard);
                    conn.Close();
                    adepter.Dispose();
                }
                DataSet ds = new DataSet();
                ds.Tables.Add(LeaderBoard);
                StringWriter writer = new StringWriter();
                ds.WriteXml(writer);
                result = writer.ToString();
            }
            catch (Exception)
            {
                MessageBox.Show("Error sending leader boards");
                result = "Error";
            }
            return result;
        }
    }

    
}

