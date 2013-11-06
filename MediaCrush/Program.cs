using MediaCrush;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MediaCrushWindows
{
    static class Program
    {
        static UploadWindow UploadWindow;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            var icon = new NotifyIcon();
            icon.Icon = Icon.ExtractAssociatedIcon(Assembly.GetEntryAssembly().Location);
            icon.Visible = true;
            icon.Text = "Click here to upload files to MediaCrush";

            var menu = new ContextMenu();
            var settings = new MenuItem("Settings");
            var exit = new MenuItem("Exit");
            menu.MenuItems.Add(settings);
            menu.MenuItems.Add(exit);
            exit.Click += (s, e) => Application.Exit();
            settings.Click += settings_Click;
            icon.ContextMenu = menu;

            icon.Click += icon_Click;

            Application.ApplicationExit += (s, e) => icon.Dispose();
            Application.Run();
        }

        static void icon_Click(object sender, EventArgs e)
        {
            if (UploadWindow == null)
            {
                UploadWindow = new UploadWindow();
                UploadWindow.Show();
                return;
            }
            if (UploadWindow.Visibility == System.Windows.Visibility.Visible)
                UploadWindow.Visibility = System.Windows.Visibility.Hidden;
            else
                UploadWindow.Visibility = System.Windows.Visibility.Visible;
        }

        static void settings_Click(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}