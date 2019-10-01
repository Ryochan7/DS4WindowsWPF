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
using System.Windows.Navigation;
using System.Windows.Shapes;
using DS4WinWPF.DS4Forms.ViewModel;

namespace DS4WinWPF.DS4Forms
{
    /// <summary>
    /// Interaction logic for AutoProfiles.xaml
    /// </summary>
    public partial class AutoProfiles : UserControl
    {
        protected String m_Profile = DS4Windows.Global.appdatapath + "\\Auto Profiles.xml";
        public const string steamCommx86Loc = @"C:\Program Files (x86)\Steam\steamapps\common";
        public const string steamCommLoc = @"C:\Program Files\Steam\steamapps\common";
        private string steamgamesdir;
        private AutoProfilesViewModel autoProfVM;
        private AutoProfileHolder autoProfileHolder;

        public AutoProfileHolder AutoProfileHolder { get => autoProfileHolder;
            set => autoProfileHolder = value; }
        public AutoProfilesViewModel AutoProfVM { get => autoProfVM; }

        public AutoProfiles()
        {
            InitializeComponent();

            if (!File.Exists(DS4Windows.Global.appdatapath + @"\Auto Profiles.xml"))
                DS4Windows.Global.CreateAutoProfiles(m_Profile);

            //LoadP();

            if (DS4Windows.Global.UseCustomSteamFolder &&
                Directory.Exists(DS4Windows.Global.CustomSteamFolder))
                steamgamesdir = DS4Windows.Global.CustomSteamFolder;
            else if (Directory.Exists(steamCommx86Loc))
                steamgamesdir = steamCommx86Loc;
            else if (Directory.Exists(steamCommLoc))
                steamgamesdir = steamCommLoc;
            else
                addProgramsBtn.ContextMenu.Items.Remove(steamMenuItem);

            autoProfileHolder = new AutoProfileHolder();
            autoProfVM = new AutoProfilesViewModel(autoProfileHolder);
            DataContext = autoProfVM;

            autoProfVM.SearchFinished += AutoProfVM_SearchFinished;
        }

        private void AutoProfVM_SearchFinished(object sender, EventArgs e)
        {
            this.IsEnabled = true;
        }

        public void SetDataContext()
        {
            DataContext = autoProfVM;
        }

        private void SteamMenuItem_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BrowseProgsMenuItem_Click(object sender, RoutedEventArgs e)
        {

        }

        private void StartMenuItem_Click(object sender, RoutedEventArgs e)
        {
            this.IsEnabled = false;
            addProgramsBtn.ContextMenu.Items.Remove(startMenuItem);
            programListLV.ItemsSource = null;
            autoProfVM.AddProgramsFromStartMenu();
            autoProfVM.SearchFinished += StartMenuSearchFinished;
        }

        private void StartMenuSearchFinished(object sender, EventArgs e)
        {
            autoProfVM.SearchFinished -= StartMenuSearchFinished;
            programListLV.ItemsSource = autoProfVM.ProgramColl;
        }

        private void AddProgramsBtn_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            addProgramsBtn.ContextMenu.IsOpen = true;
            e.Handled = true;
        }

        private void AddProgramsBtn_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            addProgramsBtn.ContextMenu.IsOpen = true;
            e.Handled = true;
        }

        private void AddProgramsBtn_Click(object sender, RoutedEventArgs e)
        {
            addProgramsBtn.ContextMenu.IsOpen = true;
            e.Handled = true;
        }
    }
}
