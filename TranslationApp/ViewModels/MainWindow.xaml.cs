using System;
using System.IO;
using System.Windows;
using Google.Cloud.Translation.V2;
using Microsoft.Win32;

namespace TranslationApp
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Translate(object sender, RoutedEventArgs e)
        {
            if (textToTranslate.Text.Length >= 5000)
            {
                translatedText.Text = "Google API does not support translation above 5000 characters.";
                return;
            }
            
            // retrieve API Key from Environment variable set up
            // TODO v0.5 - set up the key in a config file
            var client = TranslationClient.CreateFromApiKey(Environment.GetEnvironmentVariable("api_key"));
            var response = client.TranslateText(textToTranslate.Text, LanguageCodes.French);

            translatedText.Text = response.TranslatedText;
        }
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
    }
}