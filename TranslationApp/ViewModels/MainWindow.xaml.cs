using System;
using System.IO;
using System.Windows;
using Google.Cloud.Translation.V2;
using Microsoft.Win32;
using PdfSharp.Pdf;
using PdfSharp.Pdf.Content;
using PdfSharp.Pdf.Content.Objects;
using static TranslationApp.Classes.PdfSharpExtensions;

namespace TranslationApp
{
    public partial class MainWindow : Window
    {
        /*
         * Setting gloabl variables for the purpose of testing, should be removed in future 
         * 
         * */
        string FPATH = "";
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
        private void btnExportPDFFile_Click(object sender, RoutedEventArgs e)
        {
            if (translatedText.Text == "")
            {
                //add error handling 
            }
            else
            {
                //assume that there is a existing PDF for now 
                ExportPDF(FPATH, translatedText.Text);
            }
        }

        private void btnOpenFile_Click(object sender, RoutedEventArgs e)
        {
            // Create OpenFileDialog
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
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