using System.Windows;
using System.Windows.Controls;

namespace Flarial.Controls
{
    /// <summary>
    /// Interaction logic for SidebarButton.xaml
    /// </summary>
    public partial class SidebarButton : UserControl
    {
        public SidebarButton()
        {
            InitializeComponent();
            DataContext = this;
            Loaded += (s, e) =>
            {
                ApplyStyle(IsActive);
            };
        }


        private void ApplyStyle(bool value)
        {
            if (MyBtn == null)
                return;

            if (value)
                MyBtn.Style = (Style)Application.Current.TryFindResource("EnabledTabButton");
            else
                MyBtn.Style = (Style)Application.Current.TryFindResource("DisabledTabButton");
        }


        /// <summary>
        /// Link The icon of the button, using Segoe Fluent Icons.
        /// </summary>
        public string Icon
        {
            get { return (string)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }
        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register(nameof(Icon), typeof(string), typeof(SidebarButton), new PropertyMetadata(""));

        /// <summary>
        /// Property to choose wiether the button is enabled or not.
        /// For inactive tabs.
        /// </summary>
        public bool IsActive
        {
            get { return (bool)GetValue(IsActiveProperty); }
            set { SetValue(IsActiveProperty, value); }
        }

        public static readonly DependencyProperty IsActiveProperty =
            DependencyProperty.Register(
                nameof(IsActive),
                typeof(bool),
                typeof(SidebarButton),
                new PropertyMetadata(false, OnIsCustomEnabledChanged));

        private static void OnIsCustomEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SidebarButton control = (SidebarButton)d;

            bool value = (bool)e.NewValue;

            if (value)
            {
                control.MyBtn.Style = (Style)Application.Current.TryFindResource("EnabledTabButton");
            }
            else
            {
                control.MyBtn.Style = (Style)Application.Current.TryFindResource("DisabledTabButton");

            }
        }
        /// <summary>
        /// Text of the button.
        /// </summary>
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(nameof(Text), typeof(string), typeof(SidebarButton), new PropertyMetadata(string.Empty));


        /// <summary>
        /// Expose Click event of the internal button.
        /// </summary>
        private RoutedEventHandler _click;
        public event RoutedEventHandler Click
        {
            add { _click += value; }
            remove { _click -= value; }
        }
        private void MyBtn_Click(object sender, RoutedEventArgs e)
        {
            // Raise the UserControl's Click event
            _click?.Invoke(this, e); // Note: 'this' instead of 'sender'
        }
    }
}
