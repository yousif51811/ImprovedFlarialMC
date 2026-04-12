using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Flarial.Controls
{
    /// <summary>
    /// Interaction logic for Modal.xaml
    /// </summary>
    public partial class ModalMessage : UserControl
    {


        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(nameof(Title), typeof(string), typeof(ModalMessage), new PropertyMetadata(string.Empty));



        public string Message
        {
            get { return (string)GetValue(MessageProperty); }
            set { SetValue(MessageProperty, value); }
        }
        public static readonly DependencyProperty MessageProperty =
            DependencyProperty.Register(nameof(Message), typeof(string), typeof(ModalMessage), new PropertyMetadata(string.Empty));




        public ModalMessage()
        {
            InitializeComponent();
            DataContext = this;
        }


        /// <summary>
        /// Copy button logic
        /// </summary>
        private async void Copy_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(Message);
            (sender as Button).Content = ""; // Check icon
            await Task.Delay(2000);
            (sender as Button).Content = ""; // Copy icon
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            UIContainer.Modal.IsOpen = false;
        }
    }
}
