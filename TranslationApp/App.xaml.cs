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
        private static Dictionary<string, string> m_languagesKeys = new Dictionary<string, string>();
        private static TranslationClient m_client = TranslationClient.CreateFromApiKey(Environment.GetEnvironmentVariable("api_key"));
        public static Dictionary<string, string> LanguageKeys { get => m_languagesKeys; set => m_languagesKeys = value; }
        public static TranslationClient Client { get => m_client; }
        private void Application_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {

        }
    }
}
