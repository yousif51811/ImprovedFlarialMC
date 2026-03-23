using Flarial.Pages;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Flarial
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Options Page object
        private Options options = new Options();
        public MainWindow()
        {
            InitializeComponent();
            GetTime();
            OptionsBorder.Child = options;
        }
        /// <summary>
        /// Window Controls Logic.
        /// </summary>
        private void Rectangle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                DragMove();
            Console.WriteLine("Dragging window...");
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void MinimizeBtn_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }



        /// <summary>
        /// Launching the game.
        /// </summary>
        private async void LaunchBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                LaunchBtn.IsEnabled = false;
                LaunchContent.Text = "Updating";
                LaunchIcon.Text = " ";
                bool? update = await ClientHandler.CheckForUpdates();
                if (update == false)
                {
                    FailedLaunch();
                    return;
                }

                LaunchContent.Text = "Starting";
                LaunchIcon.Text = " ";
                if (!await ClientHandler.StartGame())
                {
                    FailedLaunch();
                    return;
                }
                await Task.Delay(2000);
                LaunchContent.Text = "Launch";
                LaunchBtn.IsEnabled = true;
            }
            catch { FailedLaunch(); }
        }
        private async void FailedLaunch()
        {
            LaunchContent.Text = "Failed";
            LaunchIcon.Text = " ";
            await Task.Delay(2000).ContinueWith(_ =>
            {
                Dispatcher.Invoke(() =>
                {
                    LaunchContent.Text = "Launch";
                    LaunchIcon.Text = " ";
                    LaunchBtn.IsEnabled = true;
                });
            });
            LaunchBtn.IsEnabled = true;
        }

        /// <summary>
        /// Sets the greeting text based on the current time of day.
        /// </summary>
        private async void GetTime()
        {
            switch (DateTime.Now.Hour) 
            {
                case >= 5 and < 12:
                    GreetingMain.Text = "Good Morning!";
                    break;
                case >= 12 and < 18:
                    GreetingMain.Text = "Good Afternoon!";
                    break;
                default:
                    GreetingMain.Text = "Good Evening!";
                    break;
            }
        }

        /// <summary>
        /// opening or closing the options menu.
        /// </summary>
        /// <param name="open"> Wether to close or open the options. </param>
        private async void ShowOptions(bool open = true)
        {
            Duration time = TimeSpan.FromSeconds(0.2);
            if (open)
            {
                DoubleAnimation OpenAnimation = new DoubleAnimation
                {
                    To = 250,
                    Duration = time,
                    AccelerationRatio = 0.5,
                };
                OptionsBorder.BeginAnimation(HeightProperty, OpenAnimation);
                DoubleAnimation FadeInDIm = new DoubleAnimation
                {
                    To = 0.5,
                    Duration = time,
                };
                dimwindow.BeginAnimation(OpacityProperty, FadeInDIm);
                dimwindow.IsHitTestVisible = true;
            }
            else
            {
                DoubleAnimation CloseAnimation = new DoubleAnimation
                {
                    To = 0,
                    Duration = time,
                };
                OptionsBorder.BeginAnimation(HeightProperty, CloseAnimation);
                DoubleAnimation FadeInDIm = new DoubleAnimation
                {
                    To = 0,
                    Duration = time,

                };
                dimwindow.BeginAnimation(OpacityProperty, FadeInDIm);
                dimwindow.IsHitTestVisible = false;
            }
            
        }

        private void SettingsBtn_Click(object sender, RoutedEventArgs e)
        {
            ShowOptions();
        }

        private void dimwindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ShowOptions(false);
        }
    }
}