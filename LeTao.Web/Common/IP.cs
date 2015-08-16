using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Net;
using System.Text.RegularExpressions;

namespace LeTao.Web.Common
{
    public static class IP
    {

        #region zz@20140220:一些int函数:zzMin,等
        public static int zzMin(int a, int b)
        {
            return a < b ? a : b;
        }
        #endregion

        #region zz@20140220:string静态扩展:zzLeft,等
        public static string zzLeft(this string s, int l)//要求l>=0
        {
            System.Diagnostics.Debug.Assert(l >= 0);
            return s.Substring(0, zzMin(s.Length, l));
        }
        public static string zzMid(this string s, int b, int l)//要求b>=0 l>=0
        {
            System.Diagnostics.Debug.Assert(b >= 0);
            System.Diagnostics.Debug.Assert(l >= 0);
            int len = s.Length;
            if (b > len - 1)
            {
                return "";
            }
            return s.Substring(b, zzMin(len - b, l));
        }
        #endregion
        private static Regex zzIpPortStrRegex = new Regex(@"^\d{1,3}[.]\d{1,3}[.]\d{1,3}[.]\d{1,3}(:\d{1,5})?$");
        public static Boolean zzIsIpPortStr(string s)//判断字符串是否为ip:port格式
        {
            return zzIpPortStrRegex.IsMatch(s);
        }
        public static Boolean zzIpPortStrIsLocal(string s)//已知字符串是ip:port格式,判断是否问本地ip
        {
            string t;
            return s.zzLeft(4) == "127."
                || s.zzLeft(3) == "10."
                || s.zzLeft(8) == "192.168."
                || string.Compare((t = s.zzLeft(7)), "172.16.") >= 0 && string.Compare(t, "172.31.") <= 0 && t.zzMid(6, 1) == ".";
        }


        /// <summary>
        /// 获取本地string类型ip地址
        /// </summary>
        /// <returns></returns>
        public static string GetIP()
        {
            IPHostEntry ipHost = Dns.GetHostEntry(Dns.GetHostName());
            for (int i = 0; i < ipHost.AddressList.Length; i++)
            {
                if (ipHost.AddressList[i].AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    return ipHost.AddressList[i].ToString();
                }
            }
            return "127.0.0.1";
        }

        /// <summary>
        /// 获取网页请求的用户地址
        /// </summary>
        /// <returns></returns>

        public static string GetWanIp()
        {
            string s;
            string result = String.Empty;
            string resultB = String.Empty;//备用结果

            result = System.Web.HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            if (!(null == result || result == String.Empty))
            {
                //可能有代理
                if (result.IndexOf(".") < 0)//快速排除无效字符串
                {    //没有“.”肯定是非IPv4格式
                    result = null;
                }
                else
                {
                    if (result.IndexOf(",") >= 0 || result.IndexOf(";") >= 0)//快速区分是否多代理
                    {
                        //有“,”，是多个代理。取第一个不是内网的IP。
                        result = result.Replace(" ", "").Replace("'", "");
                        string[] temparyip = result.Split(',', ';');
                        result = String.Empty;

                        for (int i = 0; i < temparyip.Length; i++)
                        {
                            if (zzIsIpPortStr(temparyip[i]))
                            {
                                if (zzIpPortStrIsLocal(temparyip[i]))
                                {
                                    resultB = temparyip[i];//最后一个本地ip可以作为备用结果
                                }
                                else
                                {
                                    result = temparyip[i];     //找到不是内网的地址
                                    break;
                                }
                            }
                        }
                    }
                    else if (zzIsIpPortStr(result))
                    {
                        //代理即是IP格式
                    }
                    else
                    {
                        result = null; //代理中的内容非IP
                    }
                }
            }

            if (null == result || result == String.Empty)
            {
                s = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
                if ((null == resultB || resultB == String.Empty) || !(null == resultB || resultB == String.Empty) && !zzIpPortStrIsLocal(s))
                {
                    result = s;
                }
            }
            if (null == result || result == String.Empty)
            {
                s = HttpContext.Current.Request.UserHostAddress;
                if ((null == resultB || resultB == String.Empty) || !(null == resultB || resultB == String.Empty) && !zzIpPortStrIsLocal(s))
                {
                    result = s;
                }
            }

            if (null == result || result == String.Empty)
            {
                if (resultB == String.Empty)
                {
                    return "0.0.0.0";
                }
                else
                {
                    return resultB.Split(':')[0].Trim();
                }
            }
            else
            {
                return result.Split(':')[0].Trim();
            }
        }


        /// <summary>
        /// 获取长整形ip
        /// </summary>
        /// <param name="IP">string类型ip地址</param>
        /// <returns></returns>
        public static long getIP_256(string IP)
        {
            if (!String.IsNullOrEmpty(IP))
            {
                string[] array = IP.Split('.');
                if (array.Length == 4)
                {
                    return (Int64)Convert.ToInt32(array[0]) * 256 * 256 * 256 + Convert.ToInt32(array[1]) * 256 * 256 + Convert.ToInt32(array[2]) * 256 + Convert.ToInt32(array[3]);
                }
            }
            return 0;
        }
        /// <summary>
        /// 获取本地长整形ip
        /// </summary>
        /// <returns></returns>
        public static long getIP_256()
        {
            return getIP_256(GetWanIp());
        }

        /// <summary>
        /// 长整形ip转换为string类型
        /// </summary>
        /// <param name="ip">长整形ip</param>
        /// <returns></returns>
        public static string getIP_Long(long ip)
        {
            if (ip > 0)
            {
                int i = (int)(ip / (256 * 256 * 256));
                long not = ip % (256 * 256 * 256);
                int j = (int)(not / (256 * 256));
                not = not % (256 * 256);
                int k = (int)(not / 256);
                not = not % 256;
                return i + "." + j + "." + k + "." + not;
            }
            return "";
        }
    }
}