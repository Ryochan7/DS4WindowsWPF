using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DS4WinWPF.DS4Forms
{
    /// <summary>
    /// Interaction logic for SaveWhere.xaml
    /// </summary>
    public partial class SaveWhere : Window
    {
        private bool multisaves;

        public SaveWhere(bool multisavespots)
        {
            InitializeComponent();
            multisaves = multisavespots;
            if (!multisavespots)
            {
                multipleSavesDockP.Visibility = Visibility.Collapsed;
                pickWhereLb.Content += Properties.Resources.OtherFileLocation;
            }

            if (DS4Windows.Global.AdminNeeded())
            {
                progFolderBtn.IsEnabled = false;
            }
        }

        private void ProgFolderBtn_Click(object sender, RoutedEventArgs e)
        {
            DS4Windows.Global.SaveWhere(DS4Windows.Global.exepath);
            if (multisaves && dontDeleteCk.IsChecked == false)
            {
                try { Directory.Delete(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\DS4Windows", true); }
                catch { }
            }
            else if (!multisaves)
                DS4Windows.Global.SaveDefault(DS4Windows.Global.exepath + "\\Profiles.xml");

            Close();
        }

        private void AppdataBtn_Click(object sender, RoutedEventArgs e)
        {
            if (multisaves && dontDeleteCk.IsChecked == false)
            {
                try
                {
                    Directory.Delete(DS4Windows.Global.exepath + "\\Profiles", true);
                    File.Delete(DS4Windows.Global.exepath + "\\Profiles.xml");
                    File.Delete(DS4Windows.Global.exepath + "\\Auto Profiles.xml");
                }
                catch (UnauthorizedAccessException) { MessageBox.Show("Cannot Delete old settings, please manaully delete", "DS4Windows"); }
            }
            else if (!multisaves)
                DS4Windows.Global.SaveDefault(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\DS4Windows\\Profiles.xml");

            DS4Windows.Global.SaveWhere(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\DS4Windows");
            Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
        }
    }
}
