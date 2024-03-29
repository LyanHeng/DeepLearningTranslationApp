﻿using System;
using System.IO;
using System.Windows;
using System.Threading;
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

        #region Methods
        // populate the combo boxes with check box functionality
        private void PopulateLanguageCbComboBoxes()
        {
            multiLangSelect.Items.Clear();
            // get all supported language by Google
            // "en" - defines the language of all the names of the languages

            foreach (string language in App.LanguageKeys.Keys)
            {
                CheckBox chkbox = new CheckBox();
                chkbox.Content = language;
                chkbox.Checked += new RoutedEventHandler(SelectionChecked);
                chkbox.Unchecked += new RoutedEventHandler(SelectionChecked);
                multiLangSelect.Items.Add(chkbox);
            }
            // default language to english
            multiLangSelect.SelectedItem = "English";
        }

        // translate a given text given 
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

        // translate a substring to handle the 5000 words limit
        public string SubStringTranslate(string substring)
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
            return newString + SubStringTranslate(substring);
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
                // check characters after symbols or punctuation OR check for no symbols present
                string regex = @"^[^\p{S}\u0003]+$|(?<=\p{S}|\p{P})\w{5,}";

                if (pdfContents == null)
                {
                    fileName.Items.Remove(filePath);
                    return;
                }
                else if (Regex.IsMatch(pdfContents, regex))
                {
                    textToTranslate.AppendText(pdfContents);
                    fileTranslationStatusBox.Visibility = Visibility.Hidden;
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

        private string TranslateWithSubString(string text, string targetLanguage)
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
            string translatedResult = Translate(text, targetLanguage);
            return translatedResult;
        }
        #endregion

        #region Handlers
        // open file dialog
        private void OpenFile_Click(object sender, RoutedEventArgs e)
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

        // add language to text box when language selection changes
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

        // perform one translation in one language in a separate thread (see reference)
        private static void TranslateInNewThread(string textToTranslate, string targetLanguage, string translatedFolder)
        {
            string result = "";
            try
            {
                var response = App.Client.TranslateText(textToTranslate, App.LanguageKeys[targetLanguage]);
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

            // create file
            string pathToFileToCreate = translatedFolder + @"\" + targetLanguage + ".txt";
            File.WriteAllText(pathToFileToCreate, result);
        }

        // Perform multithread translation by iterating through the languages
        private static void MultiThreadTranslation(string textToTranslate, string[] targetLanguages, string translatedFolder)
        {
            // iterate through each language
            foreach (string language in targetLanguages)
            {
                Thread thread = new Thread(new ThreadStart(() => TranslateInNewThread(textToTranslate, language, translatedFolder)));
                thread.Start();
            }
        }

        // translate into multiple languages
        private void TranslateToMultiLanguage(object sender, RoutedEventArgs e)
        {
            string[] targetLanguages = selectedLanguagesBox.Text.Split('\n');

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

            // translate multiple languages at the same time
            MultiThreadTranslation(textToTranslate.Text, targetLanguages, translatedFolder);

            // Notify the user that it has completed and where to find the files
            string absolutePath = new Uri(Directory.GetCurrentDirectory() + @"\..\..\..\" + folderName).AbsoluteUri;
            fileTranslationStatusBox.Text = "Files can be found at\n\n" + absolutePath;
            fileTranslationStatusBox.Text += "\n\nPlease wait for all files to be translated.";
            fileTranslationStatusBox.Visibility = Visibility.Visible;
        }

        // clear text box
        private void Clear(object sender, RoutedEventArgs e)
        {
            textToTranslate.Text = String.Empty;
            fileName.Items.Clear();
            fileTranslationStatusBox.Text = "";
            fileTranslationStatusBox.Visibility = Visibility.Hidden;
            ClearList(sender, e);
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

        // clear languages list and selection box
        private void ClearList(object sender, RoutedEventArgs e)
        {
            selectedLanguagesBox.Text = "";
            foreach (CheckBox chkbox in multiLangSelect.Items)
            {
                chkbox.IsChecked = false;
            }
        }
        #endregion
    }
}

