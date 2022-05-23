# DeepLearningTranslationApp

## About the Project

---

The main aim of the project is to provide a translation application that builds on top of Google Cloud Translation API. The application provides an interface for direct text translation between various numbers of supported languages. The application also provide an interface to perform text file and PDF file translation. Furthermore, the application provides additional features including but not limited to: translating multiple files at once and translating a file into multiple languages.

![alt text](/TranslationApp/Images/FirstPage.PNG)
![alt text](/TranslationApp/Images/SecondPage.PNG)

---

The project will be used for the following objectives:
- Provide an interface to perform simple but effective translations
- Provide an interface to perform file translations
- Provide an interface to translate to multiple languages
- Provide a medium to perform verification testings of the chosen API.

---

The project is built with the help of the WPF Framework using C# .NET Framework 4.8. The project follows a semi-MVVM structural architectural style and is mainly an event-driven architectural application.

![alt text](/TranslationApp/Images/Architecture.PNG)

---

---

## Project Setup

---

To build and use the project:

1. Download and setup **Visual Studio 2019** with **.NET framework 4.8**.
2. Make a clone of this repository.
3. Load the project by running the csproj file on Visual Studio.
4. Navigate to the NUGET installer and install **Google.Cloud.Translation.V2** package.
5. Install **PDFsharp-MigraDoc-gdi** package.
6. Once done, click **Build/Run**. 
    - **Build** will created a bin folder that contains either Debug or Release folder. This can be changed in Visual Studio as well.
    - **Run** will build and run the executable created.

---

To deploy and access the built project:

1. Configure project to Release build.
2. Build project.
3. Deployed Application should be accessible in "Translation\bin\Release\net48\TranslationApp.exe"

---

---

## Application Features

---

1. Translate given text in the text box into any of the Google's supported languages.
2. Translate given txt or pdf (limited) file with the file button.
3. Export translated text into txt or pdf (limited) file.
4. Translate given file into multiple languages in MultiLanguage Page.
5. Translate multiple files in a batch.

---

---

## Dependencies

---

- Google Cloud Translation API (Google.Cloud.Translation.V2 on NUGET)
- Google Cloud Account & API keys
- PDFSharp & MigraDocs
