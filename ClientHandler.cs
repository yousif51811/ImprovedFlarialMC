using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text.Json;
using System.Windows;
using Path = System.IO.Path;

namespace Flarial
{
    /// <summary>
    /// Responsible for handling the ACTUAL flarial client
    /// such as downloading the DLL and Launcher if they don't exist
    /// </summary>
    static class ClientHandler
    {
        private const string LauncherURL = "https://cdn.flarial.xyz/launcher/Flarial.Launcher.exe";
        public const string LauncherPath = "./Assets/DLL/Launcher.exe";
        private const string LauncherVersion = "https://cdn.flarial.xyz/launcher/launcherVersion.txt";
        private const string DLLURL = "https://cdn.flarial.xyz/dll/latest.dll";
        public const string DLLPath = "./Assets/DLL/Flarial.dll";
        private const string DLLHASHES = "https://cdn.flarial.xyz/dll_hashes.json";


        /// <summary>
        /// Responsible for downloading the Launcher
        /// </summary>
        private static async Task<bool> DownloadLauncher()
        {
            // Create an HTTPclient (we will later dispose of it)
            HttpClient client = new HttpClient();
            try
            {
                // Delete existing launcher if it exists
                if (File.Exists(LauncherPath))
                {
                    File.Delete(LauncherPath);
                }
                // Ensure directory exists
                string directory = System.IO.Path.GetDirectoryName(LauncherPath)!;
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                var response = await client.GetAsync(LauncherURL);
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
            catch
            {
                return false;
            }
            finally
            {
                client.Dispose();
            }
        }
            



        /// <summary>
        /// Responsible for downloading the DLL
        /// </summary>
        private static async Task<bool> DownloadDLL()
        {
            // Create an HTTPclient (we will later dispose of it)
            HttpClient client = new HttpClient();
            try
            {
                // Delete existing DLL if it exists
                if (File.Exists(DLLPath))
                {
                    File.Delete(DLLPath);
                }
                // Ensure directory exists
                string directory = System.IO.Path.GetDirectoryName(DLLPath)!;
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                var response = await client.GetAsync(DLLURL);
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
            catch {  return false; }
            finally
            {
                client.Dispose();
            }
        }


        /// <summary>
        /// Checks for Updates if needed and downloads them
        /// </summary>
        public static async Task<bool> CheckForUpdates()
        {
            // Create an HTTPclient (we will later dispose of it)
            HttpClient client = new HttpClient();
            /*
             * Start by getting the launcher version from the flarial
             * CDN and checking if it matches the local version.
             * if not, Update the launcher.
             */
            try
            {
                if (Properties.Settings.Default.CustomLauncher)
                {
                    if (!File.Exists(Properties.Settings.Default.LauncherDir))
                    {
                        return false;
                    }
                    return true;
                }
                    
                if (!File.Exists(LauncherPath))
                {
                    bool success = await DownloadLauncher();
                    return success;
                    
                }
                // The file exists, Now we check the version of the local launcher against the version on the CDN
                // In order to make sure its on the latest version, if not, we download the new version.
                string json = await client.GetStringAsync(LauncherVersion);
                using JsonDocument doc = JsonDocument.Parse(json);
                string? version = doc.RootElement.GetProperty("version").GetString();

                var info = FileVersionInfo.GetVersionInfo(LauncherPath);
                if (info.FileVersion != version)
                {
                    bool success = await DownloadLauncher();
                    return success;
                }
            }
            catch { return false; }

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
                    return success;
                }
                string json = await client.GetStringAsync(DLLHASHES);
                using JsonDocument doc = JsonDocument.Parse(json);
                string? hash = doc.RootElement.GetProperty("Release").GetString();

                if (await GetLocalHashAsync() != hash)
                {
                    bool success = await DownloadDLL();
                    return success;
                }
            }
            catch { return false; }
            finally
            {
                client.Dispose();
            }
            return false;
        }


        /// <summary>
        /// Get the local hash of the DLL, used for comparing 
        /// with the remote hash to check for updates
        /// </summary>
        static readonly HashAlgorithm _algorithm = SHA256.Create();
        static readonly object _lock = new();
        static async Task<string> GetLocalHashAsync() => await Task.Run(() =>
        {
            try
            {
                lock (_lock)
                {
                    using var stream = File.OpenRead(DLLPath);
                    var value = _algorithm.ComputeHash(stream);
                    var @string = BitConverter.ToString(value);
                    return @string.Replace("-", string.Empty);
                }
            }
            catch { return string.Empty; }
        });



        /// <summary>
        /// Interact with the launcher to start the game with the dll injected,
        /// this is done by starting the launcher with the appropriate arguments.
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
            catch { }
            {
                return false;
            }
        }


        /// <summary>
        /// Manages the current folder in Windows Defender exclusions.
        /// Requires for administrator permissions.
        /// </summary>
        /// <param name="isAdding">True to add exclusion, False to remove it.</param>
        static void ManageFolderExclusion(bool isAdding)
        {
            string currentDir = AppDomain.CurrentDomain.BaseDirectory;

            // Toggle between Add and Remove
            string action = isAdding ? "Add" : "Remove";
            string psCommand = $"{action}-MpPreference -ExclusionPath '{currentDir}'";

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-NoProfile -WindowStyle Hidden -Command \"{psCommand}\"",
                Verb = "runas",
                UseShellExecute = true
            };

            try
            {
                Console.WriteLine($"{action}ing exclusion for: {currentDir}");
                Process.Start(startInfo);
                Console.WriteLine($"Success: {action} command sent to PowerShell.");
            }
            catch (Win32Exception)
            {
                Console.WriteLine("Error: User declined the administrator prompt.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An unexpected error occurred: {ex.Message}");
            }
        }


        private static void RequestExclude()
        {
            MessageBoxResult result = MessageBox.Show("Error!",
                "Microsoft Windows defender likely deleted the Flarial DLL\nAdd current folder to defender exclusions?",
                MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                ManageFolderExclusion(true);
            }
        } 
    }
}
