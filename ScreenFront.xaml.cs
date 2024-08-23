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
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace grace
{
    /// <summary>
    /// Interaction logic for ScreenFront.xaml
    /// </summary>
    public partial class ScreenFront : Window
    {
        public ScreenFront()
        {
            InitializeComponent();
            SizeChanged += (s, e) => AdjustFontSize();
        }

        public void SetVerseContent(string tamilText, string englishText, string Eng, string tam, string chap, string ver)
        {
            VerseTextBlock.Inlines.Clear();
            VerseTextBlock.Inlines.Add(new Run(tamilText) { FontWeight = FontWeights.Bold });
            VerseTextBlock.Inlines.Add(new LineBreak());
            VerseTextBlock.Inlines.Add(new LineBreak());
            VerseTextBlock.Inlines.Add(new Run(englishText) { FontWeight = FontWeights.Bold });

            
            AdjustFontSize();
            left_eng.Inlines.Clear();
            left_eng.Inlines.Add(new Run(Eng + " " + chap + ":" + ver) { FontWeight = FontWeights.Bold });
            right_tam.Inlines.Clear();
            right_tam.Inlines.Add(new Run(tam + " " + chap + ":" + ver) { FontWeight = FontWeights.Bold });
        }
        private void AdjustFontSize()
        {
            if (ActualHeight > 0)
            {
                double proportionalFontSize = ActualHeight / 20; 
                VerseTextBlock.FontSize = proportionalFontSize;
            }
        }
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            double newFontSize = Math.Min(Width, Height) / 20; 
            //VerseTextBlock.FontSize = newFontSize;
        }
        public void SetBackgroundImage(ImageSource imageSource,Color foregroundColor)
        {
            ImageBrush imageBrush = new ImageBrush
            {
                ImageSource = imageSource,
                Stretch = Stretch.UniformToFill
            };
            Background = imageBrush;
            VerseTextBlock.Foreground = new SolidColorBrush(foregroundColor);
            left_eng.Foreground = new SolidColorBrush(foregroundColor);
            right_tam.Foreground = new SolidColorBrush(foregroundColor);
        }

        
    }
}
