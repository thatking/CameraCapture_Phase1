using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;

namespace CameraTool
{
    class Configuration
    {
        private static string m_ConfigFileName = "configuration.xml";

        private string m_GammaEna;

        public string GammaEna
        {
            get { return m_GammaEna; }
            set { UpdateConfig("RAWInterpolation", "GammaEna", value); }
        }

        private string m_GammaValue;

        public string GammaValue
        {
            get { return m_GammaValue; }
            set { UpdateConfig("RAWInterpolation", "GammaValue", value); }
        }

        private string m_RGBGainOffsetEna;

        public string RGBGainOffsetEna
        {
            get { return m_RGBGainOffsetEna; }
            set { UpdateConfig("RAWInterpolation", "RGBGainOffsetEna", value); }
        }

        private string m_RGBGainOffset;

        public string RGBGainOffset
        {
            get { return m_RGBGainOffset; }
            set { UpdateConfig("RAWInterpolation", "RGBGainOffset", value); }
        }

        private string m_RGB2RGBMatrixEna;

        public string RGB2RGBMatrixEna
        {
            get { return m_RGB2RGBMatrixEna; }
            set { UpdateConfig("RAWInterpolation", "RGB2RGBMatrixEna", value); }
        }

        private string m_RGB2RGBMatrix;

        public string RGB2RGBMatrix
        {
            get { return m_RGB2RGBMatrix; }
            set { UpdateConfig("RAWInterpolation", "RGB2RGBMatrix", value); }
        }

        private string m_CaptureNum;

        public string CaptureNum
        {
            get { return m_CaptureNum; }
            set { UpdateConfig("Capture", "CaptureNum", value); }
        }

        private string m_CaptureRAW;

        public string CaptureRAW
        {
            get { return m_CaptureRAW; }
            set { UpdateConfig("Capture", "CaptureRAW", value); }
        }

        private string m_CaptureBMP;

        public string CaptureBMP
        {
            get { return m_CaptureBMP; }
            set { UpdateConfig("Capture", "CaptureBMP", value); }
        }

        // default resolution mode, starting from 0
        private string m_CameraDefaultMode;

        public string CameraDefaultMode
        {
            get { return m_CameraDefaultMode; }
            set { UpdateConfig("Camera", "CameraDefaultMode", value); }
        }

        // if "yes", focus proc will down sample to 720p when resolution is > 720p for faster process
        private string m_FocusProcDownSampling;

        public string FocusProcDownSampling
        {
            get { return m_FocusProcDownSampling; }
            set { UpdateConfig("Camera", "FocusProcDownSampling", value); }
        }

        private string m_RegisterSetting;
        public string RegisterSetting
        {
            get { return m_RegisterSetting; }
        }

        public Configuration()
        {
           if ( !File.Exists(m_ConfigFileName))
           {
               CreateConfigFile();
           }

           LoadConfigFile();
        }

        private void CreateConfigFile()
        {
            XmlDocument xmlDoc = new XmlDocument();
            XmlDeclaration declare = xmlDoc.CreateXmlDeclaration("1.0", "utf-8", "yes");
            xmlDoc.AppendChild(declare);

            XmlElement userListEle = xmlDoc.CreateElement("Global");
            xmlDoc.AppendChild(userListEle);

            XmlElement userEle = xmlDoc.CreateElement("Camera");
            userListEle.AppendChild(userEle);

            XmlElement nameEle = xmlDoc.CreateElement("CameraDefaultMode");
            nameEle.InnerText = "0";
            userEle.AppendChild(nameEle);

            nameEle = xmlDoc.CreateElement("FocusProcDownSampling");
            nameEle.InnerText = "YES";
            userEle.AppendChild(nameEle);

            userEle = xmlDoc.CreateElement("RAWInterpolation");
            userListEle.AppendChild(userEle);

            nameEle = xmlDoc.CreateElement("GammaEna");
            nameEle.InnerText = "YES";
            userEle.AppendChild(nameEle);

            nameEle = xmlDoc.CreateElement("GammaValue");
            nameEle.InnerText = "1.0";
            userEle.AppendChild(nameEle);

            nameEle = xmlDoc.CreateElement("RGBGainOffsetEna");
            nameEle.InnerText = "YES";
            userEle.AppendChild(nameEle);

            nameEle = xmlDoc.CreateElement("RGBGainOffset");
            nameEle.InnerText = "512, 512, 512, 0, 0, 0";
            userEle.AppendChild(nameEle);

            nameEle = xmlDoc.CreateElement("RGB2RGBMatrixEna");
            nameEle.InnerText = "YES";
            userEle.AppendChild(nameEle);

            nameEle = xmlDoc.CreateElement("RGB2RGBMatrix");
            nameEle.InnerText = "256, 0, 0, 0, 256, 0, 0, 0, 256";
            userEle.AppendChild(nameEle);

            userEle = xmlDoc.CreateElement("Capture");
            userListEle.AppendChild(userEle);

            nameEle = xmlDoc.CreateElement("CaptureNum");
            nameEle.InnerText = "1";
            userEle.AppendChild(nameEle);

            nameEle = xmlDoc.CreateElement("CaptureBMP");
            nameEle.InnerText = "YES";
            userEle.AppendChild(nameEle);

            nameEle = xmlDoc.CreateElement("CaptureRAW");
            nameEle.InnerText = "YES";
            userEle.AppendChild(nameEle);

            userEle = xmlDoc.CreateElement("Register");
            userListEle.AppendChild(userEle);

            nameEle = xmlDoc.CreateElement("RegisterSetting");
            nameEle.InnerText = "{OV10635:(0xffff,0xffff),(0xffff,0xffff)};{M031:(0xffff,0xffff),(0xffff,0xffff)}";
            userEle.AppendChild(nameEle);

            xmlDoc.Save(m_ConfigFileName);
        }

