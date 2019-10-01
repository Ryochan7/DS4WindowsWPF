using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Xml;

namespace DS4WinWPF
{
    public class AutoProfileHolder
    {
        private object _colLockobj = new object();
        private ObservableCollection<AutoProfileEntity> autoProfileColl;
        public ObservableCollection<AutoProfileEntity> AutoProfileColl { get => autoProfileColl; }
        public Dictionary<string, AutoProfileEntity> AutoProfileDict { get => autoProfileDict; }

        private Dictionary<string, AutoProfileEntity> autoProfileDict;

        public AutoProfileHolder()
        {
            autoProfileColl = new ObservableCollection<AutoProfileEntity>();
            autoProfileDict = new Dictionary<string, AutoProfileEntity>();
            Load();

            BindingOperations.EnableCollectionSynchronization(autoProfileColl, _colLockobj);
        }

        private void Load()
        {
            try
            {
                XmlDocument doc = new XmlDocument();

                if (!File.Exists(DS4Windows.Global.appdatapath + "\\Auto Profiles.xml"))
                    return;

                doc.Load(DS4Windows.Global.appdatapath + "\\Auto Profiles.xml");
                XmlNodeList programslist = doc.SelectNodes("Programs/Program");
                foreach (XmlNode x in programslist)
                {
                    string path = x.Attributes["path"]?.Value ?? string.Empty;
                    AutoProfileEntity autoprof = new AutoProfileEntity()
                    { Path = path,
                      Title = x.Attributes["title"]?.Value ?? string.Empty
                    };

                    XmlNode item;
                    for (int i = 0; i < 4; i++)
                    {
                        item = x.SelectSingleNode($"Controller{i+1}");
                        if (item != null)
                        {
                            autoprof.ProfileNames[i] = item.InnerText;
                        }
                    }

                    item = x.SelectSingleNode($"TurnOff");
                    if (item != null && bool.TryParse(item.InnerText, out bool turnoff))
                    {
                        autoprof.Turnoff = turnoff;
                    }

                    autoProfileColl.Add(autoprof);
                    autoProfileDict.Add(path, autoprof);
                }
            }
            catch (Exception) { }
        }

        public bool Save(string m_Profile)
        {
            XmlDocument doc = new XmlDocument();
            XmlNode Node;
            bool saved = true;
            try
            {
                Node = doc.CreateXmlDeclaration("1.0", "utf-8", string.Empty);
                doc.AppendChild(Node);

                Node = doc.CreateComment(string.Format(" Auto-Profile Configuration Data. {0} ", DateTime.Now));
                doc.AppendChild(Node);

                Node = doc.CreateWhitespace("\r\n");
                doc.AppendChild(Node);

                Node = doc.CreateNode(XmlNodeType.Element, "Programs", "");
                doc.AppendChild(Node);
                foreach (AutoProfileEntity entity in autoProfileColl)
                {
                    XmlElement el = doc.CreateElement("Program");
                    el.SetAttribute("path", entity.Path);
                    if (!string.IsNullOrEmpty(entity.Title))
                    {
                        el.SetAttribute("title", entity.Title);
                    }

                    el.AppendChild(doc.CreateElement("Controller1")).InnerText = entity.ProfileNames[0];
                    el.AppendChild(doc.CreateElement("Controller2")).InnerText = entity.ProfileNames[1];
                    el.AppendChild(doc.CreateElement("Controller3")).InnerText = entity.ProfileNames[2];
                    el.AppendChild(doc.CreateElement("Controller4")).InnerText = entity.ProfileNames[3];
                    el.AppendChild(doc.CreateElement("TurnOff")).InnerText = entity.Turnoff.ToString();

                    Node.AppendChild(el);
                }

                doc.Save(m_Profile);
            }
            catch (Exception) { saved = false; }
            return saved;
        }

        public void Remove(AutoProfileEntity item)
        {
            autoProfileDict.Remove(item.Path);
            autoProfileColl.Remove(item);
        }
    }

    public class AutoProfileEntity
    {
        private string path = string.Empty;
        private string title = string.Empty;
        private bool turnoff;
        private string[] profileNames = new string[4] { string.Empty, string.Empty,
            string.Empty, string.Empty };

        public string Path { get => path; set => path = value; }
        public string Title { get => title; set => title = value; }
        public bool Turnoff { get => turnoff; set => turnoff = value; }
        public string[] ProfileNames { get => profileNames; set => profileNames = value; }
    }
}
