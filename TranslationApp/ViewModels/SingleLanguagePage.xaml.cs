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
using TranslationApp.Views;

namespace TranslationApp
{
    /// <summary>
    /// Interaction logic for SingleLanguagePage.xaml
    /// </summary>
    public partial class SingleLanguagePage : Page
    {
        public SingleLanguagePage()
        {
            InitializeComponent();
            PopulateLanguageComboBoxes(); 
        }

        #region Methods
        // populate the check boxes for language selection
        private void PopulateLanguageComboBoxes()
        {
            box2.Items.Clear();
            // get all supported language by Google
            // "en" - defines the language of all the names of the languages
            foreach (string language in App.LanguageKeys.Keys)
            {
                box2.Items.Add(language);
            }
            // default language to english
            box2.SelectedItem = "English";
        }

        // translate a given text provided a given languages
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

        // translate a substring to handle the 5000 characters limit
        public string SubStringTranslate(string substring)
        {
            if (substring.Length < 5000)
            {
                var response = App.Client.TranslateText(substring, App.LanguageKeys[box2.SelectedItem.ToString()]);
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
                    var response = App.Client.TranslateText(substring.Substring(0, positionOfNewline), App.LanguageKeys[box2.SelectedItem.ToString()]);
                    newString = response.TranslatedText;
                    substring = substring.Substring(positionOfNewline, substring.Length - positionOfNewline);
                    break;
                }
            }
            return newString + SubStringTranslate(substring);
        }

        // read from a file with different file extension and print into text box
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
            }
            else if (ext == ".pdf")
            {
                string pdfContents = GetText(filePath);
                // check characters after symbols or punctuation OR check for no symbols present
                string regex = @"^[^\p{S}\u0003]+$|(?<=\p{S}|\p{P})\w{5,}";

                if (pdfContents == null)
                {
                    fileName.Items.Remove(filePath);
                    return;
                }                
                else if (Regex.IsMatch(pdfContents, regex))
                {
                    // line separation formatting
                    if (textToTranslate.Text.Length > 0) textToTranslate.AppendText("\n\n");
                    textToTranslate.AppendText(pdfContents);
                }
                else
                {
                    MessageBox.Show("Unable to read contents of pdf from " + filePath + ". Please try copying contents of pdf into a txt file");
                    fileName.Items.Remove(filePath);
                }
            }
            else
                textToTranslate.Text = "Current file format is not supported";
        }
        #endregion


        #region Handlers
        // translate provided text
        private void TranslateText(object sender, RoutedEventArgs e)
        {
            string translatedSubString = "";
            //positive lookbehind for grabbing the 20 characters after it.
            var regex = new Regex(@"((?<=(\.)*(\r\n)+).{20}|(?<=\.(\s)+)).{20}", RegexOptions.Compiled);

            string newString = textToTranslate.Text;
            // guard to prevent API character limit
            if (textToTranslate.Text.Length >= 5000)
            {
                /*translatedText.Text = "Google API does not support translation above 5000 characters.";
                return;*/

                List<string> matchList = new List<string>();

                foreach (Match match in regex.Matches(textToTranslate.Text))
                {
                    matchList.Add(match.Value);
                }
                for (int i = matchList.Count - 1; i > 0; i--)
                {
                    int positionOfNewline = textToTranslate.Text.LastIndexOf(matchList[i]);
                    if (positionOfNewline < 5000)
                    {
                        string partAfterNewline = textToTranslate.Text.Substring(positionOfNewline, textToTranslate.Text.Length - positionOfNewline);
                        translatedSubString = SubStringTranslate(partAfterNewline);
                        newString = textToTranslate.Text.Substring(0, positionOfNewline);
                        break;
                    }
                }
            }

            // translate
            string translatedResult = Translate(textToTranslate.Text, box2.SelectedItem.ToString());
            translatedText.Text = translatedResult;
        }

        // clear filename textbox
        private void Clear(object sender, RoutedEventArgs e)
        {
            textToTranslate.Text = String.Empty;
            fileName.Items.Clear();
        }

        // open file dialog
        private void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            // Create OpenFileDialog
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "All files (*.*)|*.*";
            openFileDialog.Multiselect = true;
            if (openFileDialog.ShowDialog() == true)
            {
                foreach (string filePath in openFileDialog.FileNames)
                {
                    // in case file is already added - skip loading the file
                    if (fileName.Items.Contains(filePath)) continue;

                    fileName.Items.Add(Path.GetFullPath(filePath));

                    ReadFromFile(filePath);
                }
            }
        }

        // delete selected file
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

        // handler to export to txt file
        private void ExportTxtFile_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Text Files(*.txt)|*.txt|All(*.*)|*";
            if (saveFileDialog.ShowDialog() == true)
                File.WriteAllText(saveFileDialog.FileName, translatedText.Text);
        }

        // handler to export to multiple files
        private void ExportMultiFile_Click(object sender, RoutedEventArgs e)
        {
            //open popup window
            if (fileName.Items.Count != 0)
            {
                MultiFileExport window = new MultiFileExport(fileName, box2.SelectedItem.ToString());
                window.ShowDialog();
            }
            else
                MessageBox.Show("Please add files first!");
        }

        // triggers application light mode
        private void ExportPDFFile_Click(object sender, RoutedEventArgs e)
        {
            if (translatedText.Text == "")
            {
                //add error handling
                MessageBox.Show("Must have text to translate & export first");
            }
            else
            {
                ExportPDF(translatedText.Text, box2.SelectedItem.ToString());
            }
        }

        // triggers application light mode
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

        // triggers single page
        private void SingleLangButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService nav = NavigationService.GetNavigationService(this);
            nav.Navigate(new Uri("./Views/SingleLanguagePage.xaml", UriKind.RelativeOrAbsolute));
        }

        // triggers multi page
        private void MultiLangButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService nav = NavigationService.GetNavigationService(this);
            nav.Navigate(new Uri("./Views/MultiLanguagesPage.xaml", UriKind.RelativeOrAbsolute));
        }
        #endregion
    }
}

