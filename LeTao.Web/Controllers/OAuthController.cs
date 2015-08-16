using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using LeTao.Web.Models;
using LeTao.Web.Common;
using System.Text;
using System.Runtime.Serialization.Json;
using LeTao.Web.Filters;
using System.Xml;

namespace LeTao.Web.Controllers
{
    public class OAuthController : Controller
    {
        //
        // GET: /OAuth/
        //[AccessOpenIDFilter]
        public ActionResult Index()
        {
            string str = "获取结果：";
            if (SessionManager.OpenID != null)
            {
                str += SessionManager.OpenID;
            }
            else
            {
                str += "失败哦了";
            }
            ViewBag.result = "__" + str;
            return View();
        }



        #region -- 网页授权无效

        /// <summary>
        /// 获取微信access_token
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        public ActionResult GetAccessToken()
        {
            try
            {
                //开始获取access_token
                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create("https://api.weixin.qq.com/cgi-bin/token?grant_type=client_credential&appid=wxaa58982117696428&secret=bcfbc6d635104cb072bc127a8635a32e");
                HttpWebResponse reponse = (HttpWebResponse)request.GetResponse();
                using (TextReader reader = new StreamReader(reponse.GetResponseStream()))
                {
                    //JsonResult js= Json(reader.ReadToEnd(), JsonRequestBehavior.AllowGet);
                    WXAccessTokenJson wtj = new WXAccessTokenJson();
                    wtj = (WXAccessTokenJson)JsonToObject(reader.ReadToEnd(), wtj);
                    //将access_token放到Session中，方便下次调用
                    SessionManager.AccessToken = wtj.access_token;
                    //开始获取jsapi_ticket
                    request = (HttpWebRequest)HttpWebRequest.Create(string.Format("https://api.weixin.qq.com/cgi-bin/ticket/getticket?access_token={0}&type=jsapi", wtj.access_token));
                    reponse = (HttpWebResponse)request.GetResponse();
                    TextReader reader2 = new StreamReader(reponse.GetResponseStream());
                    WXAccessTokenJson wtj2 = new WXAccessTokenJson();
                    wtj2 = (WXAccessTokenJson)JsonToObject(reader2.ReadToEnd(), wtj2);
                    SessionManager.Ticket = wtj2.ticket;
                    return RedirectToAction("GetShai");
                }
            }
            catch (Exception ex)
            {
                // Logger.LogInstance.Write(ex, MessageType.Error);
                return new JsonResult { Data = new { Flag = -1 } };
            }

        }
        public void log(string e)
        {
            Utils.CreateFile(@"/OAuth.txt", e + "\n__时间：" + DateTime.Now.ToShortTimeString());
        }
        /// <summary>
        /// 获取签名字符串
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost]
        public ActionResult GetShai(string timeStamp, string url)
        {
            try
            {
                url = url.Replace("#", "&");
                string appId = System.Web.Configuration.WebConfigurationManager.AppSettings["APPID"];
                string appSecret = System.Web.Configuration.WebConfigurationManager.AppSettings["AppSecret"];
                string accessTokenURL = System.Web.Configuration.WebConfigurationManager.AppSettings["AccessTokenURL"];
                string ticketURL = System.Web.Configuration.WebConfigurationManager.AppSettings["TicketURL"];
                if (string.IsNullOrEmpty(SessionManager.Ticket))
                {
                    //开始获取access_token
                    HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(string.Format("{0}?grant_type=client_credential&appid={1}&secret={2}", accessTokenURL, appId, appSecret));
                    HttpWebResponse reponse = (HttpWebResponse)request.GetResponse();
                    using (TextReader reader = new StreamReader(reponse.GetResponseStream()))
                    {
                        //JsonResult js= Json(reader.ReadToEnd(), JsonRequestBehavior.AllowGet);
                        WXAccessTokenJson wtj = new WXAccessTokenJson();
                        wtj = (WXAccessTokenJson)JsonToObject(reader.ReadToEnd(), wtj);
                        //将access_token放到Session中，方便下次调用
                        //VoBao.Base.Common.File_Ext.CreateFile(@"/log/1.txt", "wtj.access_token:" + wtj.access_token);
                        SessionManager.AccessToken = wtj.access_token;
                        //开始获取jsapi_ticket
                        request = (HttpWebRequest)HttpWebRequest.Create(string.Format("{0}?access_token={1}&type=jsapi", ticketURL, wtj.access_token));
                        reponse = (HttpWebResponse)request.GetResponse();
                        TextReader reader2 = new StreamReader(reponse.GetResponseStream());
                        WXAccessTokenJson wtj2 = new WXAccessTokenJson();
                        wtj2 = (WXAccessTokenJson)JsonToObject(reader2.ReadToEnd(), wtj2);
                        SessionManager.Ticket = wtj2.ticket;

                    }

                }
                if (!string.IsNullOrEmpty(SessionManager.Ticket))
                {
                    //String noncestr = Random.randomAlphanumeric(16);
                    //int timestamp = 0;
                    //string appId = "wxaa58982117696428";
                    string nonceStr = string.Empty, signature = string.Empty;
                    //开始获取sha1 签名字符串
                    nonceStr = Utils.NewUID().ToString();
                    //timestamp = ConvertDateTimeInt(DateTime.Now);
                    StringBuilder sbStr = new StringBuilder();
                    sbStr.AppendFormat("jsapi_ticket={0}", SessionManager.Ticket);
                    sbStr.AppendFormat("&noncestr={0}", nonceStr);
                    sbStr.AppendFormat("&timestamp={0}", timeStamp);
                    sbStr.AppendFormat("&url={0}", url);

                    byte[] cleanBytes = Encoding.Default.GetBytes(sbStr.ToString());
                    byte[] hashedBytes = System.Security.Cryptography.SHA1.Create().ComputeHash(cleanBytes);
                    signature = BitConverter.ToString(hashedBytes).Replace("-", "");
                    return new JsonResult { Data = new { Flag = 1, appId = appId, timestamp = timeStamp, nonceStr = nonceStr, signature = signature } };
                }
                else
                {
                    return new JsonResult { Data = new { Flag = -1, Message = "Ticket为空，可能会造成分享朋友圈失败，请尝试退出微信后重新进入" } };
                }
            }
            catch (Exception ex)
            {
                return new JsonResult { Data = new { Flag = -1, Message = "Exception，可能会造成分享朋友圈失败，请尝试退出微信后重新进入" } };
            }
        } 
        #endregion

