using Flarial.Pages;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Flarial.Services;
using Flarial.Properties;

namespace Flarial
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        enum LaunchState
        {
            Idle,
            Updating,
            Starting,
            Failed
        }
        // Options Page object
        private Options options = new Options();
        public MainWindow()
        {
            InitializeComponent();
            GetTime(); // Set The greeting
            OptionsBorder.Child = options;
            ModalArea.Content = Container.Modal;
        }

        
        #region Window Controls
        // Area for window controls (Dragging, minimzing, closing)
        private void Rectangle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                DragMove();
        }
        
        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown(); // Shutdown the entire application
        }

        private void MinimizeBtn_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }
        #endregion


        #region Launch Button
        /// <summary>
        /// Launch Button logic
        /// </summary>
        private async void LaunchBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Check for updates
                SetLaunchState(LaunchState.Updating, false);
                bool? update = await FlarialHandler.CheckForUpdates();
                if (update == false)
                {
                    FailedLaunch();
                    return;
                }

                // Launch the game
                SetLaunchState(LaunchState.Starting, false);
                if (!await FlarialHandler.StartGame())
                {
                    FailedLaunch();
                    return;
                }

                // Wait 2 seconds then go back to original state
                await Task.Delay(2000);
                SetLaunchState(LaunchState.Idle);
            }
            catch { FailedLaunch(); }
        }

        /// <summary>
        /// Simple function to set the LaunchState to failed and then back to idle after 2 seconds
        /// </summary>
        private async void FailedLaunch()
        {
            SetLaunchState(LaunchState.Failed, false);
            await Task.Delay(2000);
            SetLaunchState(LaunchState.Idle);
        }


        /// <summary>
        /// Set the launch buttons state and text based on the provided arguments (to avoid repetitive code in the launch method).
        /// </summary>
        /// <param name="state">LaunchState enum: The state of the button</param>
        /// <param name="enable">(Default true) Wether or not to enable the Launch button</param>
        private void SetLaunchState(LaunchState state, bool enable = true)
        {
            LaunchContent.Text = state switch
            {
                LaunchState.Idle => "Launch",
                LaunchState.Updating => "Updating",
                LaunchState.Starting => "Starting",
                LaunchState.Failed => "Failed",
                _ => "Launch"
            };
            LaunchIcon.Text = state switch
            {
                LaunchState.Idle => " ",
                LaunchState.Updating => " ",
                LaunchState.Starting => " ",
                LaunchState.Failed => " ",
                _ => " "
            };
            LaunchBtn.IsEnabled = enable;
        }
        #endregion





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
            Logging.Log(@$"Set greeting to: {GreetingMain.Text}", "INFO");
        }




        /// <summary>
        /// opening or closing the options menu.
        /// </summary>
        /// <param name="open"> Wether to close or open the options. </param>
        public async void ShowOptions(bool open = true)
        {
            CubicEase ease = new CubicEase { EasingMode = EasingMode.EaseInOut };
            if (Settings.Default.AnimationsEnabled)
            {
                Duration time = TimeSpan.FromSeconds(0.2);
                if (open)
                {
                    DoubleAnimation OpenAnimation = new DoubleAnimation
                    {
                        To = 275,
                        Duration = time,
                        EasingFunction = ease,
                    };
                    OptionsBorder.BeginAnimation(HeightProperty, OpenAnimation);
                    DoubleAnimation FadeInDIm = new DoubleAnimation
                    {
                        To = 0.5,
                        Duration = time,
                        EasingFunction = ease,
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
                        EasingFunction= ease,
                    };
                    OptionsBorder.BeginAnimation(HeightProperty, CloseAnimation);
                    DoubleAnimation FadeInDIm = new DoubleAnimation
                    {
                        To = 0,
                        Duration = time,
                        EasingFunction = ease
                    };
                    dimwindow.BeginAnimation(OpacityProperty, FadeInDIm);
                    dimwindow.IsHitTestVisible = false;
                }
            }
            else
            {
                Duration time = TimeSpan.FromSeconds(0.2);
                if (open)
                {
                    OptionsBorder.Height = 275;
                    dimwindow.Opacity = 0.5;
                    dimwindow.IsHitTestVisible = true;
                }
                else
                {
                    OptionsBorder.BeginAnimation(HeightProperty, null);
                    dimwindow.BeginAnimation(OpacityProperty, null);

                    OptionsBorder.Height = 0;
                    dimwindow.Opacity = 0;
                    dimwindow.IsHitTestVisible = false;
                }
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