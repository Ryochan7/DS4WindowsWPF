using DS4WinWPF.DS4Forms.ViewModel;
using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DS4WinWPF.DS4Forms
{
    /// <summary>
    /// Interaction logic for ProfileEditor.xaml
    /// </summary>
    public partial class ProfileEditor : UserControl
    {
        private ProfileSettingsViewModel profileSettingsVM;
        private ProfileEntity currentProfile;

        public event EventHandler Closed;

        public ProfileEditor(int device)
        {
            InitializeComponent();

            emptyColorGB.Visibility = Visibility.Collapsed;
            profileSettingsVM = new ProfileSettingsViewModel(device);
        }

        public void Reload(int device, ProfileEntity profile = null)
        {
            profileSettingsTabCon.DataContext = null;
            touchpadSettingsPanel.DataContext = null;
            mappingListBox.DataContext = null;

            if (profile != null)
            {
                currentProfile = profile;
                if (device == 4)
                {
                    DS4Windows.Global.ProfilePath[4] = profile.Name;
                }

                DS4Windows.Global.LoadProfile(device, false, App.rootHub);
                profileNameTxt.Text = profile.Name;
            }
            else
            {
                currentProfile = null;
            }

            profileSettingsTabCon.DataContext = profileSettingsVM;
            touchpadSettingsPanel.DataContext = profileSettingsVM;
            mappingListBox.DataContext = null;
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            Closed?.Invoke(this, EventArgs.Empty);
        }
    }
}
