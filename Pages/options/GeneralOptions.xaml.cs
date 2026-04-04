using Flarial.Services;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Flarial.Pages.options
{
    /// <summary>
    /// Interaction logic for GeneralOptions.xaml
    /// </summary>
    public partial class GeneralOptions : UserControl
    {
        public GeneralOptions()
        {
            InitializeComponent();
            Loaded += (sender, e) => LoadSettings();
        }

        /// <summary>
        /// Load the settings as soon as the control is loaded.
        /// </summary>
        private void LoadSettings()
        {
            // Load the settings for the DLL selection
            if (Properties.Settings.Default.CustomDLL)
            {
                SelectDLLCombo.SelectedIndex = 1;
            }
            else
            {
                SelectDLLCombo.SelectedIndex = 0;
                CustomDLLPath.IsEnabled = false;
                Browse.IsEnabled = false;
                CustomDLLPath.Text = FlarialHandler.DLLPath;
            }
            // Load the settings for the Launcher selection
            if (Properties.Settings.Default.CustomLauncher)
            {
                SelectLauncherCombo.SelectedIndex = 1;
            }
            else
            {
                SelectLauncherCombo.SelectedIndex = 0;
                CustomLauncherPath.IsEnabled = false;
                BrowseLauncher.IsEnabled = false;
                CustomLauncherPath.Text = FlarialHandler.LauncherPath;
            }
        }

        private void SelectDLLCombo_Selected(object sender, SelectionChangedEventArgs e)
        {
            // Default DLL selected
            if (SelectDLLCombo.SelectedIndex == 0)
            {
                CustomDLLPath.IsEnabled = false;
                Browse.IsEnabled = false;
                CustomDLLPath.Text = FlarialHandler.DLLPath;
                Properties.Settings.Default.CustomDLL = false;
                Properties.Settings.Default.Save();
            }
            // Custom DLL selected
            else if (SelectDLLCombo.SelectedIndex == 1)
            {
                CustomDLLPath.IsEnabled = true;
                Browse.IsEnabled = true;
                CustomDLLPath.Text = Properties.Settings.Default.DLLDir;
                Properties.Settings.Default.CustomDLL = true;
                Properties.Settings.Default.Save();
            }
        }
        private void SelectLauncherCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Default Launcher selected
            if (SelectLauncherCombo.SelectedIndex == 0)
            {
                CustomLauncherPath.IsEnabled = false;
                BrowseLauncher.IsEnabled = false;
                CustomLauncherPath.Text = FlarialHandler.LauncherPath;
                Properties.Settings.Default.CustomLauncher = false;
                Properties.Settings.Default.Save();
            }
            // Custom Launcher selected
            else if (SelectLauncherCombo.SelectedIndex == 1)
            {
                CustomLauncherPath.IsEnabled = true;
                BrowseLauncher.IsEnabled = true;
                CustomLauncherPath.Text = Properties.Settings.Default.LauncherDir;
                Properties.Settings.Default.CustomLauncher = true;
                Properties.Settings.Default.Save();
            }
        }

        private void Browse_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                Filter = "DLL files (*.dll)|*.dll|All files (*.*)|*.*",
                Title = "Select Custom DLL"
            };
            bool? result = dialog.ShowDialog();
            if (result == true)
            {
                CustomDLLPath.Text = dialog.FileName;
                CustomDLLPath.Text = dialog.FileName;
                Properties.Settings.Default.DLLDir = dialog.FileName;
                Properties.Settings.Default.Save();
            }
        }
        private void BrowseLauncher_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                Filter = "Executable files (*.exe)|*.exe",
                Title = "Select Custom Launcher"
            };
            bool? result = dialog.ShowDialog();
            if (result == true)
            {
                CustomLauncherPath.Text = dialog.FileName;
                CustomLauncherPath.Text = dialog.FileName;
                Properties.Settings.Default.LauncherDir = dialog.FileName;
                Properties.Settings.Default.Save();
            }
        }

        /// <summary>
        /// Responsible for saving the custom path(s) with a debounce to prevent
        /// excessive writes to settings while the user is typing.
        /// </summary>
        DispatcherTimer _debounceTimer = new DispatcherTimer();
        private void CustomDLLPath_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (SelectDLLCombo.SelectedIndex == 0) { return; }

            _debounceTimer?.Stop();
            _debounceTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _debounceTimer.Tick += (s, args) =>
            {
                _debounceTimer.Stop();
                Properties.Settings.Default.DLLDir = CustomDLLPath.Text;
                Properties.Settings.Default.Save();
            };

            _debounceTimer.Start();
        }
        DispatcherTimer _debounceTimerLauncher = new DispatcherTimer();
        private void CustomLauncherPath_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (SelectLauncherCombo.SelectedIndex == 0) { return; }
            _debounceTimerLauncher?.Stop();
            _debounceTimerLauncher = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _debounceTimerLauncher.Tick += (s, args) =>
            {
                _debounceTimerLauncher.Stop();
                Properties.Settings.Default.DLLDir = CustomLauncherPath.Text;
                Properties.Settings.Default.Save();
            };

            _debounceTimerLauncher.Start();
        }

        private void Folder_Click(object sender, RoutedEventArgs e)
        {
            string FlarialPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Flarial");
            Button btn = (Button)sender;
            switch (btn.Content.ToString())
            {
                case "Open Launcher folder":
                    Process.Start("explorer.exe", Path.Combine(FlarialPath, "Launcher"));
                    break;
                case "Open Client folder":
                    Process.Start("explorer.exe", Path.Combine(FlarialPath, "Client"));
                    break;
            }
        }
    }
}
