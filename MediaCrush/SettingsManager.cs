using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MediaCrush
{
    public class SettingsManager : INotifyPropertyChanged
    {
        public SettingsManager()
        {
            SetToDefaults();
        }

        public static string SettingsPath
        {
            get { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ".mediacrush"); }
        }

        public static string SettingsFile
        {
            get { return Path.Combine(SettingsPath, "settings"); }
        }

        public static void Initialize()
        {
            if (!Directory.Exists(SettingsPath))
                Directory.CreateDirectory(SettingsPath);
        }

        public void SetToDefaults()
        {
            EnableScreenCapture = true;
            EnableTracking = false;
            CheckForUpdates = true;
            var sha1 = SHA1.Create();
            UserTrackingId = Convert.ToBase64String(sha1.ComputeHash(Guid.NewGuid().ToByteArray()));
        }

        protected bool _EnableScreenCapture;
        public bool EnableScreenCapture
        {
            get { return _EnableScreenCapture; }
            set
            {
                _EnableScreenCapture = value;
                OnPropertyChanged("EnableScreenCapture");
            }
        }

        protected bool _EnableTracking;
        public bool EnableTracking
        {
            get { return _EnableTracking; }
            set
            {
                _EnableTracking = value;
                OnPropertyChanged("EnableTracking");
            }
        }

        protected string _UserTrackingId;
        public string UserTrackingId
        {
            get { return _UserTrackingId; }
            set
            {
                _UserTrackingId = value;
                OnPropertyChanged("TrackingUserId");
            }
        }

        protected bool _CheckForUpdates;
        public bool CheckForUpdates
        {
            get { return _CheckForUpdates; }
            set
            {
                _CheckForUpdates = value;
                OnPropertyChanged("CheckForUpdates");
            }
        }

        public void ForcePropertyUpdate()
        {
            // Calls OnPropertyChanged for all properties
            var properties = GetType().GetProperties();
            foreach (var property in properties)
                OnPropertyChanged(property.Name);
        }

        protected internal virtual void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(name));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
