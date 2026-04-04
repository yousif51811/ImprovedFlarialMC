using Flarial.Models;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
namespace Flarial.Pages.options
{
    /// <summary>
    /// Interaction logic for VersionsOptions.xaml
    /// </summary>
    public partial class VersionsOptions : UserControl
    {
        public ObservableCollection<VersionItem> versions { get; set; } = new ObservableCollection<VersionItem>();
        public VersionsOptions()
        {
            InitializeComponent();
            DataContext = this;
            versions.Add(new VersionItem { Name = "1.26.12" });
            versions.Add(new VersionItem { Name = "1.26.11" });
            versions.Add(new VersionItem { Name = "1.26.10" });
            versions.Add(new VersionItem { Name = "1.26.3" });
            versions.Add(new VersionItem { Name = "1.26.2" });
            versions.Add(new VersionItem { Name = "1.26.1" });
            versions.Add(new VersionItem { Name = "1.26.0" });
        }

        private void ListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show("Version: " + ((VersionItem)VersionsListBox.SelectedItem).Name);
        }
    }
}
