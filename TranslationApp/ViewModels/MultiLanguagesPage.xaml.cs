using System;
using System.IO;
using System.Windows;
using System.Collections.Generic;
using Google.Cloud.Translation.V2;
using Microsoft.Win32;
using static TranslationApp.Classes.PdfSharpExtensions;
using System.Windows.Controls;
using System.Text.RegularExpressions;
using System.Windows.Navigation;

namespace TranslationApp
{
    /// <summary>
    /// Interaction logic for MultiLanguagesPage.xaml
    /// </summary>
    public partial class MultiLanguagesPage : Page
    {
        public MultiLanguagesPage()
        {
            InitializeComponent();
            PopulateLanguageCbComboBoxes();
        }

        // populate the combo boxes with check box functionality
        private void PopulateLanguageCbComboBoxes()
        {
            multiLangSelect.Items.Clear();
            // get all supported language by Google
            // "en" - defines the language of all the names of the languages
            IList<Language> supportedLanguages = App.Client.ListLanguages("en");

            foreach (Language language in supportedLanguages)
            {
                if (!App.LanguageKeys.ContainsKey(language.Name))
                {
                    App.LanguageKeys.Add(language.Name, language.Code);
                }
                CheckBox chkbox = new CheckBox();
                chkbox.Content = language.Name;
                chkbox.Checked += new RoutedEventHandler(SelectionChecked);
                chkbox.Unchecked += new RoutedEventHandler(SelectionChecked);
                multiLangSelect.Items.Add(chkbox);              
            }
            // default language to english
            multiLangSelect.SelectedItem = "English";
        }

        private string Translate(string text, string targetLanguage)
        {
            string result = "";
            try
            {
                var response = App.Client.TranslateText(text, App.LanguageKeys[targetLanguage]);
                if (response.TranslatedText != null)
                {
                    result = response.TranslatedText;
                }
                else
                {
                    result = "There exists issues with translation.";
                }
            }
            // we typically do not want this to happen, handle as much failure cases as possible
            catch (Exception exc)
            {
                result = "Unexpected Error\n" + exc.Message;
            }
            return result;
        }

        public string subStringTranslate(string substring)
        {
            if (substring.Length < 5000)
            {
                var response = App.Client.TranslateText(substring, App.LanguageKeys[multiLangSelect.SelectedItem.ToString()]);
                return response.TranslatedText;
            }

            var regex = new Regex(@"((?<=(\.)*(\r\n)+).{20}|(?<=\.(\s)+)).{20}", RegexOptions.Compiled);
            string newString = "";

            List<string> matchList = new List<string>();

            foreach (Match match in regex.Matches(substring))
            {
                matchList.Add(match.Value);
            }

            for (int i = matchList.Count - 1; i > 0; i--)
            {
                int positionOfNewline = substring.LastIndexOf(matchList[i]);
                if (positionOfNewline < 5000)
                {
                    var response = App.Client.TranslateText(substring.Substring(0, positionOfNewline), App.LanguageKeys[multiLangSelect.SelectedItem.ToString()]);
                    newString = response.TranslatedText;
                    substring = substring.Substring(positionOfNewline, substring.Length - positionOfNewline);
                    break;
                }
            }
            return newString + subStringTranslate(substring);
        }

        // translate into multiple languages
        private void TranslateToMultiLanguage(object sender, RoutedEventArgs e)
        {
            string[] targetLanguages = selectedLanguagesBox.Text.Split('\n');

            // temporary storage of translated text in different languages
            List<string> filePaths = new List<string>();

            // folder handling
            string folderName = "translated";
            string translatedFolder = @"..\..\..\" + folderName;
            string targetDirectory = Directory.GetCurrentDirectory() + @"\" + translatedFolder;

            // reset directory
            DirectoryInfo di = new DirectoryInfo(targetDirectory);
            if (Directory.Exists(targetDirectory))
            {
                foreach (FileInfo file in di.GetFiles())
                {
                    file.Delete();
                }
                Directory.Delete(targetDirectory);
            }
            Directory.CreateDirectory(translatedFolder);

            // iterate through each language
            foreach (string language in targetLanguages)
            {
                string translatedResult = Translate(textToTranslate.Text, language);

                // create file
                string pathToFileToCreate = translatedFolder + @"\" + language + ".txt";
                filePaths.Add(pathToFileToCreate);
                File.WriteAllText(pathToFileToCreate, translatedResult);
            }

            // Notify the user that it has completed and where to find the files
            string absolutePath = new Uri(Directory.GetCurrentDirectory() + @"\..\..\..\" + folderName).AbsoluteUri;
            fileTranslationStatusBox.Text = "Files can be found at\n\n" + absolutePath;
            fileTranslationStatusBox.Visibility = Visibility.Visible;
        }

        private void Clear(object sender, RoutedEventArgs e)
        {
            textToTranslate.Text = String.Empty;
            fileName.Items.Clear();
        }

        private void SingleLangButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService nav = NavigationService.GetNavigationService(this);
            nav.Navigate(new Uri("./Views/SingleLanguagePage.xaml", UriKind.RelativeOrAbsolute));
        }

