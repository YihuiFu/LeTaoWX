using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Web;
using System.IO;


namespace LeTao.Web.Common
{
    public class Utils
    {
        private static Random rand = new Random();
        private static object obj = new object();
        public static long NewUID()
        {
            DateTime dt = DateTime.Now;
            Int64 UUID;
            lock (obj)
            {
                UUID = ((((((((Int64)dt.Year - 1990) * 12 + dt.Month) * 31 + dt.Day) * 24 + dt.Hour) * 60 + dt.Minute) * 60 + dt.Second) * 1000 + dt.Millisecond) * 1000000 + rand.Next(0, 999999);
            }
            return UUID;
        }

        public static string MapPath(string strPath)
        {
            if (HttpContext.Current != null)
            {
                return HttpContext.Current.Server.MapPath(strPath);
            }
            else //非web程序引用
            {
                strPath = strPath.Replace("/", "\\");
                if (strPath.StartsWith("\\"))
                {
                    //strPath = strPath.Substring(strPath.IndexOf('\\', 1)).TrimStart('\\');
                    strPath = strPath.TrimStart('\\');
                }
                return System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, strPath);
            }
        }
        public static void CreateFile(string path, string str)
        {
            string url = MapPath(path.Substring(0, path.LastIndexOf("/") + 1));
            path = MapPath(path);
            if (!Directory.Exists(url))
            {
                Directory.CreateDirectory(url);
            }
            if (!File.Exists(path))
            {
                FileStream fs = File.Create(path);
                fs.Close();
            }
            StreamWriter sw = new StreamWriter(path, true, Encoding.GetEncoding("utf-8"));
            sw.WriteLine(str);
            sw.Close();
        }


       
    }

   
}
