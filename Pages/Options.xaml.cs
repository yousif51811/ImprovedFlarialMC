using Flarial.Controls;
using Flarial.Pages.options;
using System;
using System.Collections.Generic;
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
        public Options()
        {
            InitializeComponent();
            ContentArea.Content = general;
            Loaded += (s,e) => appearance.LoadSettings();
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
            SidebarButton? btn = sender as SidebarButton;
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
            }
        }
    }
}