        private void MultiLangButton_Click(object sender, RoutedEventArgs e)
        {
       
            NavigationService nav = NavigationService.GetNavigationService(this);
            nav.Navigate(new Uri("./Views/MultiLanguagesPage.xaml", UriKind.RelativeOrAbsolute));
        }

        private void ClearList(object sender, RoutedEventArgs e)
        {
            selectedLanguagesBox.Text = "";
            foreach (CheckBox chkbox in multiLangSelect.Items)
            {
                chkbox.IsChecked = false;
            }
        }

        // Read from a file with different file extension and print into text box
        private void ReadFromFile(string filePath)
        {
            //get the current file then read it
            string ext = Path.GetExtension(filePath);

            // appends the files text to its current contents
            if (ext == ".txt")
            {
                //textToTranslate.Text = File.ReadAllText(openFileDialog.FileName);
                string temp = File.ReadAllText(filePath);
                textToTranslate.AppendText(temp);
                fileTranslationStatusBox.Visibility = Visibility.Hidden;
            }
            else if (ext == ".pdf")
            {
                string pdfContents = GetText(filePath);
                if (pdfContents == null)
                {
                    fileName.Items.Remove(filePath);
                    return;
                }
                else
                {
                    textToTranslate.AppendText(pdfContents);
                    fileTranslationStatusBox.Visibility = Visibility.Hidden;
                }
            }
            else
                textToTranslate.Text = "Current file format is not supported";
        }

        #region Handlers
        // open file dialog
        private void btnOpenFile_Click(object sender, RoutedEventArgs e)
        {
            // Create OpenFileDialog
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "All files (*.*)|*.*";
            openFileDialog.Multiselect = true;
            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileNames[0];
                // in case file is already added - skip loading the file
                if (fileName.Items.Contains(filePath)) return;
                // this feature should only allow one file to be loaded
                if (fileName.Items.Count > 0)
                {
                    fileName.Items.Clear();
                    textToTranslate.Text = "";
                }

                fileName.Items.Add(Path.GetFullPath(filePath));

                ReadFromFile(filePath);
            }
        }

        private void DelItem_Click(object sender, RoutedEventArgs e)
        {
            if (fileName.SelectedItem != null)
            {
                fileName.Items.Remove(fileName.SelectedItem);
            }
            textToTranslate.Text = String.Empty;
            for (int i = 0; i < fileName.Items.Count; i++)
            {
                //get the current file then read it
                ListBoxItem file = (ListBoxItem)fileName.ItemContainerGenerator.ContainerFromIndex(i);
                ReadFromFile(file.Content.ToString());
            }
        }

        private void LightModeChecked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.TranslationApp = "Light";

            //and to save the settings
            Properties.Settings.Default.Save();
        }

        // triggers application dark mode
        private void DarkModeChecked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.TranslationApp = "Dark";

            //and to save the settings
            Properties.Settings.Default.Save();
        }

        private void SelectionChecked(object sender, RoutedEventArgs e)
        {
            selectedLanguagesBox.Text = "";
            foreach (CheckBox chkbox in multiLangSelect.Items)
            {
                if (chkbox.IsChecked == true)
                {
                    if (selectedLanguagesBox.Text.Length != 0)
                        selectedLanguagesBox.Text += "\n";
                    selectedLanguagesBox.Text += chkbox.Content;
                }
            }
        }
        #endregion
    }
}

