using System;
using System.Windows;
using Google.Cloud.Translation.V2;

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
                translatedText.Text = "Google API only supports 5000 words limit per request.";
                return;
            }

            // retrieve API Key from Environment variable set up
            // TODO v0.5 - set up the key in a config file
            var client = TranslationClient.CreateFromApiKey(Environment.GetEnvironmentVariable("api_key"));
            // TODO v1.1 - have language be dynamic
            // Blocked - waiting on UI to be finished to retrieve from UI
            var response = client.TranslateText(textToTranslate.Text, LanguageCodes.French);

            translatedText.Text = response.TranslatedText;
        }
    }
}