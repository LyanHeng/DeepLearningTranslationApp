using System;
using System.IO;
using System.Windows;
using System.Collections.Generic;
using Google.Cloud.Translation.V2;
using Microsoft.Win32;
using static TranslationApp.Classes.PdfSharpExtensions;

namespace TranslationApp
{
    public partial class MainWindow : Window
    {
        private Dictionary<string, string> m_languagesKeys = new Dictionary<string, string>();
        private TranslationClient m_client = TranslationClient.CreateFromApiKey(Environment.GetEnvironmentVariable("api_key"));
        public Dictionary<string, string> LanguageKeys { get => m_languagesKeys; set => m_languagesKeys = value; }
        public TranslationClient Client { get => m_client; }

        string FPATH = "";

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

        // translate provided text
        private void Translate(object sender, RoutedEventArgs e)
        {
            // guard to prevent API character limit
            if (textToTranslate.Text.Length >= 5000)
            {
                translatedText.Text = "Google API does not support translation above 5000 characters.";
                return;
            }

            try
            {
                var response = Client.TranslateText(textToTranslate.Text, LanguageKeys[box2.SelectedItem.ToString()]);
                translatedText.Text = response.TranslatedText;
            }
            // we typically do not want this to happen, handle as much failure cases as possible
            catch (Exception exc)
            {
                translatedText.Text = "Unexpected Error\n"
                                    + exc.Message;
            }

        }
        #endregion

        #region Handlers
        // open file dialog
        private void btnOpenFile_Click(object sender, RoutedEventArgs e)
        {
            // Create OpenFileDialog
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                string ext = Path.GetExtension(openFileDialog.FileName);
                if (ext == ".txt")
                {
                    textToTranslate.Text = File.ReadAllText(openFileDialog.FileName);
                }
                else if (ext == ".pdf")
                {
                    string pdfContents = GetText(openFileDialog.FileName);
                    textToTranslate.Text = pdfContents;
                    FPATH = openFileDialog.FileName;
                }
                else
                    textToTranslate.Text = "Current file format is not supported";
            }
        }

        private void btnExportTxtFile_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Text Files(*.txt)|*.txt|All(*.*)|*";
            if (saveFileDialog.ShowDialog() == true)
                File.WriteAllText(saveFileDialog.FileName, translatedText.Text);
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
            }
            else
            {
                //assume that there is a existing PDF for now
                if (FPATH != "")
                {
                    ExportPDF(FPATH, translatedText.Text);
                }
                else
                {
                    textToTranslate.Text = "Must be a original PDF for now";
                }

            }
        }
        #endregion
    }
}