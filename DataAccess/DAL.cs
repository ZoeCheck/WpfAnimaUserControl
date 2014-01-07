using System;
using System.Data.SqlClient;
using System.Data;
using System.Collections;

namespace DataAccess
{
    public class DAL
    {
        DbHelperSQL db = new DbHelperSQL();
        string sql;
        public static string connectionString = ConfigSet.GetConfigValue("sqlconn");

        #region 获取基本数据
        public DataSet GetEmpList(string where)
        {
            sql =
                  "select "
                + "case len(css.CodeSenderAddress) "
                + "css.CodeSenderAddress 标识卡,"
                + "ei.EmpName 员工姓名,"
                + "ei.DeptName 员工部门,"
                + "ei.DutyName AS 职务名,"
                + "ei.WorkTypeName 工种,"
                + "riom.InTime AS 入井时间,"
                + "ei.EmpID,"
                + "eli.EmpID "
                + "from Emp_Info ei "
                + "left join CodeSender_Set css on css.UserID = ei.EmpID "
                + "left join RT_InOutMine riom on riom.CsSetID = css.CsSetID "
                + "and css.CodeSenderAddress is not null "
                + where
                + " order by css.CodeSenderAddress";
            try
            {
                return db.Query(sql);
            }
            catch
            {
                return null;
            }
        }

        public DataTable GetDept()
        {
            sql = "select DeptID id,DeptName nm from dbo.Dept_Info order by nm";
            try
            {
                return db.Query(sql).Tables[0];
            }
            catch
            {
                return null;
            }
        }

        public DataTable GetDuty()
        {
            sql = "select DutyID id,DutyName nm from dbo.Duty_Info order by nm";
            try
            {
                return db.Query(sql).Tables[0];
            }
            catch
            {
                return null;
            }
        }

        public DataTable GetWorkType()
        {
            sql = "select WorkTypeID id,replace(WtName,' ','') nm from dbo.WorkType_Info order by nm";
            try
            {
                return db.Query(sql).Tables[0];
            }
            catch
            {
                return null;
            }
        }

        public DataSet GetStationHead()
        {
            sql = "select StationHeadID,StationHeadPlace from dbo.Station_Head_Info";
            return db.Query(sql);
        }

        public void CmdOutMine(DateTime CheckTime, int StationAddress, int StationHeadAddress, bool isFlag, string Cards)
        {
            SqlParameter[] par = {
                                     new SqlParameter("@DetectTime",SqlDbType.DateTime),
                                     new SqlParameter("@StationAddress",SqlDbType.Int),
                                     new SqlParameter("@StationHeadAddress",SqlDbType.Int),
                                     new SqlParameter("@HeadA",SqlDbType.Bit),
                                     new SqlParameter("@HeadB",SqlDbType.Bit),
                                     new SqlParameter("@IsLowerPower",SqlDbType.Bit),
                                     new SqlParameter("@Cards",SqlDbType.VarChar,6000),
                                 };
            par[0].Value = CheckTime;
            par[1].Value = StationAddress;
            par[2].Value = StationHeadAddress;
            par[3].Value = isFlag;
            par[4].Value = false;
            par[5].Value = false;
            par[6].Value = Cards;
            try
            {
                SqlConnection conn = new SqlConnection(connectionString);
                conn.Open();
                SqlCommand comm = new SqlCommand("Wwy_Station_InOutStation", conn);
                comm.CommandType = CommandType.StoredProcedure;
                foreach (SqlParameter b in par)
                {
                    comm.Parameters.Add(b);
                }
                comm.ExecuteNonQuery();

                conn.Close();
            }
            catch
            {
            }
        }

        public DataSet GetStationIDHeadID(string ID)
        {
            sql = "select StationAddress,StationHeadAddress from dbo.Station_Head_Info where StationHeadID=" + ID;
            return db.Query(sql);
        }

