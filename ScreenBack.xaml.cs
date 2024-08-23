using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace grace
{
    /// <summary>
    /// Interaction logic for ScreenBack.xaml
    /// </summary>
    public partial class ScreenBack : Window
    {
        public ScreenBack()
        {
            InitializeComponent();

            Background = new SolidColorBrush(Colors.DarkGray);
            SolidColorBrush whiteBrush = new SolidColorBrush(Colors.Black);
            SolidColorBrush colorBrush = new SolidColorBrush(Colors.LightGoldenrodYellow);
            left_eng.Foreground = whiteBrush;
            right_tam.Foreground = whiteBrush;
            v1.Foreground = whiteBrush;
            v2.Foreground = colorBrush;
            var timer = new System.Timers.Timer(5000); // 5000 milliseconds = 5 seconds
            timer.Elapsed += (s, e) =>
            {
                // Ensure that this code runs on the UI thread
                Dispatcher.Invoke(() =>
                {
                    // Find and start the storyboard
                    var storyboard = (Storyboard)FindResource("HideTextBlockStoryboard");
                    storyboard.Begin();
                });
            };
            timer.Start();
        }
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            double proportionalFontSize = Math.Min(ActualWidth, ActualHeight) / 25;
            v1.FontSize = proportionalFontSize;
            v2.FontSize = proportionalFontSize;
        }
        private void ShowInMainWindow()
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.CopyRearWindowContent(this);
            mainWindow.Show();
        }
        public void IncreaseFontSize()
        {
            double currentFontSizeV1 = v1.FontSize;
            double currentFontSizeV2 = v2.FontSize;
            v1.FontSize = currentFontSizeV1 + 2;
            v2.FontSize = currentFontSizeV2 + 2;
        }

        public void DecreaseFontSize()
        {
            double currentFontSizeV1 = v1.FontSize;
            double currentFontSizeV2 = v2.FontSize;
            v1.FontSize = currentFontSizeV1 - 2;
            v2.FontSize = currentFontSizeV2 - 2;
        }
        public void SetVerseContent(string tamilText, string englishText, string Eng, string tam, string chap, string ver, string tamilNText, string englishNText)
        {
            int ver_2 = int.Parse(ver) + 1;
            v1.Inlines.Clear();
            v1.Inlines.Add(new Run(ver+"."+tamilText) { FontWeight = FontWeights.Bold });
            v1.Inlines.Add(new LineBreak());
            v1.Inlines.Add(new Run(englishText) { FontWeight = FontWeights.Bold });
            v2.Inlines.Clear();
            v2.Inlines.Add(new Run(ver_2 + "." + tamilNText) { FontWeight = FontWeights.Bold });
            v2.Inlines.Add(new LineBreak());
            v2.Inlines.Add(new Run(englishNText) { FontWeight = FontWeights.Bold });


            left_eng.Inlines.Clear();
            left_eng.Inlines.Add(new Run(Eng + " " + chap + ":" + ver) { FontWeight = FontWeights.Bold });
            right_tam.Inlines.Clear();
            right_tam.Inlines.Add(new Run(tam + " " + chap + ":" + ver) { FontWeight = FontWeights.Bold });
        }

        public void SetBackgroundImage()
        {
            Background = new SolidColorBrush(Colors.DarkGray);
            SolidColorBrush whiteBrush = new SolidColorBrush(Colors.Cyan);

            left_eng.Foreground = whiteBrush;
            right_tam.Foreground = whiteBrush;
            v1.Foreground = whiteBrush;
            v2.Foreground = whiteBrush;
        }
    }
}
