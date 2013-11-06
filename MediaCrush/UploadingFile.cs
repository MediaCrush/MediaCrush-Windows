using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaCrush
{
    public class UploadingFile : INotifyPropertyChanged
    {
        public UploadingFile(string file)
        {
            File = file;
            Status = FileStatus.Uploading;
            Progress = 0.5;
        }

        public string File { get; set; }
        public string FileName
        {
            get
            {
                return Path.GetFileName(File);
            }
        }
        private FileStatus _Status;
        public FileStatus Status
        {
            get { return _Status; }
            set
            {
                _Status = value;
                OnPropertyChanged("Status");
            }
        }
        private double _Progress;
        public double Progress
        {
            get { return _Progress; }
            set
            {
                _Progress = value;
                OnPropertyChanged("Progress");
            }
        }

        private string _Hash;
        public string Hash
        {
            get { return _Hash; }
            set
            {
                _Hash = value;
                OnPropertyChanged("Hash");
                OnPropertyChanged("Url");
            }
        }

        public string Url
        {
            get
            {
                return "https://mediacru.sh/" + Hash;
            }
        }

        public enum FileStatus
        {
            Uploading,
            Processing,
            Finished,
            Error
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected internal virtual void OnPropertyChanged(string property)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(property));
        }
    }
}
