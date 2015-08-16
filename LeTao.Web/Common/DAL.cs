using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Text;
using System.Data.SqlClient;
using LeTao.Web.Common;


namespace LeTao.Web.Common
{
    public class DAL
    {
        public int InsertInsertImg(long imgID, long userID, string imgName)
        {

            SqlParameter[] parms ={
                                      new SqlParameter("@imgID",SqlDbType.BigInt,8),
                                     new SqlParameter("@userID",SqlDbType.BigInt,8),
                                     new SqlParameter("@imgName",SqlDbType.VarChar,30)
                                    

                                 };
            parms[0].Value = imgID;
            parms[1].Value = userID;
            parms[2].Value = imgName;

           return SQLHelper.ExecuteNonQuery("InsertUserImages", CommandType.StoredProcedure, parms);
        }

        public int InsertUserInfo(long userID, string openID, string mobile, string userName, string jobName, string remark, string userImage, int points, int userTP)
        {

            SqlParameter[] parms ={
                                     new SqlParameter("@userID",SqlDbType.BigInt,8),
                                     new SqlParameter("@mobile",SqlDbType.VarChar,20),
                                     new SqlParameter("@userName",SqlDbType.VarChar,30),
                                      new SqlParameter("@jobName",SqlDbType.VarChar,30),
                                     new SqlParameter("@remark",SqlDbType.VarChar,200),
                                     new SqlParameter("@imgName",SqlDbType.VarChar,30),
                                      new SqlParameter("@points",SqlDbType.Int,4),
                                     new SqlParameter("@openID",SqlDbType.VarChar,25),
                                     new SqlParameter("@userTP",SqlDbType.Int)
                                    

                                 };
            parms[0].Value = userID;
            parms[1].Value = mobile;
            parms[2].Value = userName;
            parms[3].Value = jobName;
            parms[4].Value = remark;
            parms[5].Value = userImage;
            parms[6].Value = points;
            parms[7].Value = openID;
            parms[8].Value = userTP;

            return SQLHelper.ExecuteNonQuery("DoEnroll", CommandType.StoredProcedure, parms);
        }

        public bool UpdateZanNum(long userID, string openID)
        {
            string sqlStr = string.Format("insert into Votes (userID,openID,addTimne) values ('{0}','{1}','{2}')", userID, openID, DateTime.Now);
            DBHelper.ExecuteNonQuery(sqlStr);
            return false;
        }

        public DataTable GetUserList()
        {
            string sqlStr = "select * from UserInfo ";
            return DBHelper.ExecuteQuery(sqlStr);
        }

        public int GetScale(string str)
        {
            return DBHelper.ExecuteScalar(str);
        }

        public string GetStrIDs(string str, int passCount)
        {
            return DBHelper.GetStrIDs(str, passCount);
        }

        public DataTable GetDataTable(string str)
        {
            return DBHelper.ExecuteQuery(str);
        }

        public int DoNoQuery(string str)
        {
            return DBHelper.ExecuteNonQuery(str);
        }


        //SQL server 

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sqlStr"></param>
        /// <param name="cmdTp"> 1语句 2过程</param>
        /// <returns></returns>
        public DataTable Query(string sqlStr, int cmdTp)
        {
            SqlParameter[] parms = { };
            if (cmdTp == 1) //text
            {
                return SQLHelper.ExecuteQuery(sqlStr, CommandType.Text);
            }
            else
            {
                return SQLHelper.ExecuteQuery(sqlStr, CommandType.StoredProcedure);
            }
        }

        public int UpdateVisitNum(long IP, string openID, int points, long uuid)
        {
            SqlParameter[] parms ={
                                     new SqlParameter("@IP",SqlDbType.BigInt,8),
                                     new SqlParameter("@openID",SqlDbType.VarChar,25),
                                     new SqlParameter("@points",SqlDbType.Int,4),
                                     new SqlParameter("@uuid",SqlDbType.BigInt,8)
                                 };
            parms[0].Value = IP;
            parms[1].Value = openID;
            parms[2].Value = points;
            parms[3].Value = uuid;

            return SQLHelper.ExecuteNonQuery("InsertVisits", CommandType.StoredProcedure, parms);
        }

