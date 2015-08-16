using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Xml;
using System.Web;
using System.IO;
using System.Web.Mvc;
using LeTao.Web.Common;
using LeTao.Web.Models;
using System.Configuration;
using System.Text;

namespace LeTao.Web.Controllers
{
    public class HomeController : Controller
    {
        protected BLL bll = new BLL();
        public ActionResult Index(int tp = 0, int page = 1, string keyword = "")
        {
            Utils.CreateFile(@"/log/Oauth33.txt", "home 进来了");
            //FileStream fs1 = new FileStream(Server.MapPath(".") + "\\menumenu.txt", FileMode.Open);
            //StreamReader sr = new StreamReader(fs1, Encoding.GetEncoding("GBK"));
            //string menu = sr.ReadToEnd();
            //ViewBag.openIDFyh = menu;
            
            Utils.CreateFile(@"/log/Oauth33.txt", "home 进来了：session 有没有：" +Session.SessionID);
            Utils.CreateFile(@"/log/Oauth33.txt", "home 空");
            string strOpenID = "";
            //if (string.IsNullOrEmpty(SessionManager.OpenID))
            //{
            //    SessionManager.OpenID=Utils.NewUID().ToString();
            //}
            //else
            //{
            //    strOpenID= SessionManager.OpenID;
            //}
            //SessionManager.OpenID = "123";

            int orderTp = 1;
            if (tp == 1)
            {
                //totalVotes desc 
                orderTp = 0;
            }
            else
            {
                //addTime desc
                orderTp = 1;
            }
            ViewBag.tp = tp;
            int pageCount = 0;
            int totalCount = 0;
            int pageSize = int.Parse(ConfigurationManager.AppSettings["NumberShowEachPage"].ToString());
            if (!string.IsNullOrEmpty(keyword))
            {
                pageSize = 10000;
            }
            DataTable dt = bll.GetUserList(page, pageSize, orderTp, keyword, out pageCount, out totalCount);
            int enrollNum = 0; int voteNum = 0; int TotalVisitedNum = 0;
            bll.GetAllKindNum(out enrollNum, out voteNum, out TotalVisitedNum);

            //添加访问量
            //long longIP = IP.getIP_256(IP.GetWanIp());
            //int iii = bll.UpdateVisitNum(longIP, SessionManager.OpenID.ToString(), 0, Utils.NewUID());

            ViewBag.TotalVisitedNum = TotalVisitedNum;
            ViewBag.enrollNum = enrollNum;
            ViewBag.voteNum = voteNum;
            ViewBag.pageCount = pageCount;
            return View(dt);
        }

        public ActionResult Enroll()
        {
            ModelEnroll model = new ModelEnroll();
            model.MaxPhotoUploadNumber = int.Parse(ConfigurationManager.AppSettings["MaxPhotoUploadNumber"].ToString());
            return View(model);
        }

        [HttpPost]
        public ActionResult Enroll(ModelEnroll model)
        {
            if (ModelState.IsValid)
            {
                long userID = Utils.NewUID();
                string userImage = "";
                try
                {
                    //判断手机号是否报名
                    // string sql = string.Format("select count(1) from UserInfo where mobile ='{0}' ", model.mobile.Trim());
                    int alreadyNum = -1;
                    bll.CheckTelOrOpenID(model.mobile, SessionManager.OpenID, out alreadyNum);
                    if (alreadyNum < 0)
                    {
                        return View(model);
                    }
                    else
                    {
                        int index = 0;
                        foreach (string upload in Request.Files)
                        {
                            if (!Request.Files[upload].HasFile()) continue;
                            index++;
                            string path = AppDomain.CurrentDomain.BaseDirectory + ConfigurationManager.AppSettings["UploadImgPath"].ToString();
                            string filename = Path.GetFileName(Request.Files[upload].FileName);
                            string fileType = filename.Substring(filename.LastIndexOf('.') + 1);
                            //string fileName2 = Path.GetFileName(Request.Files[upload].ContentType);
                            string newFileName = Utils.NewUID() + "." + fileType;
                            Request.Files[upload].SaveAs(Path.Combine(path, newFileName));

                            // 插入到数据库
                            int imgResult = bll.InsertImg(Utils.NewUID(), userID, newFileName);
                            if (index == 1)
                            {
                                userImage = newFileName;
                            }
                        }
                        long longIP = IP.getIP_256(IP.GetWanIp());
                        int userResult = bll.InsertUserInfo(userID, SessionManager.OpenID, model.mobile, model.userName, model.jobName, string.IsNullOrEmpty(model.remarks) ? "" : model.remarks, userImage, 0, 1);
                        return RedirectToAction("Detail", new { userid = userID });
                    }

                }
                catch (Exception ex)
                {
                    Utils.CreateFile(@"/log/Home_Enroll.txt", ex.Message);
                    return View();
                }

            }
            else
            {
                return View(model);
            }

        }

        public ActionResult Detail(long userID)
        {
            // string sqlStr = "select top 1 * from UserInfo where userID=" + userid ;
            DataTable dt = bll.GetUserInfoDetail(userID);

            DataTable imgList = bll.GetImgListByUserID(userID);
            ViewData["ImgList"] = imgList;
            return View(dt);
        }


        public ActionResult UpdateVotesCount(long userID)
        {
            long longIP = IP.getIP_256(IP.GetWanIp());
            int perdayNum = int.Parse(ConfigurationManager.AppSettings["NumberVotesPerDay"].ToString());
            int num = 0;
            bll.CheckOpenIDPerday(SessionManager.OpenID, perdayNum, out num);
            if (num < 0)
            {
                return Json(new { Flag = -1, Message = "今天已投过，请明天再来" }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                try
                {
                    int result = -1;
                    bll.DoVote(userID, longIP, SessionManager.OpenID, 0, Utils.NewUID(), out result);
                    if (result >= 0)
                    {
                        return Json(new { Flag = 1, Message = "投票成功" }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(new { Flag = -1, Message = "投票失败,请重试" }, JsonRequestBehavior.AllowGet);
                    }

                }
                catch (Exception ex)
                {
                    Utils.CreateFile(@"/log/Home_UpdateVotesCount.txt", ex.Message);
                    return Json(new { Flag = -1, Message = "投票出错，请重试" }, JsonRequestBehavior.AllowGet);
                }
            }

        }

        public ActionResult My()
        {

            string str222 = "delete from UserInfo";
            bll.DoNoQuery(str222);

            string str22 = "delete from UserImages";
            bll.DoNoQuery(str22);

            string str2 = "delete from UserVisits";
            bll.DoNoQuery(str2);

            string str = "delete from Votes";
            bll.DoNoQuery(str);

            return HttpNotFound();
        }

        public void DoTest()
        {
            Utils.CreateFile(@"/log/Home_Enroll.txt", "456789");
            Response.Write("hello789");
        }



    }




}
