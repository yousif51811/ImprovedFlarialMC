using Flarial.Properties;
using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Path = System.IO.Path;

namespace Flarial.Pages.options
{
    /// <summary>
    /// Interaction logic for AppearanceOptions.xaml
    /// </summary>
    public partial class AppearanceOptions : UserControl
    {


        public AppearanceOptions()
        {
            InitializeComponent();

            // Set the Font combo box items to system fonts
            SelectFontCombo.ItemsSource = Fonts.SystemFontFamilies
            .OrderBy(f => f.Source);
            LoadSettings();

            
        }

        /// <summary>
        /// Load all settings right away.
        /// </summary>
        public void LoadSettings()
        {
            #region Background Loading
            // Load the background
            LoadBackgrounds();
            foreach (Button button in BackgroundPanel.Children)
            {
                if (button.Tag as string == Settings.Default.BackgroundDir)
                {
                    button.Opacity = 1;
                    MainWindow? mainWindow = Application.Current.MainWindow as MainWindow;
                    if (mainWindow != null)
                    {
                        mainWindow.bg.Source = new BitmapImage(new Uri(Settings.Default.BackgroundDir, UriKind.Absolute));
                    }

                }
                else
                {
                    button.Opacity = 0.5;
                }

            }
            #endregion


            #region Font Loading
            // Load the font
            string fontName = Settings.Default.MainFont;
            FontFamily? font = Fonts.SystemFontFamilies.FirstOrDefault(f => f.Source == fontName);
            SaveFont(font ?? new FontFamily(fontName) ?? new FontFamily("Segoe UI"));
            SelectFontCombo.SelectedItem = font;
            #endregion


            #region Checkbox(es) Loading
            // Load the animations setting
            AnimationsCheck.IsChecked = Settings.Default.AnimationsEnabled;
            #endregion
        }

        #region Fonts
        private void SelectFontCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SelectFontCombo.SelectedItem is FontFamily selectedFont)
            {
                SaveFont(selectedFont);
            }
        }

        /// <summary>
        /// Save font to resources and settings. (Makes it update immediately and persist across sessions)
        /// </summary>
        /// <param name="font"></param>
        private void SaveFont(FontFamily font)
        {
            // Save the selected font to resources (apply it to the entire application)
            Application.Current.Resources["MainFont"] = font;

            // Save in settings
            Settings.Default.MainFont = font.Source;
            Settings.Default.Save();
        }

        #endregion

        private void Animations_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox)
            {
                Settings.Default.AnimationsEnabled = checkBox.IsChecked == true;
                Settings.Default.Save();
            }
        }
    }
}
