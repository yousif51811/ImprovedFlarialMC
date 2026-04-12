using System.Threading.Tasks;
using System.Windows;
namespace Flarial.Controls
{
    /// <summary>
    /// A modal that returns true or false based on the user's choice. (Yes or No)
    /// </summary>
    public partial class ConfirmationModal : System.Windows.Controls.UserControl
    {
        public TaskCompletionSource<bool> _tcs = new TaskCompletionSource<bool>();
        public ConfirmationModal()
        {
            InitializeComponent();
            DataContext = this;
        }

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(nameof(Title), typeof(string), typeof(ConfirmationModal), new PropertyMetadata("Lorem Ipsum"));



        public string Message
        {
            get { return (string)GetValue(MessageProperty); }
            set { SetValue(MessageProperty, value); }
        }
        public static readonly DependencyProperty MessageProperty =
            DependencyProperty.Register(nameof(Message), typeof(string), typeof(ConfirmationModal), new PropertyMetadata("Stuff Go Here and lorem ipsum"));

        private void CloseModal()
        {
            UIContainer.Modal.IsOpen = false;
        }

        private void YesBtn_Click(object sender, RoutedEventArgs e)
        {
            _tcs.TrySetResult(true);
            CloseModal();
        }

        private void NoBtn_Click(object sender, RoutedEventArgs e)
        {
            _tcs.TrySetResult(false);
            CloseModal();
        }
    }
}
