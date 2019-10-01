using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace DS4WinWPF.DS4Forms.ViewModel
{
    public class AutoProfilesViewModel
    {
        private object _colLockobj = new object();
        private ObservableCollection<ProgramItem> programColl;
        private AutoProfileHolder autoProfileHolder;
        private ProfileList profileList;
        private int selectedIndex = -1;
        private ProgramItem selectedItem;
        private HashSet<string> existingapps;

        public ObservableCollection<ProgramItem> ProgramColl { get => programColl; }
        
        public AutoProfileHolder AutoProfileHolder { get => autoProfileHolder; }

        public int SelectedIndex { get => selectedIndex; set => selectedIndex = value; }
        public ProgramItem SelectedItem
        {
            get => selectedItem;
            set
            {
                selectedItem = value;
                CurrentItemChange?.Invoke(this, value);
            }
        }

        public ProfileList ProfileList { get => profileList; }

        public delegate void CurrentItemChangeHandler(AutoProfilesViewModel sender, ProgramItem item);
        public event CurrentItemChangeHandler CurrentItemChange;

        public event EventHandler SearchFinished;
        public delegate void AutoProfileHandler(AutoProfilesViewModel sender,
            ProgramItem item, bool state);
        public event AutoProfileHandler AutoProfileUpdated;

        public AutoProfilesViewModel(AutoProfileHolder autoProfileHolder, ProfileList profileList)
        {
            programColl = new ObservableCollection<ProgramItem>();
            existingapps = new HashSet<string>();
            this.autoProfileHolder = autoProfileHolder;
            this.profileList = profileList;
            PopulateCurrentEntries();

            BindingOperations.EnableCollectionSynchronization(programColl, _colLockobj);
        }

        private void PopulateCurrentEntries()
        {
            foreach(AutoProfileEntity entry in autoProfileHolder.AutoProfileColl)
            {
                ProgramItem item = new ProgramItem(entry.Path, entry);

                item.AutoProfileAction += ProgramItem_AutoProfileAction;
                item.SelectProfChange += ProgramItem_SelectProfChange;
                programColl.Add(item);
                existingapps.Add(entry.Path);
            }
        }

        public void RemoveUnchecked()
        {
            programColl.Clear();
            existingapps.Clear();
            PopulateCurrentEntries();
        }

        public void GetApps(string path)
        {
            foreach(string file in Directory.GetFiles(path, "*.exe", SearchOption.AllDirectories))
            {
                ProgramItem item = new ProgramItem(file);
                if (autoProfileHolder.AutoProfileDict.TryGetValue(file, out AutoProfileEntity autoEntity))
                {
                    item.MatchedAutoProfile = autoEntity;
                }

                programColl.Add(item);
                existingapps.Add(file);
            }
        }

        public async void AddProgramsFromStartMenu()
        {
            await Task.Run(() =>
            {
                AddFromShortcuts(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu) + "\\Programs");
            });

            SearchFinished?.Invoke(this, EventArgs.Empty);
        }

        private void AddFromShortcuts(string path)
        {
            List<string> lnkpaths = new List<string>();
            lnkpaths.AddRange(Directory.GetFiles(path, "*.lnk", SearchOption.AllDirectories));
            lnkpaths.AddRange(Directory.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu) + "\\Programs", "*.lnk", SearchOption.AllDirectories));

            foreach(string link in lnkpaths)
            {
                string target = GetTargetPath(link);
                bool skip = !File.Exists(target) || Path.GetExtension(target) != ".exe";
                skip = skip || target.Contains("etup") || target.Contains("dotnet") || target.Contains("SETUP")
                    || target.Contains("edist") || target.Contains("nstall") || String.IsNullOrEmpty(target);
                skip = skip || existingapps.Contains(target);
                if (!skip)
                {
                    ProgramItem item = new ProgramItem(target);
                    if (autoProfileHolder.AutoProfileDict.TryGetValue(target, out AutoProfileEntity autoEntity))
                    {
                        item.MatchedAutoProfile = autoEntity;
                    }

                    item.AutoProfileAction += ProgramItem_AutoProfileAction;
                    item.SelectProfChange += ProgramItem_SelectProfChange;
                    programColl.Add(item);
                    existingapps.Add(target);
                }
            }
        }

        private void ProgramItem_SelectProfChange(ProgramItem sender, int devindex, int profindex)
        {
            if (profindex <= 0)
            {
                sender.MatchedAutoProfile.ProfileNames[devindex] = string.Empty;
            }
            else
            {
                sender.MatchedAutoProfile.ProfileNames[devindex] = profileList.ProfileListCol[profindex - 1].Name;
            }
        }

        private void ProgramItem_AutoProfileAction(ProgramItem sender, bool added)
        {
            if (added)
            {
                sender.MatchedAutoProfile = new AutoProfileEntity()
                {
                    Path = sender.Path,
                    Title = sender.Title,
                };

                autoProfileHolder.AutoProfileColl.Add(sender.MatchedAutoProfile);
                autoProfileHolder.AutoProfileDict.Add(sender.Path, sender.MatchedAutoProfile);
            }
            else
            {
                autoProfileHolder.AutoProfileColl.Remove(sender.MatchedAutoProfile);
                autoProfileHolder.AutoProfileDict.Remove(sender.Path);
            }

            AutoProfileUpdated?.Invoke(this, sender, added);
        }

        private string GetTargetPath(string filePath)
        {
            string targetPath = ResolveMsiShortcut(filePath);
            if (targetPath == null)
            {
                targetPath = ResolveShortcut(filePath);
            }

            return targetPath;
        }

        public string ResolveShortcutAndArgument(string filePath)
        {
            Type t = Type.GetTypeFromCLSID(new Guid("72C24DD5-D70A-438B-8A42-98424B88AFB8")); // Windows Script Host Shell Object
            dynamic shell = Activator.CreateInstance(t);
            string result;

            try
            {
                var shortcut = shell.CreateShortcut(filePath);
                result = shortcut.TargetPath + " " + shortcut.Arguments;
                Marshal.FinalReleaseComObject(shortcut);
            }
            catch (COMException)
            {
                // A COMException is thrown if the file is not a valid shortcut (.lnk) file 
                result = null;
            }
            finally
            {
                Marshal.FinalReleaseComObject(shell);
            }

            return result;
        }

        public string ResolveMsiShortcut(string file)
        {
            StringBuilder product = new StringBuilder(NativeMethods2.MaxGuidLength + 1);
            StringBuilder feature = new StringBuilder(NativeMethods2.MaxFeatureLength + 1);
            StringBuilder component = new StringBuilder(NativeMethods2.MaxGuidLength + 1);

            NativeMethods2.MsiGetShortcutTarget(file, product, feature, component);

            int pathLength = NativeMethods2.MaxPathLength;
            StringBuilder path = new StringBuilder(pathLength);

            NativeMethods2.InstallState installState = NativeMethods2.MsiGetComponentPath(product.ToString(), component.ToString(), path, ref pathLength);
            if (installState == NativeMethods2.InstallState.Local)
            {
                return path.ToString();
            }
            else
            {
                return null;
            }
        }

        public string ResolveShortcut(string filePath)
        {
            Type t = Type.GetTypeFromCLSID(new Guid("72C24DD5-D70A-438B-8A42-98424B88AFB8")); // Windows Script Host Shell Object
            dynamic shell = Activator.CreateInstance(t);
            string result;

            try
            {
                var shortcut = shell.CreateShortcut(filePath);
                result = shortcut.TargetPath;
                Marshal.FinalReleaseComObject(shortcut);
            }
            catch (COMException)
            {
                // A COMException is thrown if the file is not a valid shortcut (.lnk) file 
                result = null;
            }
            finally
            {
                Marshal.FinalReleaseComObject(shell);
            }

            return result;
        }
    }

    public class ProgramItem
    {
        private string path;
        private string path_lowercase;
        private string filename;
        private string title;
        private string title_lowercase;
        private AutoProfileEntity matchedAutoProfile;
        private ImageSource exeicon;

        public string Path { get => path; }
        public string Title { get => title;
            set
            {
                if (title == value) return;
                title = value;
                if (matchedAutoProfile != null)
                {
                    matchedAutoProfile.Title = value;
                }

                TitleChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler TitleChanged;
        public AutoProfileEntity MatchedAutoProfile
        {
            get => matchedAutoProfile;
            set
            {
                matchedAutoProfile = value;
                if (matchedAutoProfile != null)
                {
                    title = matchedAutoProfile.Title ?? string.Empty;
                    title_lowercase = title.ToLower();
                }
            }
        }
        public event EventHandler MatchedAutoProfileChanged;
        public delegate void AutoProfileHandler(ProgramItem sender, bool added);
        public event AutoProfileHandler AutoProfileAction;
        public string Filename { get => filename;  }
        public ImageSource Exeicon { get => exeicon; }

        public bool Turnoff
        {
            get
            {
                bool result = false;
                if (matchedAutoProfile != null)
                {
                    result = matchedAutoProfile.Turnoff;
                }

                return result;
            }
            set
            {
                if (matchedAutoProfile != null)
                {
                    matchedAutoProfile.Turnoff = value;
                    TurnoffChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }
        public event EventHandler TurnoffChanged;

        public bool Exists
        {
            get => matchedAutoProfile != null;
            set
            {
                if (matchedAutoProfile != null && !value)
                {
                    matchedAutoProfile = null;
                    AutoProfileAction?.Invoke(this, false);
                }
                else if (matchedAutoProfile == null && value)
                {
                    AutoProfileAction?.Invoke(this, true);
                }

                MatchedAutoProfileChanged?.Invoke(this, EventArgs.Empty);
                ExistsChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler ExistsChanged;

        private int selectedIndexCon1 = 0;
        private int selectedIndexCon2 = 0;
        private int selectedIndexCon3 = 0;
        private int selectedIndexCon4 = 0;

        public int SelectedIndexCon1
        {
            get => selectedIndexCon1;
            set
            {
                if (selectedIndexCon1 == value) return;
                selectedIndexCon1 = value;
                SelectProfChange?.Invoke(this, 0, value);
            }
        }

        public int SelectedIndexCon2
        {
            get => selectedIndexCon2;
            set
            {
                if (selectedIndexCon2 == value) return;
                selectedIndexCon2 = value;
                SelectProfChange?.Invoke(this, 1, value);
            }
        }
        public int SelectedIndexCon3
        {
            get => selectedIndexCon3;
            set
            {
                if (selectedIndexCon3 == value) return;
                selectedIndexCon3 = value;
                SelectProfChange?.Invoke(this, 2, value);
            }
        }
        public int SelectedIndexCon4
        {
            get => selectedIndexCon4;
            set
            {
                if (selectedIndexCon4 == value) return;
                selectedIndexCon4 = value;
                SelectProfChange?.Invoke(this, 3, value);
            }
        }

        public delegate void SelectedProfChangeHandler(ProgramItem sender, int devindex, int profindex);
        public event SelectedProfChangeHandler SelectProfChange;

        public ProgramItem(string path, AutoProfileEntity autoProfileEntity = null)
        {
            this.path = path;
            this.path_lowercase = path.ToLower();
            filename = System.IO.Path.GetFileNameWithoutExtension(path);
            this.matchedAutoProfile = autoProfileEntity;
            if (autoProfileEntity != null)
            {
                title = autoProfileEntity.Title;
                title_lowercase = title.ToLower();
            }

            using (Icon ico = Icon.ExtractAssociatedIcon(path))
            {
                exeicon = Imaging.CreateBitmapSourceFromHIcon(ico.Handle, Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());
                exeicon.Freeze();
            }
        }
    }

    [SuppressUnmanagedCodeSecurity]
    class NativeMethods2
    {
        [DllImport("msi.dll", CharSet = CharSet.Auto)]
        public static extern uint MsiGetShortcutTarget(string targetFile, StringBuilder productCode, StringBuilder featureID, StringBuilder componentCode);

        [DllImport("msi.dll", CharSet = CharSet.Auto)]
        public static extern InstallState MsiGetComponentPath(string productCode, string componentCode, StringBuilder componentPath, ref int componentPathBufferSize);

        public const int MaxFeatureLength = 38;
        public const int MaxGuidLength = 38;
        public const int MaxPathLength = 1024;

        public enum InstallState
        {
            NotUsed = -7,
            BadConfig = -6,
            Incomplete = -5,
            SourceAbsent = -4,
            MoreData = -3,
            InvalidArg = -2,
            Unknown = -1,
            Broken = 0,
            Advertised = 1,
            Removed = 1,
            Absent = 2,
            Local = 3,
            Source = 4,
            Default = 5
        }
    }
}
