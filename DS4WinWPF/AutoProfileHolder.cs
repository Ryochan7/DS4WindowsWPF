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
                    string path = x.Attributes["path"]?.Value;
                    AutoProfileEntity autoprof = new AutoProfileEntity()
                    { Path = path,
                      Title = x.Attributes["title"]?.Value
                    };

                    XmlNode item;
                    for (int i = 0; i < 4; i++)
                    {
                        item = x.SelectSingleNode("Controller{i+1}");
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
    }

    public class AutoProfileEntity
    {
        private string path;
        private string title;
        private bool turnoff;
        private string[] profileNames = new string[4] { string.Empty, string.Empty,
            string.Empty, string.Empty };

        public string Path { get => path; set => path = value; }
        public string Title { get => title; set => title = value; }
        public bool Turnoff { get => turnoff; set => turnoff = value; }
        public string[] ProfileNames { get => profileNames; set => profileNames = value; }
    }
}