        public void GetAllKindNum(out int enrollNum, out int votesNum, out int totalVisits)
        {
            DataTable dt = SQLHelper.ExecuteQuery("GetKindsOfNum", CommandType.StoredProcedure);
            if (dt != null && dt.Rows.Count > 0 && dt.Rows.Count == 1)
            {
                enrollNum = (int)dt.Rows[0]["enrollNum"];
                votesNum = (int)dt.Rows[0]["votesNum"];
                totalVisits = (int)dt.Rows[0]["totalVisits"];

            }
            else
            {
                enrollNum = 0;
                votesNum = 0;
                totalVisits = 0;
            }
        }

        public DataTable GetUserList(int pageIndex, int pageSize, int orderTp, string keyword, out int pageCount, out int totalCount)
        {
            SqlParameter[] parms ={
                                     new SqlParameter("@keyWord",SqlDbType.NVarChar,50),
                                     new SqlParameter("@orderTP",SqlDbType.Int),
                                     new SqlParameter("@pageIndex",SqlDbType.Int),
                                     new SqlParameter("@pageSize",SqlDbType.Int),
                                     new SqlParameter("@totalPage",SqlDbType.Int),
                                     new SqlParameter("@totalCount",SqlDbType.Int)

                                 };
            if (string.IsNullOrEmpty(keyword))
            {
                parms[0].Value = DBNull.Value;
            }
            else
            {
                parms[0].Value = keyword;
            }

            parms[1].Value = orderTp;
            parms[2].Value = pageIndex;
            parms[3].Value = pageSize;
            //parms[4].Value = pageCount;
            //parms[5].Value = totalCount;
            parms[4].Direction = ParameterDirection.Output;
            parms[5].Direction = ParameterDirection.Output;

            DataTable dt = SQLHelper.ExecuteQuery("GetUserInfoList", CommandType.StoredProcedure, parms);
            pageCount = (int)parms[4].Value;
            totalCount = (int)parms[5].Value;
            return dt;
        }

        public void CheckOpenIDPerday(string openID, int num, out int result)
        {
            SqlParameter[] parms ={
                                     new SqlParameter("@openID",SqlDbType.VarChar,25),
                                     new SqlParameter("@num",SqlDbType.Int),
                                     new SqlParameter("@returnVal",SqlDbType.Int)
                                    

                                 };
            parms[0].Value = openID;
            parms[1].Value = num;
            parms[2].Direction = ParameterDirection.Output;

            SQLHelper.ExecuteNonQuery("CheckOpenIDPerday", CommandType.StoredProcedure, parms);
            result = (int)parms[2].Value;

        }

        public void DoVote(long userID, long IP, string openID,int points,long uuid,out int result)
        {
            SqlParameter[] parms ={
                                      new SqlParameter("@userID",SqlDbType.BigInt,8),
                                     new SqlParameter("@IP",SqlDbType.BigInt,8),
                                      new SqlParameter("@openID",SqlDbType.VarChar,25),
                                     new SqlParameter("@points",SqlDbType.Int,4),
                                      new SqlParameter("@uuid",SqlDbType.BigInt,8),
                                     new SqlParameter("@returnVal",SqlDbType.Int)
                                    

                                 };
            parms[0].Value = userID;
            parms[1].Value = IP;
            parms[2].Value = openID;
            parms[3].Value = points;
            parms[4].Value = uuid;
            parms[5].Direction = ParameterDirection.Output;

            SQLHelper.ExecuteNonQuery("DoVote", CommandType.StoredProcedure, parms);
            result = (int)parms[5].Value;
        }

        public DataTable GetUserInfoDetail(long userID)
        {
            string sqlStr = string.Format("select * from ( select  ROW_NUMBER() OVER(Order by u.totalVotes DESC ) AS Rank,"
                + "* from UserInfo as u ) t where t.userID ={0}", userID);

            return SQLHelper.ExecuteQuery(sqlStr);
        }

        public DataTable GetImgListByUserID(long userID)
        {
            string sqlStr = string.Format("select * from UserImages where userID={0} ", userID);
            return SQLHelper.ExecuteQuery(sqlStr);
        }

        public void CheckTelOrOpenID(string tel, string openID, out int result)
        {
            SqlParameter[] parms ={
                                     new SqlParameter("@tel",SqlDbType.VarChar,20),
                                     new SqlParameter("@openID",SqlDbType.VarChar,25),
                                     new SqlParameter("@returnVal",SqlDbType.Int)
                                    

                                 };
            parms[0].Value = tel;
            parms[1].Value = openID;
            parms[2].Direction = ParameterDirection.Output;

            SQLHelper.ExecuteNonQuery("CheckTelIsEnrolled", CommandType.StoredProcedure, parms);
            result = (int)parms[2].Value;
        }

    }
}
