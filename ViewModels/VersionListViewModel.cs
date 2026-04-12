using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Flarial.Models;
using Flarial.Services;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;

namespace Flarial.ViewModels
{
    public partial class VersionListViewModel : ObservableObject
    {
        // An observable collection of VersionItem objects, which will be used to display a list of versions in the UI.
        public ObservableCollection<VersionItem> Versions { get; set; } = new ObservableCollection<VersionItem>();
        public VersionListViewModel()
        {
            _ = LoadVersionsAsync();
        }
        private async Task LoadVersionsAsync()
        {
            var versions = await VersioningHandler.GetVersions();
            Versions.Clear();
            foreach (var version in versions)
            {
                Versions.Add(version);
            }
        }
        // Current selected item
        [ObservableProperty]
        private VersionItem selectedVersion;



        #region State Properties
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsNotDownloading))]
        [NotifyPropertyChangedFor(nameof(IsBusy))]
        [NotifyPropertyChangedFor(nameof(ProgressVisibility))]
        [NotifyPropertyChangedFor(nameof(Status))]
        [NotifyPropertyChangedFor(nameof(DownloadObjectVisible))]
        private bool isDownloading = false;
        public bool IsNotDownloading => !IsDownloading;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(DownloadObjectVisible))]
        [NotifyPropertyChangedFor(nameof(IsBusy))]
        [NotifyPropertyChangedFor(nameof(ProgressVisibility))]
        [NotifyPropertyChangedFor(nameof(Status))]
        private bool isInstalling = false;

        // Either installation or download are in progress, use this to show the progress bar and hide the download button.
        public bool IsBusy => IsInstalling || IsDownloading;

        public string Status => IsDownloading ? "Downloading..." : IsInstalling ? "Installing..." : "Idle";
        #endregion
        public Visibility DownloadObjectVisible => IsDownloading ? Visibility.Visible : Visibility.Collapsed;


        public Visibility ProgressVisibility => IsBusy ? Visibility.Visible : Visibility.Collapsed;
        


        [ObservableProperty]
        private double downloadSize;

        [ObservableProperty]
        private double downloadedSize;
        [ObservableProperty]
        private int progress = 0;

        
        /// <summary>
        /// Starts the download and installation process for the selected version.
        /// </summary>
        /// <remarks>
        /// The code is pretty messy, tried my best to make it readable. (A method shouldn't be this huge but whatever)
        /// </remarks>
        [RelayCommand]
        private async Task DownloadAsync(VersionItem version)
        {
            #region Checks
            if (version == null)
                return;
            string path = Path.Combine("Versions", $"{version.Name}.msixvc");
            try
            {
                // Make sure the game isn't already installed before attempting to download and install a new version.
                // Show a confirmation dialog to the user before starting the download.
                bool confirm = await UIContainer.Modal.ShowConfirmationAsync(
                    "Download Confirmation",
                    $"Are you sure you want to install {version?.Name ?? SelectedVersion?.Name}?"
                );
                if (!confirm)
                    return;
#endregion
                // Set the IsNotDownloading property to false to indicate that a download is in progress, and reset the DownloadProgress to 0.
                Progress = 0;
                IsDownloading = true;

                // Ensure the directory exists.
                if (!Directory.Exists("Versions")) { Directory.CreateDirectory("Versions"); }

                Logging.Log($"Starting download for {version.Name} from {version.Url}", "INFO");

                
                
                #region Download
                
                // Use the global client to get the headers first.
                var response = await Container.Client.GetAsync(version.Url, HttpCompletionOption.ResponseHeadersRead);


                // Ensure the request was successful before proceeding.
                response.EnsureSuccessStatusCode();

                // Retrieve the total content length
                var totalBytes = response.Content.Headers.ContentLength;

                // If the total content length is available, convert it to GB and update the DownloadSize property.
                if (totalBytes.HasValue)
                {
                    double totalGB = ConvertValue.ToGB(totalBytes.Value);
                    DownloadSize = totalGB;
                    Logging.Log($"Total size: {totalGB:F2} GB", "INFO");
                }

                // Check if the file exists, also compare file sizes (to make sure its not corrupted)
                // It's a pretty bad way to check file integrity but it should be good enough.
                bool skipDownload = false;
                if (File.Exists(path) && new FileInfo(path).Length == totalBytes)
                {
                    Logging.Log($"File already exists for {version.Name}, skipping download.", "INFO");
                    Progress = 100;
                    skipDownload = true;
                }


                if (!skipDownload)
                {
                    // Open the stream aka start the download and write to file.
                    using var input = await response.Content.ReadAsStreamAsync();
                    using var output = File.Create(path);

                    // Create a buffer which will read the incoming data in chunks (for progress reporting)
                    byte[] buffer = new byte[8192];
                    long downloadedBytes = 0;
                    int bytesRead;

                    // Start a loop that continues until there are no more bytes to read from the input stream.
                    while ((bytesRead = await input.ReadAsync(buffer, 0, buffer.Length)) > 0)
                    {
                        await output.WriteAsync(buffer, 0, bytesRead);
                        downloadedBytes += bytesRead;

                        if (totalBytes.HasValue)
                        {
                            Progress = (int)((double)downloadedBytes / totalBytes.Value * 100);
                            DownloadedSize = ConvertValue.ToGB(downloadedBytes);
                        }

                    }
                    Progress = 100;
                    IsDownloading = false;
                }
                #endregion

                #region Installation
                // Download finished, now we can proceed to install the package, I removed progress tracking for installation.
                // It's pretty much useless and is worse than just using indeterminate progress.
                Progress = 0;
                IsInstalling = true;
                var fullPath = Path.GetFullPath(path);
                bool success = await Minecraft.InstallAsync(fullPath);
                if (!success) {throw new Exception("Installation failed!");}
                Logging.Log($"Installation successful for {version.Name}", "INFO");
                IsInstalling = false;
                UIContainer.Modal.ShowMessage("Installation Complete", $"{version.Name} has been successfully installed!");
                #endregion
            }
            catch (Exception ex)
            {
                Logging.Log($"Error: {ex.Message}", "ERROR");
                UIContainer.Modal.ShowMessage("Error","An error occurred during installation!\nCheck Logs for more information");
            }
            finally
            {
                IsDownloading = false;
                IsInstalling = false;
            }
        }
    }
}
