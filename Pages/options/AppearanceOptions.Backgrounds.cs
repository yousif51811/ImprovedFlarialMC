using Flarial.Properties;
using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Path = System.IO.Path;
using Flarial.Services;

namespace Flarial.Pages.options
{
    /// <summary>
    /// Interaction logic for AppearanceOptions.xaml
    /// </summary>
    public partial class AppearanceOptions : UserControl
    {


        #region Background Options
        /// <summary>
        /// Load all backgrounds in Assets\\Backgrounds and use CreateImageButton to create buttons for each image. (And add the Add Background button at the end)
        /// </summary>
        private void LoadBackgrounds()
        {
            string folderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets\\Backgrounds");

            if (!Directory.Exists(folderPath))
                return;

            var imageFiles = Directory.GetFiles(folderPath)
                .Where(f => f.EndsWith(".png") ||
                            f.EndsWith(".jpg") ||
                            f.EndsWith(".jpeg") ||
                            f.EndsWith(".bmp") ||
                            f.EndsWith(".gif") ||
                            f.EndsWith(".tiff") ||
                            f.EndsWith(".ico"));

            BackgroundPanel.Children.Clear();
            // Add a default background (cant be removed)
            Button originalbgbtn = CreateImageButton("pack://application:,,,/Assets/Images/background.png", false);
            BackgroundPanel.Children.Add(originalbgbtn);

            // Add all backgrounds in the directory
            foreach (var file in imageFiles)
            {
                Button button = CreateImageButton(file);
                BackgroundPanel.Children.Add(button);
            }

            // Finally add the "Add background" Button
            Button addbutton = CreateAddBackgroundButton();
            BackgroundPanel.Children.Add(addbutton);
        }

        #region Creation
        /// <summary>
        /// Create a button with the provided path of an image
        /// </summary>
        /// <param name="imagePath">A direct path to the image of which the button will use</param>
        /// <returns>(Button) A button with the associated image</returns>
        private Button CreateImageButton(string imagePath, bool UseLoadImage = true)
        {
            var image = new Image
            {
                Source = UseLoadImage ? CreateControl.LoadImage(imagePath) : new BitmapImage(new Uri(imagePath, UriKind.RelativeOrAbsolute)),
                Stretch = Stretch.Fill
            };

            var border = new Border
            {
                CornerRadius = new CornerRadius(10),
                Child = image,
                ClipToBounds = true
            };

            var button = new Button
            {
                Content = border,
                Style = (Style)Application.Current.FindResource("ImgButton"),
                Tag = imagePath // Set the tag as the image path to be used later.
            };

            button.Click += SetBackground;
            if (imagePath.Contains("pack://")) return button;
            // Right-click menu
            var menu = new ContextMenu();

            var removeItem = new MenuItem { Header = "Remove" };
            removeItem.Click += (s, e) => RemoveBackground(imagePath, button);

            menu.Items.Add(removeItem);

            button.ContextMenu = menu;

            return button;
        }

        /// <summary>
        /// Create the button for adding an image.
        /// </summary>
        /// <returns> (Button) Add Background Button </returns>
        private Button CreateAddBackgroundButton()
        {
            var addButton = new Button
            {
                Content = "",
                Height = 70,
                Width = 110,
                FontSize = 30,
                Style = (Style)Application.Current.FindResource("Button"),
                Tag = "Add"
            };
            addButton.Click += AddBgBtn_Click;
            return addButton;
        }
        #endregion

        /// <summary>
        /// Set the current background and save it in settings.
        /// </summary>
        private void SetBackground(object sender, RoutedEventArgs e)
        {
            // Set all buttons in Background to 0.5 opacity except the one that was clicked
            foreach (Button button in BackgroundPanel.Children)
            {
                if (button.Tag as string == "Add") { continue; }
                button.Opacity = 0.5;
            }

            (sender as Button)?.Opacity = 1;
            MainWindow? mainWindow = Application.Current.MainWindow as MainWindow;
            if (mainWindow == null) return;
            string? path = (sender as Button)?.Tag.ToString();
            if (path == null) return;

            mainWindow.bg.Source = new BitmapImage(new Uri(path, UriKind.Absolute));

            // Now, add to settings
            Settings.Default.BackgroundDir = path;
            Settings.Default.Save();
        }

        /// <summary>
        /// Delete a specific background (With file locking prevention). 
        /// </summary>
        /// <param name="path"> Path of image to delete </param>
        /// <param name="button"> Button to delete (Image container) </param>
        private async void RemoveBackground(string path, Button? button)
        {
            try
            {
                if (button == null) return;

                // Check if the image is currently the background
                // if so reset to default background and update settings before deleting
                if (button.Tag as string == Settings.Default.BackgroundDir)
                {
                    // Reset settings
                    Settings.Default.BackgroundDir = "pack://application:,,,/Assets/Images/background.png";
                    Settings.Default.Save();

                    // Clear the Main Window's reference immediately
                    if (Application.Current.MainWindow is MainWindow mw)
                    {
                        mw.bg.Source = new BitmapImage(new Uri(Settings.Default.BackgroundDir, UriKind.Absolute));
                    }
                }

                // Remove the button and force garbage collection to release file locks (With a small delay)
                button.Content = null;
                BackgroundPanel.Children.Remove(button);
                await Task.Delay(100);
                GC.Collect();
                GC.WaitForPendingFinalizers();

                // Finally delete the file and refresh the background list
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
                LoadBackgrounds();

                // Call load settings to update the UI (In case the deleted background was the active one)
                // Probably change this later to only update the necessary parts instead of reloading everything
                // I'm just too lazy to do it right now and this works fine for now
                LoadSettings();
            }
            catch { }
        }

        /// <summary>
        /// Responsible for adding new backgrounds through the UI. Copies selected image to the Assets/Images folder and refreshes the background list.
        /// </summary>
        private void AddBgBtn_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                Filter = "Image files (*.png;*.jpg;*.jpeg;*.bmp;*.gif;*.tiff;*.ico)|*.png;*.jpg;*.jpeg;*.bmp;*.gif;*.tiff;*.ico|All files (*.*)|*.*",
                Title = "Select Image"
            };
            bool? result = dialog.ShowDialog();
            if (result == true)
            {
                string sourceFile = dialog.FileName;

                // Get the folder path of the application and combine it with the Backgrounds folder
                string targetFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets\\Backgrounds");

                // Ensure folder exists
                if (!Directory.Exists(targetFolder))
                    Directory.CreateDirectory(targetFolder);

                // Generate a unique file name (to avoid conflicts)
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(sourceFile);
                string destFile = Path.Combine(targetFolder, fileName);

                // Copy file (overwrite if exists)
                File.Copy(sourceFile, destFile, true);

                // Refresh Backgrounds
                LoadBackgrounds();
            }
        }


        #endregion



    }
}
