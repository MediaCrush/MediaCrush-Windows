using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
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
