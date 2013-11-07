using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.IO;
using Microsoft.Win32;
using System.Reflection;

namespace Uninstaller
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void cancelButtonClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void nextButtonClick(object sender, RoutedEventArgs e)
        {
            // First delete settings and cache
            Visibility = Visibility.Hidden;
            try
            {
                if (removeSettingsCheckBox.IsChecked.Value)
                    Directory.Delete(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ".mediacrush"), true);
            }
            catch
            {
                MessageBox.Show("Failed to delete settings and cache.");
            }
            // Now, delete relevant registry entries
            string installPath = null;
            try
            {
                installPath = Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\MediaCrush", "Path", null) as string;
                Registry.LocalMachine.DeleteSubKeyTree(@"SOFTWARE\MediaCrush", false);
                Registry.LocalMachine.DeleteSubKeyTree(@"Software\Microsoft\Windows\CurrentVersion\App Paths\MediaCrush.exe", false);
                Registry.ClassesRoot.DeleteSubKeyTree(@"Applications\MediaCrush.exe", false);
                Registry.LocalMachine.DeleteSubKeyTree(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\MediaCrush", false);
            }
            catch
            {
                MessageBox.Show("Failed to delete registry entries.");
            }
            // Take it out of the start menu
            var startPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), "Programs");
            if (File.Exists(Path.Combine(startPath, "MediaCrush.lnk")))
                File.Delete(Path.Combine(startPath, "MediaCrush.lnk"));
            // Finally, remove MediaCrush itself (everything but the uninstaller)
            var files = Directory.GetFiles(installPath, "*", SearchOption.AllDirectories).Where(f => f != Assembly.GetEntryAssembly().Location);
            foreach (var file in files)
                File.Delete(file);
            // Schedule the uninstaller for removal
            MoveFileEx(Assembly.GetEntryAssembly().Location, null, MoveFileFlags.MOVEFILE_DELAY_UNTIL_REBOOT);
            MessageBox.Show("MediaCrush has been removed from your computer.");
            Close();
        }

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        static extern bool MoveFileEx(string lpExistingFileName, string lpNewFileName,
           MoveFileFlags dwFlags);

        [Flags]
        enum MoveFileFlags
        {
            MOVEFILE_REPLACE_EXISTING = 0x00000001,
            MOVEFILE_COPY_ALLOWED = 0x00000002,
            MOVEFILE_DELAY_UNTIL_REBOOT = 0x00000004,
            MOVEFILE_WRITE_THROUGH = 0x00000008,
            MOVEFILE_CREATE_HARDLINK = 0x00000010,
            MOVEFILE_FAIL_IF_NOT_TRACKABLE = 0x00000020
        }
    }
}
