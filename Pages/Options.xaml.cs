using Flarial.Controls;
using Flarial.Pages.options;
using System.Windows;
using System.Windows.Controls;

namespace Flarial.Pages
{
    /// <summary>
    /// Interaction logic for Options.xaml
    /// </summary>
    public partial class Options : UserControl
    {
        public GeneralOptions general = new GeneralOptions();
        public AboutOptions about = new AboutOptions();
        public AppearanceOptions appearance = new AppearanceOptions();
        public VersionsOptions versions = new VersionsOptions();
        public Options()
        {
            InitializeComponent();
            ContentArea.Content = general;
            Loaded += (s, e) => appearance.LoadSettings();
        }

        /// <summary>
        /// Switch Tabs logic. 
        /// Uses the name of the button to determine which tab to switch to.
        /// </summary>
        private void SidebarButton_Click(object sender, RoutedEventArgs e)
        {
            AboutBtn.IsActive = false;
            GeneralBtn.IsActive = false;
            AppearanceBtn.IsActive = false;
            VersionsBtn.IsActive = false;
            SidebarButton btn = sender as SidebarButton;
            switch (btn?.Name)
            {
                case "GeneralBtn":
                    ContentArea.Content = general;
                    GeneralBtn.IsActive = true;
                    break;
                case "AboutBtn":
                    ContentArea.Content = about;
                    AboutBtn.IsActive = true;
                    break;
                case "AppearanceBtn":
                    ContentArea.Content = appearance;
                    AppearanceBtn.IsActive = true;
                    break;
                case "VersionsBtn":
                    ContentArea.Content = versions;
                    VersionsBtn.IsActive = true;
                    break;
            }
        }
    }
}
