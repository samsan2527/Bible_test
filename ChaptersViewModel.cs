using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace grace
{
    public class ChaptersViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<string> chapters;
        private string selectedChapter;

        public ObservableCollection<string> Chapters
        {
            get => chapters;
            set
            {
                chapters = value;
                OnPropertyChanged(nameof(Chapters));
            }
        }

        public string SelectedChapter
        {
            get => selectedChapter;
            set
            {
                selectedChapter = value;
                OnPropertyChanged(nameof(SelectedChapter));
            }
        }

        public ChaptersViewModel()
        {
            LoadChapters();
        }

        private void LoadChapters()
        {
            string directoryPath = @"G:\TEST PROJECTS\grace\Tamil Bible";
            string[] jsonFiles = Directory.GetFiles(directoryPath, "*.json");

            Chapters = new ObservableCollection<string>(
                jsonFiles
                .Select(file => Path.GetFileNameWithoutExtension(file))
                .Select(ToCamelCase)
            );
        }

        private string ToCamelCase(string input)
        {
            string input_ = input.Trim();
            if (string.IsNullOrEmpty(input_))
                return input;

            return char.ToUpper(input_[0]) + input_.Substring(1);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
