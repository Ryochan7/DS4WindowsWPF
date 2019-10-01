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
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace DS4WinWPF.DS4Forms.ViewModel
{
    public class AutoProfilesViewModel
    {
        private ObservableCollection<ProgramItem> programColl;
        private AutoProfileHolder autoProfileHolder;
        private int selectedIndex = -1;
        private HashSet<string> existingapps;

        public ObservableCollection<ProgramItem> ProgramColl { get => programColl; }
        
        public AutoProfileHolder AutoProfileHolder { get => autoProfileHolder; }

        public int SelectedIndex { get => selectedIndex; set => selectedIndex = value; }

        public event EventHandler SearchFinished;

        public AutoProfilesViewModel(AutoProfileHolder autoProfileHolder)
        {
            programColl = new ObservableCollection<ProgramItem>();
            existingapps = new HashSet<string>();
            this.autoProfileHolder = autoProfileHolder;
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

                    programColl.Add(item);
                    existingapps.Add(target);
                }
            }

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
        public string Title { get => title; }
        public AutoProfileEntity MatchedAutoProfile { get => matchedAutoProfile; set => matchedAutoProfile = value; }
        public string Filename { get => filename;  }
        public ImageSource Exeicon { get => exeicon; }

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
