using System;
using System.IO;
using System.Windows;
using System.Collections.Generic;
using Google.Cloud.Translation.V2;
using Microsoft.Win32;
using static TranslationApp.Classes.PdfSharpExtensions;
using System.Windows.Controls;
using System.Text.RegularExpressions;
using TranslationApp.Views;

namespace TranslationApp
{
    public partial class MainWindow : Window
    {
        // fields for text translation
        private Dictionary<string, string> m_languagesKeys = new Dictionary<string, string>();
        private TranslationClient m_client = TranslationClient.CreateFromApiKey(Environment.GetEnvironmentVariable("api_key"));

        // properties
        public Dictionary<string, string> LanguageKeys { get => m_languagesKeys; set => m_languagesKeys = value; }
        public TranslationClient Client { get => m_client; }

        public MainWindow()
        {
            InitializeComponent();
            PopulateLanguageComboBoxes();
        }

        #region Methods
        // retrieve all supported languages from Google Service
        private void PopulateLanguageComboBoxes()
        {
            box2.Items.Clear();
            // get all supported language by Google
            // "en" - defines the language of all the names of the languages
            IList<Language> supportedLanguages = Client.ListLanguages("en");
            foreach (Language language in supportedLanguages)
            {
                if (!LanguageKeys.ContainsKey(language.Name))
                {
                    LanguageKeys.Add(language.Name, language.Code);
                    box2.Items.Add(language.Name);
                }
            }
            // default language to english
            box2.SelectedItem = "English";
        }

        private string Translate(string text, string targetLanguage)
        {
            string result = "";
            try
            {
                var response = Client.TranslateText(text, LanguageKeys[targetLanguage]);
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
                var response = Client.TranslateText(substring, LanguageKeys[box2.SelectedItem.ToString()]);
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
                    var response = Client.TranslateText(substring.Substring(0, positionOfNewline), LanguageKeys[box2.SelectedItem.ToString()]);
                    newString = response.TranslatedText;
                    substring = substring.Substring(positionOfNewline, substring.Length - positionOfNewline);
                    break;
                }
            }
            return newString + subStringTranslate(substring);
        }

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
                        translatedSubString = subStringTranslate(partAfterNewline);
                        newString = textToTranslate.Text.Substring(0, positionOfNewline);
                        break;
                    }
                }
            }

            // translate
            string translatedResult = Translate(textToTranslate.Text, box2.SelectedItem.ToString());
            translatedText.Text = translatedResult;
        }

        // translate into multiple languages
        private void TranslateToMultiLanguage(object sender, RoutedEventArgs e)
        {
            // temporary string array to store multiple arrays
            // once UI is provided, this will retrieve this from the UI

            string[] targetLanguages = { "Khmer", "French", "Chinese (Simplified)" };

            // temporary storage of translated text in different languages
            List<string> filePaths = new List<string>();

            // folder handling
            string translatedFolder = "translated";
            string pathHeaders = translatedFolder + @"\";
            Directory.CreateDirectory(translatedFolder);

            // iterate through each language
            foreach (string language in targetLanguages)
            {
                string translatedResult = Translate(textToTranslate.Text, language);

                // create file
                string pathToFileToCreate = pathHeaders + language + ".txt";
                filePaths.Add(pathToFileToCreate);
                File.WriteAllText(pathToFileToCreate, translatedResult);
            }

            // TODO: Notify the user that it has completed and where to find the files
        }

        private void Clear(object sender, RoutedEventArgs e)
        {
            textToTranslate.Text = String.Empty;
            fileName.Items.Clear();
        }


        private void SingleLangButton_Click(object sender, RoutedEventArgs e)
        {
            SingleLanguagePage newPage = new SingleLanguagePage();
            this.Content = newPage;
        }

        private void MultiLangButton_Click(object sender, RoutedEventArgs e)
        {
            MultiLanguagesPage newPage = new MultiLanguagesPage();
            this.Content = newPage;
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
                }
            }
            else
                textToTranslate.Text = "Current file format is not supported";
        }
        #endregion

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
                foreach (string filePath in openFileDialog.FileNames)
                {
                    // in case file is already added - skip loading the file
                    if (fileName.Items.Contains(filePath)) continue;

                    fileName.Items.Add(Path.GetFullPath(filePath));

                    ReadFromFile(filePath);
                }
            }
        }

        private void DelItem_Click(object sender, RoutedEventArgs e)
        {
            if(fileName.SelectedItem != null)
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

        private void btnExportTxtFile_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Text Files(*.txt)|*.txt|All(*.*)|*";
            if (saveFileDialog.ShowDialog() == true)
                File.WriteAllText(saveFileDialog.FileName, translatedText.Text);

        }

        private void btnExportMultiFile_Click(object sender, RoutedEventArgs e)
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
        private void btnExportPDFFile_Click(object sender, RoutedEventArgs e)
        {
            if (translatedText.Text == "")
            {
                //add error handling
                textToTranslate.Text = "Must have text to translate & export first";
            }
            else
            {
                ExportPDF(translatedText.Text);
            }
        }
        #endregion
    }
}
