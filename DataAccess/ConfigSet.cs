using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Windows.Forms;

namespace DataAccess
{
    public class ConfigSet
    {
         public static string GetConfigValue(string appKey)
        {
            try
            {
                XmlDocument xDoc = new XmlDocument();
                //xDoc.Load(Application.ExecutablePath + @".config");
                xDoc.Load(Application.StartupPath + @"\DataAccess.dll.config");

                XmlNode xNode;
                XmlElement xElem;
                xNode = xDoc.SelectSingleNode("//appSettings");
                xElem = (XmlElement)xNode.SelectSingleNode("//add[@key='" + appKey + "']");
                if (xElem != null)
                    return xElem.GetAttribute("value");
                else
                    return "";
            }
            catch (Exception)
            {
                return "";
            }
        }

        public static void SetValue(string AppKey, string AppValue)
        {
            try
            {
                XmlDocument xDoc = new XmlDocument();
                //获取可执行文件的路径和名称
                xDoc.Load(Application.StartupPath + @"\DataAccess.dll.config");

                XmlNode xNode;
                XmlElement xElem1;
                XmlElement xElem2;
                xNode = xDoc.SelectSingleNode("//appSettings");

                xElem1 = (XmlElement)xNode.SelectSingleNode("//add[@key='" + AppKey + "']");
                if (xElem1 != null) xElem1.SetAttribute("value", AppValue);
                else
                {
                    xElem2 = xDoc.CreateElement("add");
                    xElem2.SetAttribute("key", AppKey);
                    xElem2.SetAttribute("value", AppValue);
                    xNode.AppendChild(xElem2);
                }
                xDoc.Save(Application.StartupPath + @"\DataAccess.dll.config");
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
