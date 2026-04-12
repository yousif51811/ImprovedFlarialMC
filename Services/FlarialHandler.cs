using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using Path = System.IO.Path;

namespace Flarial.Services
{
    /// <summary>
    /// Responsible for handling the ACTUAL flarial client
    /// </summary>
    static class FlarialHandler
    {
        private const string LauncherURL = "https://cdn.flarial.xyz/launcher/Flarial.Launcher.exe";
        public const string LauncherPath = "./Assets/Launcher.exe";
        private const string LauncherVersion = "https://cdn.flarial.xyz/launcher/launcherVersion.txt";

        private const string DLLURL = "https://cdn.flarial.xyz/dll/latest.dll";
        public const string DLLPath = "./Assets/Flarial.dll";
        private const string DLLHASHES = "https://cdn.flarial.xyz/dll_hashes.json";





        #region Updating
        /// <summary>
        /// Responsible for downloading the Launcher
        /// </summary>
        private static async Task<bool> DownloadLauncher()
        {
            try
            {
                // Delete existing launcher if it exists
                if (File.Exists(LauncherPath))
                {
                    File.Delete(LauncherPath);
                }
                // Ensure directory exists
                string directory = System.IO.Path.GetDirectoryName(LauncherPath);
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                var response = await Container.Client.GetAsync(LauncherURL);
                response.EnsureSuccessStatusCode();

                using (var fs = new FileStream(LauncherPath, FileMode.Create))
                {
                    await response.Content.CopyToAsync(fs);
                }
                // Check again if it exists, if not ask for exclusion
                if (!File.Exists(LauncherPath))
                {
                    RequestExclude();
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                Logging.Log($"Failed to download launcher: {ex}", "ERROR");
                return false;
            }
        }











        /// <summary>
        /// Responsible for downloading the DLL
        /// </summary>
        private static async Task<bool> DownloadDLL()
        {
            try
            {
                // Delete existing DLL if it exists
                if (File.Exists(DLLPath))
                {
                    Logging.Log("Existing DLL found, deleting...", "DEBUG");
                    File.Delete(DLLPath);
                }
                // Ensure directory exists
                string directory = Path.GetDirectoryName(DLLPath);
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                var response = await Container.Client.GetAsync(DLLURL);
                response.EnsureSuccessStatusCode();

                using (var fs = new FileStream(DLLPath, FileMode.Create))
                {
                    await response.Content.CopyToAsync(fs);
                }
                // Check again if it exists, if not ask for exclusion
                if (!File.Exists(DLLPath))
                {
                    RequestExclude();
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                Logging.Log($"Failed to download DLL: {ex}", "ERROR");
                return false;
            }
        }









        /// <summary>
        /// Checks for Updates if needed and downloads them
        /// </summary>
        public static async Task<bool> CheckForUpdates()
        {
            /*
            * This part consists of 2 regions:
            * - Launcher Check
            * - DLL check
            */

            bool launcherSuccess = false;
            bool DllSuccess = false;

            #region Launcher Check
            try
            {
                /*
                    * Get the launcher version from the flarial CDN and compare
                    * wether it needs updating or not.
                    */

                // If using a custom launcher, check if it exists.
                if (Properties.Settings.Default.CustomLauncher)
                {
                    if (!File.Exists(Properties.Settings.Default.LauncherDir))
                    {
                        return false;
                    }
                    return true;
                }

                // If the launcher doesn't exist, go ahead and download it
                if (!File.Exists(LauncherPath))
                {
                    Logging.Log("Launcher not found, downloading...", "DEBUG");
                    bool success = await DownloadLauncher();
                    launcherSuccess = success;
                }
                // The launcher exists, Now we check the version of the local launcher against the version on the CDN
                // In order to make sure its on the latest version, if not, we download the new version.
                Logging.Log("Launcher exists", "DEBUG");
                string json = await Container.Client.GetStringAsync(LauncherVersion);
                string version;
                using (JsonDocument doc = JsonDocument.Parse(json))
                {
                    version = doc.RootElement.GetProperty("version").GetString();
                }


                var info = FileVersionInfo.GetVersionInfo(LauncherPath);
                if (info.FileVersion != version)
                {
                    bool success = await DownloadLauncher();
                    launcherSuccess = success;
                }
            }
            catch (Exception ex)
            {
                Logging.Log($"Failed to check for launcher updates: {ex}", "ERROR");
                return false;
            }

            #endregion




            #region DLL Check
            /*
                * The dll doesnt have a version, so we get the hash of the 
                * local dll and compare it to the hash of the remote dll, 
                * if they dont match, we download the new dll.
                */
            try
            {
                if (Properties.Settings.Default.CustomDLL)
                {
                    if (!File.Exists(Properties.Settings.Default.DLLDir))
                    {
                        return false;
                    }
                    return true;
                }

                if (!File.Exists(DLLPath))
                {
                    bool success = await DownloadDLL();
                    DllSuccess = success;
                }
                string json = await Container.Client.GetStringAsync(DLLHASHES);

                string hash;
                using (JsonDocument doc = JsonDocument.Parse(json))
                {
                    hash = doc.RootElement.GetProperty("Release").GetString();
                }


                if (await Machine.GetFileHashAsync(DLLPath) != hash)
                {
                    Logging.Log("DLL hash mismatch, downloading new version...", "DEBUG");
                    bool success = await DownloadDLL();
                    Logging.Log($"DLL Downloaded Success: {success}", "DEBUG");
                    DllSuccess = success;
                }
            }
            catch (Exception ex)
            {
                Logging.Log($"Failed to check for DLL updates: {ex}", "ERROR");
                return false;
            }
            #endregion

            // Finally return whether both updates were successful or not
            return launcherSuccess && DllSuccess;
        }
        #endregion






        /// <summary>
        /// Interact with the launcher to start the game with the dll injected,
        /// this is done by starting the launcher with the appropriate arguments (--inject [DLLPath]).
        /// </summary>
        public static async Task<bool> StartGame()
        {
            /* 
             * Use custom path if enabled, otherwise use the default path.
             */
            string path = Path.GetFullPath(DLLPath);
            if (Properties.Settings.Default.CustomDLL)
            {
                if (!File.Exists(Properties.Settings.Default.DLLDir))
                {
                    return false;
                }
                else
                {
                    path = Path.GetFullPath(Properties.Settings.Default.DLLDir);
                }
            }

            try
            {
                // Run the launcher with the --inject [DLLPath] argument to start the game
                await Task.Run(() =>
                {
                    ProcessStartInfo startInfo = new ProcessStartInfo
                    {
                        FileName = LauncherPath,
                        Arguments = $"--inject \"{path}\"",
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };
                    Process.Start(startInfo);
                });
                return true;
            }
            catch (Exception ex)
            {
                Logging.Log($"Failed to start the game: {ex}", "ERROR");
                return false;
            }
        }






        /// <summary>
        /// Show a messagebox to ask to add to defender exclusions
        /// </summary>
        private static void RequestExclude()
        {
            MessageBoxResult result = MessageBox.Show("Error!",
                "Microsoft Windows defender likely deleted the Flarial DLL\nAdd current folder to defender exclusions?",
                MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                Machine.ManageFolderExclusion(AppDomain.CurrentDomain.BaseDirectory, true);
            }
        }
    }
}
