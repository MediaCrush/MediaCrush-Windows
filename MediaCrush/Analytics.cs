using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Web;

namespace MediaCrush
{
    // This is an OPT-IN feature of the MediaCrush application to help us learn more about how people use the application.
    public static class Analytics
    {
        public const string GATrackingID = "UA-42686851-2";
        public const int APIVersion = 1;

        public static void TrackFeatureUse(string feature)
        {
            if (!Program.SettingsManager.EnableTracking)
                return;
            Task.Factory.StartNew(() =>
            {
                var request = (HttpWebRequest)WebRequest.Create("http://www.google-analytics.com/collect");
                request.Method = "POST";
                var _ = new Func<object, string>(o => HttpUtility.UrlEncode(o.ToString()));
                using (var stream = new StreamWriter(request.GetRequestStream()))
                {
                    // Breakdown of what's being sent:
                    //  v: Google API version
                    //  tid: MediaCrush-Windows tracking ID (identifies us as using the MediaCrush app)
                    //  cid: Unique ID for the current user
                    //       This is a hashed GUID, nothing special
                    //  t: The type of action (appview is what we use for tracking feature use)
                    //  an: The name of the app ("MediaCrush-Windows")
                    //  av: The app version in use here
                    //  cd: The feature we're tracking (i.e. "Application startup")
                    //  aip: Set to 1 to instruct google to anonymize your IP address
                    stream.Write("v={0}&tid={1}&cid={2}&t=appview&an={3}&av={4}&cd={5}&aip=1",
                        _(APIVersion), _(GATrackingID), _(Program.SettingsManager.UserTrackingId),
                        _("MediaCrush-Windows"), _(Program.Version.ToString()), _(feature));
                }
                var response = request.GetResponse();
                response.Close(); // Discard response
            });
        }
    }
}
