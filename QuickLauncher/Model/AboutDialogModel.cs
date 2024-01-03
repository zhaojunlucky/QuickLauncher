using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using Utility.Model;
using Windows.Web.Http;

namespace QuickLauncher.Model
{
    class AboutDialogModel : AbstractNotifyPropertyChanged, IDataErrorInfo
    {
        private string version;
        private string fullVersion;
        private string copyright;
        private bool checkingUpdates;
        private string newVersionResult;
        private Visibility newVersionUiVisibility;
        private Visibility progressRingVisibility;
        private bool allowNavigateToNewVer;
        private string allowNavigateToolTip;

        public AboutDialogModel()
        {
            NewVersionUiVisibility = Visibility.Collapsed;
            newVersionResult = "(QuickLauncher is up to date)";

            var rAssembly = Assembly.GetEntryAssembly();
            Version = ParseVersion(rAssembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion);

#if DEBUG
            FullVersion = $"Version {Version} Debug Build";
#else
            FullVersion = $"Version {Version}";
#endif

            var rCopyrightAttribute = (AssemblyCopyrightAttribute)rAssembly.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), true)[0];
            Copyright = rCopyrightAttribute.Copyright;
            AllowNavigateToolTip = "";
        }

        private string ParseVersion(string informationalVersion)
        {
            //3.1.0-rc.48+Branch.master.Sha.09defdc8c281779943956efd1c96547115fa29a8
            Regex rg = new Regex(@"^(\d+\.\d+\.\d+)-rc\.(\d+).+$");
            MatchCollection matchedVers = rg.Matches(informationalVersion.Trim());
            if (matchedVers.Count > 0)
            {
                return $"{matchedVers[0].Groups[1].Value}.{matchedVers[0].Groups[2].Value}";
            }
            return informationalVersion;
        }

        public async Task CheckUpdates()
        {
            string result = "";
            bool allowNavigate = false;
            UpdateCheckingStatus(true, result, allowNavigate);

            try
            {
                LatestRelease = await CheckNewVersion();
                var newVer = ParseGHRelease(LatestRelease.Name);
                Trace.TraceInformation($"GitHub version {newVer}");
                if (string.Compare(newVer, version, StringComparison.CurrentCulture) > 0)
                {
                    result = $"New version {newVer}";
                    allowNavigate = true;
                }
                else
                {
                    result = "QuickLauncher is up to date";
                }
            }
            catch (Exception e)
            {
                Trace.TraceError(e.Message);
                result = "Network Error";
            }
            UpdateCheckingStatus(false, result, allowNavigate);
        }

        private string ParseGHRelease(string name)
        {
            Regex rg = new Regex(@"^.*(\d+\.\d+\.\d+\.\d+).*$");
            MatchCollection matchedVers = rg.Matches(name.Trim());
            if (matchedVers.Count > 0)
            {
                return matchedVers[0].Groups[1].Value;
            }
            return name;
        }

        private void UpdateCheckingStatus(bool start, string result, bool allowNavigate)
        {
            if (start)
            {
                CheckingUpdates = true;
                ProgressRingVisibility = Visibility.Visible;
                NewVersionUiVisibility = Visibility.Collapsed;
            }
            else
            {
                CheckingUpdates = false;
                NewVersionResult = $"({result})";
                NewVersionUiVisibility = Visibility.Visible;
                ProgressRingVisibility = Visibility.Collapsed;
            }
            AllowNavigateToNewVer = allowNavigate;
            if (allowNavigate)
            {
                AllowNavigateToolTip = "Click to download the newer version";
            }
        }

        public async Task<GithubRelease> CheckNewVersion()
        {
            var json = await DownloadGitHubReleaseAsync();
            var serializer = new DataContractJsonSerializer(typeof(List<GithubRelease>));
            byte[] byteArray = Encoding.UTF8.GetBytes(json);
            MemoryStream stream = new MemoryStream(byteArray);
            var releases = (List<GithubRelease>)serializer.ReadObject(stream);
            var latest = releases.Where(r => !r.Draft && !r.Prerelease).ToList()[0];
            return latest;
        }

        private async Task<string> DownloadGitHubReleaseAsync()
        {
            var url = "https://api.github.com/repos/zhaojunlucky/quicklauncher/releases";
            int retry = 5;

            while (true)
            {
                try
                {
                    using var client = new HttpClient();
                    client.DefaultRequestHeaders.Add("user-agent", $"QuickLauncher {version}");
                    return await client.GetStringAsync(new Uri(url));
                }
                catch (Exception)
                {
                    if (retry-- <= 0)
                    {
                        throw;
                    }
                    else
                    {
                        await Task.Delay(TimeSpan.FromSeconds(1)).ConfigureAwait(false);
                    }
                }
            }
        }

        public string this[string columnName] => null;

        public string Error => string.Empty;

        public string Version
        {
            get => version;
            set
            {
                version = value;
                RaisePropertyChanged("Version");
            }
        }

        public string FullVersion
        {
            get => fullVersion;
            set
            {
                fullVersion = value;
                RaisePropertyChanged("FullVersion");
            }
        }

        public string Copyright
        {
            get => copyright;
            set
            {
                copyright = value;
                RaisePropertyChanged("Copyright");
            }
        }

        public bool CheckingUpdates
        {
            get => checkingUpdates;
            set
            {
                checkingUpdates = value;
                RaisePropertyChanged("CheckingUpdates");
            }
        }

        public string NewVersionResult
        {
            get => newVersionResult;
            set
            {
                newVersionResult = value;
                RaisePropertyChanged("NewVersionResult");
            }
        }

        public string AllowNavigateToolTip
        {
            get => allowNavigateToolTip;
            set
            {
                allowNavigateToolTip = value;
                RaisePropertyChanged("AllowNavigateToolTip");
            }
        }

        public Visibility ProgressRingVisibility
        {
            get => progressRingVisibility;
            set
            {
                progressRingVisibility = value;
                RaisePropertyChanged("ProgressRingVisibility");
            }
        }

        public Visibility NewVersionUiVisibility
        {
            get => newVersionUiVisibility;
            set
            {
                newVersionUiVisibility = value;
                RaisePropertyChanged("NewVersionUIVisibility");
            }
        }

        public bool AllowNavigateToNewVer
        {
            get => allowNavigateToNewVer;
            set
            {
                allowNavigateToNewVer = value;
                RaisePropertyChanged("AllowNavigateToNewVer");
            }
        }

        public GithubRelease LatestRelease { get; set; }
    }


    [DataContract]
    class GithubRelease
    {
        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "draft")]
        public bool Draft { get; set; }

        [DataMember(Name = "prerelease")]
        public bool Prerelease { get; set; }

        [DataMember(Name = "created_at")]
        public string CreatedAt { get; set; }

        [DataMember(Name = "published_at")]
        public string PublishedAt { get; set; }

        [DataMember(Name = "html_url")]
        public string HtmlUrl { get; set; }
    }
}
