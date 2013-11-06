using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Shapes;

namespace MediaCrush
{
    /// <summary>
    /// Interaction logic for UploadWindow.xaml
    /// </summary>
    public partial class UploadWindow : Window
    {
        public ObservableCollection<UploadingFile> FileList;

        public UploadWindow()
        {
            InitializeComponent();
            FileList = new ObservableCollection<UploadingFile>();
            Left = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Right - this.Width;
            Top = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Bottom - this.Height;
            AllowDrop = true;
            DragEnter += UploadWindow_DragEnter;
            Drop += UploadWindow_Drop;
        }

        void UploadWindow_Drop(object sender, DragEventArgs e)
        {
            var data = e.Data.GetData(DataFormats.FileDrop);
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (var file in files)
                UploadFile(file);
        }

        void UploadWindow_DragEnter(object sender, System.Windows.DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effects = System.Windows.DragDropEffects.Copy;
            else
                e.Effects = System.Windows.DragDropEffects.None;
        }

        private void DismissButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void UploadFile_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var dialog = new System.Windows.Forms.OpenFileDialog();
            dialog.Filter = "Media files|*.png;*.jpg;*.jpe;*.jpeg;*.gif;*.svg;*.mp4;*.ogg;*.oga;*.ogv;*.webm;*.mp3";
            dialog.Multiselect = true;
            var result = dialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                // Upload files
                foreach (var file in dialog.FileNames)
                    UploadFile(file);
            }
        }

        private void UploadFile(string file)
        {
            // Modify UI
            if (FileList.Count == 0)
            {
                rowCollapse1.Height = new GridLength(0);
                rowCollapse2.Height = new GridLength(0);
                rowExpand.Height = new GridLength(5, GridUnitType.Star);
                uploadingFilesContainer.Visibility = Visibility.Visible;
                uploadingFiles.ItemsSource = FileList;
            }
            FileList.Add(new UploadingFile(file));
        }
    }
}
