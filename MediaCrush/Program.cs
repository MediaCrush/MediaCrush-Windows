using MediaCrush;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Threading;

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
            _hookID = SetHook(_proc);

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
            UnhookWindowsHookEx(_hookID);
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

        private static bool ControlPressed = false;
        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam) 
        {
            if (nCode >= 0)
            {
                Keys number = (Keys)Marshal.ReadInt32(lParam);
                if (wParam.ToInt32() == WM_KEYDOWN)
                {
                    if (number == Keys.LControlKey || number == Keys.RControlKey || number == Keys.Control || number == Keys.ControlKey)
                        ControlPressed = true;
                }
                else if (wParam.ToInt32() == WM_KEYUP)
                {
                    if (number == Keys.LControlKey || number == Keys.RControlKey || number == Keys.Control || number == Keys.ControlKey)
                        ControlPressed = false;
                    else if (number == Keys.PrintScreen && ControlPressed)
                    {
                        Task.Factory.StartNew(() => Dispatcher.CurrentDispatcher.Invoke(() => MessageBox.Show("Ctrl+PrintScreen pressed")));
                    }
                }
            }
            return CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);
        }

        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_KEYUP = 0x0101;
        private static LowLevelKeyboardProc _proc = HookCallback;
        private static IntPtr _hookID = IntPtr.Zero;
        private static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
            }
        }
        private delegate IntPtr LowLevelKeyboardProc( int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);  
    }
}