using MarkdownSharp;
using Octokit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection;
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
    /// Interaction logic for UpdateDetails.xaml
    /// </summary>
    public partial class UpdateDetails : Window
    {
        public UpdateDetails(Version version)
        {
            InitializeComponent();
            Task.Factory.StartNew(async () =>
            {
                var client = new GitHubClient(new ProductHeaderValue("MediaCrush-Windows"));
                var releases = await client.Release.GetAll("MediaCrush", "MediaCrush-Windows");
                var release = releases.SingleOrDefault(r => r.TagName == version.ToString());
                if (release == null)
                {
                    Dispatcher.Invoke(() => updateInfo.NavigateToString("<p>Something went wrong. This version does not exist.</p>"));
                    return;
                }
                else
                {
                    string template;
                    using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("MediaCrush.ChangelogTemplate.txt"))
                        template = new StreamReader(stream).ReadToEnd();
                    var markdown = new Markdown();
                    var markup = markdown.Transform(string.Format(template, release.Body));
                    Dispatcher.Invoke(() => updateInfo.NavigateToString(markup));
                }
            });
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            this.Close();
        }

        private void Apply_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            MessageBox.Show("The update will be downloaded. You'll be notified when it's ready to install.");
            this.Close();
        }
    }
}
