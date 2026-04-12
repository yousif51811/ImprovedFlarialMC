using System;
using System.Diagnostics;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Security.Policy;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Windows.Shell;
using Windows.Foundation;

using Windows.Management.Deployment;
using Package = Windows.ApplicationModel.Package;
namespace Flarial.Services
{
    public static class Minecraft
    {
        private const string GameLaunchHelperURL = "https://cdn.flarial.xyz/launcher/gamelaunchhelper.dll";
        private const string MinecraftName = "Microsoft.MinecraftUWP_8wekyb3d8bbwe";

        /// <summary>
        /// Check if minecraft is installed.
        /// </summary>
        /// <returns>(Bool) Wether the game is installed or not. </returns>
        public static bool IsInstalled => GetPackage() is { };
        private static readonly PackageManager _pm = new PackageManager();
        private static readonly Package Package;
        static Minecraft()
        {
            if (IsInstalled)
            {
                Logging.Log($"Minecraft Is installed at: {GetInstallationPath()}", "Debug");
            }
            else
            {
                Logging.Log("Minecraft Is Not installed", "Debug");
            }
        }

        private static Package GetPackage() => _pm.FindPackagesForUser(string.Empty, MinecraftName).FirstOrDefault();


        /// <summary>
        /// Get the installation path of minecraft.
        /// </summary>
        public static string GetInstallationPath() => GetPackage()?.InstalledPath ?? string.Empty;


        /// <summary>
        /// Awaitable method to wait for the installation of minecraft (Or any package really but who cares)
        /// </summary>
        /// <remarks>
        /// The progress is pretty messed up but it's not my fault, blame Microslop.
        /// </remarks>
        /// <param name="uriPath"> The path of the package to install </param>
        /// <param name="progress"> An optional IProgress to report installation progress (0-100) </param>
        /// <returns> (bool) Wether the installation was successful or not </returns>
        public static async Task<bool> InstallAsync(string uriPath)
        {
            try
            {
                Logging.Log($"Started installing package: {uriPath}", "INFO");

                var operation = _pm.AddPackageAsync(
                    new Uri(uriPath),
                    null,
                    DeploymentOptions.ForceApplicationShutdown |
                    DeploymentOptions.ForceUpdateFromAnyVersion
                );

                var result = await operation.AsTask();

                if (result?.ExtendedErrorCode != null && result.ExtendedErrorCode.HResult != 0)
                {
                    Logging.Log($"Installation error: {result.ErrorText}", "ERROR");
                    return false;
                }

                Logging.Log("Installation completed successfully.", "INFO");

                var helperResult = await AddGameLaunchHelper();
                Logging.Log($"Game launch helper success: {helperResult}", "INFO");

                return true;
            }
            catch (Exception ex)
            {
                Logging.Log($"Install failed: {ex}", "ERROR");
                return false;
            }
        }



        /// <summary>
        /// Uninstall minecraft. (Unused but keep it just in case, also it has a couple issues, uses a pretty bad confirmation pattern,
        /// But i don't use it so who even cares.)
        /// </summary>
        /// <returns></returns>
        public static Task<bool> UninstallAsync()
        {
            Logging.Log("Starting uninstallation of Minecraft.", "INFO");
            var Package = GetPackage();
            try
            {
                // Use TaskCompletionSource to convert the async operation to a Task
                var tcs = new TaskCompletionSource<bool>();

                _pm.RemovePackageAsync(Package.Id.FullName).Completed = (info, status) =>
                {
                    if (status == AsyncStatus.Completed)
                    {
                        tcs.SetResult(true);
                        Logging.Log("Uninstallation completed successfully.", "INFO");
                    }
                    else
                    {
                        tcs.SetResult(false);
                        Logging.Log($"Uninstallation failed: {info.GetResults().ErrorText}", "ERROR");
                    }
                };
                return tcs.Task;
            }
            catch (Exception ex)
            {
                Logging.Log($"Uninstall failed: {ex.Message}", "ERROR");
                return Task.FromResult(false);
            }

        }

        public static async Task<bool> AddGameLaunchHelper()
        {
            try
            {
                Logging.Log($"Downloading GameLaunchHelper from {GameLaunchHelperURL}.", "INFO");
                // Send request
                using var response = await Container.Client.GetAsync(GameLaunchHelperURL);
                response.EnsureSuccessStatusCode(); 

                // Read content as stream 
                using var stream = await response.Content.ReadAsStreamAsync();

                // Use the installation path of Minecraft to determine where to place the GameLaunchHelper
                string path = Path.Combine(Path.GetTempPath(), "GameLaunchHelper.dll");

                // Ensure directory exists
                Directory.CreateDirectory(Path.GetDirectoryName(path)!);

                // Write to file
                using var fileStream = File.Create(path);
                await stream.CopyToAsync(fileStream);
                Logging.Log("GameLaunchHelper downloaded successfully.", "INFO");

                // Use cmd to copy the file with admin (since minecraft's installation directory is protected)
                var copyProcess = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c copy /Y \"{path}\" \"{GetInstallationPath()}\"",
                    Verb = "runas",
                    UseShellExecute = true
                };

                Process process = Process.Start(copyProcess);
                return true;
            }
            catch (Exception ex)
            {
                Logging.Log($"Failed to install GameLaunchHelper: {ex}", "ERROR");
                return false;
            }
        }
    }
}
