using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Microsoft.Win32;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Newtonsoft.Json;
using vbAccelerator.Components.Shell;
using MediaCrush;

namespace Installer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            if (App.IsShutdown)
            {
                return;
            }
            SettingsManager.Initialize();
            Program.SettingsManager = new SettingsManager();
            Program.LoadSettings();
            Program.SettingsManager.ForcePropertyUpdate();
            InitializeComponent();
            var reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Installer.LICENSE"));
            licenseText.Text = reader.ReadToEnd();
            reader.Close();
            installPathTextBox.Text = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "MediaCrush");
            // Check for an existing install
            var priorInstall = Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\MediaCrush", "Path", null) as string;
            if (!string.IsNullOrEmpty(priorInstall))
                installPathTextBox.Text = priorInstall;
        }

        private void previousButtonClick(object sender, RoutedEventArgs e)
        {
            progressTabs.SelectedIndex--;
            if (progressTabs.SelectedIndex != progressTabs.Items.Count - 1)
                nextButton.Content = "Next";
            previousButton.IsEnabled = (progressTabs.SelectedIndex != 0);
        }

        private void nextButtonClick(object sender, RoutedEventArgs e)
        {
            if (progressTabs.SelectedIndex == 1) // Install path
            {
                if (!Directory.Exists(installPathTextBox.Text))
                    Directory.CreateDirectory(installPathTextBox.Text);
            }
            if (progressTabs.SelectedIndex == progressTabs.Items.Count - 1)
            {
                // "Finish"
                FinishInstallation();
            }
            progressTabs.SelectedIndex++;
            if (progressTabs.SelectedIndex == progressTabs.Items.Count - 1)
                nextButton.Content = "Finish";
            previousButton.IsEnabled = (progressTabs.SelectedIndex != 0);
        }

        private void FinishInstallation()
        {
            // Copy files
            var path = installPathTextBox.Text;
            CopyFileFromAssembly("MarkdownSharp.dll", path);
            CopyFileFromAssembly("SharpCrush4.dll", path);
            CopyFileFromAssembly("Octokit.dll", path);
            CopyFileFromAssembly("Newtonsoft.Json.dll", path);
            CopyFileFromAssembly("MediaCrush.exe", path);
            CopyFileFromAssembly("Uninstaller.exe", path);
            // Associations
            RegisterApplication(path);
            RegisterUninstaller(path);
            SetToStartup(path);
            if (addToStartMenuCheckBox.IsChecked.Value)
                CreateStartMenuIcon(path);
            PopulateSettings();
            if (Program.SettingsManager.EnableTracking)
                Analytics.TrackFeatureUse("Install");

            Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\MediaCrush", "Path", path);
            Process.Start(Path.Combine(path, "MediaCrush.exe"));
            Close();
        }

        private void PopulateSettings()
        {
            Program.SettingsManager.RunOnStartup = startWithWindowsCheckBox.IsChecked.GetValueOrDefault(true);
            Program.SettingsManager.EnableTracking = enableTrackingCheckBox.IsChecked.GetValueOrDefault(false);
        }

        private void SetToStartup(string path)
        {
            path = Path.Combine(path, "MediaCrush.exe");
            var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
            key.SetValue("Patchy", "\"" + path + "\" --startup");
            key.Close();
        }

        private void CreateStartMenuIcon(string path)
        {
            var startPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), "Programs");
            using (ShellLink shortcut = new ShellLink())
            {
                shortcut.Target = Path.Combine(path, "MediaCrush.exe");
                shortcut.Description = "MediaCrush";
                shortcut.WorkingDirectory = path;
                shortcut.DisplayMode = ShellLink.LinkDisplayMode.edmNormal;
                shortcut.Save(Path.Combine(startPath, "MediaCrush.lnk"));
            }
        }

        private static void RegisterApplication(string installationPath)
        {
            var path = Path.Combine(installationPath, "MediaCrush.exe");
            var startup = string.Format("\"{0}\" \"%1\"", path);

            // Register app path
            Registry.SetValue(@"HKEY_LOCAL_MACHINE\Software\Microsoft\Windows\CurrentVersion\App Paths\MediaCrush.exe", null, path);
            // Register application
            using (var applications = Registry.ClassesRoot.CreateSubKey("Applications"))
                using (var patchy = applications.CreateSubKey("MediaCrush.exe"))
                    patchy.SetValue("FriendlyAppName", "MediaCrush");
        }

        public void RegisterUninstaller(string installPath)
        {
            using (RegistryKey parent = Registry.LocalMachine.CreateSubKey(
                 @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall"))
            {
                RegistryKey key = null;

                try
                {
                    key = parent.CreateSubKey("Patchy");

                    key.SetValue("DisplayName", "MediaCrush");
                    key.SetValue("ApplicationVersion", MediaCrush.Program.Version.ToString());
                    key.SetValue("Publisher", "MediaCrush");
                    key.SetValue("DisplayIcon", Path.Combine(installPath, "MediaCrush.exe"));
                    key.SetValue("DisplayVersion", MediaCrush.Program.Version.ToString());
                    key.SetValue("URLInfoAbout", "https://mediacru.sh");
                    key.SetValue("Contact", "sir@cmpwn.com");
                    key.SetValue("InstallDate", DateTime.Now.ToString("yyyyMMdd"));
                    key.SetValue("UninstallString", Path.Combine(installPath, "Uninstaller.exe"));
                }
                finally
                {
                    if (key != null)
                    {
                        key.Close();
                    }
                }
            }
        }

        private void CopyFileFromAssembly(string file, string path)
        {
            var stream = App.GetEmbeddedResource(file);
            using (var destination = File.Create(Path.Combine(path, file)))
                Extensions.CopyTo(stream, destination);
            stream.Close();
        }

        private void browseSourceClick(object sender, RoutedEventArgs e)
        {
            Process.Start("https://github.com/MediaCrush/MediaCrush-Windows");
        }

        private void browseInstallLocationClick(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                installPathTextBox.Text = dialog.SelectedPath;
        }
    }
}