        private void LoadConfigFile()
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(m_ConfigFileName);

            //find all the nodes
            foreach (XmlNode nodeL1 in xmlDoc.DocumentElement.ChildNodes)
            {
                if (nodeL1.Name == "Camera")
                {
                    foreach (XmlNode nodeL2 in nodeL1.ChildNodes)
                    {
                        if (nodeL2.Name == "CameraDefaultMode")
                            m_CameraDefaultMode = nodeL2.InnerText;

                        if (nodeL2.Name == "FocusProcDownSampling")
                            m_FocusProcDownSampling = nodeL2.InnerText;
                    }
                }

                if (nodeL1.Name == "RAWInterpolation")
                {
                    foreach (XmlNode nodeL2 in nodeL1.ChildNodes)
                    {
                        if (nodeL2.Name == "GammaEna")
                            m_GammaEna = nodeL2.InnerText;

                        if (nodeL2.Name == "GammaValue")
                            m_GammaValue = nodeL2.InnerText;

                        if (nodeL2.Name == "RGBGainOffsetEna")
                            m_RGBGainOffsetEna = nodeL2.InnerText;

                        if (nodeL2.Name == "RGBGainOffset")
                            m_RGBGainOffset = nodeL2.InnerText;

                        if (nodeL2.Name == "RGB2RGBMatrixEna")
                            m_RGB2RGBMatrixEna = nodeL2.InnerText;

                        if (nodeL2.Name == "RGB2RGBMatrix")
                            m_RGB2RGBMatrix = nodeL2.InnerText;
                    }
                }

                if (nodeL1.Name == "Capture")
                {
                    foreach (XmlNode nodeL2 in nodeL1.ChildNodes)
                    {
                        if (nodeL2.Name == "CaptureNum")
                            m_CaptureNum = nodeL2.InnerText;

                        if (nodeL2.Name == "CaptureRAW")
                            m_CaptureRAW = nodeL2.InnerText;

                        if (nodeL2.Name == "CaptureBMP")
                            m_CaptureBMP = nodeL2.InnerText;
                    }
                }

                if (nodeL1.Name == "Register")
                {
                    foreach (XmlNode nodeL2 in nodeL1.ChildNodes)
                    {
                        if (nodeL2.Name == "RegisterSetting")
                        {
                            m_RegisterSetting = nodeL2.InnerText;
                        }
                    }
                }

            }
        }

        private void UpdateConfig(string catalog, string item, string value)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(m_ConfigFileName);

            foreach (XmlNode nodeL1 in xmlDoc.DocumentElement.ChildNodes)
            {
                if (nodeL1.Name == catalog)
                {
                    foreach (XmlNode nodeL2 in nodeL1.ChildNodes)
                    {
                        if (nodeL2.Name == item)
                            nodeL2.InnerText = value;
                    }
                }
            }

            xmlDoc.Save(m_ConfigFileName);
        }
    }
}
