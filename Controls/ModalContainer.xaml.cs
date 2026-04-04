using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Flarial.Controls
{
    /// <summary>
    /// Interaction logic for ModalContainer.xaml
    /// </summary>
    public partial class ModalContainer : UserControl
    {
        private bool isopen;
        public bool IsOpen 
        { get => isopen; 
          set
            {
                isopen = value;
                if (value)
                {
                    AnimateShow();
                }
                else
                {
                    AnimateHide();
                }
                IsHitTestVisible = value;
                (MainWindow.GetWindow(this) as MainWindow)?.dimwindow.Opacity = value ? 0.6 : 0;
            }
        }


        public ModalContainer()
        {
            InitializeComponent();
        }
        
        /// <summary>
        /// Show a message modal.
        /// </summary>
        public void ShowMessage(string title, string message)
        {
            ModalMessage modal = new ModalMessage();
            modal.Title = title;
            modal.Message = message;
            ModalArea.Content = modal;
            IsOpen = true;
        }

        /// <summary>
        /// Await a response from a confirmation modal.
        /// </summary>
        /// <param name="title"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public Task<bool> ShowConfirmationAsync(string title, string message)
        {
            // Create the modal
            ConfirmationModal modal = new ConfirmationModal();
            // Set the title and message
            modal.Title = title;
            modal.Message = message;
            // Create a TaskCompletionSource to await the result
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
            modal._tcs = tcs;
            // Show the modal
            ModalArea.Content = modal;
            IsOpen = true;
            // Return the Task to await the result
            return tcs.Task;
        }
        TimeSpan Duration = TimeSpan.FromSeconds(0.3);
        private void AnimateShow()
        {
            CubicEase ease = new CubicEase { EasingMode = EasingMode.EaseInOut };
            DoubleAnimation AnimateOpen = new DoubleAnimation
            {
                Duration = Duration,
                To = 0.95,
                EasingFunction = ease,
            };
            BeginAnimation(OpacityProperty, AnimateOpen);
        }
        private void AnimateHide()
        {
            CubicEase ease = new CubicEase { EasingMode = EasingMode.EaseInOut };
            DoubleAnimation AnimateOpen = new DoubleAnimation
            {
                Duration = Duration,
                To = 0,
                EasingFunction = ease,
            };
            BeginAnimation(OpacityProperty, AnimateOpen);
        }
    }
}
