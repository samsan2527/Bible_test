using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using static System.Net.Mime.MediaTypeNames;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using Path = System.IO.Path;
using Window = System.Windows.Window;
using Microsoft.Win32;
using System.Text.Json;
using System.Configuration;
using static grace.MainWindow;
using System.Windows.Markup;
using System.Diagnostics;
using static grace.ESV;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;
using System.Media;
using Newtonsoft.Json.Linq;

namespace grace
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
         private bool _isDragging;
        string LogfilePath = ConfigurationManager.AppSettings["LogfilePath"].ToString();
        private ObservableCollection<string> chapters;
        private string selectedChapter;
        private bool _isChapterSelected;
        Tamil_Bible tamilBibleData;
        ESV esv;
        string tamil_chapter = "";
        string English_chapter = "";
        string _chapter = "";
        string _verse = "";
        KJV kjv;
        NIV niv;
        private Point _startPoint;
        private TextPointer _startPointer;
        private TextPointer _endPointer;
        private Brush _highlightBrush = Brushes.Yellow;
        public bool IsChapterSelected
        {
            get => _isChapterSelected;
            set
            {
                _isChapterSelected = value;
                OnPropertyChanged(nameof(IsChapterSelected));
            }
        }
        private ScreenFront _imageWindow;
        private ScreenBack _rearWindow;
        private Brush _originalBackground;  
        private Brush _originalBackground1;
        private string[] allFileNames;
        BitmapImage bg = new BitmapImage();
        string ver = "ESV";
       
        Color fg= Colors.WhiteSmoke;
        public class VerseWithTranslation
        {
            public string VerseNumber { get; set; }
            public string TamilText { get; set; }
            public string EnglishText { get; set; }
        }
        public MainWindow()
        {
            InitializeComponent();
            chapters = new ObservableCollection<string>();
            cmbExternalAuditor.ItemsSource = chapters;
            cmbExternalAuditor.SelectedItem = selectedChapter;
            BibleBooksComboBox.SelectionChanged += BibleBooksComboBox_SelectionChanged;
            Chapter_no.SelectionChanged += Chapter_no_SelectionChanged;
            Verse.SelectionChanged += Verse_SelectionChanged;
            InitializeWindows();
            this.Closed += MainWindow_Closed;
            this.Closing += MainWindow_Closed;
            LoadJsonFiles();
            IsChapterSelected=false;
            LoadVerseDetails();
            string appDirectory = AppDomain.CurrentDomain.BaseDirectory; 
            string relativePath = "bg.jpg"; 
            string fullPath = System.IO.Path.Combine(appDirectory, relativePath);
            bg.BeginInit();
            bg.UriSource = new Uri(fullPath, UriKind.Absolute); 
            bg.EndInit();
        }
        private void LoadRearWindowContent()
        {
            if (_rearWindow != null && _rearWindow.IsVisible)
            {
                // Attach event handler to capture updated content
                _rearWindow.ContentRendered += RearWindow_ContentRendered;
            }
            else
            {
                _rearWindow.ContentRendered += RearWindow_ContentRendered;
                _rearWindow.Show();
            }

        }
        private void RearWindow_ContentRendered(object sender, EventArgs e)
        {
            // Copy the content of the rear window to the main window
            CopyRearWindowContent(_rearWindow);
        }
        public static T CloneVisualElement<T>(T original) where T : UIElement
        {
            // Clone the visual element
            if (original == null) return null;

            var xamlString = XamlWriter.Save(original);
            var reader = new System.Xml.XmlTextReader(new System.IO.StringReader(xamlString));
            var cloned = XamlReader.Load(reader) as T;

            return cloned;
        }
        public void CopyRearWindowContent(Window rearWindow)
        {
            if (rearWindow.Content is UIElement rearContent)
            {
                // Add content to the grid
                var visualBrush = new VisualBrush
                {
                    Visual = rearContent,
                    Stretch = Stretch.Uniform
                };

                // Create a Rectangle to display the VisualBrush
                var rectangle = new Rectangle
                {
                    Fill = visualBrush,
                    Width = 800, // Fixed size or adjust as needed
                    Height = 500
                };

                // Clear existing content
                MainGrid.Children.Clear();

                // Add the Rectangle to the Grid
                MainGrid.Children.Add(rectangle);
            }
        }
        private void CmbExternalAuditor_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var comboBox = sender as System.Windows.Controls.ComboBox;
            //e.Handled = true;
            if (comboBox != null && comboBox.SelectedItem != null)
            {
                // Update selected chapter
                selectedChapter = comboBox.SelectedItem.ToString();
                IsChapterSelected = true;
                string directoryPath = @"G:\TEST PROJECTS\grace\Tamil Bible";
                string EngPath = @"G:\TEST PROJECTS\grace\KJV";
                string ESVPath = @"G:\TEST PROJECTS\grace\ESV";

                string filePath = Path.Combine(directoryPath, selectedChapter + ".json");
                string fileKJVPath = Path.Combine(EngPath, selectedChapter + ".json");
                string fileESVPath = Path.Combine(ESVPath, selectedChapter + ".json");
                if (File.Exists(filePath))
                {
                    try
                    {
                        string jsonContent = File.ReadAllText(filePath);
                        string jsonKJVContent = File.ReadAllText(fileKJVPath);
                        string jsonESVContent = File.ReadAllText(fileESVPath);
                        tamilBibleData = JsonConvert.DeserializeObject<Tamil_Bible>(jsonContent);
                        kjv = JsonConvert.DeserializeObject<KJV>(jsonKJVContent);
                        esv = JsonConvert.DeserializeObject<ESV>(jsonESVContent);
                        List<string> chapterNumbers = new List<string>();
                        foreach (var chapter in tamilBibleData.chapters)
                        {
                            chapterNumbers.Add(chapter.chapter);
                        }
                        Chapter_no.ItemsSource = chapterNumbers;
                    }
                    catch (Exception ex)
                    {
                        
                    }
                }
                else
                {
                   
                }

            }
        }

        private void Chapter_no_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var comboBox = sender as System.Windows.Controls.ComboBox;
            //e.Handled = true;
            if (comboBox != null && comboBox.SelectedItem != null)
            {

                selectedChapter = comboBox.SelectedItem.ToString();
                _chapter = selectedChapter;
                var chapter = tamilBibleData.chapters.FirstOrDefault(c => c.chapter == selectedChapter);
                chapters KJVchapter=null; chapters ESVchapter = null; chapters NIVchapter = null;
                if (ESV.IsChecked == true)
                {
                     ESVchapter = esv.Chapters.FirstOrDefault(c => c.chapter == selectedChapter);
                }
                else if (KJV.IsChecked == true)
                {
                    KJVchapter = kjv.chapters.FirstOrDefault(c => c.chapter == selectedChapter);
                }
                else if (NIV.IsChecked == true)
                {
                    NIVchapter = niv.Chapters.FirstOrDefault(c => c.chapter == selectedChapter);
                }
                if (chapter != null)
                {
                    List<string> verseNumbers = chapter.verses.Select(v => v.verse).ToList();
                    Verse.ItemsSource = verseNumbers;
                    List<VerseWithTranslation> versesWithTranslation = null;
                    if (ESV.IsChecked == true)
                    {
                        versesWithTranslation = chapter.verses.Zip(ESVchapter.verses, (tamilVerse, ESVVerse) =>
                        new VerseWithTranslation
                        {
                            VerseNumber = tamilVerse.verse,
                            TamilText = tamilVerse.text,
                            EnglishText = ESVVerse.text
                        }).ToList();
                    }
                    else if (KJV.IsChecked == true)
                    {
                        versesWithTranslation = chapter.verses.Zip(KJVchapter.verses, (tamilVerse, KJVVerse) =>
                        new VerseWithTranslation
                        {
                            VerseNumber = tamilVerse.verse,
                            TamilText = tamilVerse.text,
                            EnglishText = KJVVerse.text
                        }).ToList();
                    }
                    else if (NIV.IsChecked == true)
                    {
                        versesWithTranslation = chapter.verses.Zip(NIVchapter.verses, (tamilVerse, NIVVerse) =>
                        new VerseWithTranslation
                        {
                            VerseNumber = tamilVerse.verse,
                            TamilText = tamilVerse.text,
                            EnglishText = NIVVerse.text
                        }).ToList();
                    }
                    Verse.ItemsSource = verseNumbers;
                    VerseListView.ItemsSource = versesWithTranslation;
                }
                else
                {
                    Console.WriteLine($"KJV chapter {selectedChapter} not found.");
                }
                //if (chapter != null)
                //{
                //    List<string> verseNumbers = chapter.verses.Select(v => v.verse).ToList();
                //    Verse.ItemsSource = verseNumbers;

                //    var versesWithTranslation = chapter.verses.Zip(KJVchapter.verses, (tamilVerse, kjvVerse) =>
                //    new VerseWithTranslation
                //    {
                //        VerseNumber = tamilVerse.verse,
                //        TamilText = tamilVerse.text,
                //        EnglishText = kjvVerse.text
                //    }).ToList();
                //    Verse.ItemsSource = versesWithTranslation.Select(v => v.VerseNumber).ToList();
                //    VerseListView.ItemsSource = versesWithTranslation;
                //}
            }
        }


        private void Verse_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var comboBox = sender as System.Windows.Controls.ComboBox;
            string selectedVerseNumber = comboBox.SelectedItem.ToString();
            var chapter = tamilBibleData.chapters.FirstOrDefault(c => c.chapter == selectedChapter);
            chapters KJVchapter =null;
            chapters ESVchapter = null;
            chapters NIVchapter = null;
            if (KJV.IsChecked == true)
            {
                KJVchapter = kjv.chapters.FirstOrDefault(c => c.chapter == selectedChapter);
            }
            else if (ESV.IsChecked==true)
            {
                ESVchapter = esv.Chapters.FirstOrDefault(c => c.chapter == selectedChapter);
            }
            else if (NIV.IsChecked == true)
            {
                NIVchapter = niv.Chapters.FirstOrDefault(c => c.chapter == selectedChapter);
            }


            //e.Handled = true;
            if (comboBox != null && comboBox.SelectedItem != null)
            {
                if (KJV.IsChecked == true)
                {

                    var selectedVerse = chapter.verses.FirstOrDefault(v => v.verse == selectedVerseNumber);
                    var selectedKJVVerse = KJVchapter.verses.FirstOrDefault(v => v.verse == selectedVerseNumber);

                    if (selectedVerse != null && selectedKJVVerse != null)
                    {
                        var verseWithTranslation = new VerseWithTranslation
                        {
                            VerseNumber = selectedVerse.verse,
                            TamilText = selectedVerse.text,
                            EnglishText = selectedKJVVerse.text
                        };

                        // Clear previous data and add the selected verse
                        VerseListView.ItemsSource = new List<VerseWithTranslation> { verseWithTranslation };
                    }
                }
                else if (ESV.IsChecked == true)
                {
                    var selectedVerse = chapter.verses.FirstOrDefault(v => v.verse == selectedVerseNumber);
                    var selectedESVVerse = ESVchapter.verses.FirstOrDefault(v => v.verse == selectedVerseNumber);

                    if (selectedVerse != null && selectedESVVerse != null)
                    {
                        var verseWithTranslation = new VerseWithTranslation
                        {
                            VerseNumber = selectedVerse.verse,
                            TamilText = selectedVerse.text,
                            EnglishText = selectedESVVerse.text
                        };

                        // Clear previous data and add the selected verse
                        VerseListView.ItemsSource = new List<VerseWithTranslation> { verseWithTranslation };
                    }
                }
                else if (NIV.IsChecked == true)
                {
                    var selectedVerse = chapter.verses.FirstOrDefault(v => v.verse == selectedVerseNumber);
                    var selectedNIVVerse = NIVchapter.verses.FirstOrDefault(v => v.verse == selectedVerseNumber);

                    if (selectedVerse != null && selectedNIVVerse != null)
                    {
                        var verseWithTranslation = new VerseWithTranslation
                        {
                            VerseNumber = selectedVerse.verse,
                            TamilText = selectedVerse.text,
                            EnglishText = selectedNIVVerse.text
                        };

                        // Clear previous data and add the selected verse
                        VerseListView.ItemsSource = new List<VerseWithTranslation> { verseWithTranslation };
                    }
                }
            }
        }

        private void PART_EditableTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as System.Windows.Controls.TextBox;
            if (textBox != null)
            {
                e.Handled = true;
                string filterText = textBox.Text.ToLower();

                // Filter the items in the ComboBox
                var filteredItems = allFileNames
                    .Where(name => name.ToLower().Contains(filterText))
                    .ToList();

                // Update the ComboBox ItemsSource with filtered items
                cmbExternalAuditor.ItemsSource = new ObservableCollection<string>(filteredItems);
            }
        }
        private void LoadJsonFiles()
        {
            string directoryPath = @"G:\TEST PROJECTS\grace\Tamil Bible";
            string[] jsonFiles = Directory.GetFiles(directoryPath, "*.json");
            allFileNames = jsonFiles
               .Select(file => Path.GetFileNameWithoutExtension(file))
               .Select(ToCamelCase)
               .ToArray();
            var fileNames = jsonFiles
            .Select(file => Path.GetFileNameWithoutExtension(file)) // Get file name without extension
            .Select(ToCamelCase) // Convert to camel case
            .ToList(); // Convert to a list
            chapters.Clear();
            foreach (var fileName in fileNames)
            {
                chapters.Add(fileName);
            }
        }
        private string ToCamelCase(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            // Convert to camel case
            return char.ToLower(input[0]) + input.Substring(1);
        }
        private void MainWindow_Closed(object sender, System.EventArgs e)
        {
            _imageWindow?.Close();
        }
        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            _rearWindow?.Close();
            _imageWindow?.Close();
            System.Windows.Forms.Application.Exit();
        }
        private void InitializeWindows()
        {
            _imageWindow = new ScreenFront();
            _rearWindow = new ScreenBack();
        }

        private int GetScreenIndexForWindow(Window window)
        {
            var screens = Screen.AllScreens;
            for (int i = 0; i < screens.Length; i++)
            {
                var screen = screens[i];
                if (window.Left >= screen.Bounds.Left &&
                    window.Left + window.Width <= screen.Bounds.Right &&
                    window.Top >= screen.Bounds.Top &&
                    window.Top + window.Height <= screen.Bounds.Bottom)
                {
                    return i;
                }
            }
            return 0; 
        }
        private void MoveWindowToScreen(Window window, int screenIndex)
        {
            var screens = Screen.AllScreens;
            if (screenIndex < 0 || screenIndex >= screens.Length)
                throw new ArgumentOutOfRangeException(nameof(screenIndex), "Invalid screen index");

            var screen = screens[screenIndex];
            var bounds = screen.Bounds;

            window.WindowStartupLocation = WindowStartupLocation.Manual;
            window.Left = bounds.Left;
            window.Top = bounds.Top;
            window.Width = bounds.Width;
            window.Height = bounds.Height;
            window.WindowState = WindowState.Normal;  
            window.WindowState = WindowState.Maximized;

            window.Topmost = true; 
            window.Activate(); 
            window.Topmost = false;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var screens = Screen.AllScreens;

            int imageWindowScreenIndex = GetScreenIndexForWindow(_imageWindow);
            int rearWindowScreenIndex = GetScreenIndexForWindow(_rearWindow);

            if (imageWindowScreenIndex == -1 || rearWindowScreenIndex == -1)
            {
                throw new InvalidOperationException("One or both windows are not found on any screen.");
            }

            int secondaryScreenIndex = 0;
            int tertiaryScreenIndex = 1;

            if (imageWindowScreenIndex == secondaryScreenIndex && rearWindowScreenIndex == tertiaryScreenIndex ||
                imageWindowScreenIndex == tertiaryScreenIndex && rearWindowScreenIndex == secondaryScreenIndex)
            {
                MoveWindowToScreen(_imageWindow, tertiaryScreenIndex);
                MoveWindowToScreen(_rearWindow, secondaryScreenIndex);
            }
            else
            {
                throw new InvalidOperationException("Windows are not on the expected screens for swapping.");
            }
            //       int secondaryScreenIndex = 0; 
            //       int tertiaryScreenIndex = 1;
            //       int rearWindowScreenIndex = GetScreenIndexForWindow(_rearWindow);
            //       int imageWindowScreenIndex = GetScreenIndexForWindow(_imageWindow);


            //       if ((imageWindowScreenIndex == secondaryScreenIndex && rearWindowScreenIndex == tertiaryScreenIndex) ||
            //(imageWindowScreenIndex == tertiaryScreenIndex && rearWindowScreenIndex == secondaryScreenIndex))
            //       {
            //           // Swap the screens
            //           MoveWindowToScreen(_imageWindow, rearWindowScreenIndex);  
            //           MoveWindowToScreen(_rearWindow, imageWindowScreenIndex);  
            //       }
            //       else
            //       {
            //           //MessageBox.Show("Both windows must be on either the secondary or tertiary screen to swap.");
            //       }
            //var screens = Screen.AllScreens;
            //if (screens.Length == 1)
            //{
            //    _imageWindow.WindowStartupLocation = WindowStartupLocation.Manual;
            //    var secondScreen = screens[1];
            //    var secondScreenBounds = secondScreen.Bounds;
            //    _imageWindow.Left = secondScreenBounds.Left;
            //    _imageWindow.Top = secondScreenBounds.Top;
            //    _imageWindow.Loaded += (s, ev) =>
            //    {
            //        _imageWindow.Width = secondScreenBounds.Width;
            //        _imageWindow.Height = secondScreenBounds.Height;
            //        _imageWindow.WindowStyle = WindowStyle.None;
            //        _imageWindow.WindowState = WindowState.Maximized;


            //    };
            //    _imageWindow.Show();
            //}
            //else if (screens.Length == 2)
            //{
            //    _rearWindow.WindowStartupLocation = WindowStartupLocation.Manual;
            //    var secondScreen = screens[1];
            //    var secondScreenBounds = secondScreen.Bounds;
            //    _rearWindow.Left = secondScreenBounds.Left;
            //    _rearWindow.Top = secondScreenBounds.Top;
            //    _rearWindow.Loaded += (s, ev) =>
            //    {
            //        _rearWindow.Width = secondScreenBounds.Width;
            //        _rearWindow.Height = secondScreenBounds.Height;
            //        _rearWindow.WindowStyle = WindowStyle.None;
            //        _rearWindow.WindowState = WindowState.Maximized;
            //        _rearWindow.Background = new SolidColorBrush(Color.FromRgb(0, 0, 255));
            //    };
            //    _rearWindow.Show();

            //    _imageWindow.WindowStartupLocation = WindowStartupLocation.Manual;
            //    var thirdScreen = screens[0];
            //    var thirdScreenBounds = thirdScreen.Bounds;
            //    _imageWindow.Left = thirdScreenBounds.Left;
            //    _imageWindow.Top = thirdScreenBounds.Top;
            //    _imageWindow.Loaded += (s, ev) =>
            //    {
            //        _imageWindow.Width = thirdScreenBounds.Width;
            //        _imageWindow.Height = thirdScreenBounds.Height;
            //        _imageWindow.WindowStyle = WindowStyle.None;
            //        _imageWindow.WindowState = WindowState.Maximized;
            //        _imageWindow.Background = new SolidColorBrush(Color.FromRgb(255,0,0));
            //    };
            //    _imageWindow.Show();
            //}
            //else
            //{
            //    System.Windows.MessageBox.Show("Second screen not detected.");
            //}
        }
        public event PropertyChangedEventHandler PropertyChanged;
        void InitializeImageWindow(Screen[] screens)
        {
            var secondScreen = screens[0];
            var secondScreenBounds = secondScreen.Bounds;

            _imageWindow.WindowStartupLocation = WindowStartupLocation.Manual;

            // Subscribe to the Loaded event to reposition after the window is fully loaded
            _imageWindow.Loaded += (sender, args) =>
            {
                _imageWindow.Left = secondScreenBounds.Left;
                _imageWindow.Top = secondScreenBounds.Top;
                _imageWindow.Width = secondScreenBounds.Width;
                _imageWindow.Height = secondScreenBounds.Height;
                _imageWindow.WindowStyle = WindowStyle.None;
                _imageWindow.WindowState = WindowState.Maximized;
            };

            // Show the window (this will trigger the Loaded event)
            _imageWindow.Show();
        }

        void InitializeRearWindow(Screen[] screens)
        {
            var screens1 = System.Windows.Forms.Screen.AllScreens;

            // Check if there are more than one screen
            if (screens1.Length > 1)
            {
                // Get the secondary screen (assuming index 1 is the secondary screen)
                var secondScreen = screens1[1];
                var secondScreenBounds = secondScreen.Bounds;

                // Configure the rear window to be displayed on the secondary screen
                _rearWindow.WindowStartupLocation = WindowStartupLocation.Manual;
                _rearWindow.Left = secondScreenBounds.Left;
                _rearWindow.Top = secondScreenBounds.Top;
                _rearWindow.Width = secondScreenBounds.Width;
                _rearWindow.Height = secondScreenBounds.Height;
                _rearWindow.WindowStyle = WindowStyle.None;
                _rearWindow.WindowState = WindowState.Normal; // Use Normal to ensure proper positioning

                // Show the window
                _rearWindow.Show();
            }
            else
            {
                // Handle the case where there is no secondary screen
                Debug.WriteLine("No secondary screen found.");
                _rearWindow.WindowState = WindowState.Normal;
                _rearWindow.Left = 0;
                _rearWindow.Top = 0;
                _rearWindow.Width = 800; // Set a default size
                _rearWindow.Height = 600; // Set a default size
                _rearWindow.Show();
            }
        }


        // Update content without repositioning or resizing the window

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private void ShowWindowOnScreen(int screenIndex, Color bgColor)
        {
            var screens = Screen.AllScreens;
            if(screenIndex==1)
                _originalBackground = _imageWindow.Background;
            else
                _originalBackground = _rearWindow.Background;


            if (screenIndex < screens.Length)
            {

                Window window = new Window
                {
                    WindowStartupLocation = WindowStartupLocation.Manual,
                    Background = new SolidColorBrush(bgColor),
                    Width = 200, // Square width
                    Height = 200, // Square height
                    Content = new TextBlock
                    {
                        Text = $"Screen {screenIndex + 1}",
                        Foreground = new SolidColorBrush(Colors.White),
                        FontSize = 24,
                        HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        TextAlignment = TextAlignment.Center
                    }
                };
                window.Show();

                DispatcherTimer timer = new DispatcherTimer();
                timer.Interval = TimeSpan.FromSeconds(3);
                timer.Tick += (sender, args) =>
                {
                    window.Background = _originalBackground;
                    timer.Stop();
                };
                timer.Start();
            }
            else
            {
                System.Windows.MessageBox.Show($"Screen {screenIndex + 1} not detected.");
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {



            var screens = Screen.AllScreens;

            if (screens.Length == 2)
            {
                ShowWindowOnScreen(2, Colors.Blue);
            }
            else if (screens.Length == 3)
            {
                ShowWindowOnScreen(2, Colors.Blue);
                ShowWindowOnScreen(3, Colors.Green);
            }
            else
            {
                System.Windows.MessageBox.Show("Second screen not detected.");
            }
        }
        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            var selectedRadioButton = sender as System.Windows.Controls.RadioButton;
            if (selectedRadioButton != null)
            {
                string selectedOption = selectedRadioButton.Content.ToString();
                HandleSelection(selectedOption);
            }
        }

        private void HandleSelection(string option)
        {
            switch (option)
            {
                case "ESV":
                    ver = "ESV";
                    break;
                case "KJV":
                    ver = "KJV";
                    break;
                case "NIV":
                    ver = "NIV";
                    break;
                default:
                    // Default case if needed
                    break;
            }
        }
        private void BibleBooksComboBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            English_chapter = "";
            tamil_chapter = "";
            var comboBox = sender as System.Windows.Controls.ComboBox;
            if (comboBox != null && comboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                selectedChapter = BibleBooksComboBox.SelectedItem.ToString();
                IsChapterSelected = true;
                string AppPath = ConfigurationManager.AppSettings["AppPath"].ToString();
                string fullPath = "Tamil Bible";
                string directoryPath = Path.Combine(AppPath, fullPath);
                string EngPath = AppPath+"\\KJV";
                string ESVPath = AppPath+"\\ESV";
                string NIVPath = AppPath+"\\NIV";
                string content = selectedItem.Content.ToString();
                string[] parts = content.Split('-');
                string englishPart = "";
                if (parts.Length > 1)
                {
                    englishPart = parts[0].Trim();
                    English_chapter = parts[0].Trim();
                    tamil_chapter = parts[1].Trim();
                }
                englishPart = englishPart.Replace(" ", "");
               
                string filePath = Path.Combine(directoryPath, englishPart + ".json");
                string fileKJVPath = Path.Combine(EngPath, englishPart + ".json");
                string fileESVPath = Path.Combine(ESVPath, englishPart + ".json");
                string fileNIVPath = Path.Combine(NIVPath, englishPart + ".json");
                if (File.Exists(filePath))
                {
                    try
                    {
                        string jsonContent = File.ReadAllText(filePath);
                        string jsonKJVContent ;
                        string jsonESVContent ;
                        string jsonNIVContent ;
                        if (ESV.IsChecked==true)
                        {
                             jsonESVContent = File.ReadAllText(fileESVPath);
                            tamilBibleData = JsonConvert.DeserializeObject<Tamil_Bible>(jsonContent);
                            esv = JsonConvert.DeserializeObject<ESV>(jsonESVContent);
                        }
                        else if(KJV.IsChecked == true)
                        {
                             jsonKJVContent = File.ReadAllText(fileKJVPath);
                            tamilBibleData = JsonConvert.DeserializeObject<Tamil_Bible>(jsonContent);
                            kjv = JsonConvert.DeserializeObject<KJV>(jsonKJVContent);
                        }
                        else if (NIV.IsChecked == true)
                        {
                            jsonNIVContent = File.ReadAllText(fileKJVPath);
                            tamilBibleData = JsonConvert.DeserializeObject<Tamil_Bible>(jsonContent);
                            niv = JsonConvert.DeserializeObject<NIV>(jsonNIVContent);
                        }
                       
                        
                        List<string> chapterNumbers = new List<string>();
                        chapterNumbers = tamilBibleData.chapters
                        .Select(chapter => chapter.chapter)
                        .ToList();
                        Chapter_no.ItemsSource = chapterNumbers;
                    }
                    catch (Exception ex)
                    {

                    }
                }
                else
                {

                }
            }
        }
        private JObject jsonData;
        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            string AppPath = ConfigurationManager.AppSettings["AppPath"].ToString();
            var jsonFilePath = AppPath+"\\ESV.json"; 
            var jsonContent = File.ReadAllText(jsonFilePath);
            jsonData = JObject.Parse(jsonContent);
            string searchTerm = txt_search.Text.Trim();
            search_results.Text = SearchJson(jsonData, searchTerm);
            SearchPopup.IsOpen = !SearchPopup.IsOpen; 
        }
        private string SearchJson(JObject jsonObject, string searchTerm)
        {
            var results = new List<string>();

            foreach (var book in jsonObject)
            {
                foreach (var chapter in book.Value.Children<JObject>())
                {
                    foreach (var verse in chapter.Children<JProperty>())
                    {

                        string verseText = verse.Value.ToString();
                        if (verseText.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            results.Add($"{book.Key} {chapter.Path}:{verse.Name}: \"{verseText}\"");
                        }
                    }
                }
            }
            return results.Count > 0 ? string.Join("\n", results) : "No results found.";
        }
        private void VerseListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var screens = Screen.AllScreens;
            System.Windows.Controls.ListBox listBox = sender as System.Windows.Controls.ListBox;

            if (listBox != null)
            {
                var selectedItem = listBox.SelectedItem;

                if (selectedItem != null)
                {
                    var verse = selectedItem as VerseWithTranslation;
                    _verse = verse.VerseNumber;
                    if (verse != null)
                    {
                        if (screens.Length == 2)
                        {
                            if (!_rearWindow.IsVisible)
                            {
                                InitializeRearWindow(screens);
                            }
                            if (!_imageWindow.IsVisible)
                            {
                                InitializeImageWindow(screens);
                            }
                            int selectedIndex = listBox.SelectedIndex;
                            if (selectedItem != null)
                            {
                                _verse = verse.VerseNumber;
                            }
                            VerseWithTranslation nextVerse = null;
                            if (selectedIndex < listBox.Items.Count - 1)
                            {
                                nextVerse = listBox.Items[selectedIndex + 1] as VerseWithTranslation;
                            }
                            string nextTamilText = nextVerse != null ? nextVerse.TamilText : string.Empty;
                            string nextEnglishText = nextVerse != null ? nextVerse.EnglishText : string.Empty;

                            _rearWindow.SetVerseContent(verse.TamilText, verse.EnglishText, English_chapter, tamil_chapter, _chapter, _verse, nextTamilText, nextEnglishText);
                            string formattedDate = DateTime.Now.ToString("ddMMyyyy");
                            string filepath = LogfilePath + formattedDate + ".json";
                            SaveVerseDetails(verse.TamilText, verse.EnglishText, English_chapter, tamil_chapter, _chapter, _verse, filepath);
                            LoadRearWindowContent();
                            if (selectedItem != null)
                            {
                                _verse = verse.VerseNumber;
                            }
                            _imageWindow.SetBackgroundImage(bg, fg);
                            _imageWindow.SetVerseContent(verse.TamilText, verse.EnglishText, English_chapter, tamil_chapter, _chapter, _verse);
                            DisplayVerse(verse.TamilText, verse.EnglishText);
                        }
                        else if (screens.Length == 2)
                        {
                            if (!_imageWindow.IsVisible)
                            {
                                InitializeImageWindow(screens);
                            }
                            if (selectedItem != null)
                            {
                                _verse = verse.VerseNumber;
                            }
                            _imageWindow.SetBackgroundImage(bg, fg);
                            _imageWindow.SetVerseContent(verse.TamilText, verse.EnglishText, English_chapter, tamil_chapter, _chapter, _verse);
                            string formattedDate = DateTime.Now.ToString("ddMMyyyy");
                            string filepath = LogfilePath + formattedDate + ".json";
                            SaveVerseDetails(verse.TamilText, verse.EnglishText, English_chapter, tamil_chapter, _chapter, _verse, filepath);

                        }
                        else
                        {
                            System.Windows.MessageBox.Show("Second screen not detected.");
                        }
                    }
                }
            }
        }

        private void RecentView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var screens = Screen.AllScreens;
            System.Windows.Controls.ListBox listBox = sender as System.Windows.Controls.ListBox;

            if (listBox != null)
            {
                var selectedItem = listBox.SelectedItem;

                if (selectedItem != null)
                {
                    var verse = selectedItem as VerseDetail;
                    _verse = verse.Verse;
                    if (verse != null)
                    {
                        if (screens.Length == 2)
                        {
                            if (!_rearWindow.IsVisible)
                            {
                                InitializeRearWindow(screens);
                            }
                            if (!_imageWindow.IsVisible)
                            {
                                InitializeImageWindow(screens);
                            }


                            if (selectedItem != null)
                            {
                                _verse = verse.Verse;
                            }
                            int selectedIndex = listBox.SelectedIndex;
                            if (selectedItem != null)
                            {
                                _verse = verse.Verse;
                            }
                            VerseWithTranslation nextVerse = null;
                            if (selectedIndex < listBox.Items.Count - 1)
                            {
                                nextVerse = listBox.Items[selectedIndex + 1] as VerseWithTranslation;
                            }
                            string nextTamilText = nextVerse != null ? nextVerse.TamilText : string.Empty;
                            string nextEnglishText = nextVerse != null ? nextVerse.EnglishText : string.Empty;

                            _rearWindow.SetVerseContent(verse.TamilText, verse.EnglishText, English_chapter, tamil_chapter, _chapter, _verse, nextTamilText, nextEnglishText);
                            // Update the content directly
                            _imageWindow.SetBackgroundImage(bg, fg);
                            _imageWindow.SetVerseContent(verse.TamilText, verse.EnglishText, verse.EnglishChapter, verse.TamilChapter, verse.Chapter, _verse);
                            string formattedDate = DateTime.Now.ToString("ddMMyyyy");
                            string filepath = LogfilePath + formattedDate + ".json";
                            SaveVerseDetails(verse.TamilText, verse.EnglishText,verse.EnglishChapter ,verse.TamilChapter,verse.Chapter, _verse, filepath);
                           
                        }
                        else if (screens.Length == 2)
                        {
                            _rearWindow.WindowStartupLocation = WindowStartupLocation.Manual;
                            var secondScreen = screens[1];
                            var secondScreenBounds = secondScreen.Bounds;
                            _rearWindow.Left = secondScreenBounds.Left;
                            _rearWindow.Top = secondScreenBounds.Top;
                            _rearWindow.Loaded += (s, ev) =>
                            {
                                _rearWindow.Width = secondScreenBounds.Width;
                                _rearWindow.Height = secondScreenBounds.Height;
                                _rearWindow.WindowStyle = WindowStyle.None;
                                _rearWindow.WindowState = WindowState.Maximized;
                                _rearWindow.Background = new SolidColorBrush(Color.FromRgb(0, 0, 255));
                            };
                            _rearWindow.Show();

                            _imageWindow.WindowStartupLocation = WindowStartupLocation.Manual;
                            var thirdScreen = screens[0];
                            var thirdScreenBounds = thirdScreen.Bounds;
                            _imageWindow.Left = thirdScreenBounds.Left;
                            _imageWindow.Top = thirdScreenBounds.Top;
                            _imageWindow.Loaded += (s, ev) =>
                            {
                                _imageWindow.Width = thirdScreenBounds.Width;
                                _imageWindow.Height = thirdScreenBounds.Height;
                                _imageWindow.WindowStyle = WindowStyle.None;
                                _imageWindow.WindowState = WindowState.Maximized;
                                _imageWindow.Background = new SolidColorBrush(Color.FromRgb(255, 0, 0));
                            };
                            _imageWindow.Show();
                        }
                        else
                        {
                            System.Windows.MessageBox.Show("Second screen not detected.");
                        }
                    }
                }
            }
        }
        private void DisplayVerse(string tamil, string english)
        {
            string combinedText = $"{tamil}\n\n{english}";
            VerseTextBox.Text = combinedText;
        }

        public void SaveVerseDetails(string TamilText, string EnglishText, string English_chapter, string tamil_chapter, string _chapter, string _verse, string filePath)
        {
            string formattedDate = DateTime.Now.ToString("ddMMyyyy");
            var verseDetail = new VerseDetail
            {

                TamilText = TamilText,
                EnglishText = EnglishText,
                EnglishChapter = English_chapter,
                TamilChapter = tamil_chapter,
                Chapter = _chapter,
                Verse = _verse,
                Timestamp = formattedDate,
            };
            List<VerseDetail> verseDetails;
            if (File.Exists(filePath))
            {
                string jsonRString = File.ReadAllText(filePath);
                verseDetails = JsonConvert.DeserializeObject<List<VerseDetail>>(jsonRString) ?? new List<VerseDetail>();
            }
            else
            {
                verseDetails = new List<VerseDetail>();
            }
            verseDetails.Add(verseDetail);
            string jsonString = JsonConvert.SerializeObject(verseDetails, Formatting.Indented);
            File.WriteAllText(filePath, jsonString);
        }
        public class VerseDetail
        {
            public string EnglishText { get; set; }
            public string TamilText { get; set; }
            public string EnglishChapter { get; set; }
            public string TamilChapter { get; set; }
            public string Chapter { get; set; }
            public string Verse { get; set; }
            public string Timestamp { get; set; }

            public string FormattedText => $"{EnglishChapter} - {TamilChapter} {Chapter}:{Verse}";
        }
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.Filter = "Image files (*.jpg, *.jpeg, *.png)|*.jpg;*.jpeg;*.png";
            openFileDialog.Title = "Select an Image";
            bool? result = openFileDialog.ShowDialog();

            if (result == true)
            {
                bg = new BitmapImage();
                bg.BeginInit();
                bg.UriSource = new Uri(openFileDialog.FileName);
                bg.EndInit();
                UploadedImage.Source = bg;
            }
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            
            using (var colorDialog = new System.Windows.Forms.ColorDialog())
            {
                if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    System.Drawing.Color selectedColor = colorDialog.Color;
                    System.Windows.Media.Color wpfColor = System.Windows.Media.Color.FromArgb(
                        selectedColor.A, selectedColor.R, selectedColor.G, selectedColor.B);
                    fg = wpfColor;
                }
            }
        }

        private void LoadVerseDetails()
        {
            string formatdate = DateTime.Now.ToString("ddMMyyyy");
            string LoadfilePath = LogfilePath + formatdate + ".json"; // Update this to your file path

            List<VerseDetail> verseDetails;

            if (File.Exists(LoadfilePath))
            {
                string jsonString = File.ReadAllText(LoadfilePath);
                verseDetails = JsonConvert.DeserializeObject<List<VerseDetail>>(jsonString) ?? new List<VerseDetail>();
            }
            else
            {
                verseDetails = new List<VerseDetail>();
            }

            // Bind the list to the ListView
            RecentListView.ItemsSource = verseDetails;
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            _rearWindow.IncreaseFontSize();
        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            _rearWindow.DecreaseFontSize();
        }

    

        private void VerseTextBox_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {

        }
    }



    public class Tamil_Bible
    {
        public book book { get; set; }
        public string count { get; set; }
        public List<chapters> chapters { get; set; }
    }

    public class book
    {
        public string english { get; set; }
        public string tamil { get; set; }
    }

    public class chapters
    {
        public string chapter { get; set; }
        public List<verses> verses { get; set; }
    }

    public class verses
    {
        public string text { get; set; }
        public string verse { get; set; }
    }
    public class KJV
    {
        public string book { get; set; }
        public List<chapters> chapters { get; set; }
    }
    public class ESV
    {
        public string Book { get; set; }
        public List<chapters> Chapters { get; set; }
        public Info Info { get; set; }
    }
    public class NIV
    {
        public string Book { get; set; }
        public List<chapters> Chapters { get; set; }
        public Info Info { get; set; }
    }

    public class Info
    {
        public string Language { get; set; }
        public string Meaningless { get; set; }
        public DateTime Timestamp { get; set; }
        public string Translation { get; set; }
    }

}
