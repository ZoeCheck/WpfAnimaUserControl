using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;

namespace DataAccess
{
    public class Converter
    {
        public static XmlDocument BytesToXml(byte[] xmlbyte)
        {
            string filePath = AppDomain.CurrentDomain.BaseDirectory + @"\Buffer.shz";
            FileStream fs = new FileStream(filePath, FileMode.Create);
            fs.Write(xmlbyte, 0, xmlbyte.Length);
            fs.Flush();
            fs.Close();
            fs.Dispose();
            XmlDocument xmldoc = new XmlDocument();
            xmldoc.Load(filePath);
            return xmldoc;
        }

        public static void CreateFile(string filepath, byte[] filebyte)
        {
            System.IO.FileStream fs = new System.IO.FileStream(filepath, System.IO.FileMode.OpenOrCreate);
            fs.Write(filebyte, 0, filebyte.Length);
            fs.Flush();
            fs.Close();
            fs.Dispose();
        }
    }
}
