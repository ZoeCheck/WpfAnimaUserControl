using System;
using System.Data;
using System.Xml;
using System.Collections.Generic;

namespace DataAccess
{
    public class BLL
    {
        public string mapID = "";
        public DAL dal = new DAL();
        DataSet ds = new DataSet();
        //public DataTable GetEmpList(string where)
        //{
        //    using (ds = new DataSet())
        //    {
        //        ds = DAL.GetEmpList(where);
        //        if (ds.Tables.Count > 0)
        //        {
        //            return ds.Tables[0];
        //        }
        //        else
        //        {
        //            return null;
        //        }
        //    }
        //}

        #region[登录界面]
        public bool LoginSystem(string User, string Pass)
        {
            if (dal.Login(User, Pass, "") == "")
            {
                return true;
            }
            return false;
        }
        #endregion

        public byte[] GetMapBytes()
        {
            byte[] bytesXmlCfg = GetMapCfgXmlBytes();
            XmlDocument xmldoc = Converter.BytesToXml(bytesXmlCfg);
            XmlNode node = xmldoc.SelectSingleNode("//Map");
            mapID = node.InnerText;
            return dal.GetMapBytesByFileID(mapID);
        }

        public byte[] GetMapCfgXmlBytes()
        {
            return dal.GetMapCfgXmlBytes();
        }

        public List<Station> GetStationList()
        {
            List<Station> listStation = new List<Station>();
            byte[] bytesXmlCfg = GetMapCfgXmlBytes();
            XmlDocument xmldoc = Converter.BytesToXml(bytesXmlCfg);
            XmlNode StationRoot = xmldoc.SelectSingleNode("//Stations");

            if (StationRoot != null && StationRoot.ChildNodes.Count > 0)
            {
                DataTable stationinfodt = dal.GetStationInfo();
                if (stationinfodt == null || stationinfodt.Rows.Count <= 0)
                {
                    return null;
                }
                try
                {
                    for (int i = 0; i < stationinfodt.Rows.Count; i++)
                    {
                        string stationID = stationinfodt.Rows[i][0].ToString() + "." + stationinfodt.Rows[i][1].ToString();
                        string stationName = stationinfodt.Rows[i][2].ToString();
                        float stationheadx = float.Parse(stationinfodt.Rows[i][3].ToString());
                        float stationheady = float.Parse(stationinfodt.Rows[i][4].ToString());
                        string stationstate = stationinfodt.Rows[i][5].ToString();
                        XmlNode stationnode = xmldoc.GetElementById(stationName);
                        if (stationnode != null)
                        {
                            string stationdivname = stationnode.InnerText;

                            Station station = new Station(stationheadx, stationheady, stationName, stationID, stationstate, stationdivname);
                            listStation.Add(station);
                            //if (stationstate == "正常" || stationstate == "未初始化")
                            //    mapgis.AddStation(stationheadx, stationheady, stationName, stationID, "正常", new Bitmap(Application.StartupPath + "\\MapGis\\ShineImage\\Signal.gif"), stationdivname);
                            //if (stationstate == "故障")
                            //    mapgis.AddStation(stationheadx, stationheady, stationName, stationID, stationstate, new Bitmap(Application.StartupPath + "\\MapGis\\ShineImage\\No-Signal.gif"), stationdivname);
                            //if (stationstate == "休眠")
                            //    mapgis.AddStation(stationheadx, stationheady, stationName, stationID, stationstate, new Bitmap(Application.StartupPath + "\\MapGis\\ShineImage\\Station.GIF"), stationdivname);

                        }
                    }
                    //mapgis.FalshStations();
                    return listStation;
                }
                catch (Exception ex)
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        public DataTable GetHisInOutStationHead(string name, DateTime beginDate, DateTime endDate)
        {
            return dal.GetHisInOutStationHead(name, beginDate, endDate);
        }

        public DataTable GetRouteByUserID(int userID, DateTime startDate, DateTime endDate, int fileid)
        {
            return dal.GetRouteByUserID(userID, startDate, endDate, fileid);
        }

        public DataTable GetRoutePointByID(string ID, bool isdesc, string fileid)
        {
            return dal.GetRoutePointByID(ID, isdesc, fileid);
        }

        public string GetEmpIDByEmpName(string empName)
        {
            return dal.GetEmpIDByEmpName(empName);
        }

        public DataTable GetEmpList()
        {
            return dal.GetEmpList();
        }

        public DataTable GetDept()
        {
            return dal.GetDept();
        }

        public DataTable GetDuty()
        {
            return dal.GetDuty();
        }

        public DataTable GetWorkType()
        {
            return dal.GetWorkType();
        }

        public DataTable GetEmpDataTable(string deptName,string dutyName,string woryTypeName,string blockID,string empName)
        {
            return dal.GetEmpDataTable(deptName, dutyName, woryTypeName, blockID, empName);
        }
    }
}
