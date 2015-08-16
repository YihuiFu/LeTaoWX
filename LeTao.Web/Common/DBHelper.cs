using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Configuration;
using System.Data.OleDb;
using System.Reflection;

namespace LeTao.Web.Common
{
    public class DBHelper
    {
        /// <summary>
        ///  connect to db
        /// </summary>
        /// <returns></returns>
        public static OleDbConnection OleDbConnect()
        {
            string connectString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=";
            connectString += System.Web.HttpContext.Current.Server.MapPath(ConfigurationManager.AppSettings["ConnectionString"]);

            // ConfigurationManager.ConnectionStrings["connectionString_Write"].ConnectionString
            //ConfigurationManager.AppSettings["ConnectionString"].ToString();
            OleDbConnection con = new OleDbConnection(connectString);
            return con;
        }

        /// <summary>
        /// Execute  Insert,Update
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="cmdParams"></param>
        /// <returns></returns>
        public static int ExecuteNonQuery(string strSql, params OleDbParameter[] cmdParams)
        {
            OleDbConnection sqlCon = OleDbConnect();
            OleDbCommand sqlCmd = new OleDbCommand(strSql, sqlCon);
            sqlCmd.CommandType = CommandType.Text;

            //OleDbTransaction trans = sqlCon.BeginTransaction();
            //sqlCmd.Transaction = trans;
            if (cmdParams != null)
            {
                foreach (OleDbParameter parm in cmdParams)
                {
                    sqlCmd.Parameters.Add(parm);
                }
            }

            try
            {
                sqlCon.Open();
                int num = sqlCmd.ExecuteNonQuery();
                //trans.Commit();
                return num;
            }
            catch
            {
                //trans.Rollback();
                return 0;
            }
            finally
            {
                sqlCmd.Dispose();
                sqlCon.Close();
                sqlCon.Dispose();
            }
        }

        /// <summary>
        /// Execute Select
        /// </summary>
        /// <param name="strSql"></param>
        /// <param name="cmdParams"></param>
        /// <returns></returns>
        public static DataTable ExecuteQuery(string strSql, params OleDbParameter[] cmdParams)
        {
            OleDbConnection sqlCon = OleDbConnect();
            OleDbCommand sqlCmd = new OleDbCommand(strSql, sqlCon);
            if (cmdParams != null)
            {
                foreach (OleDbParameter parm in cmdParams)
                {
                    sqlCmd.Parameters.Add(parm);
                }
            }

            DataTable dt = new DataTable();
            OleDbDataAdapter dap = new OleDbDataAdapter(sqlCmd);
            try
            {
                dap.Fill(dt);
                return dt;
            }
            catch
            {
                return null;
            }
            finally
            {
                sqlCmd.Dispose();
                sqlCon.Close();
                sqlCon.Dispose();
            }
        }


        public static int ExecuteScalar(string strSql, params OleDbParameter[] cmdParams)
        {
            OleDbConnection sqlCon = OleDbConnect();
            OleDbCommand sqlCmd = new OleDbCommand(strSql, sqlCon);
            if (cmdParams != null)
            {
                foreach (OleDbParameter parm in cmdParams)
                {
                    sqlCmd.Parameters.Add(parm);
                }
            }
            try
            {
                sqlCon.Open();
                int count = Convert.ToInt32(sqlCmd.ExecuteScalar());
                return count;
            }
            catch
            {
                return 0;
            }
            finally
            {
                sqlCmd.Dispose();
                sqlCon.Close();
                sqlCon.Dispose();
            }
        }

        public static string GetStrIDs(string str, int passCount)
        {
            OleDbConnection sqlCon = OleDbConnect();
            sqlCon.Open();
            OleDbCommand cmd = new OleDbCommand(str, sqlCon);
            string result = string.Empty;
            using (OleDbDataReader dr = cmd.ExecuteReader())
            {
                while (dr.Read())
                {
                    if (passCount < 1)
                    {
                        result += ",'" + dr.GetString(0)+"'";// dr.GetInt32(0);

                    }
                    passCount--;
                }
            }
            sqlCon.Close();
            sqlCon.Dispose();
            return result.Substring(1);
        }
        public static DataTable test()
        {
            string connectionString = @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=G:\LeTaoDB\LeTao.mdb";
            string sql = "select * from  UserInfo";
            OleDbConnection connection = new OleDbConnection(connectionString);
            connection.Open();
            //OleDbCommand cmd = new OleDbCommand(sql, connection);

            //connection.Open();
            OleDbDataAdapter da = new OleDbDataAdapter(sql, connectionString);
            //da.SelectCommand = com;
            DataSet ds = new DataSet();

            da.Fill(ds, "users");
            // dataGridView1.DataSource = ds.Tables["student"].DefaultView;//将查询结果放到datagridview中，显示表查询结果
            connection.Close();
            return ds.Tables["users"];

        }


    }

}