using System.Data;
using System.Data.SqlClient;
using System.Data.OleDb;
using System.Windows;
namespace StockExchange
{
    class DBconnection
    {

        public void ConnectToData()
        {
            OleDbConnection conn = new OleDbConnection("Data Source=D:\\homework\\uni work\\year 2\\Application Development\\StockExchangeDatabase.accdb;Provider=Microsoft.ACE.OLEDB.12.0;");

            // TODO: Modify the connection string and include any
            // additional required properties for your database.
            conn.Open();
        }
        
    }
}