        public byte[] GetMapBytesByFileID(string fileid)
        {
            string sqlstr = string.Format("select Fileimg,Filename from G_DPicFile where FileID={0}", fileid);
            DataSet ds = db.Query(sqlstr);

            //Czlt-2012-12-14 假如图形丢失就从备份图层中还原一条记录回来
            if (ds.Tables[0].Rows.Count == 0)
            {
                CzltGetBackFile();
            }
            ds = ds = db.Query(sqlstr);

            if (ds != null)
            {
                return (byte[])ds.Tables[0].Rows[0][0];
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// czlt-2012-11-21 从备份表中读取图形信息
        /// </summary>
        public void CzltGetBackFile()
        {
            string sqlstr = "Proc_GetBackFile";
            SqlParameter[] Parameters = {                     
                                        };

            db.ExecuteSql(sqlstr, Parameters);

        }

        /// <summary>
        /// 获取xml字节数组
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public byte[] GetMapCfgXmlBytes()
        {
            string sqlstr = string.Format("select ConfigFile from G_DConfigFile where filename='{0}'", "新建图形d");
            DataSet ds = db.Query(sqlstr);
            if (ds != null)
                return (byte[])ds.Tables[0].Rows[0][0];
            else
                return null;
        }

        /// <summary>
        /// 得到所有探头信息
        /// </summary>
        /// <returns>信息表</returns>
        public DataTable GetStationInfo()
        {
            string selectstring = "select * from A_Graphics_StationHeadState";
            DataSet ds = db.Query(selectstring);
            if (ds != null)
                return ds.Tables[0];
            else
                return null;
        }

        /// <summary>
        /// 获取历史轨迹
        /// </summary>
        /// <param name="name"></param>
        /// <param name="beginDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public DataTable GetHisInOutStationHead(string name, DateTime beginDate, DateTime endDate)
        {
            string tabName = string.Format("His_InOutStationHead_{0}{1}", beginDate.Year, beginDate.Month);

            string sql = string.Format(
                        "select * from {0} where UserName = '{1}' "
                        + "and InStationHeadTime >= '{2}' "
                        + "and OutStationHeadTime <= '{3}' "
                        + "order by InStationHeadTime",
                        tabName, name, beginDate, endDate
                        );

            return db.Query(sql).Tables[0];
        }

        /// <summary>
        /// 根据人员ID和开始结束日期得到该人员进出分站信息
        /// </summary>
        /// <param name="userid">人员ID</param>
        /// <param name="startDate">开始日期</param>
        /// <param name="endDate">结束日期</param>
        /// <returns>信息表</returns>
        public DataTable GetRouteByUserID(int userid, DateTime startDate, DateTime endDate, int fileid)
        {
            string selectstring = "Shine_HistoryInOutStation_QueryView_ZZHA";
            SqlParameter[] parameters = new SqlParameter[] {
																new SqlParameter("@strTableName", SqlDbType.VarChar, 50),
																new SqlParameter("@intBlock", SqlDbType.Int),
																new SqlParameter("@strName", SqlDbType.VarChar, 20),
																new SqlParameter("@intUserType", SqlDbType.Int),
																new SqlParameter("@strStartDateTime", SqlDbType.VarChar, 50),
																new SqlParameter("@strEndDateTime", SqlDbType.VarChar, 50),
                                                                new SqlParameter("@FileID", SqlDbType.Int)};
            parameters[0].Value = "Shen_HisInOutStationHeadInfo_zdc";
            parameters[1].Value = userid;
            parameters[2].Value = "";
            parameters[3].Value = 0;
            parameters[4].Value = startDate;
            parameters[5].Value = endDate;
            parameters[6].Value = fileid;
            try
            {
                return db.GetDataSet(selectstring, parameters).Tables[0];
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public DataTable GetRoutePointByID(string ID, bool isdesc, string fileid)
        {
            string selectstring = string.Empty;
            if (isdesc)
                selectstring = string.Format("select x,y from G_DPoint where pointid='{0}' and fileid={1} order by [id] desc", ID, fileid);
            else
                selectstring = string.Format("select x,y from G_DPoint where pointid='{0}' and fileid={1} order by [id] asc", ID, fileid);
            return db.Query(selectstring).Tables[0];
        }

        public string GetEmpIDByEmpName(string empName)
        {
            string sql = string.Format("select EmpID from Emp_Info where EmpName = '{0}'", empName);
            return db.Query(sql).Tables[0].Rows[0][0].ToString();
        }

        public DataTable GetEmpList()
        {
            string sql = string.Format("select EmpID,EmpName from emp_info order by EmpName");
            return db.Query(sql).Tables[0];
        }

        public DataTable GetEmpDataTable(string deptName, string dutyName, string woryTypeName, string blockID, string empName)
        {
            string where = "";
            if (deptName != "所有")
            {
                where += string.Format(" and ei.DeptName = '{0}' ", deptName);
            }
            if (dutyName != "所有")
            {
                where += string.Format(" and ei.DutyName = '{0}' ", dutyName);
            }
            if (woryTypeName != "所有")
            {
                where += string.Format(" and ei.WorkTypeName = '{0}' ", woryTypeName);
            }
            if (!blockID.Equals(""))
            {
                where += string.Format(" and css.CodeSenderAddress = '{0}' ", blockID);
            }
            if (!empName.Equals(""))
            {
                where += string.Format(" and ei.empName like '%{0}%' ", empName);
            }

            string sql = string.Format(
                          "select "
                        + "css.CodeSenderAddress 标识卡,"
                        + "ei.EmpName 员工姓名,"
                        + "ei.DeptName 员工部门,"
                        + "ei.DutyName AS 职务名,"
                        + "ei.WorkTypeName 工种, "
                        + "ei.EmpID ID "
                        + "from Emp_Info ei "
                        + "left join CodeSender_Set css on css.UserID = ei.EmpID "
                        + "where 1=1 {0}"
                        + "order by convert(int,css.CodeSenderAddress)"
                        , where
                        );

            return db.Query(sql).Tables[0];
        }
        #endregion

        #region 【方法组：保存到数据库】
        #region 【方法：操作人员下井超员信息】
        /// <summary>
        /// 操作人员下井超员信息
        /// </summary>
        /// <returns></returns>
        private bool SaveOverEmp()
        {
            return ExecuteSql(true, "proc_InsertUpdateRealTimeOverEmp", null);
        }
        #endregion

        #region 【方法：保存传输分站状态】
        /// <summary>
        /// 保存传输分站状态
        /// </summary>
        /// <param name="stationAddress"></param>
        /// <param name="stationHeadAddress"></param>
        /// <param name="stationState"></param>
        /// <param name="breakTime"></param>
        /// <returns></returns>
        private bool SaveStationState(int stationAddress, int stationHeadAddress, int stationState, DateTime breakTime)
        {
            SqlParameter[] sqlParmeters = { 
                                              new SqlParameter("StationAddress", SqlDbType.Int),
                                              new SqlParameter("StationHeadAddress", SqlDbType.Int),
                                              new SqlParameter("StationState", SqlDbType.Int),
                                              new SqlParameter("BreakTime", SqlDbType.DateTime)
                                          };
            sqlParmeters[0].Value = stationAddress;
            sqlParmeters[1].Value = stationHeadAddress;
            sqlParmeters[2].Value = stationState;
            sqlParmeters[3].Value = breakTime;

            return ExecuteSql(true, "Station_StateChange", sqlParmeters);
        }
        #endregion

        #region 【方法：保存读卡分站状态】
        /// <summary>
        /// 保存读卡分站状态
        /// </summary>
        /// <param name="stationAddress"></param>
        /// <param name="stationHeadAddress"></param>
        /// <param name="stationState"></param>
        /// <param name="breakTime"></param>
        /// <returns></returns>
        private bool SaveStationHeadState(int stationAddress, int stationHeadAddress, int stationState, DateTime breakTime)
        {
            SqlParameter[] sqlParmeters = { 
                                              new SqlParameter("StationAddress", SqlDbType.Int),
                                              new SqlParameter("StationHeadAddress", SqlDbType.Int),
                                              new SqlParameter("StationState", SqlDbType.Int),
                                              new SqlParameter("BreakTime", SqlDbType.DateTime)
                                          };
            sqlParmeters[0].Value = stationAddress;
            sqlParmeters[1].Value = stationHeadAddress;
            sqlParmeters[2].Value = stationState;
            sqlParmeters[3].Value = breakTime;

            return ExecuteSql(true, "StationHead_StateChange", sqlParmeters);
        }
        #endregion

        #region 【方法：置标示卡状态为低电量】
        /// <summary>
        /// 置标识卡状态为低电量
        /// </summary>
        /// <param name="strCards"></param>
        /// <returns></returns>
        private bool SaveCodeSenderStatebyLow(string strCards)
        {
            SqlParameter[] sqlParmeters = { 
                                              new SqlParameter("Cards", SqlDbType.VarChar,6000)
                                          };
            sqlParmeters[0].Value = strCards;

            return ExecuteSql(true, "KJ128N_Update_CodeSenderLow", sqlParmeters);
        }
        #endregion

        #region 【方法：置标识卡状态为正常】
        /// <summary>
        /// 置标识卡状态为正常
        /// </summary>
        /// <param name="strCares"></param>
        /// <returns></returns>
        private bool SaveCodeSenderStateByOk(string strCards)
        {
            SqlParameter[] sqlParmeters = { 
                                              new SqlParameter("Cards", SqlDbType.VarChar,8000)
                                          };
            sqlParmeters[0].Value = strCards;

            return ExecuteSql(true, "Shine_Shen_Update_CodeSender_Common", sqlParmeters);
        }
        #endregion

        #region 【方法：添加一条进出读卡分站历史记录】
        private bool AddHisStationHeadInfo(int codeSenderAddress, DataTable dtHisInOutStationHead, DataTable dtStationHead, DataRowView drEmp, DateTime detecTime, DateTime dtimeInStationHeadTime, bool isMend)
        {
            DataRow drHis = dtHisInOutStationHead.NewRow();
            drHis["HisStationHeadID"] = Int64.Parse(DateTime.Now.ToString("yyMMddHHmmssfff") + int.Parse(drEmp["CodeSenderAddress"].ToString()).ToString("0000"));
            drHis["StationAddress"] = dtStationHead.Rows[0]["stationAddress"];
            drHis["StationHeadAddress"] = dtStationHead.Rows[0]["stationHeadAddress"];
            drHis["StationHeadPlace"] = dtStationHead.Rows[0]["StationHeadPlace"];
            drHis["CodeSenderAddress"] = drEmp["CodeSenderAddress"];
            drHis["CsTypeID"] = drEmp["CsTypeID"];
            drHis["UserID"] = drEmp["UserID"];
            drHis["UserNo"] = drEmp["UserNo"];
            drHis["UserName"] = drEmp["UserName"];
            drHis["DeptID"] = drEmp["DeptID"];
            drHis["DeptName"] = drEmp["DeptName"];
            drHis["DutyID"] = drEmp["DutyID"];
            drHis["DutyName"] = drEmp["DutyName"];
            drHis["WorkTypeID"] = drEmp["WorkTypeID"];
            drHis["WorkTypeName"] = drEmp["WorkTypeName"];
            drHis["InStationHeadTime"] = dtimeInStationHeadTime;
            drHis["OutStationHeadTime"] = detecTime;
            drHis["ContinueTime"] = (int)(((TimeSpan)(detecTime - dtimeInStationHeadTime)).TotalSeconds);
            drHis["IsMend"] = isMend;
            //添加到历史进出井记录中
            return AddHisStationHeadInfo(drHis, dtimeInStationHeadTime.ToString("yyyyM"));
        }

        private bool AddHisStationHeadInfo(int codeSenderAddress, DataTable dtHisInOutStationHead, DataTable dtStationHead, DataRow drEmp, DateTime detecTime, DateTime dtimeInStationHeadTime, bool isMend)
        {
            DataRow drHis = dtHisInOutStationHead.NewRow();
            drHis["HisStationHeadID"] = Int64.Parse(DateTime.Now.ToString("yyMMddHHmmssfff") + int.Parse(drEmp["CodeSenderAddress"].ToString()).ToString("0000"));
            drHis["StationAddress"] = dtStationHead.Rows[0]["stationAddress"];
            drHis["StationHeadAddress"] = dtStationHead.Rows[0]["stationHeadAddress"];
            drHis["StationHeadPlace"] = dtStationHead.Rows[0]["StationHeadPlace"];
            drHis["CodeSenderAddress"] = drEmp["CodeSenderAddress"];
            drHis["CsTypeID"] = drEmp["CsTypeID"];
            drHis["UserID"] = drEmp["UserID"];
            drHis["UserNo"] = drEmp["UserNo"];
            drHis["UserName"] = drEmp["UserName"];
            drHis["DeptID"] = drEmp["DeptID"];
            drHis["DeptName"] = drEmp["DeptName"];
            drHis["DutyID"] = drEmp["DutyID"];
            drHis["DutyName"] = drEmp["DutyName"];
            drHis["WorkTypeID"] = drEmp["WorkTypeID"];
            drHis["WorkTypeName"] = drEmp["WorkTypeName"];
            drHis["InStationHeadTime"] = dtimeInStationHeadTime;
            drHis["OutStationHeadTime"] = detecTime;
            drHis["ContinueTime"] = (int)(((TimeSpan)(detecTime - dtimeInStationHeadTime)).TotalSeconds);
            drHis["IsMend"] = isMend;
            //添加到历史进出井记录中
            return AddHisStationHeadInfo(drHis, dtimeInStationHeadTime.ToString("yyyyM"));
        }

        /// <summary>
        /// 添加一条历史进出读卡分站记录
        /// </summary>
        /// <param name="hisStationHeadID"></param>
        /// <param name="stationAddress"></param>
        /// <param name="stationHeadAddress"></param>
        /// <param name="stationHeadPlace"></param>
        /// <param name="codeSenderAddress"></param>
        /// <param name="csTypeID"></param>
        /// <param name="strUserNo"></param>
        /// <param name="strUserName"></param>
        /// <param name="deptID"></param>
        /// <param name="strDeptName"></param>
        /// <param name="dutyID"></param>
        /// <param name="strDutyName"></param>
        /// <param name="workTypeID"></param>
        /// <param name="strWorkTypeName"></param>
        /// <param name="strInStationHeadTime"></param>
        /// <param name="strOutStationHeadTime"></param>
        /// <param name="strTableName"></param>
        /// <returns></returns>
        private bool AddHisStationHeadInfo(DataRow drHis, string strTableName)
        {
            SqlParameter[] sqlParmeters = {
                                                new SqlParameter("HisStationHeadID",SqlDbType.BigInt),
                                                new SqlParameter("StationAddress",SqlDbType.Int),
                                                new SqlParameter("StationHeadAddress",SqlDbType.Int),
                                                new SqlParameter("StationHeadPlace",SqlDbType.VarChar,50),
                                                new SqlParameter("CodeSenderAddress",SqlDbType.Int),
                                                new SqlParameter("CsTypeID",SqlDbType.Int),
                                                new SqlParameter("UserID",SqlDbType.Int),
                                                new SqlParameter("UserNo",SqlDbType.NVarChar,50),
                                                new SqlParameter("UserName",SqlDbType.NVarChar,20),
                                                new SqlParameter("DeptID",SqlDbType.Int),
                                                new SqlParameter("DeptName",SqlDbType.NVarChar,50),
                                                new SqlParameter("DutyID",SqlDbType.Int),
                                                new SqlParameter("DutyName",SqlDbType.NVarChar,50),
                                                new SqlParameter("WorkTypeID",SqlDbType.Int),
                                                new SqlParameter("WorkTypeName",SqlDbType.NVarChar,50),
                                                new SqlParameter("InStationHeadTime",SqlDbType.NVarChar,50),
                                                new SqlParameter("OutStationHeadTime",SqlDbType.NVarChar,50),
                                                new SqlParameter("ContinueTime",SqlDbType.BigInt),
                                                new SqlParameter("IsMend",SqlDbType.Bit),
                                                new SqlParameter("TableName",SqlDbType.VarChar,20)
                                          };

            sqlParmeters[0].Value = drHis["HisStationHeadID"];
            sqlParmeters[1].Value = drHis["StationAddress"];
            sqlParmeters[2].Value = drHis["StationHeadAddress"];
            sqlParmeters[3].Value = drHis["StationHeadPlace"];
            sqlParmeters[4].Value = drHis["CodeSenderAddress"];
            sqlParmeters[5].Value = drHis["CsTypeID"];
            sqlParmeters[6].Value = drHis["UserID"];
            sqlParmeters[7].Value = drHis["UserNo"];
            sqlParmeters[8].Value = drHis["UserName"];
            sqlParmeters[9].Value = drHis["DeptID"];
            sqlParmeters[10].Value = drHis["DeptName"];
            sqlParmeters[11].Value = drHis["DutyID"];
            sqlParmeters[12].Value = drHis["DutyName"];
            sqlParmeters[13].Value = drHis["WorkTypeID"];
            sqlParmeters[14].Value = drHis["WorkTypeName"];
            sqlParmeters[15].Value = drHis["InStationHeadTime"];
            sqlParmeters[16].Value = drHis["OutStationHeadTime"];
            sqlParmeters[17].Value = drHis["ContinueTime"];
            sqlParmeters[18].Value = drHis["IsMend"];
            sqlParmeters[19].Value = strTableName;
            return ExecuteSql(true, "proc_AddHistoryStationHead", sqlParmeters);
        }
        #endregion

        #region 【方法：添加一条实时进出读卡分站记录】
        /// <summary>
        /// 添加实时进出读卡分站记录
        /// </summary>
        /// <param name="drEmp"></param>
        /// <param name="drStationHead"></param>
        /// <param name="detecTime"></param>
        /// <param name="strDirectional"></param>
        /// <param name="inOutFlag"></param>
        /// <returns></returns>
        private bool AddRtInStationHead(DataRow drEmp, DataRow drStationHead, DateTime detecTime, string strDirectional, int inOutFlag)
        {
            SqlParameter[] sqlParmeters = {
                                                new SqlParameter("codeSenderAddress",SqlDbType.Int),
                                                new SqlParameter("stationHeadID",SqlDbType.Int),
                                                new SqlParameter("CsSetID",SqlDbType.Int),
                                                new SqlParameter("CsTypeID",SqlDbType.Int),
                                                new SqlParameter("UserID",SqlDbType.Int),
                                                new SqlParameter("InAntennaPlace",SqlDbType.VarChar,20),
                                                new SqlParameter("InStationHeadTime",SqlDbType.DateTime),
                                                new SqlParameter("stationHeadTime",SqlDbType.DateTime),
                                                new SqlParameter("Directional",SqlDbType.VarChar,50),
                                                new SqlParameter("inOutFlag",SqlDbType.SmallInt),
                                          };

            sqlParmeters[0].Value = drEmp["codeSenderAddress"];
            sqlParmeters[1].Value = drStationHead["stationHeadID"];
            sqlParmeters[2].Value = drEmp["CsSetID"];
            sqlParmeters[3].Value = drEmp["CsTypeID"];
            sqlParmeters[4].Value = drEmp["UserID"];
            sqlParmeters[5].Value = drStationHead["StationHeadPlace"];
            sqlParmeters[6].Value = detecTime;
            sqlParmeters[7].Value = detecTime;
            sqlParmeters[8].Value = strDirectional;
            sqlParmeters[9].Value = inOutFlag;
            return ExecuteSql(true, "proc_AddRTInStationHeadInfo", sqlParmeters);
        }
        #endregion

        #region 【方法：更新实时进出读卡分站记录】
        /// <summary>
        /// 更新实时进出读卡分站记录
        /// </summary>
        /// <param name="drInStationHead"></param>
        /// <param name="drStationHead"></param>
        /// <param name="detctime"></param>
        /// <param name="inOutFlag"></param>
        /// <param name="strDirectional"></param>
        /// <returns></returns>
        private bool UpdateRtInStationHead(DataRowView drInStationHead, DataRow drStationHead, DataRow drEmp, DateTime detctime, int inOutFlag, string strDirectional)
        {
            drInStationHead.BeginEdit();
            drInStationHead["stationHeadID"] = drStationHead["stationHeadID"];
            drInStationHead["CsSetID"] = drEmp["CsSetID"];
            drInStationHead["CsTypeID"] = drEmp["CsTypeID"];
            drInStationHead["UserID"] = drEmp["UserID"];
            drInStationHead["InAntennaPlace"] = drStationHead["StationHeadPlace"];
            drInStationHead["InStationHeadTime"] = detctime;
            if (inOutFlag == 0)
            {
                drInStationHead["stationHeadTime"] = detctime;
            }
            drInStationHead["Directional"] = strDirectional;
            drInStationHead["inOutFlag"] = inOutFlag;
            drInStationHead.EndEdit();

            return UpdateRtInStationHead(drInStationHead);
        }

        private bool UpdateRtInStationHead(DataRowView drInStationHead, DataRow drStationHead, DataRowView drEmp, DateTime detctime, int inOutFlag, string strDirectional)
        {
            drInStationHead.BeginEdit();
            drInStationHead["stationHeadID"] = drStationHead["stationHeadID"];
            drInStationHead["CsSetID"] = drEmp["CsSetID"];
            drInStationHead["CsTypeID"] = drEmp["CsTypeID"];
            drInStationHead["UserID"] = drEmp["UserID"];
            drInStationHead["InAntennaPlace"] = drStationHead["StationHeadPlace"];
            drInStationHead["InStationHeadTime"] = detctime;
            if (inOutFlag == 0)
            {
                drInStationHead["stationHeadTime"] = detctime;
            }
            drInStationHead["Directional"] = strDirectional;
            drInStationHead["inOutFlag"] = inOutFlag;
            drInStationHead.EndEdit();

            return UpdateRtInStationHead(drInStationHead);
        }

        /// <summary>
        /// 更新实时进出读卡分站记录
        /// </summary>
        /// <param name="dr"></param>
        /// <returns></returns>
        private bool UpdateRtInStationHead(DataRowView dr)
        {
            SqlParameter[] sqlParmeters = {
                                                new SqlParameter("codeSenderAddress",SqlDbType.Int),
                                                new SqlParameter("stationHeadID",SqlDbType.Int),
                                                new SqlParameter("CsSetID",SqlDbType.Int),
                                                new SqlParameter("CsTypeID",SqlDbType.Int),
                                                new SqlParameter("UserID",SqlDbType.Int),
                                                new SqlParameter("InAntennaPlace",SqlDbType.VarChar,20),
                                                new SqlParameter("InStationHeadTime",SqlDbType.DateTime),
                                                new SqlParameter("stationHeadTime",SqlDbType.DateTime),
                                                new SqlParameter("Directional",SqlDbType.VarChar,50),
                                                new SqlParameter("inOutFlag",SqlDbType.SmallInt),
                                          };

            sqlParmeters[0].Value = dr["codeSenderAddress"];
            sqlParmeters[1].Value = dr["stationHeadID"];
            sqlParmeters[2].Value = dr["CsSetID"];
            sqlParmeters[3].Value = dr["CsTypeID"];
            sqlParmeters[4].Value = dr["UserID"];
            sqlParmeters[5].Value = dr["InAntennaPlace"];
            sqlParmeters[6].Value = dr["InStationHeadTime"];
            sqlParmeters[7].Value = dr["stationHeadTime"];
            sqlParmeters[8].Value = dr["Directional"];
            sqlParmeters[9].Value = dr["inOutFlag"];
            return ExecuteSql(true, "proc_UpdateRTInStationHeadInfo", sqlParmeters);
        }
        #endregion

        #region 【方法：更新求救信息】
        /// <summary>
        /// 更新求救信息
        /// </summary>
        /// <param name="stationAddress"></param>
        /// <param name="stationHeadAddress"></param>
        /// <param name="strCards"></param>
        /// <param name="dtimeDecte"></param>
        /// <returns></returns>
        private bool UpdateRtEmpHelp(int stationAddress, int stationHeadAddress, string strCards, DateTime dtimeDecte)
        {
            SqlParameter[] sqlParmeters = { 
                                              new SqlParameter("DetecTime", SqlDbType.DateTime),
                                              new SqlParameter("StationAddress", SqlDbType.Int),
                                              new SqlParameter("StationHeadAddress", SqlDbType.Int),
                                              new SqlParameter("Cards", SqlDbType.VarChar,6000)
                                              
                                          };
            sqlParmeters[0].Value = dtimeDecte;
            sqlParmeters[1].Value = stationAddress;
            sqlParmeters[2].Value = stationHeadAddress;
            sqlParmeters[3].Value = strCards;

            return ExecuteSql(true, "process_EmpHelpInfo", sqlParmeters);
        }
        #endregion

        #region 【方法：添加历史人员区域超时信息】
        /// <summary>
        /// 添加历史人员区域超时信息
        /// </summary>
        /// <param name="drvTerOverTime"></param>
        /// <param name="drvEmp"></param>
        /// <returns></returns>
        private bool AddHisTerrOverTime(DataRowView drvTerOverTime, DataRowView drvEmp, DateTime detecTime)
        {
            SqlParameter[] sqlParmeters = {
                                                new SqlParameter("HisTerEmpOverTimeID",SqlDbType.BigInt),
                                                new SqlParameter("TerritorialID",SqlDbType.Int),
                                                new SqlParameter("TerritorialName",SqlDbType.NVarChar,20),
                                                new SqlParameter("TerritorialTypeName",SqlDbType.NVarChar,20),
                                                new SqlParameter("InTerritorialTime",SqlDbType.DateTime),
                                                new SqlParameter("StartOverTime",SqlDbType.DateTime),
                                                new SqlParameter("TerWorkTime",SqlDbType.Int),
                                                new SqlParameter("OutTerritorialTime",SqlDbType.DateTime),
                                                new SqlParameter("CodeSenderAddress",SqlDbType.NVarChar,20),
                                                new SqlParameter("EmpID",SqlDbType.Int),
                                                new SqlParameter("EmpName",SqlDbType.NVarChar,50),
                                                new SqlParameter("DeptID",SqlDbType.Int),
                                                new SqlParameter("DeptName",SqlDbType.NVarChar,20),
                                                new SqlParameter("WtName",SqlDbType.NVarChar,50)
                                          };

            sqlParmeters[0].Value = Int64.Parse(DateTime.Now.ToString("yyyyMMddHHmmss") + int.Parse(drvEmp["CodeSenderAddress"].ToString()).ToString("0000")); ;
            sqlParmeters[1].Value = drvTerOverTime["TerritorialID"];
            sqlParmeters[2].Value = drvTerOverTime["TerritorialName"];
            sqlParmeters[3].Value = drvTerOverTime["TerritorialTypeName"];
            sqlParmeters[4].Value = drvTerOverTime["InTerritorialTime"];
            sqlParmeters[5].Value = drvTerOverTime["StartOverTime"];
            sqlParmeters[6].Value = (int)(((TimeSpan)(detecTime - DateTime.Parse(drvTerOverTime["StartOverTime"].ToString()))).TotalSeconds);
            sqlParmeters[7].Value = detecTime;
            sqlParmeters[8].Value = drvEmp["CodeSenderAddress"];
            sqlParmeters[9].Value = drvEmp["UserID"];
            sqlParmeters[10].Value = drvEmp["UserName"];
            sqlParmeters[11].Value = drvEmp["DeptID"];
            sqlParmeters[12].Value = drvEmp["DeptName"];
            sqlParmeters[13].Value = drvEmp["WorkTypeName"];
            return ExecuteSql(true, "proc_InsertHisTerOverTime", sqlParmeters);
        }
        #endregion

        #region 【方法：添加人员到实时区域信息】
        /// <summary>
        /// 添加人员到实时区域信息
        /// </summary>
        /// <param name="strCode">要添加到实时区域表中的卡号信息</param>
        /// <param name="dtEmp">人员信息</param>
        /// <param name="drArea">区域信息</param>
        /// <param name="detecTime">日期</param>
        /// <param name="isAlarm">是否报警</param>
        /// <returns></returns>
        private bool AddRtTerritorial(string strCode, DataTable dtEmp, DataRowView drArea, DateTime detecTime, DataTable dtSworktype)
        {
            bool flag = true;
            string[] strCodeTemp = strCode.Split(',');
            bool isAlarm = false;

            foreach (string strItem in strCodeTemp)
            {
                DataView dvEmp = new DataView(dtEmp, "cstypeid=0 and CodeSenderAddress=" + strItem, "", DataViewRowState.CurrentRows);
                if (dvEmp != null && dvEmp.Count > 0)
                {
                    try
                    {
                        DataView dvSworktype = new DataView(dtSworktype, "TerrialID=" + drArea["TerritorialID"].ToString() + " and WorkTypeID=" + dvEmp[0]["WorkTypeID"].ToString(), "", DataViewRowState.CurrentRows);
                        if (dvSworktype != null && dvSworktype.Count > 0)
                            isAlarm = true;
                        else
                            isAlarm = false;
                    }
                    catch { }
                    if (!AddRtTerritorial(int.Parse(drArea["TerritorialID"].ToString()), drArea["TerritorialName"].ToString(), detecTime, int.Parse(strItem), int.Parse(dvEmp[0]["CsSetID"].ToString()), int.Parse(dvEmp[0]["CsTypeID"].ToString()), int.Parse(dvEmp[0]["UserID"].ToString()), drArea["TypeName"].ToString(), isAlarm))
                        flag = false;
                }
            }
            return flag;
        }

        private bool AddRtTerritorial(int territorialID, string strTerritorialName, DateTime inTerritorialTime, int codeSenderAddress, int csSetID, int csTypeID, int userID, string strTerritorialTypeName, bool isAlarm)
        {
            SqlParameter[] sqlParmeters = {
                                                new SqlParameter("TerritorialID",SqlDbType.Int),
                                                new SqlParameter("TerritorialName",SqlDbType.NVarChar,20),
                                                new SqlParameter("InTerritorialTime",SqlDbType.DateTime),
                                                new SqlParameter("CodeSenderAddress",SqlDbType.Int),
                                                new SqlParameter("CsSetID",SqlDbType.Int),
                                                new SqlParameter("CsTypeID",SqlDbType.Int),
                                                new SqlParameter("UserID",SqlDbType.Int),
                                                new SqlParameter("TerritorialTypeName",SqlDbType.NVarChar,20),
                                                new SqlParameter("IsAlarm",SqlDbType.Bit),
                                          };

            sqlParmeters[0].Value = territorialID;
            sqlParmeters[1].Value = strTerritorialName;
            sqlParmeters[2].Value = inTerritorialTime;
            sqlParmeters[3].Value = codeSenderAddress;
            sqlParmeters[4].Value = csSetID;
            sqlParmeters[5].Value = csTypeID;
            sqlParmeters[6].Value = userID;
            sqlParmeters[7].Value = strTerritorialTypeName;
            sqlParmeters[8].Value = isAlarm;
            return ExecuteSql(true, "proc_AddRTAreaInfo", sqlParmeters);
        }
        #endregion

        #region 【方法：添加到历史超时信息】
        /// <summary>
        /// 添加到历史超时信息
        /// </summary>
        /// <param name="detecTime"></param>
        /// <param name="strCards"></param>
        /// <returns></returns>
        private bool AddHisOverTimeEmployee(DateTime detecTime, string strCards)
        {
            SqlParameter[] sqlParmeters = {
                                                new SqlParameter("DetectTime",SqlDbType.DateTime),
                                                new SqlParameter("Cards",SqlDbType.VarChar,6000)
                                          };

            sqlParmeters[0].Value = detecTime;
            sqlParmeters[1].Value = strCards;
            return ExecuteSql(true, "proc_InsertHisOverTimeEmployee", sqlParmeters);
        }
        #endregion

        #region 【方法：操作区域超员信息】
        /// <summary>
        /// 操作区域超员信息
        /// </summary>
        /// <param name="detecTime"></param>
        /// <returns></returns>
        private bool OperatorTerOverEmp(DateTime detecTime)
        {
            SqlParameter[] sqlParmeters = {
                                                new SqlParameter("DetectTime",SqlDbType.DateTime)
                                          };

            sqlParmeters[0].Value = detecTime;
            return ExecuteSql(true, "OperatorTerOverEmp", sqlParmeters);
        }
        #endregion

        #region 【方法：添加到实时巡检路线信息】
        private bool AddRealTimePathCheck(DateTime detecTime, int stationAddress, int stationHeadAddress, string strCards)
        {
            SqlParameter[] sqlParmeters = {
                                                new SqlParameter("DetecTime",SqlDbType.DateTime),
                                                new SqlParameter("StationAddress",SqlDbType.Int),
                                                new SqlParameter("StationHeadAddress",SqlDbType.Int),
                                                new SqlParameter("Cards",SqlDbType.VarChar,6000)
                                          };

            sqlParmeters[0].Value = detecTime;
            sqlParmeters[1].Value = stationAddress;
            sqlParmeters[2].Value = stationHeadAddress;
            sqlParmeters[3].Value = strCards;
            return ExecuteSql(true, "insertRealTimePathCheck", sqlParmeters);
        }
        #endregion

        #region 【方法：保存到实时超速欠速报警信息】
        /// <summary>
        /// 操作区域超员信息
        /// </summary>
        /// <param name="detecTime"></param>
        /// <returns></returns>
        private bool AddRTOverSpeed(DateTime detecTime, int stationAddress, int stationHeadAddress, string strCards)
        {
            SqlParameter[] sqlParmeters = {
                                                new SqlParameter("DetectTime",SqlDbType.DateTime),
                                                new SqlParameter("StationAddress",SqlDbType.Int),
                                                new SqlParameter("StationHeadAddress",SqlDbType.Int),
                                                new SqlParameter("Cards",SqlDbType.VarChar,6000)
                                          };

            sqlParmeters[0].Value = detecTime;
            sqlParmeters[1].Value = stationAddress;
            sqlParmeters[2].Value = stationHeadAddress;
            sqlParmeters[3].Value = strCards;
            return ExecuteSql(true, "proc_InsertRTOverSpeed", sqlParmeters);
        }
        #endregion

        #region 【方法：修改历史超速、欠速信息】
        /// <summary>
        /// 修改历史超速、欠速信息
        /// </summary>
        /// <param name="strCards"></param>
        /// <returns></returns>
        private bool UpdateHisOverSpeed(string strCards)
        {
            SqlParameter[] sqlParmeters = {
                                                new SqlParameter("Cards",SqlDbType.VarChar,6000)
                                          };

            sqlParmeters[0].Value = strCards;
            return ExecuteSql(true, "proc_UpdateHisOverSpeed", sqlParmeters);
        }
        #endregion

        #region 【方法：删除区域超员信息】
        /// <summary>
        /// 删除区域超员信息
        /// </summary>
        /// <returns></returns>
        private bool DeleteTerOverEmp()
        {
            return ExecuteSql(true, "proc_DeleteTerOverEmp", null);
        }
        #endregion

        #region 【方法：添加进入井口分站信息】
        /// <summary>
        /// 添加进入井口分站信息
        /// </summary>
        /// <param name="stationHeadID"></param>
        /// <param name="stationAddress"></param>
        /// <param name="stationHeadAddress"></param>
        /// <param name="codeSenderAddress"></param>
        /// <param name="dectime"></param>
        /// <returns></returns>
        private bool AddInMineStation(int stationHeadID, int stationAddress, int stationHeadAddress, int codeSenderAddress, DateTime dectime)
        {
            SqlParameter[] sqlParmeters = {
                                                new SqlParameter("StationHeadID",SqlDbType.Int),
                                                new SqlParameter("@stationAddress",SqlDbType.Int),
                                                new SqlParameter("@stationHeadAddress",SqlDbType.Int),
                                                new SqlParameter("@CodeSenderAddress",SqlDbType.Int),
                                                new SqlParameter("@InMineStationTime",SqlDbType.DateTime)
                                          };

            sqlParmeters[0].Value = stationHeadID;
            sqlParmeters[1].Value = stationAddress;
            sqlParmeters[2].Value = stationHeadAddress;
            sqlParmeters[3].Value = codeSenderAddress;
            sqlParmeters[4].Value = dectime;
            return ExecuteSql(true, "proc_AddInMineStationInfo", sqlParmeters);
        }
        #endregion

        #region 【方法：更新进入井口分站信息】
        /// <summary>
        /// 更新进入井口分站信息
        /// </summary>
        /// <param name="stationHeadID"></param>
        /// <param name="stationAddress"></param>
        /// <param name="stationHeadAddress"></param>
        /// <param name="codeSenderAddress"></param>
        /// <param name="dectime"></param>
        /// <returns></returns>
        private bool UpdateInMineStation(int stationHeadID, int stationAddress, int stationHeadAddress, int codeSenderAddress, DateTime dectime)
        {
            SqlParameter[] sqlParmeters = {
                                                new SqlParameter("StationHeadID",SqlDbType.Int),
                                                new SqlParameter("@stationAddress",SqlDbType.Int),
                                                new SqlParameter("@stationHeadAddress",SqlDbType.Int),
                                                new SqlParameter("@CodeSenderAddress",SqlDbType.Int),
                                                new SqlParameter("@InMineStationTime",SqlDbType.DateTime)
                                          };

            sqlParmeters[0].Value = stationHeadID;
            sqlParmeters[1].Value = stationAddress;
            sqlParmeters[2].Value = stationHeadAddress;
            sqlParmeters[3].Value = codeSenderAddress;
            sqlParmeters[4].Value = dectime;
            return ExecuteSql(true, "proc_UpdateInMineStationInfo", sqlParmeters);
        }
        #endregion

        #region 【方法：添加实时下井人员信息】
        /// <summary>
        /// 更新实时下井人员信息
        /// </summary>
        /// <param name="codeSenderAddress"></param>
        /// <param name="stationHeadID"></param>
        /// <param name="csSetID"></param>
        /// <param name="inTime"></param>
        /// <returns></returns>
        private bool AddRtInMine(int codeSenderAddress, int stationHeadID, int csSetID, DateTime inTime)
        {
            SqlParameter[] sqlParmeters = {
                                                new SqlParameter("CodeSenderAddress",SqlDbType.Int),
                                                new SqlParameter("StationHeadID",SqlDbType.Int),
                                                new SqlParameter("CsSetID",SqlDbType.Int),
                                                new SqlParameter("InTime",SqlDbType.DateTime),
                                          };

            sqlParmeters[0].Value = codeSenderAddress;
            sqlParmeters[1].Value = stationHeadID;
            sqlParmeters[2].Value = csSetID;
            sqlParmeters[3].Value = inTime;

            return ExecuteSql(true, "proc_AddRTInMine", sqlParmeters);
        }
        #endregion

        #region 【方法：删除实时下井人员信息】
        /// <summary>
        /// 删除实时下井人员信息
        /// </summary>
        /// <param name="codeSenderAddress"></param>
        /// <returns></returns>
        private bool DeleteRtInMine(int codeSenderAddress)
        {
            SqlParameter[] sqlParmeters = {
                                                new SqlParameter("CodeSenderAddress",SqlDbType.Int)
                                          };
            sqlParmeters[0].Value = codeSenderAddress;
            return ExecuteSql(true, "proc_DeleteRTInMine", sqlParmeters);
        }
        #endregion

        #region 【方法：更新实时下井人员信息】
        /// <summary>
        /// 更新实时下井人员信息
        /// </summary>
        /// <param name="codeSenderAddress"></param>
        /// <param name="stationHeadID"></param>
        /// <param name="csSetID"></param>
        /// <param name="inTime"></param>
        /// <returns></returns>
        private bool UpdateRtInMine(int codeSenderAddress, int stationHeadID, int csSetID, DateTime inTime)
        {
            SqlParameter[] sqlParmeters = {
                                                new SqlParameter("CodeSenderAddress",SqlDbType.Int),
                                                new SqlParameter("StationHeadID",SqlDbType.Int),
                                                new SqlParameter("CsSetID",SqlDbType.Int),
                                                new SqlParameter("InTime",SqlDbType.DateTime),
                                          };

            sqlParmeters[0].Value = codeSenderAddress;
            sqlParmeters[1].Value = stationHeadID;
            sqlParmeters[2].Value = csSetID;
            sqlParmeters[3].Value = inTime;

            return ExecuteSql(true, "proc_UpdateRTInMine", sqlParmeters);
        }
        #endregion

        #region 【方法：删除实时区域信息】
        /// <summary>
        /// 删除所有的实时区域信息
        /// </summary>
        /// <returns></returns>
        private bool DeleteRtTerritorial()
        {
            return ExecuteSql(true, "proc_DeleteRtTerritorialAll", null);
        }

        /// <summary>
        /// 按照区域编号和标识卡删除实时区域信息
        /// </summary>
        /// <param name="territorialID"></param>
        /// <param name="codeSenderAddress"></param>
        /// <returns></returns>
        private bool DeleteRtTerritorial(int territorialID, int codeSenderAddress)
        {
            SqlParameter[] sqlParmeters = {
                                                new SqlParameter("TerritorialID",SqlDbType.Int),
                                                new SqlParameter("CodeSenderAddress",SqlDbType.Int)
                                          };
            sqlParmeters[0].Value = territorialID;
            sqlParmeters[1].Value = codeSenderAddress;
            return ExecuteSql(true, "proc_DeleteRtTerritorialByValue", sqlParmeters);
        }
        #endregion

        #region 【方法：添加临时实时标识卡信息】
        /// <summary>
        /// 添加临时实时标识卡信息
        /// </summary>
        /// <param name="drEmp"></param>
        /// <param name="drStationHead"></param>
        /// <param name="detecTime"></param>
        /// <returns></returns>
        private bool AddRtInStationHeadTemp(DataRow drEmp, DataRow drStationHead, DateTime detecTime)
        {
            SqlParameter[] sqlParmeters = {
                                                new SqlParameter("codeSenderAddress",SqlDbType.Int),
                                                new SqlParameter("stationAddress",SqlDbType.Int),
                                                new SqlParameter("StationHeadAddress",SqlDbType.Int),
                                                new SqlParameter("csSetID",SqlDbType.Int),
                                                new SqlParameter("csTypeID",SqlDbType.Int),
                                                new SqlParameter("UserID",SqlDbType.Int),
                                                new SqlParameter("InStationHeadTime",SqlDbType.DateTime),
                                          };

            sqlParmeters[0].Value = drEmp["codeSenderAddress"];
            sqlParmeters[1].Value = drStationHead["stationAddress"];
            sqlParmeters[2].Value = drStationHead["StationHeadAddress"];
            sqlParmeters[3].Value = drEmp["csSetID"];
            sqlParmeters[4].Value = drEmp["csTypeID"];
            sqlParmeters[5].Value = drEmp["UserID"];
            sqlParmeters[6].Value = detecTime;
            return ExecuteSql(true, "proc_AddRtInStationHeadTemp", sqlParmeters);
        }
        #endregion

        #region 【方法：修改临时实时标识卡信息】
        /// <summary>
        /// 修改临时实时标识卡信息
        /// </summary>
        /// <param name="drEmp"></param>
        /// <param name="drStationHead"></param>
        /// <param name="detecTime"></param>
        /// <returns></returns>
        private bool UpdateRtInStationHeadTemp(DataRow drEmp, DataRow drStationHead, DateTime detecTime)
        {
            SqlParameter[] sqlParmeters = {
                                                new SqlParameter("codeSenderAddress",SqlDbType.Int),
                                                new SqlParameter("stationAddress",SqlDbType.Int),
                                                new SqlParameter("StationHeadAddress",SqlDbType.Int),
                                                new SqlParameter("csSetID",SqlDbType.Int),
                                                new SqlParameter("csTypeID",SqlDbType.Int),
                                                new SqlParameter("UserID",SqlDbType.Int),
                                                new SqlParameter("InStationHeadTime",SqlDbType.DateTime),
                                          };

            sqlParmeters[0].Value = drEmp["codeSenderAddress"];
            sqlParmeters[1].Value = drStationHead["stationAddress"];
            sqlParmeters[2].Value = drStationHead["StationHeadAddress"];
            sqlParmeters[3].Value = drEmp["csSetID"];
            sqlParmeters[4].Value = drEmp["csTypeID"];
            sqlParmeters[5].Value = drEmp["UserID"];
            sqlParmeters[6].Value = detecTime;
            return ExecuteSql(true, "proc_UpdateRtInStationHeadTemp", sqlParmeters);
        }
        #endregion

        #region 【方法：删除临时实时标识卡中的数据】
        /// <summary>
        /// 删除临时实时标识卡中的数据
        /// </summary>
        /// <param name="stationAddress"></param>
        /// <param name="stationHeadAddress"></param>
        /// <returns></returns>
        private bool DeleteRtStationHeadTemp(int stationAddress, int stationHeadAddress)
        {
            SqlParameter[] sqlParmeters = { 
                                                  new SqlParameter("stationAddress", SqlDbType.Int) ,
                                                  new SqlParameter("StationHeadAddress",SqlDbType.Int)
                                              };
            sqlParmeters[0].Value = stationAddress;
            sqlParmeters[1].Value = stationHeadAddress;

            return ExecuteSql(true, "proc_DeleteRtInStationHeadTemp", sqlParmeters);
        }
        #endregion

        #region 【方法：删除临时实时标识卡中的数据】
        /// <summary>
        /// 删除临时实时标识卡中的数据
        /// </summary>
        /// <param name="stationAddress"></param>
        /// <param name="stationHeadAddress"></param>
        /// <returns></returns>
        private bool DeleteRtStationHeadTemp(int stationAddress, int stationHeadAddress, string strCards)
        {
            SqlParameter[] sqlParmeters = { 
                                                  new SqlParameter("stationAddress", SqlDbType.Int) ,
                                                  new SqlParameter("StationHeadAddress",SqlDbType.Int),
                                                  new SqlParameter("Cards",SqlDbType.VarChar,6000)
                                              };
            sqlParmeters[0].Value = stationAddress;
            sqlParmeters[1].Value = stationHeadAddress;
            sqlParmeters[2].Value = strCards;
            return ExecuteSql(true, "proc_DeleteRtInStationHeadTempByCards", sqlParmeters);
        }

        /// <summary>
        /// 删除临时实时标识卡中的数据
        /// </summary>
        /// <param name="stationAddress"></param>
        /// <param name="stationHeadAddress"></param>
        /// <returns></returns>
        private bool DeleteRtStationHeadTemp(int stationAddress, int stationHeadAddress, int codeSenderAddress)
        {
            SqlParameter[] sqlParmeters = { 
                                                  new SqlParameter("stationAddress", SqlDbType.Int) ,
                                                  new SqlParameter("StationHeadAddress",SqlDbType.Int),
                                                  new SqlParameter("codeSenderAddress",SqlDbType.Int)
                                              };
            sqlParmeters[0].Value = stationAddress;
            sqlParmeters[1].Value = stationHeadAddress;
            sqlParmeters[2].Value = codeSenderAddress;
            return ExecuteSql(true, "proc_DeleteRtInStationHeadTempByID", sqlParmeters);
        }
        #endregion

        #region 【方法：添加到实时考勤数据中】
        /// <summary>
        /// 添加到实时考勤数据
        /// </summary>
        /// <param name="detecTime"></param>
        /// <param name="strCards"></param>
        /// <returns></returns>
        private bool AddRTEmployeeAttendance(DateTime detecTime, string strCards)
        {
            SqlParameter[] sqlParmeters = {
                                                new SqlParameter("DetectTime",SqlDbType.DateTime),
                                                new SqlParameter("Cards",SqlDbType.VarChar,6000)
                                          };

            sqlParmeters[0].Value = detecTime;
            sqlParmeters[1].Value = strCards;
            return ExecuteSql(true, "proc_InsertRTEmployeeAttendance", sqlParmeters);
        }
        #endregion

        #region 【方法：删除实时考勤数据】
        /// <summary>
        /// 删除实时考勤数据
        /// </summary>
        /// <param name="codeSenderAddress"></param>
        /// <returns></returns>
        private bool DelteteRealTimeAttendance(int codeSenderAddress)
        {
            SqlParameter[] sqlParmeters = {
                                                new SqlParameter("CodeSenderAddress",SqlDbType.Int)
                                          };
            sqlParmeters[0].Value = codeSenderAddress;
            return ExecuteSql(true, "proc_DeleteRealTimeAttendanceByCodeSenderAddress", sqlParmeters);
        }
        #endregion

        #region 【方法：添加历史考勤信息】
        /// <summary>
        /// 添加到历史考勤信息
        /// </summary>
        /// <param name="drHis"></param>
        /// <param name="strTableName"></param>
        /// <returns></returns>
        private bool AddHisAttendance(DataRow drHis, string strTableName)
        {
            SqlParameter[] sqlParmeters = {
                                                new SqlParameter("ID",SqlDbType.BigInt),
                                                new SqlParameter("BlockID",SqlDbType.Int),
                                                new SqlParameter("EmployeeID",SqlDbType.Int),
                                                new SqlParameter("EmployeeName",SqlDbType.VarChar,10),
                                                new SqlParameter("DeptID",SqlDbType.Int),
                                                new SqlParameter("ClassID",SqlDbType.VarChar,20),
                                                new SqlParameter("ClassShortName",SqlDbType.VarChar,10),
                                                new SqlParameter("BeginWorkTime",SqlDbType.VarChar,50),
                                                new SqlParameter("EndWorkTime",SqlDbType.VarChar,50),
                                                new SqlParameter("WorkTime",SqlDbType.Int),
                                                new SqlParameter("TimerIntervalID",SqlDbType.Int),
                                                new SqlParameter("DataAttendance",SqlDbType.VarChar,50),
                                                new SqlParameter("IsMend",SqlDbType.Bit),
                                                new SqlParameter("TableName",SqlDbType.VarChar,20)
                                          };

            sqlParmeters[0].Value = drHis["ID"];
            sqlParmeters[1].Value = drHis["BlockID"];
            sqlParmeters[2].Value = drHis["EmployeeID"];
            sqlParmeters[3].Value = drHis["EmployeeName"];
            sqlParmeters[4].Value = drHis["DeptID"];
            sqlParmeters[5].Value = drHis["ClassID"];
            sqlParmeters[6].Value = drHis["ClassShortName"];
            sqlParmeters[7].Value = drHis["BeginWorkTime"];
            sqlParmeters[8].Value = drHis["EndWorkTime"];
            sqlParmeters[9].Value = drHis["WorkTime"];
            sqlParmeters[10].Value = drHis["TimerIntervalID"];
            sqlParmeters[11].Value = drHis["DataAttendance"];
            sqlParmeters[12].Value = drHis["IsMend"];
            sqlParmeters[13].Value = strTableName;
            return ExecuteSql(true, "proc_AddHisAttendance", sqlParmeters);
        }
        #endregion

        #region 【方法：添加考勤统计记录】
        /// <summary>
        /// 添加考勤统计记录
        /// </summary>
        /// <param name="dataAttendance"></param>
        /// <param name="codeSenderAddress"></param>
        /// <param name="strEmpName"></param>
        /// <param name="deptID"></param>
        /// <param name="strDeptName"></param>
        /// <returns></returns>
        private bool AddKQTJ(DateTime dataAttendance, int codeSenderAddress, string strEmpName, int deptID, string strDeptName)
        {
            SqlParameter[] sqlParmeters = {
                                                new SqlParameter("dataAttendance",SqlDbType.DateTime),
                                                new SqlParameter("CodeSenderAddress",SqlDbType.Int),
                                                new SqlParameter("EmpName",SqlDbType.VarChar,20),
                                                new SqlParameter("DeptID",SqlDbType.Int),
                                                new SqlParameter("DeptName",SqlDbType.VarChar,20)
                                          };

            sqlParmeters[0].Value = dataAttendance;
            sqlParmeters[1].Value = codeSenderAddress;
            sqlParmeters[2].Value = strEmpName;
            sqlParmeters[3].Value = deptID;
            sqlParmeters[4].Value = strDeptName;
            return ExecuteSql(true, "proc_InsertKQTJ", sqlParmeters);
        }
        #endregion

        #region 【方法：修改考勤统计记录】
        /// <summary>
        /// 修改考勤统计记录
        /// </summary>
        /// <param name="dataAttendance"></param>
        /// <param name="codeSenderAddress"></param>
        /// <param name="strClassShortName"></param>
        /// <returns></returns>
        private bool UpdateKQTJ(DateTime dataAttendance, int codeSenderAddress, string strClassShortName, int deptID, string strDeptName)
        {
            SqlParameter[] sqlParmeters = {
                                                new SqlParameter("dataAttendance",SqlDbType.DateTime),
                                                new SqlParameter("CodeSenderAddress",SqlDbType.Int),
                                                new SqlParameter("ClassShortName",SqlDbType.VarChar,20),
                                                new SqlParameter("deptID",SqlDbType.Int),
                                                new SqlParameter("deptName",SqlDbType.VarChar,50)
                                          };

            sqlParmeters[0].Value = dataAttendance;
            sqlParmeters[1].Value = codeSenderAddress;
            sqlParmeters[2].Value = strClassShortName;
            sqlParmeters[3].Value = deptID;
            sqlParmeters[4].Value = strDeptName;
            return ExecuteSql(true, "proc_UpdateKQTJ", sqlParmeters);
        }
        #endregion

        #region 【方法：插入历史巡检异常记录】
        private bool OperatorHisPath(DataTable dtRtPath, DataRowView drvEmp, DateTime detecTime)
        {
            bool flag = true;
            if (dtRtPath != null && dtRtPath.Rows.Count > 0)
            {
                //获取这张卡在实时巡检中有没有报警记录
                DataView dvRTPath = new DataView(dtRtPath, "empid=" + drvEmp["userid"].ToString(), "", DataViewRowState.CurrentRows);
                if (dvRTPath != null && dvRTPath.Count > 0)
                {
                    //插入历史巡检异常报警数据并删除实时
                    if (!AddHisPathAlarm(dvRTPath[0], drvEmp, detecTime))
                        flag = false;
                }
            }
            return flag;
        }

        private bool AddHisPathAlarm(DataRowView drvRtPath, DataRowView drvEmp, DateTime detecTime)
        {
            SqlParameter[] sqlParmeters = {
                                                new SqlParameter("Id",SqlDbType.BigInt),
                                                new SqlParameter("EmpID",SqlDbType.Int),
                                                new SqlParameter("EmpName",SqlDbType.VarChar,20),
                                                new SqlParameter("StationAddress",SqlDbType.Int),
                                                new SqlParameter("StationHeadAddress",SqlDbType.Int),
                                                new SqlParameter("AlertBeginTime",SqlDbType.DateTime),
                                                new SqlParameter("AlertEndTime",SqlDbType.DateTime),
                                                new SqlParameter("AlertTimeValue",SqlDbType.Int)
                                          };

            sqlParmeters[0].Value = Int64.Parse(DateTime.Now.ToString("yyyyMMddHHmmss") + int.Parse(drvEmp["CodeSenderAddress"].ToString()).ToString("0000"));
            sqlParmeters[1].Value = drvRtPath["empid"];
            sqlParmeters[2].Value = drvEmp["UserName"];
            sqlParmeters[3].Value = drvRtPath["stationAddress"];
            sqlParmeters[4].Value = drvRtPath["stationHeadAddress"];
            sqlParmeters[5].Value = drvRtPath["alarmDateTime"];
            sqlParmeters[6].Value = detecTime;
            sqlParmeters[7].Value = (int)(((TimeSpan)(detecTime - DateTime.Parse(drvRtPath["alarmDateTime"].ToString()))).TotalSeconds);
            return ExecuteSql(true, "insert_His_PathAlert", sqlParmeters);
        }
        #endregion
        #endregion

        #region 【方法组：提取数据库中数据】
        #region 【方法:获取读卡分站信息】
        /// <summary>
        /// 获取读卡分站信息
        /// </summary>
        /// <param name="stationAddress"></param>
        /// <param name="stationHeadAddress"></param>
        /// <returns></returns>
        private DataTable GetStationHeadInfoByID(int stationAddress, int stationHeadAddress)
        {
            DataTable dtStationHead = null;
            try
            {
                SqlParameter[] sqlParmeters = { 
                                                  new SqlParameter("stationAddress", SqlDbType.Int) ,
                                                  new SqlParameter("StationHeadAddress",SqlDbType.Int)
                                              };
                sqlParmeters[0].Value = stationAddress;
                sqlParmeters[1].Value = stationHeadAddress;
                dtStationHead = GetDataTabel(true, "proc_GetStationHeadByAddress", sqlParmeters);
            }
            catch (Exception ex)
            {
            }
            return dtStationHead;
        }

        /// <summary>
        /// 获取读卡分站信息
        /// </summary>
        /// <param name="stationHeadID"></param>
        /// <returns></returns>
        private DataTable GetStationHeadInfoByID(int stationHeadID)
        {
            DataTable dtStationHead = null;
            try
            {
                SqlParameter[] sqlParmeters = { 
                                                  new SqlParameter("stationHeadID", SqlDbType.Int)
                                              };
                sqlParmeters[0].Value = stationHeadID;
                dtStationHead = GetDataTabel(true, "proc_GetStationHeadByID", sqlParmeters);
            }
            catch (Exception ex)
            {
            }
            return dtStationHead;
        }
        #endregion

        #region 【方法：获取临时表中的数据】
        /// <summary>
        /// 获取临时表中的数据
        /// </summary>
        /// <param name="stationAddress"></param>
        /// <param name="stationHeadAddress"></param>
        /// <param name="strCards"></param>
        /// <returns></returns>
        private DataTable GetRTInStationHeadInfoTemp(int stationAddress, int stationHeadAddress, string strCards)
        {
            DataTable dtStationHeadTemp = null;
            try
            {
                SqlParameter[] sqlParmeters = { 
                                                  new SqlParameter("stationAddress", SqlDbType.Int) ,
                                                  new SqlParameter("StationHeadAddress",SqlDbType.Int),
                                                  new SqlParameter("Cards",SqlDbType.VarChar,6000)
                                              };
                sqlParmeters[0].Value = stationAddress;
                sqlParmeters[1].Value = stationHeadAddress;
                sqlParmeters[2].Value = strCards;
                dtStationHeadTemp = GetDataTabel(true, "proc_GetRTInStationHeadTempInfoByCards", sqlParmeters);
            }
            catch (Exception ex)
            {
            }
            return dtStationHeadTemp;
        }
        /// <summary>
        /// 获取临时表中的数据
        /// </summary>
        /// <param name="stationAddress"></param>
        /// <param name="stationHeadAddress"></param>
        /// <returns></returns>
        private DataTable GetRTInStationHeadInfoTemp(int stationAddress, int stationHeadAddress)
        {
            DataTable dtStationHeadTemp = null;
            try
            {
                SqlParameter[] sqlParmeters = { 
                                                  new SqlParameter("stationAddress", SqlDbType.Int) ,
                                                  new SqlParameter("StationHeadAddress",SqlDbType.Int)
                                              };
                sqlParmeters[0].Value = stationAddress;
                sqlParmeters[1].Value = stationHeadAddress;
                dtStationHeadTemp = GetDataTabel(true, "proc_GetRTInStationHeadTempInfoByCards", sqlParmeters);
            }
            catch (Exception ex)
            {
            }
            return dtStationHeadTemp;
        }
        /// <summary>
        /// 获取临时表中的数据
        /// </summary>
        /// <param name="stationAddress"></param>
        /// <returns></returns>
        private DataTable GetRTInStationHeadInfoTemp(int stationAddress)
        {
            DataTable dtStationHeadTemp = null;
            try
            {
                SqlParameter[] sqlParmeters = { 
                                                  new SqlParameter("stationAddress", SqlDbType.Int)
                                              };
                sqlParmeters[0].Value = stationAddress;
                dtStationHeadTemp = GetDataTabel(true, "proc_GetRTInStationHeadTempInfoByCards", sqlParmeters);
            }
            catch (Exception ex)
            {
            }
            return dtStationHeadTemp;
        }
        #endregion

        #region 【方法：获取实时考勤信息】
        /// <summary>
        /// 按照传入卡号获取实时考勤信息
        /// </summary>
        /// <param name="strCards"></param>
        /// <returns></returns>
        private DataTable GetRealTimeAttendanceByCards(string strCards)
        {
            DataTable dtRealTimeAttendance = null;
            try
            {
                SqlParameter[] sqlParmeters = { 
                                                  new SqlParameter("Cards", SqlDbType.VarChar,6000)
                                              };
                sqlParmeters[0].Value = strCards;
                dtRealTimeAttendance = GetDataTabel(true, "proc_GetRealTimeAttendanceByCards", sqlParmeters);
            }
            catch (Exception ex)
            {
            }
            return dtRealTimeAttendance;
        }
        #endregion

        #region 【方法：获取标识卡配置信息】
        /// <summary>
        /// 获取标识卡配置信息
        /// </summary>
        /// <param name="strCards"></param>
        /// <returns></returns>
        private DataTable GetCodeSenderAddressByCards(string strCards)
        {
            DataTable dtCodeSenderAddress = null;
            try
            {
                SqlParameter[] sqlParmeters = { 
                                                  new SqlParameter("Cards", SqlDbType.VarChar,6000)
                                              };
                sqlParmeters[0].Value = strCards;
                dtCodeSenderAddress = GetDataTabel(true, "proc_GetCodeSenderSetByCards", sqlParmeters);
            }
            catch (Exception ex)
            {
            }
            return dtCodeSenderAddress;
        }
        #endregion

        #region 【方法：获取实时标识卡信息】
        /// <summary>
        /// 获取实时标识卡信息
        /// </summary>
        /// <param name="strCards"></param>
        /// <returns></returns>
        private DataTable GetRTInStationHeadInfoByCards(string strCards)
        {
            DataTable dtRTInStationHeadInfo = null;
            try
            {
                SqlParameter[] sqlParmeters = { 
                                                  new SqlParameter("Cards", SqlDbType.VarChar,6000)
                                              };
                sqlParmeters[0].Value = strCards;
                dtRTInStationHeadInfo = GetDataTabel(true, "proc_GetRTInStationHeadInfoByCards", sqlParmeters);
            }
            catch (Exception ex)
            {
            }
            return dtRTInStationHeadInfo;
        }
        #endregion

        #region 【方法：获取区域配置信息】
        /// <summary>
        /// 获取区域配置信息
        /// </summary>
        /// <returns></returns>
        private DataTable GetAreaSet()
        {
            DataTable dtAreaInfo = null;
            try
            {
                dtAreaInfo = GetDataTabel(true, "proc_GetAreaSet", null);
            }
            catch (Exception ex)
            {
            }
            return dtAreaInfo;
        }
        #endregion

        #region 【方法：获取人员区域超时信息】
        /// <summary>
        /// 获取人员区域超时信息
        /// </summary>
        /// <param name="strCards"></param>
        /// <returns></returns>
        private DataTable GetTerOverTime(string strCards)
        {
            DataTable dtRTTerOverTime = null;
            try
            {
                SqlParameter[] sqlParmeters = { 
                                                  new SqlParameter("Cards", SqlDbType.VarChar,6000)
                                              };
                sqlParmeters[0].Value = strCards;
                dtRTTerOverTime = GetDataTabel(true, "proc_GetTerOverTime", sqlParmeters);
            }
            catch (Exception ex)
            {
            }
            return dtRTTerOverTime;
        }
        #endregion

        #region 【方法：获取特殊工种配置信息】
        /// <summary>
        /// 获取特殊工种区域配置
        /// </summary>
        /// <returns></returns>
        private DataTable GetSWorkTypeAreaSet()
        {
            DataTable dtSWorkTypeAreaInfo = null;
            try
            {
                dtSWorkTypeAreaInfo = GetDataTabel(true, "proc_GetSWorkTypeAresSet", null);
            }
            catch (Exception ex)
            {
            }
            return dtSWorkTypeAreaInfo;
        }
        #endregion

        #region 【方法：获取实时区域信息】
        private DataTable GetRtAreaInfo(string strCards)
        {
            DataTable dtRtAreaInfo = null;
            try
            {
                SqlParameter[] sqlParmeters = { 
                                                  new SqlParameter("Cards", SqlDbType.VarChar,6000)
                                              };
                sqlParmeters[0].Value = strCards;
                dtRtAreaInfo = GetDataTabel(true, "proc_GetRTAreaInfo", sqlParmeters);
            }
            catch (Exception ex)
            {
            }
            return dtRtAreaInfo;
        }
        #endregion

        #region 【方法：获取实时井下人员信息】
        /// <summary>
        /// 获取传入的卡号的实时井下人员信息
        /// </summary>
        /// <param name="strCards"></param>
        /// <returns></returns>
        private DataTable GetRtInWell(string strCards)
        {
            DataTable dtRtInWell = null;
            try
            {
                SqlParameter[] sqlParmeters = { 
                                                  new SqlParameter("Cards", SqlDbType.VarChar,6000)
                                              };
                sqlParmeters[0].Value = strCards;
                dtRtInWell = GetDataTabel(true, "proc_GetRTInMine", sqlParmeters);
            }
            catch (Exception ex)
            {
            }
            return dtRtInWell;
        }
        #endregion

        #region 【方法：获取进入井口分站信息】
        /// <summary>
        /// 获取最后一次进入井口分站信息
        /// </summary>
        /// <param name="strCards"></param>
        /// <returns></returns>
        private DataTable GetMineStationHead(string strCards)
        {
            DataTable dtInMineStationHeadInfo = null;
            try
            {
                SqlParameter[] sqlParmeters = { 
                                                  new SqlParameter("Cards", SqlDbType.VarChar,6000)
                                              };
                sqlParmeters[0].Value = strCards;
                dtInMineStationHeadInfo = GetDataTabel(true, "proc_GetInMineStationInfo", sqlParmeters);
            }
            catch (Exception ex)
            {
            }
            return dtInMineStationHeadInfo;
        }
        #endregion

        #region 【方法：获取方向性配置信息】
        /// <summary>
        /// 按照地点获取方向性配置信息
        /// </summary>
        /// <param name="stationAddress"></param>
        /// <param name="stationHeadAddress"></param>
        /// <returns></returns>
        private DataTable GetCodeSenderDirectionalAntennaByAddress(int stationAddress, int stationHeadAddress)
        {
            DataTable dtDirectional = null;
            try
            {
                SqlParameter[] sqlParmeters = { 
                                                  new SqlParameter("stationAddress", SqlDbType.Int) ,
                                                  new SqlParameter("StationHeadAddress",SqlDbType.Int)
                                              };
                sqlParmeters[0].Value = stationAddress;
                sqlParmeters[1].Value = stationHeadAddress;
                dtDirectional = GetDataTabel(true, "proc_GetCodeSenderDirectionalAntennaByAddress", sqlParmeters);
            }
            catch (Exception ex)
            {
            }
            return dtDirectional;
        }
        #endregion

        #region 【方法：获取考勤统计信息】
        /// <summary>
        /// 按照卡号获取考勤统计信息
        /// </summary>
        /// <param name="strCards"></param>
        /// <param name="dtime"></param>
        /// <returns></returns>
        private DataTable GetKQTJbyCards(string strCards, DateTime dtime)
        {
            DataTable dtDirectional = null;
            DateTime dTimeKQTJ = new DateTime(dtime.Year, dtime.Month, 1, 0, 0, 0);

            try
            {
                SqlParameter[] sqlParmeters = { 
                                                  new SqlParameter("Cards", SqlDbType.VarChar,6000) ,
                                                  new SqlParameter("dataAttendance",SqlDbType.DateTime)
                                              };
                sqlParmeters[0].Value = strCards;
                sqlParmeters[1].Value = dTimeKQTJ;
                dtDirectional = GetDataTabel(true, "proc_GetKQTJByCard", sqlParmeters);
            }
            catch (Exception ex)
            {
            }
            return dtDirectional;
        }
        #endregion

        #region 【方法：获取实时巡检异常信息】
        /// <summary>
        /// 获取实时巡检异常信息
        /// </summary>
        /// <param name="strCards"></param>
        /// <returns></returns>
        private DataTable GetRealTimePathAlarm(string strCards)
        {
            DataTable dtRealTimePath = null;
            try
            {
                SqlParameter[] sqlParmeters = { 
                                                  new SqlParameter("Cards", SqlDbType.VarChar,6000)
                                              };
                sqlParmeters[0].Value = strCards;
                dtRealTimePath = GetDataTabel(true, "proc_GetRealTimePath", sqlParmeters);
            }
            catch (Exception ex)
            {
            }
            return dtRealTimePath;
        }
        #endregion
        #endregion

        #region 【方法：获取实时分站信息】
        /// <summary>
        /// 获取最后一次进入井口分站信息
        /// </summary>
        /// <param name="strCards"></param>
        /// <returns></returns>
        public DataTable GetInStationHead(string strCards)
        {
            DataTable dtInStationHeadInfo = null;
            try
            {
                SqlParameter[] sqlParmeters = { 
                                                  new SqlParameter("@Cards", SqlDbType.VarChar,6000)
                                              };
                sqlParmeters[0].Value = strCards;
                dtInStationHeadInfo = GetDataTabel(true, "proc_GetStationHeadPlaceByCode", sqlParmeters);
            }
            catch (Exception ex)
            {
            }
            return dtInStationHeadInfo;
        }
        #endregion

        #region[登录]
        public string Login(string strAccount, string strPassword, string strRemark)
        {
            string sql = string.Format(" select * from Admins join UserGroups on admins.UserGroupID=UserGroups.ID  where Account ='{0}' and [Password]='{1}' and Admins.IsEnable=1 and UGName='管理员'", strAccount, strPassword);
            try
            {
                DbHelperSQL DbHelper = new DbHelperSQL();
                if (DbHelper.Exists(sql))
                {
                    return "";
                }
                else
                {
                    return "1";
                }
            }
            catch
            {
                return "1";
            }
        }
        #endregion

        #region[执行数据库操作]
        /// <summary>
        ///  执行SQL语句或存储过程，成功返回True
        /// </summary>
        /// <param name="flag">主/备服务器标识</param>
        /// <param name="procName">存储过程名</param>
        /// <param name="conn">连接</param>
        /// <param name="sqlParameters">存储过程参数</param>
        /// <returns>false执行失败</returns>
        private bool ExecuteSql(bool flag, string procName, SqlParameter[] sqlParameters)
        {
            bool falg = true;
            SqlConnection conn = new SqlConnection(connectionString);
            SqlCommand sqlComm = null;
            try
            {
                if (conn == null || conn.ConnectionString.Equals("") || conn.ConnectionString.Equals(string.Empty))
                {
                    conn = new SqlConnection(connectionString);
                }
                if (conn != null && conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                }

                sqlComm = new SqlCommand(procName, conn);
                sqlComm.CommandTimeout = 0;
                sqlComm.CommandType = CommandType.StoredProcedure;
                if (sqlParameters != null && sqlParameters.Length > 0)
                {
                    foreach (SqlParameter sqlParameter in sqlParameters)
                    {
                        sqlComm.Parameters.Add(sqlParameter);
                    }
                }
                sqlComm.ExecuteNonQuery();
            }
            catch (SqlException se)
            {
                falg = false;
            }
            catch (Exception ex)
            {
                falg = false;
            }
            finally
            {
                if (sqlComm != null)
                {
                    sqlComm.Dispose();
                    sqlComm = null;
                }
                if (conn.State != ConnectionState.Closed)
                {
                    conn.Close();
                }
                if (conn != null)
                {
                    conn.Dispose();
                    conn = null;
                }
            }
            return falg;
        }

        /// <summary>
        ///  执行SQL语句或存储过程，成功则返回DataTabel，失败则返回NULL
        /// </summary>
        /// <param name="flag">True:KJ128N数据库;False:KJ128NBackUp数据库</param>
        /// <param name="procName">存储过程名</param>
        /// <param name="sqlParameters">存储过程参数</param>
        /// <returns>成功则返回查询结果(DataTable),失败则返回NULL</returns>
        public DataTable GetDataTabel(bool flag, string procName, SqlParameter[] sqlParameters)
        {
            DataSet ds;
            if (flag)
            {
                ds = GetDataSet(flag, procName, sqlParameters);
                if (ds != null)
                {
                    return ds.Tables[0];
                }
                else
                {
                    return null;
                }
            }
            return null;
        }

        #region [ 方法: 构造一个SqlCommand对象 ]
        /// <summary>
        /// 构造一个SqlCommand对象
        /// </summary>
        /// <param name="sqlConn">SQLConnection的连接</param>
        /// <param name="procName">过程名称</param>
        /// <param name="sqlParmeters">参数内容</param>
        /// <returns>SqlCommand</returns>
        private SqlCommand BuildSqlCommand(SqlConnection sqlConn, string procName, SqlParameter[] sqlParmeters)
        {
            SqlCommand sqlComm = new SqlCommand(procName, sqlConn);
            sqlComm.CommandTimeout = 0;
            sqlComm.CommandType = CommandType.StoredProcedure;
            if (sqlParmeters != null)
            {
                foreach (SqlParameter sqlParameter in sqlParmeters)
                {
                    sqlComm.Parameters.Add(sqlParameter);
                }
            }
            return sqlComm;
        }

        #endregion

        /// <summary>
        /// 执行一个存储过程，返回一个DataSet记录集
        /// </summary>
        /// <param name="flag">True:KJ128N数据库;False:KJ128NBackUp数据库</param>
        /// <param name="procName">存储过程的名字</param>
        /// <param name="conn">SQLConnection的连接</param>
        /// <param name="sqlParmeters">存储过程的参数</param>
        /// <returns>DataSet表的记录集</returns>
        private DataSet GetDataSet(bool flag, string procName, SqlParameter[] sqlParmeters)
        {
            DataSet ds = null;
            SqlConnection conn = new SqlConnection(connectionString);
            SqlDataAdapter sqlDataAdapter = null;
            //打开一个连接
            try
            {
                if (conn == null || conn.ConnectionString.Equals("") || conn.ConnectionString.Equals(string.Empty))
                {
                    conn = new SqlConnection(connectionString);
                }
                ds = new DataSet();
                sqlDataAdapter = new SqlDataAdapter();
                sqlDataAdapter.SelectCommand = BuildSqlCommand(conn, procName, sqlParmeters);
                sqlDataAdapter.Fill(ds);
            }
            catch (SqlException se)
            {
            }
            catch (Exception ex)
            {
            }
            finally
            {
                if (sqlDataAdapter != null)
                {
                    sqlDataAdapter.Dispose();
                    sqlDataAdapter = null;
                }
                if (conn != null)
                {
                    if (conn.State != ConnectionState.Closed)
                    {
                        conn.Close();
                    }
                    conn.Dispose();
                    conn = null;
                }
            }
            return ds;
        }

        #endregion
    }
}
