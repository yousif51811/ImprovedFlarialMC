using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
namespace Flarial.Services
{
    public static class Machine
    {
        private static readonly HashAlgorithm _algorithm = SHA256.Create();
        private static readonly object _lock = new();
        /// <summary>
        /// Get the local hash of a file
        /// </summary>
        /// <param name="Path"> Full path of the requested file. </param>
        /// <returns>(String) The hash of the file requested (Empty if failed)</returns>
        public static async Task<string> GetFileHashAsync(string Path) => await Task.Run(() =>
        {
            try
            {
                lock (_lock)
                {
                    using var stream = File.OpenRead(Path);
                    var value = _algorithm.ComputeHash(stream);
                    var @string = BitConverter.ToString(value);
                    return @string.Replace("-", string.Empty);
                }
            }
            catch { Logging.Log($"Couldn't retrieve hash of {Path}", "ERROR"); return string.Empty; }
        });



        /// <summary>
        /// Manages the current folder in Windows Defender exclusions.
        /// Requires for administrator permissions.
        /// </summary>
        /// <param name="isAdding"> True to add exclusion, False to remove it. </param>
        /// <param name="Path"> Path of the folder to exclude </param>
        public static void ManageFolderExclusion(string Path, bool isAdding)
        {
            /*
             * Use powershell command (Add/Remove)-MpPreference
             * To add or remove an exclusion (which will be the running path of the app)
             */

            // Toggle between Add and Remove
            string action = isAdding ? "Add" : "Remove";

            // Create the command
            string psCommand = $"{action}-MpPreference -ExclusionPath '{Path}'";

            // Log the process
            Logging.Log($"Excluding with command: {psCommand}", "DEBUG");
            
            // Create the process
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-NoProfile -WindowStyle Hidden -Command \"{psCommand}\"", // Hidden window
                Verb = "runas", // Run as administrator
                UseShellExecute = true
            };

                Process.Start(startInfo);
        }

        /// <summary>
        /// Check if provided path is excluded in windows defender.
        /// </summary>
        /// <param name="path"></param>
        /// <returns>(Bool) Wether the path is excluded or not</returns>
        public static bool IsPathExcluded(string path)
        {
            string[] registryKeys = 
            {
                @"SOFTWARE\Microslop\Windows Defender\Exclusions\Paths",
                @"SOFTWARE\Policies\Microslop\Windows Defender\Exclusions\Paths"
            };

            foreach (var keyPath in registryKeys)
            {
                using (RegistryKey? key = Registry.LocalMachine.OpenSubKey(keyPath))
                {
                    if (key != null)
                    {
                        // Defender stores paths as Value Names with a value of 0
                        if (key.GetValueNames().Any(name => name.Equals(path, StringComparison.OrdinalIgnoreCase)))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
    }
}
