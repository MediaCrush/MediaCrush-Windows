using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;

namespace MediaCrush
{
    /// <summary>
    /// Interaction logic for ScreenCapture.xaml
    /// </summary>
    public partial class ScreenCapture : Window
    {
        private bool CaptureStarted = false;
        public Rect Selection { get { return foregroundRectangle.Rect; } }

        public ScreenCapture(Bitmap source)
        {
            InitializeComponent();
            Left = SystemInformation.VirtualScreen.Left;
            Top = SystemInformation.VirtualScreen.Top;
            Width = SystemInformation.VirtualScreen.Width;
            Height = SystemInformation.VirtualScreen.Height;
            backgroundRectangle.Rect = new Rect(0, 0, Width, Height);
            foregroundRectangle.Rect = new Rect(0, 0, 1, 1); // Try to get it to render, speeds things up
            Background = new ImageBrush(Imaging.CreateBitmapSourceFromHBitmap(source.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions()));
            KeyDown += ScreenCapture_KeyDown;
        }

        void ScreenCapture_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                DialogResult = false;
                this.Close();
            }
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            var position = e.GetPosition(this);
            if (CaptureStarted)
            {
                foregroundRectangle.Rect = new Rect(foregroundRectangle.Rect.X, foregroundRectangle.Rect.Y,
                    position.X - foregroundRectangle.Rect.X, position.Y - foregroundRectangle.Rect.Y);
            }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            CaptureStarted = true;
            var position = e.GetPosition(this);
            foregroundRectangle.Rect = new Rect(position.X, position.Y, 0, 0);
        }

        private void Window_MouseUp(object sender, MouseButtonEventArgs e)
        {
            DialogResult = true;
            this.Close();
        }
    }
}
