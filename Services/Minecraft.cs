using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Flarial.Services
{
    public static class Minecraft
    {


        static Minecraft()
        {
            if (IsInstalled())
            {
                Logging.Log($"Minecraft Is installed at: {GetInstallationPath()}", "Debug");
            }
            else
            {
                Logging.Log("Minecraft Is Not installed", "Debug");
            }
        }
        /// <summary>
        /// Check if minecraft is installed.
        /// </summary>
        /// <returns>(Bool) Wether the game is installed or not. </returns>
        public static bool IsInstalled()
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = "Get-AppxPackage \"Microsoft.MinecraftUWP\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            var response = Process.Start(startInfo);
            if (response != null)
            {
                string output = response.StandardOutput.ReadToEnd();
                response.WaitForExit();
                return !string.IsNullOrEmpty(output);
            }
            return false;
        }

        /// <summary>
        /// Get the installation path of minecraft.
        /// </summary>
        /// <returns></returns>
        public static string GetInstallationPath()
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = "(Get-AppxPackage \"Microsoft.MinecraftUWP\").InstallLocation",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            var response = Process.Start(startInfo);
            if (response != null)
            {
                string output = response.StandardOutput.ReadToEnd().Trim();
                response.WaitForExit();
                return output;
            }
            return string.Empty;
        }
    }
}
