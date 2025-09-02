using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace AlarmGenertorConsole.Services
{
    public class MsSqlService
    {
        public DataTable Get(string connectionString,string query)
        {
            string connStr = System.Configuration.ConfigurationManager.ConnectionStrings[connectionString].ConnectionString;
            var conn = new SqlConnection(connStr);
            var cmd = new SqlCommand(query, conn);
            cmd.CommandTimeout = 30;
            var da = new SqlDataAdapter();
            da.SelectCommand = cmd;
            conn.Open();
            var ds = new DataSet();
            da.Fill(ds);
            conn.Close();
            return ds.Tables[0];
        }

        public bool Set(string connectionString, string nonQuery)
        {
            bool status = false;
            string connStr = System.Configuration.ConfigurationManager.ConnectionStrings[connectionString].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                SqlCommand cmd = conn.CreateCommand();
                SqlTransaction tran;

                // Start a local transaction.
                tran = conn.BeginTransaction("oneTran");

                // Must assign both transaction object and connection
                // to Command object for a pending local transaction
                cmd.Connection = conn;
                cmd.Transaction = tran;

                try
                {

                    cmd.CommandText = nonQuery;
                    int affected = cmd.ExecuteNonQuery();

                    if (affected > 0)
                    {
                        // Attempt to commit the transaction.
                        tran.Commit();

                        //Console.WriteLine("record is written to database.");
                        status = true;
                    }
                    else
                    {
                        tran.Rollback();
                    }
                    
                }
                catch //(Exception ex)
                {
                    //Console.WriteLine("Commit Exception Type: {0}", ex.GetType());
                    //Console.WriteLine("  Message: {0}", ex.Message);

                    // Attempt to roll back the transaction.
                    try
                    {
                        tran.Rollback();
                    }
                    catch (Exception ex2)
                    {
                        // This catch block will handle any errors that may have occurred
                        // on the server that would cause the rollback to fail, such as
                        // a closed connection.
                        //Console.WriteLine("Rollback Exception Type: {0}", ex2.GetType());
                        //Console.WriteLine("  Message: {0}", ex2.Message);

                        throw ex2;
                    }
                }
            }
            return status;
        }

        public int Set(string connectionString, string[] nonQueries)
        {
            //bool status = false;
            int nEffect = 0;
            string connStr = System.Configuration.ConfigurationManager.ConnectionStrings[connectionString].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                SqlCommand cmd = conn.CreateCommand();
                SqlTransaction tran;

                // Start a local transaction.
                tran = conn.BeginTransaction("oneTran");

                // Must assign both transaction object and connection
                // to Command object for a pending local transaction
                cmd.Connection = conn;
                cmd.Transaction = tran;

                try
                {
                    if (nonQueries != null && nonQueries.Length > 0)
                    {
                        for (int i = 0; i < nonQueries.Length; i++)
                        {
                            cmd.CommandText = nonQueries[i];
                            nEffect += cmd.ExecuteNonQuery();
                        }
                    }

                    // Attempt to commit the transaction.
                    tran.Commit();
                    //Console.WriteLine("Both records are written to database.");
                    //status = true;
                }
                catch //(Exception ex)
                {
                    //Console.WriteLine("Commit Exception Type: {0}", ex.GetType());
                    //Console.WriteLine("  Message: {0}", ex.Message);

                    // Attempt to roll back the transaction.
                    try
                    {
                        tran.Rollback();
                    }
                    catch (Exception ex2)
                    {
                        // This catch block will handle any errors that may have occurred
                        // on the server that would cause the rollback to fail, such as
                        // a closed connection.
                        //Console.WriteLine("Rollback Exception Type: {0}", ex2.GetType());
                        //Console.WriteLine("  Message: {0}", ex2.Message);

                        throw ex2;
                    }
                }
            }
            return nEffect;
        }
    }
}