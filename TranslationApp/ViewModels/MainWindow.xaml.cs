using System;
using System.IO;
using System.Windows;
using System.Collections.Generic;
using Google.Cloud.Translation.V2;
using Microsoft.Win32;

namespace TranslationApp
{
    public partial class MainWindow : Window
    {
        private Dictionary<string, string> m_languagesKeys = new Dictionary<string, string>();
        private TranslationClient m_client = TranslationClient.CreateFromApiKey(Environment.GetEnvironmentVariable("api_key"));

        public Dictionary<string, string> LanguageKeys { get => m_languagesKeys; set => m_languagesKeys = value; }
        public TranslationClient Client { get => m_client; }

        public MainWindow()
        {
            InitializeComponent();
            PopulateLanguageComboBoxes();
        }

        // retrieve all supported languages from Google Service
        private void PopulateLanguageComboBoxes()
        {
            // add default auto-detection
            box1.Items.Add("Auto");

            // get all supported language by Google
            // "en" - defines the language of all the names of the languages
            IList<Language> supportedLanguages = Client.ListLanguages("en");
            foreach (Language language in supportedLanguages)
            {
                if (!LanguageKeys.ContainsKey(language.Name))
                {
                    LanguageKeys.Add(language.Name, language.Code);
                    box1.Items.Add(language.Name);
                    box2.Items.Add(language.Name);
                }
            }
        }

        // API call using Google Cloud Library
        private void Translate(object sender, RoutedEventArgs e)
        {
            // clear past messages
            errorMessage.Text = "";
            translatedText.Text = "";
            errorMessage.Visibility = Visibility.Hidden;
            // check text length
            string text = textToTranslate.Text;
            if (text.Length >= 5000)
            {
                errorMessage.Text = "Google API does not support translation above 5000 characters.";
                return;
            }
            // retrieve API Key from Environment variable set up
            Detection languageDetected = box1.SelectedItem.ToString() != "Auto" ? Client.DetectLanguage(text) : null;
            // translate
            try
            {
                // check language is the same as requested language
                if (languageDetected == null)
                {
                    translatedText.Text = Client.TranslateText(text, LanguageKeys[box2.SelectedItem.ToString()]).TranslatedText;
                }
                else if (languageDetected.Language == LanguageKeys[box1.SelectedItem.ToString()])
                {
                    translatedText.Text = Client.TranslateText(text, LanguageKeys[box2.SelectedItem.ToString()], LanguageKeys[box1.SelectedItem.ToString()]).TranslatedText;
                }
                else
                {
                    errorMessage.Visibility = Visibility.Visible;
                    errorMessage.Text = "Language Defined Did not Match";
                }
            }
            catch (Exception exc)
            {
                errorMessage.Visibility = Visibility.Visible;
                errorMessage.Text = "Unexpected Error: " + exc.ToString();
            }
        }

        #region Handlers
        private void btnOpenFile_Click(object sender, RoutedEventArgs e)
        {
            // Create OpenFileDialog
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
                textToTranslate.Text = File.ReadAllText(openFileDialog.FileName);

        }
        private void LightModeChecked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.TranslationApp = "Light";

            //and to save the settings
            Properties.Settings.Default.Save();
        }

        private void DarkModeChecked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.TranslationApp = "Dark";

            //and to save the settings
            Properties.Settings.Default.Save();
        }
        #endregion
    }
}