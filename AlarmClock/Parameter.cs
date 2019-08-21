using System;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Xml;
using System.Xml.Serialization;

namespace AlarmClock
{
    public class Parameter
    {
        public int[] delays = new int[] { 5, 10, 15, 20, 30, 60 };
        public int delayIndex = 4;
        public int precision = 1;
        public string[] timeName = new string[] { "凌晨", "早上", "清晨", "早晨", "上午", "中午", "下午", "傍晚", "晚上", "半夜" };
        public int[] timeNameIndex = new int[] { 8, 0, 0, 0, 0, 1, 1, 1, 4, 4, 4, 4, 5, 5, 5, 6, 6, 6, 6, 8, 8, 8, 8, 8 };
        public int speechRate = 0;
        public int speechVolume = 100;
        public string speechHead = "现在时刻，";
        public bool is12Hour = true;
        public bool isHalfCall = true;
        public string speakerName = "";
        public string[] speechTail = new string[24];

        public void LoadConfig()
        {
            XmlDocument configFile = new XmlDocument();
            string path = GetConfigFile();
            try
            {
                configFile.Load(path);

                // Read delays
                var delayXML = configFile.GetElementsByTagName("delays")[0].ChildNodes;
                delays = new int[delayXML.Count];
                for (int i = 0; i < delayXML.Count; i++)
                {
                    delays[i] = int.Parse(delayXML[i].InnerXml);
                }

                // delayIndex
                delayIndex = int.Parse(configFile.GetElementsByTagName("delayIndex")[0].InnerXml);

                // precision
                precision = int.Parse(configFile.GetElementsByTagName("precision")[0].InnerXml);

                // time name
                var timeNameXML = configFile.GetElementsByTagName("timeName")[0].ChildNodes;
                timeName = new string[timeNameXML.Count];
                for (int i = 0; i < timeNameXML.Count; i++)
                {
                    timeName[i] = timeNameXML[i].InnerXml;
                }

                for (int i = 0; i < 24; i++)
                {
                    timeNameIndex[i] = int.Parse(configFile
                        .GetElementsByTagName("timeNameIndex")[0]
                        .ChildNodes[i].InnerXml);
                }

                // speech rate
                speechRate = int.Parse(configFile.GetElementsByTagName("speechRate")[0].InnerXml);

                // speech volume
                speechVolume = int.Parse(configFile.GetElementsByTagName("speechVolume")[0].InnerXml);

                // speech head
                speechHead = configFile.GetElementsByTagName("speechHead")[0].InnerXml;

                // speech tail
                var speechTails = configFile.GetElementsByTagName("speechTail")[0].ChildNodes;
                for (int i = 0; i < speechTails.Count; i++)
                {
                    speechTail[i] = speechTails[i].InnerXml;
                }

                // is12Hour
                is12Hour = bool.Parse(configFile.GetElementsByTagName("is12Hour")[0].InnerXml);

                // isHalfHour
                isHalfCall = bool.Parse(configFile.GetElementsByTagName("isHalfCall")[0].InnerXml);

                // speaker name
                speakerName = configFile.GetElementsByTagName("speakerName")[0].InnerXml;
            } catch (FileNotFoundException)
            {
                WriteConfig();
            } catch (Exception)
            {
                MessageBox.Show("Unable to parse config file.");
            }
        }

        public string GetConfigFile()
        {
            string path = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            return path + "\\config.xml";
        }

        public void WriteConfig()
        {
            XmlSerializer writer = new XmlSerializer(typeof(Parameter));
            string path = GetConfigFile();
            FileStream file = File.Create(path);
            writer.Serialize(file, this);
            file.Close();
        }
    }
}
