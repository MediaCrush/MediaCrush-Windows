using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.ServiceModel;
using System.Threading;
using System.Windows;
using Microsoft.Win32;

namespace Installer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static Stream GetEmbeddedResource(string name)
        {
            var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(name);
            if (stream == null)
                return null;
            var memStream = new MemoryStream();
            using (var gStream = new GZipStream(stream, CompressionMode.Decompress))
                Extensions.CopyTo(gStream, memStream);
            memStream.Seek(0, SeekOrigin.Begin);
            return memStream;
        }

        // Crazy hacky stuff to make it so we can bundle the installer up in a single file
        static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            var assemblyName = new AssemblyName(args.Name);

            string path = assemblyName.Name + ".dll";
            if (assemblyName.CultureInfo.Equals(CultureInfo.InvariantCulture) == false)
                path = String.Format(@"{0}\{1}", assemblyName.CultureInfo, path);

            var stream = GetEmbeddedResource(path);
            if (stream == null)
                return null;

            var assemblyRawBytes = new byte[stream.Length];
            stream.Read(assemblyRawBytes, 0, assemblyRawBytes.Length);
            return Assembly.Load(assemblyRawBytes);
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
            App.Current.DispatcherUnhandledException += (s, ex) => MessageBox.Show(ex.Exception.ToString());
            // Check for permissions
            if (!UacHelper.IsProcessElevated && !Debugger.IsAttached)
            {
                var info = new ProcessStartInfo(Assembly.GetEntryAssembly().Location);
                info.Verb = "runas";
                Process.Start(info);
                Application.Current.Shutdown();
                return;
            }
            // Check for .NET
            var value = Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full", "Install", null);
            if (value == null)
            {
                var result = MessageBox.Show("You must install Microsoft.NET to run MediaCrush. Would you like to do so now?", 
                    "Error", MessageBoxButton.YesNo, MessageBoxImage.Error);
                if (result == MessageBoxResult.Yes)
                    Process.Start("http://www.microsoft.com/en-us/download/details.aspx?id=17851");
                Application.Current.Shutdown();
                return;
            }
        }
    }
}
