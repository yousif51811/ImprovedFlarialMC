using Flarial.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Policy;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
namespace Flarial.Services
{
    public static class VersioningHandler
    {
        private const string SupportedVersionsURI = "https://cdn.flarial.xyz/launcher/Supported.json";
        private const string VersionDownloadsURI = "https://cdn.jsdelivr.net/gh/MinecraftBedrockArchiver/GdkLinks@latest/urls.json";
        public static List<string> SupportedVersions;

        #region Retrieving Versions
        /// <summary>
        /// Get the list of versionitems that the launcher supports.
        /// </summary>
        /// <returns></returns>
        public static async Task<List<VersionItem>> GetVersions()
        {
            List<VersionItem> versionItems = new List<VersionItem>();

            var names = await GetSupportedVersions();
            var (mainUrls, secondaryUrls) = await GetVersionDownloads(names);

            for (int i = 0; i < names.Count; i++)
            {
                versionItems.Add(new VersionItem
                {
                    Version = names[i],
                    Name = FormatVersion(names[i]),
                    Url = mainUrls.Count > i ? mainUrls[i] : null,
                    AlternateUrl = secondaryUrls.Count > i ? secondaryUrls[i] : null
                });
            }

            return versionItems;
        }


        /// <summary>
        /// Format later minecraft versions properly (since 26.0 changed the versioning scheme)
        /// </summary>
        /// <param name="version"></param>
        /// <returns></returns>
        public static string FormatVersion(string version)
        {
            var parts = version.Split('.');

            int major = int.Parse(parts[0]);
            int minor = int.Parse(parts[1]);
            int patch = int.Parse(parts[2]);

            return minor >= 26
                ? $"{minor}.{patch}"
                : $"{major}.{minor}.{patch}";
        }


        private static async Task<List<string>> GetSupportedVersions()
        {
            List<string> supportedVersions = new List<string>();
            string json = await Container.Client.GetStringAsync(SupportedVersionsURI);

            using (JsonDocument doc = JsonDocument.Parse(json))
            {
                foreach (var property in doc.RootElement.EnumerateObject())
                {
                    if (property.Value.ValueKind == JsonValueKind.True)
                    {
                        supportedVersions.Add(property.Name);
                    }
                }
            }
            SupportedVersions = supportedVersions;
            return supportedVersions;
        }


        /// <summary>
        /// Get the urls for a specified list of versions from the json.
        /// </summary>
        /// <param name="names"> a list of the versions you'd like to get </param>
        /// <returns>2 lists containing alternative urls and main urls</returns>
        private static async Task<(List<string> mainUrls, List<string> secondaryUrls)> GetVersionDownloads(List<string> names)
        {
            // Initialize the 2 lists
            var mainUrls = new List<string>();
            var secondaryUrls = new List<string>();

            // Get the json file (The versions)
            string json = await Container.Client.GetStringAsync(VersionDownloadsURI);

            using (JsonDocument doc = JsonDocument.Parse(json))
            {
                // Get the json file actual element
                var urlsjson = doc.RootElement;

                // Filter out releases
                if (!urlsjson.TryGetProperty("release", out var release))
                    return (mainUrls, secondaryUrls);

                // Now use the names to find the properties that start with the version names, and get the urls from those properties
                foreach (var name in names)
                {
                    foreach (var property in release.EnumerateObject())
                    {
                        if (property.Name.StartsWith(name))
                        {
                            var Ver = property.Value;

                            if (Ver.ValueKind == JsonValueKind.Array)
                            {
                                var urls = Ver.EnumerateArray().ToList();
                                Logging.Log($"Version: {property.Name}, URLs found: {urls.Count}", "DEBUG");

                                if (urls.Count > 0)
                                    mainUrls.Add(urls[0].GetString());

                                if (urls.Count > 1)
                                    secondaryUrls.Add(urls[1].GetString());
                            }

                            break;
                        }
                    }
                }
            }
            return (mainUrls, secondaryUrls);
        }
        #endregion

    }
}