        /// <summary>  
        /// DateTime时间格式转换为Unix时间戳格式  
        /// </summary>  
        /// <param name="time"> DateTime时间格式</param>  
        /// <returns>Unix时间戳格式</returns>  
        public static int ConvertDateTimeInt(System.DateTime time)
        {
            System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1));
            return (int)(time - startTime).TotalSeconds;
        }

        #region --网页授权无效
        /// <summary>
        /// 微信获取Code回调URL地址
        /// 通过Code获取用户Open_ID
        /// </summary>
        /// <param name="code"></param>
        /// <param name="ReturnUrl"></param>
        /// <returns></returns>
        [AllowAnonymous]
        public void VerificationPage(string code, string ReturnUrl = "")
        {
            //VoBao.Base.Common.File_Ext.CreateFile(@"/log/1.txt", "ReturnUrl1:" + ReturnUrl);
            try
            {
                if (!string.IsNullOrEmpty(ReturnUrl) && !ReturnUrl.ToLower().Contains("http:"))
                {
                    byte[] outputb = Convert.FromBase64String(ReturnUrl);
                    ReturnUrl = Encoding.Default.GetString(outputb);
                    //VoBao.Base.Common.File_Ext.CreateFile(@"/log/1.txt", "code:" + code);
                }
                //VoBao.Base.Common.File_Ext.CreateFile(@"/log/1.txt", "ReturnUrl1:" + ReturnUrl);
                if (!string.IsNullOrEmpty(code))
                {
                    HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(string.Format(System.Web.Configuration.WebConfigurationManager.AppSettings["WXUserInfo"], code).Replace("$", "&"));
                    HttpWebResponse reponse = (HttpWebResponse)request.GetResponse();
                    using (TextReader reader = new StreamReader(reponse.GetResponseStream()))
                    {
                        WXSecondJson wxj = new WXSecondJson();
                        WXSecondJson item = (WXSecondJson)JsonToObject(reader.ReadToEnd(), wxj);
                        if (item != null && !string.IsNullOrEmpty(item.openid))
                        {
                            SessionManager.OpenID = item.openid;
                            //VoBao.Base.Common.File_Ext.CreateFile(@"/log/1.txt", "ReturnUrl:" + ReturnUrl);
                            Response.Redirect(ReturnUrl);
                        }
                    }
                }

            }
            catch (Exception exx)
            {
                // Logger.LogInstance.Write("yanghaijun:" + exx, MessageType.Error);
                //SessionManager.Code = exx.Message;
            }
        }
        // 从一个Json串生成对象信息
        public static object JsonToObject(string jsonString, object obj)
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(obj.GetType());
            MemoryStream mStream = new MemoryStream(Encoding.UTF8.GetBytes(jsonString));
            return serializer.ReadObject(mStream);
        }

        /// <summary>
        /// 模拟微信接口，获取Code
        /// </summary>
        /// <param name="ReturnUrl"></param>
        public void WXApiTestGetCode(string ReturnUrl)
        {
            Response.Redirect("http://XXX.com/u/seller/VerificationPage/?code=11234567890&state=STATE&ReturnUrl=" + ReturnUrl);
        }
        /// <summary>
        /// 模拟微信接口，获取OpenID
        /// </summary>
        public void WXApiTestGetOpenID()
        {
            Response.Write("{\"access_token\":\"ACCESS_TOKEN\",\"expires_in\":7200,\"refresh_token\":\"REFRESH_TOKEN\",\"openid\":\"ohJ2LjnwzSd2vycc-5Uj9u6-GDRY\",\"scope\":\"SCOPE\"}");
        }

        // 此方法 仅用于第一次验证
        //[AllowAnonymous]
        //public string WXVerification(string signature, string timestamp, string nonce, string echostr)
        //{
        //    return echostr;
        //}
        
        #endregion


        #region 无效
        ////创建菜单获取AccessToken
        //public void AccessAccToken()
        //{
        //    Utils.CreateFile(@"/log/token.txt", "1.开始...");
        //    DateTime expireTime;
        //    if (SessionManager.TokenExpireTime == null)
        //    {
        //        expireTime = DateTime.Now.AddYears(-100);
        //    }
        //    else
        //    {
        //        expireTime = DateTime.Parse(SessionManager.TokenExpireTime);
        //    }
        //    TimeSpan span = DateTime.Now - expireTime;
        //    int leftSeconds = span.Seconds;
        //    Utils.CreateFile(@"/log/token.txt", "1.11 空了没？？...");
        //    if (SessionManager.AccessToken == null || SessionManager.AccessToken != null && leftSeconds > 7000)
        //    {
        //        Utils.CreateFile(@"/log/token.txt", "2.为空...");
        //        try
        //        {
        //            //开始获取access_token
        //            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create("https://api.weixin.qq.com/cgi-bin/token?grant_type=client_credential&appid=wxa1ddf49daf3a8856&secret=b3eab302feba8ea9e4c9e865bf801310");
        //            HttpWebResponse reponse = (HttpWebResponse)request.GetResponse();
        //            using (TextReader reader = new StreamReader(reponse.GetResponseStream()))
        //            {
        //                //JsonResult js= Json(reader.ReadToEnd(), JsonRequestBehavior.AllowGet);
        //                WXAccessTokenJson wtj = new WXAccessTokenJson();
        //                wtj = (WXAccessTokenJson)JsonToObject(reader.ReadToEnd(), wtj);
        //                //将access_token放到Session中，方便下次调用
        //                Utils.CreateFile(@"/log/token.txt", "3.token：" + wtj.access_token);
        //                SessionManager.AccessToken = wtj.access_token;
        //                Utils.CreateFile(@"/log/token.txt", "4.token还活着没：" + SessionManager.AccessToken);
        //                SessionManager.TokenExpireTime = DateTime.Now.ToString();
        //                //开始获取jsapi_ticket
        //                //request = (HttpWebRequest)HttpWebRequest.Create(string.Format("https://api.weixin.qq.com/cgi-bin/ticket/getticket?access_token={0}&type=jsapi", wtj.access_token));
        //                //reponse = (HttpWebResponse)request.GetResponse();
        //                //TextReader reader2 = new StreamReader(reponse.GetResponseStream());
        //                //WXAccessTokenJson wtj2 = new WXAccessTokenJson();
        //                //wtj2 = (WXAccessTokenJson)JsonToObject(reader2.ReadToEnd(), wtj2);
        //                //SessionManager.Ticket = wtj2.ticket;
        //                // return RedirectToAction("GetShai");
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            // Logger.LogInstance.Write(ex, MessageType.Error);
        //            // return new JsonResult { Data = new { Flag = -1 } };
        //        }
        //    }
        //    Utils.CreateFile(@"/log/token.txt", "5.获取token结束：");

        //}
        // 自定义消息回复开发 
        #endregion


        public void WXVerification()
        {
            Utils.CreateFile(@"/log/Oauth22.txt", "wexin进来了...：");
            string varToken = "";

            #region --判断session 是否过期 7200秒
            //DateTime expireTime;
            //if (SessionManager.TokenExpireTime == null)
            //{
            //    expireTime = DateTime.Now.AddYears(-100);
            //}
            //else
            //{
            //    expireTime = DateTime.Parse(SessionManager.TokenExpireTime);
            //}
            //TimeSpan span = DateTime.Now - expireTime;
            //int leftSeconds = span.Seconds;
            //if (string.IsNullOrEmpty(SessionManager.AccessToken) || (string.IsNullOrEmpty(SessionManager.AccessToken) && leftSeconds > 7000))
            //{
            //    // token 已过期，重新获取
            //}
            #endregion

            //获取AccessToken
            #region --获取AccessToken
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create("https://api.weixin.qq.com/cgi-bin/token?grant_type=client_credential&appid=wxa1ddf49daf3a8856&secret=b3eab302feba8ea9e4c9e865bf801310");
            HttpWebResponse reponse = (HttpWebResponse)request.GetResponse();
            using (TextReader reader = new StreamReader(reponse.GetResponseStream()))
            {
                WXAccessTokenJson wtj = new WXAccessTokenJson();
                wtj = (WXAccessTokenJson)JsonToObject(reader.ReadToEnd(), wtj);
                //将access_token放到Session中，方便下次调用
                SessionManager.AccessToken = wtj.access_token;
                varToken = wtj.access_token;
                SessionManager.TokenExpireTime = DateTime.Now.ToString();
            } 
            #endregion

            //创建微信菜单
            #region -- 创建微信菜单
            FileStream ffss = new FileStream(HttpContext.Server.MapPath(@"/menu.txt"), FileMode.Open);
            StreamReader sr = new StreamReader(ffss, Encoding.GetEncoding("GBK"));
            string menu = sr.ReadToEnd();
            sr.Close();
            ffss.Close();
            string rr = GetPage("https://api.weixin.qq.com/cgi-bin/menu/create?access_token=" + varToken, menu);
            
            #endregion

            //发消息..............

            Utils.CreateFile(@"/log/Oauth22.txt", "消息开始...：" );
            #region --发送消息
            string weixin = "";
            weixin = PostInput();//获取xml数据
            if (!string.IsNullOrEmpty(weixin))
            {
                Utils.CreateFile(@"/log/Oauth22.txt", "消息meiyou ??");
                ResponseMsg(weixin);//调用消息适配器
            } 
            #endregion
        }

     


        public string GetPage(string posturl, string postData)
        {
            Stream outstream = null;
            Stream instream = null;
            StreamReader sr = null;
            HttpWebResponse response = null;
            HttpWebRequest request = null;
            Encoding encoding = Encoding.UTF8;
            byte[] data = encoding.GetBytes(postData);
            // 准备请求...
            try
            {
                // 设置参数
                request = WebRequest.Create(posturl) as HttpWebRequest;
                CookieContainer cookieContainer = new CookieContainer();
                request.CookieContainer = cookieContainer;
                request.AllowAutoRedirect = true;
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = data.Length;
                outstream = request.GetRequestStream();
                outstream.Write(data, 0, data.Length);
                outstream.Close();
                //发送请求并获取相应回应数据
                response = request.GetResponse() as HttpWebResponse;
                //直到request.GetResponse()程序才开始向目标网页发送Post请求
                instream = response.GetResponseStream();
                sr = new StreamReader(instream, encoding);
                //返回结果网页（html）代码
                string content = sr.ReadToEnd();
                string err = string.Empty;
                Response.Write(content);
                return content;
            }
            catch (Exception ex)
            {
                string err = ex.Message;
                return string.Empty;
            }
        }

        public string SendRequest(Uri uri, string body)
        {
            WebClient wc = new WebClient();
            Encoding enc = Encoding.UTF8;
            return enc.GetString(wc.UploadData(uri, enc.GetBytes(body)));
        }
        private string PostInput()
        {
            Stream s = System.Web.HttpContext.Current.Request.InputStream;
            byte[] b = new byte[s.Length];
            s.Read(b, 0, (int)s.Length);
            return Encoding.UTF8.GetString(b);
        }

        #region 消息类型适配器
        private void ResponseMsg(string weixin)// 服务器响应微信请求
        {
            Utils.CreateFile(@"/log/Oauth22.txt", "消息进来了：" + weixin);
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(weixin);//读取xml字符串
            XmlElement root = doc.DocumentElement;
            ExmlMsg xmlMsg = GetExmlMsg(root);
            //XmlNode MsgType = root.SelectSingleNode("MsgType");
            //string messageType = MsgType.InnerText;
            string messageType = xmlMsg.MsgType;//获取收到的消息类型。文本(text)，图片(image)，语音等。
            SessionManager.OpenID = xmlMsg.FromUserName;
            HttpContext.Session["openID22"] = xmlMsg.FromUserName;
            Utils.CreateFile(@"/log/Oauth22.txt", "sessionID：" + Session.SessionID);
            Utils.CreateFile(@"/log/Oauth22.txt", "消息有啦：" + xmlMsg.FromUserName+"_"+xmlMsg.EventName);
            
            try
            {

                switch (messageType)
                {
                    //当消息为文本时
                    case "text":
                        if (xmlMsg.Content == "letaonc_chushen")
                        {
                            Utils.CreateFile(@"/log/Oauth33.txt", "菜单收到key：" + xmlMsg.Content);
                            HttpContext.Response.Redirect("http://www.700195.com/home/index");
                        }
                        else
                        {
                            Utils.CreateFile(@"/log/Oauth33.txt", "菜单收到key但是不对啊：" + xmlMsg.Content);
                            textCase(xmlMsg);
                        }

                        break;
                    case "event":
                        if (!string.IsNullOrEmpty(xmlMsg.EventName) && xmlMsg.EventName.Trim().ToLower() == "subscribe")
                        {
                            //刚关注时的时间，用于欢迎词  
                            int nowtime = ConvertDateTimeInt(DateTime.Now);
                            string msg = "民以食为天，吃货站中间\n\n您可以选择的模式：\n√ 1：进入\"大厨当道趣味报名\" \n√ 2：进入\"两点疯狂抢惊喜\" \n√ 3：进入\"超人气赛--最淘妹\" \n√ 4：\"开机必做的头等大事\" \n\n动起你的手指来操作吧！";
                            string resxml = "<xml><ToUserName><![CDATA[" + xmlMsg.FromUserName + "]]></ToUserName><FromUserName><![CDATA[" + xmlMsg.ToUserName + "]]></FromUserName><CreateTime>" + nowtime + "</CreateTime><MsgType><![CDATA[text]]></MsgType><Content><![CDATA[" + msg + "]]></Content><FuncFlag>0</FuncFlag></xml>";
                            SessionManager.OpenID = xmlMsg.FromUserName;
                            Utils.CreateFile(@"/log/Oauth22.txt", xmlMsg.FromUserName+"__"+xmlMsg.EventKey+"__"+xmlMsg.MsgType);
                            Response.Write(resxml);
                        }

                        if (!string.IsNullOrEmpty(xmlMsg.EventName) && xmlMsg.EventName.Trim().ToLower() == "view" && xmlMsg.EventKey.Trim() == "http://www.700195.com/home/index")
                        {
                            Utils.CreateFile(@"/log/Oauth33.txt", "菜单收到Click的key：" + xmlMsg.EventKey + "session:" + SessionManager.OpenID);
                        }
                        break;
                    case "image":
                        break;
                    case "voice":
                        break;
                    case "vedio":
                        break;
                    case "location":
                        break;
                    case "link":
                        break;
                    default:
                        break;
                }
                Response.End();
            }
            catch (Exception)
            {

            }
        }
        #endregion

        private string getText(ExmlMsg xmlMsg)
        {
            string con = xmlMsg.Content.Trim();

            System.Text.StringBuilder retsb = new StringBuilder(200);


            SessionManager.OpenID = xmlMsg.FromUserName;
            retsb.Append("谢谢您的消息！\n 我们正紧锣密鼓地开发中,敬请期待！");
            retsb.Append("接收到的消息：" + xmlMsg.Content);
            retsb.Append("正在开发中 ...：" + xmlMsg.FromUserName);
            Utils.CreateFile(@"/log/Oauth.txt", xmlMsg.Content + "ResponseMsg 最新 openid:" + xmlMsg.FromUserName);
            return retsb.ToString();
        }


        #region 操作文本消息 + void textCase(XmlElement root)
        private void textCase(ExmlMsg xmlMsg)
        {
            int nowtime = ConvertDateTimeInt(DateTime.Now);
            string msg = "";
            msg = getText(xmlMsg);
            SessionManager.OpenID = xmlMsg.FromUserName;

            Utils.CreateFile(@"/log/Oauth33.txt", xmlMsg.Content + "菜单 获取到openid:" + xmlMsg.FromUserName);
            string resxml = "<xml><ToUserName><![CDATA[" + xmlMsg.FromUserName + "]]></ToUserName><FromUserName><![CDATA[" + xmlMsg.ToUserName + "]]></FromUserName><CreateTime>" + nowtime + "</CreateTime><MsgType><![CDATA[text]]></MsgType><Content><![CDATA[" + msg + "]]></Content><FuncFlag>0</FuncFlag></xml>";
            Response.Write(resxml);

        }
        #endregion

        #region 将datetime.now 转换为 int类型的秒
        /// <summary>
        /// datetime转换为unixtime
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        //private int ConvertDateTimeInt(System.DateTime time)
        //{
        //    System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1));
        //    return (int)(time - startTime).TotalSeconds;
        //}
        //private int converDateTimeInt(System.DateTime time)
        //{
        //    System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1));
        //    return (int)(time - startTime).TotalSeconds;
        //}

        /// <summary>
        /// unix时间转换为datetime
        /// </summary>
        /// <param name="timeStamp"></param>
        /// <returns></returns>
        private DateTime UnixTimeToTime(string timeStamp)
        {
            DateTime dtStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            long lTime = long.Parse(timeStamp + "0000000");
            TimeSpan toNow = new TimeSpan(lTime);
            return dtStart.Add(toNow);
        }
        #endregion

        #region 验证微信签名 保持默认即可
        /// <summary>
        /// 验证微信签名
        /// </summary>
        /// * 将token、timestamp、nonce三个参数进行字典序排序
        /// * 将三个参数字符串拼接成一个字符串进行sha1加密
        /// * 开发者获得加密后的字符串可与signature对比，标识该请求来源于微信。
        /// <returns></returns>
        //private bool CheckSignature()
        //{
        //    string signature = Request.QueryString["signature"].ToString();
        //    string timestamp = Request.QueryString["timestamp"].ToString();
        //    string nonce = Request.QueryString["nonce"].ToString();
        //    string[] ArrTmp = { Token, timestamp, nonce };
        //    Array.Sort(ArrTmp);     //字典排序
        //    string tmpStr = string.Join("", ArrTmp);
        //    tmpStr = FormsAuthentication.HashPasswordForStoringInConfigFile(tmpStr, "SHA1");
        //    tmpStr = tmpStr.ToLower();
        //    if (tmpStr == signature)
        //    {
        //        return true;
        //    }
        //    else
        //    {
        //        return false;
        //    }
        //}

        //private void Valid()
        //{
        //    string echoStr = Request.QueryString["echoStr"].ToString();
        //    if (CheckSignature())
        //    {
        //        if (!string.IsNullOrEmpty(echoStr))
        //        {
        //            Response.Write(echoStr);
        //            Response.End();
        //        }
        //    }
        //}
        #endregion

        #region 写日志(用于跟踪) ＋　WriteLog(string strMemo, string path = "*****")
        /// <summary>
        /// 写日志(用于跟踪)
        /// 如果log的路径修改,更改path的默认值
        /// </summary>
        //private void WriteLog(string strMemo, string path = "wx.txt")
        //{
        //    string filename = Server.MapPath(path);
        //    StreamWriter sr = null;
        //    try
        //    {
        //        if (!File.Exists(filename))
        //        {
        //            sr = File.CreateText(filename);
        //        }
        //        else
        //        {
        //            sr = File.AppendText(filename);
        //        }
        //        sr.WriteLine(strMemo);
        //    }
        //    catch
        //    {

        //    }
        //    finally
        //    {
        //        if (sr != null)
        //            sr.Close();
        //    }
        //}
        //#endregion 
        #endregion

        #region 接收的消息实体类 以及 填充方法
        private class ExmlMsg
        {
            /// <summary>
            /// 本公众账号
            /// </summary>
            public string ToUserName { get; set; }
            /// <summary>
            /// 用户账号
            /// </summary>
            public string FromUserName { get; set; }
            /// <summary>
            /// 发送时间戳
            /// </summary>
            public string CreateTime { get; set; }
            /// <summary>
            /// 发送的文本内容
            /// </summary>
            public string Content { get; set; }
            /// <summary>
            /// 消息的类型
            /// </summary>
            public string MsgType { get; set; }
            /// <summary>
            /// 事件名称
            /// </summary>
            public string EventName { get; set; }

            public string EventKey { get; set; }

        }

        private ExmlMsg GetExmlMsg(XmlElement root)
        {
            ExmlMsg xmlMsg = new ExmlMsg()
            {
                FromUserName = root.SelectSingleNode("FromUserName").InnerText,
                ToUserName = root.SelectSingleNode("ToUserName").InnerText,
                CreateTime = root.SelectSingleNode("CreateTime").InnerText,
                MsgType = root.SelectSingleNode("MsgType").InnerText,

            };
            if (xmlMsg.MsgType.Trim().ToLower() == "text")
            {
                xmlMsg.Content = root.SelectSingleNode("Content").InnerText;
            }
            else if (xmlMsg.MsgType.Trim().ToLower() == "event")
            {
                xmlMsg.EventName = root.SelectSingleNode("Event").InnerText;
                xmlMsg.EventKey = root.SelectSingleNode("EventKey").InnerText;
            }
            return xmlMsg;
        }
        #endregion



    }
}
