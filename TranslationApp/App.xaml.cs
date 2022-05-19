using System;
using System.Collections.Generic;
using System.Windows;
using Google.Cloud.Translation.V2;

namespace TranslationApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static TranslationClient m_client = TranslationClient.CreateFromApiKey(Environment.GetEnvironmentVariable("api_key"));
        public static TranslationClient Client { get => m_client; }

        private static Dictionary<string, string> m_languagesKeys = GetLanguageLists();
        public static Dictionary<string, string> LanguageKeys { get => m_languagesKeys; set => m_languagesKeys = value; }
        
        private void Application_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            
        }

        // get language lists
        public static Dictionary<string, string> GetLanguageLists()
        {
            var list = new Dictionary<string, string>();
            foreach (Language language in Client.ListLanguages("en"))
            {
                if (!list.ContainsKey(language.Name))
                {
                    list.Add(language.Name, language.Code);
                }
            }
            return list;
        }
    }
}
