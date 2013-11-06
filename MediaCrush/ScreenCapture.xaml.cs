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
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using SystemInformation = System.Windows.Forms.SystemInformation;
using Point = System.Windows.Point;

namespace MediaCrush
{
    /// <summary>
    /// Interaction logic for ScreenCapture.xaml
    /// </summary>
    public partial class ScreenCapture : Window
    {
        private bool CaptureStarted = false;
        public Rect Selection { get { return foregroundRectangle.Rect; } }
        private Point SelectionStart;

        public ScreenCapture(Bitmap source)
        {
            InitializeComponent();
            Left = SystemInformation.VirtualScreen.Left;
            Top = SystemInformation.VirtualScreen.Top;
            Width = SystemInformation.VirtualScreen.Width;
            Height = SystemInformation.VirtualScreen.Height;
            backgroundRectangle.Rect = new Rect(0, 0, Width, Height);
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
                double x = SelectionStart.X;
                double y = SelectionStart.Y;
                double width = position.X - SelectionStart.X;
                double height = position.Y - SelectionStart.Y;
                if (width < 0) { x += width; width = Math.Abs(width); }
                if (height < 0) { y += height; height = Math.Abs(height); }
                foregroundRectangle.Rect = new Rect(x, y, width, height);
            }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (CaptureStarted)
                return;
            CaptureStarted = true;
            var position = e.GetPosition(this);
            SelectionStart = new Point(position.X, position.Y);
            Window_MouseMove(sender, e);
        }

        private void Window_MouseUp(object sender, MouseButtonEventArgs e)
        {
            DialogResult = true;
            this.Close();
        }
    }
}
