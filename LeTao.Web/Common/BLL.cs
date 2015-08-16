using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LeTao.Web.Common;
using System.Data;

namespace LeTao.Web.Common
{
    public class BLL
    {
        protected DAL dal = new DAL();
        public int  InsertImg(long imgID, long userID, string imgName)
        {
            return dal.InsertInsertImg(imgID, userID, imgName);
        }

        public int InsertUserInfo(long userID, string openID, string mobile, string userName, string jobName, string remark, string userImage, int points, int userTP)
        {
            return dal.InsertUserInfo(userID, openID, mobile, userName, jobName, remark, userImage,points ,userTP);
        }

        public DataTable GetUserList(int pageIndex, int pageSize, string where, string orderStr, string key,out int pageCount, out int totalCount)
        {
            if (pageIndex < 1) pageIndex = 1;
            if (pageSize < 1) pageSize = 10;

            string strWhere = " where 1=1 ";
            if (!string.IsNullOrEmpty(where))
            {
                strWhere += " and " + where;
            }
            if (!string.IsNullOrEmpty(key))
            {
                strWhere += " and ( userName like '%" + key+"%' or ID like '%"+key+"%' ) ";
            }
            totalCount = dal.GetScale("select count(*) from UserInfo" + strWhere);

            if ((totalCount % pageSize) > 0)
                pageCount = totalCount / pageSize + 1;
            else
                pageCount = totalCount / pageSize;

            string sqlStr = "";
            if (pageIndex == 1) //第一页
            {
                sqlStr = string.Format("select top {0} * from UserInfo {1} order by {2} ", pageSize,  strWhere,orderStr);
            }
            else if (pageIndex > pageCount)
            {
                sqlStr = string.Format("select top {0} * from UserInfo {1} order by {2} ", pageSize, "where 1=2", orderStr);
            }
            else
            {
                int pageLowerBound = pageSize * pageIndex;
                int pageUpperBound = pageLowerBound - pageSize;
               string recordIDs= dal.GetStrIDs(string.Format("select top {0} userID from UserInfo {1} order by {2} ", pageLowerBound, strWhere, orderStr),pageUpperBound);
               // string recordIDs = string.Format("select top {0} userID from UserInfo {1} order by {2} ", pageLowerBound,strWhere,orderStr);
                sqlStr=string.Format("select * from UserInfo where userID in ({0}) order by {1} ", recordIDs,orderStr);
               // sqlStr = "select * from UserInfo where userID in ('825887892402719623','123456789') order by  addTime desc  ";
            }
            return DBHelper.ExecuteQuery(sqlStr);

        }

        public DataTable GetDataTable(string str)
        {
            return dal.GetDataTable(str);
        }

        public int GetScale(string str)
        {
            return dal.GetScale(str);
        }

        //private static string recordID(string query, int passCount)
        //{
        //    using (OleDbConnection m_Conn = new OleDbConnection(ConnectionString))
        //    {
        //        m_Conn.Open();
        //        OleDbCommand cmd = new OleDbCommand(query, m_Conn);
        //        string result = string.Empty;
        //        using (OleDbDataReader dr = cmd.ExecuteReader())
        //        {
        //            while (dr.Read())
        //            {
        //                if (passCount < 1)
        //                {
        //                    result += "," + dr.GetInt32(0);
        //                }
        //                passCount--;
        //            }
        //        }
        //        m_Conn.Close();
        //        m_Conn.Dispose();
        //        return result.Substring(1);
        //    }
        //}

        public int DoNoQuery(string str)
        {
            return dal.DoNoQuery(str);
        }


        public DataTable Query(string sqlStr, int cmdTp)
        {
            return dal.Query(sqlStr,cmdTp);
        }

        public void GetAllKindNum(out int enrollNum, out int votesNum, out int totalVisits)
        {
            dal.GetAllKindNum(out enrollNum,out votesNum,out totalVisits);
        }

        public int UpdateVisitNum(long IP,string openID,int points,long uuid)
        {
            return dal.UpdateVisitNum(IP,openID,points,uuid);
        }

        public DataTable GetUserList(int pageIndex, int pageSize, int orderTp, string keyword, out int pageCount, out int totalCount)
        {
            return dal.GetUserList(pageIndex,pageSize,orderTp,keyword,out pageCount,out totalCount);
        }

        public void CheckOpenIDPerday(string openID, int num, out int result)
        {
            dal.CheckOpenIDPerday(openID, num, out result);
        }


        public void DoVote(long userID, long IP, string openID, int points, long uuid, out int result)
        {
           dal.DoVote(userID,IP,openID,points,uuid,out result);
        }

        public DataTable GetUserInfoDetail(long userID)
        {
            return dal.GetUserInfoDetail(userID);
        }

        public DataTable GetImgListByUserID(long userID)
        {
            return dal.GetImgListByUserID(userID);
        }

        public void CheckTelOrOpenID(string tel, string openID, out int result)
        {
            dal.CheckTelOrOpenID(tel,openID,out result);
        }
    }
}
