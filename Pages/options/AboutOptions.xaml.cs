using Flarial.Services;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Policy;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Flarial.Pages.options
{
    /// <summary>
    /// Interaction logic for AboutOptions.xaml
    /// </summary>
    public partial class AboutOptions : UserControl
    {
        private const string GitHubURL = "https://github.com/yousif51811/FlarialReskin";
        public AboutOptions()
        {
            InitializeComponent();
            Versiontxt.Text = FileVersionInfo
                            .GetVersionInfo(System.Reflection.Assembly.GetExecutingAssembly().Location)
                            .FileVersion;
        }

        private void Social_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Open the GitHub URL in the default web browser
            Process.Start(new ProcessStartInfo(GitHubURL) { UseShellExecute = true });
            Logging.Log($"Visiting Github...", "INFO");
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = e.Uri.AbsoluteUri,
                UseShellExecute = true
            });
           Logging.Log($"Visiting {e.Uri.AbsoluteUri}...", "INFO");
        }
    }
}
