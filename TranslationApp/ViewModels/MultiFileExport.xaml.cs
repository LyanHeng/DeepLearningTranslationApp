using Google.Cloud.Translation.V2;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
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
using static TranslationApp.Classes.PdfSharpExtensions;

namespace TranslationApp.Views
{
    /// <summary>
    /// Interaction logic for MultiFileExport.xaml
    /// </summary>
    public partial class MultiFileExport : Window
    {
        private Dictionary<string, string> m_languagesKeys = new Dictionary<string, string>();
        private TranslationClient m_client = TranslationClient.CreateFromApiKey(Environment.GetEnvironmentVariable("api_key"));
        public Dictionary<string, string> LanguageKeys { get => m_languagesKeys; set => m_languagesKeys = value; }
        public TranslationClient Client { get => m_client; }
        public string _language = "English";
        public MultiFileExport(ListBox files, string language)
        {
            InitializeComponent();
            PopulateLanguage();
            _language = language;
            foreach (var item in files.Items)
            {
                popup.Items.Add(item);
            }
        }

        private void PopulateLanguage()
        {
            // get all supported language by Google
            // "en" - defines the language of all the names of the languages
            IList<Language> supportedLanguages = Client.ListLanguages("en");
            foreach (Language language in supportedLanguages)
            {
                if (!LanguageKeys.ContainsKey(language.Name))
                {
                    LanguageKeys.Add(language.Name, language.Code);
                }
            }
        }

        private void btnExportTxtFile_Click(object sender, RoutedEventArgs e)
        {
            if(popup.SelectedIndex == -1)
            {
                MessageBox.Show("Please select an Item first!");
            }
            else
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "Text Files(*.txt)|*.txt|All(*.*)|*";
                if (saveFileDialog.ShowDialog() == true)
                {
                    string ext = System.IO.Path.GetExtension(popup.SelectedItem.ToString());

                    if (ext == ".txt")
                    {
                        string temp = File.ReadAllText(popup.SelectedItem.ToString());
                        var response = Client.TranslateText(temp, LanguageKeys[_language]);
                        File.WriteAllText(saveFileDialog.FileName, response.TranslatedText);
                    }
                    else if (ext == ".pdf")
                    {
                        string pdfContents = GetText(popup.SelectedItem.ToString());
                        var response = Client.TranslateText(pdfContents, LanguageKeys[_language]);
                        File.WriteAllText(saveFileDialog.FileName, response.TranslatedText);
                    }
                }
            }
        }

        private void btnExportPDFFile_Click(object sender, RoutedEventArgs e)
        {
            if (popup.SelectedIndex == -1)
            {
                MessageBox.Show("Please select an Item first!");
            }
            else 
            {
                string ext = System.IO.Path.GetExtension(popup.SelectedItem.ToString());

                if (ext == ".txt")
                {
                    ListBoxItem file = (ListBoxItem)popup.ItemContainerGenerator.ContainerFromItem(popup.SelectedItem);
                    var response = Client.TranslateText(File.ReadAllText(file.Content.ToString()), LanguageKeys[_language]);
                    ExportPDF(response.TranslatedText);
                }
                else if (ext == ".pdf")
                {
                    string pdfContents = GetText(popup.SelectedItem.ToString());
                    var response = Client.TranslateText(pdfContents, LanguageKeys[_language]);
                    ExportPDF(response.TranslatedText);
                }
            }   
        }
    }
}
