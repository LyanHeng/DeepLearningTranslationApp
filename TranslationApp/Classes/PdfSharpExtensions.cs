using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfSharp.Pdf.Content;
using PdfSharp.Pdf.Content.Objects;
using PdfSharp.Pdf.IO;
using System.Text.RegularExpressions;
using PdfSharp.Drawing.Layout;
using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering;
using System.IO;

namespace TranslationApp.Classes
{
    public static class PdfSharpExtensions
    {
        /*PDF sharp default library is not able to read text from an existing PDF
         * this extension allows us to read the doc, courtesy of stackoverflow + various hours of research &testing
         * Than other additional code that allows us to handle creating the translated PDFS
         */
        public static string Formatting(string text)
        {
            //since the text in PDFS don't contain things like linebreaks
            //We need to somewhat create some formatting 
            //Imperfect solution to this problem but makes it readable for users
            List<string> matchList = new List<string>();
            List<int> matchIndex = new List<int>();
            var regex = new Regex(@"(?<!\w\.\w.)(?<![A-Z][a-z]\.)(?<=\.|\?)\s", RegexOptions.Compiled);
            foreach (Match match in regex.Matches(text))
            {
                matchIndex.Add(match.Index);
            }
            for (int i = matchIndex.Count -1; i > 0; i--)
            {
                int position = matchIndex[i] ;
                text =text.Insert(position, "\n");
                text = text.Remove(position + 1, 1);
  
            }
            return text;
        }
        public static Document CreateDocument(string toPrint)
        {
            //create our Migradoc
            Document mydoc = new Document();
            Section section = mydoc.AddSection();
    
            Paragraph header = section.AddParagraph();
            //Mock wordewise plug :)
            header.AddFormattedText("Document translated by wordwise ", TextFormat.Italic);
            header.AddLineBreak();
            //We add our text here
            Paragraph paragraph = section.AddParagraph();
            paragraph.AddFormattedText(toPrint);
            
            paragraph.Format.Font.Color = Color.FromCmyk(100, 30, 20, 50);

            return mydoc;
        }

        public static void AssemblePages(ref PdfDocument document, Document migraDocument )
        {
            var pdfRenderer = new DocumentRenderer(migraDocument);
            pdfRenderer.PrepareDocument();
            int pages = pdfRenderer.FormattedDocument.PageCount;
            for (int i = 1; i <= pages; ++i)
            {
                var page = document.AddPage();

                PageInfo pageInfo = pdfRenderer.FormattedDocument.GetPageInfo(i);
                page.Width = pageInfo.Width;
                page.Height = pageInfo.Height;
                page.Orientation = pageInfo.Orientation;

                using (XGraphics gfx = XGraphics.FromPdfPage(page))
                {
                    // HACK²
                    gfx.MUH = PdfFontEncoding.Unicode;
                    pdfRenderer.RenderPage(gfx, i);
                }
            }
        }

        public static void ExportPDF(string text, string lang)
        {

            //create our PDF document 
            PdfDocument document = new PdfDocument();
            //format our string to a satisfactory degree
            string formatted = Formatting(text);
            //create a migra doc as it is able to handle multupage formatting much better
            Document migraDocument = CreateDocument(formatted);

            //this allows us to handle cases where text doesnt fit on all pages
            AssemblePages(ref document,migraDocument);
            //document info 
            document.Info.Title = "Translated into" + lang ;

            //In future improve so that it allows the users to name and chose where to save document 
            // folder handling
            string folderName = "translated";
            string translatedFolder = @"..\..\..\" + folderName;
            string targetDirectory = Directory.GetCurrentDirectory() + @"\" + translatedFolder;
            //really basic but does the job
            string filename = lang + text.Substring(0, 10)+".pdf";
            string savePath = Path.Combine(targetDirectory, filename);
            // reset directory
            DirectoryInfo di = new DirectoryInfo(targetDirectory);
            if (Directory.Exists(targetDirectory))
            {
                foreach (FileInfo file in di.GetFiles())
                {
                    if (file.Name == filename)
                    {
                        //this should prevent app crashing when opening a existing document
                        //OS prevents us from deleting file if its open 
                        file.Delete();
                    }
                    
                }
            }else
            Directory.CreateDirectory(translatedFolder);
            //Improve for user friendliness
            try
            {
                document.Save(savePath);
                //start directory instead of file to show user where file is without crashing app 
                Process.Start(targetDirectory);

            }catch (Exception ex)
            {
                Console.WriteLine(ex.ToString()); 
            }           

        }


        public static string GetText(string pdfFileName)
        {
            // this could fail in many cases. There is no perfect way in handling this.
            PdfDocument OriginPDF = null;
            try
            {
                OriginPDF = PdfReader.Open(pdfFileName, PdfDocumentOpenMode.ReadOnly);
            }
            catch (Exception exc)
            {
                MessageBox.Show("Given PDF File not supported/invalid\n" + exc.Message);
                return null;
            }
            string sentence = "";
 
            foreach (PdfPage page in OriginPDF.Pages)
            {
                PdfDictionary.PdfStream stream = page.Contents.Elements.GetDictionary(0).Stream;
                var content = ContentReader.ReadContent(page);
                //var result = new StringBuilder();
                var text = ExtractText(content);
               
                foreach (var i in text)
                {
                    sentence += i.ToString();
                }
            }
            return sentence;
        }

        public static IEnumerable<string> ExtractText(this PdfPage page)
        {
            var content = ContentReader.ReadContent(page);
            var text = content.ExtractText();
            return text;
        }

        public static IEnumerable<string> ExtractText(this CObject cObject)
        {
            if (cObject is COperator)
            {
                var cOperator = cObject as COperator;
                if (cOperator.OpCode.Name == OpCodeName.Tj.ToString() ||
                    cOperator.OpCode.Name == OpCodeName.TJ.ToString())
                {
                    foreach (var cOperand in cOperator.Operands)
                        foreach (var txt in ExtractText(cOperand))
                            yield return txt;
                }
            }
            else if (cObject is CSequence)
            {
                var cSequence = cObject as CSequence;
                foreach (var element in cSequence)
                    foreach (var txt in ExtractText(element))
                        yield return txt;
            }
            else if (cObject is CString)
            {
                var cString = cObject as CString;
                yield return cString.Value;
            }
        }
    }
}



