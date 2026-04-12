using Flarial.Models;
using Flarial.Services;
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
        
        public VersionsOptions()
        {
            InitializeComponent();
            DataContext = new ViewModels.VersionListViewModel();
        }
    }
}
