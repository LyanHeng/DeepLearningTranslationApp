using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfSharp.Pdf.Content;
using PdfSharp.Pdf.Content.Objects;
using PdfSharp.Pdf.IO;
using System.Text.RegularExpressions;
using PdfSharp.Drawing.Layout;
using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering;

namespace TranslationApp.Classes
{
    public static class PdfSharpExtensions
    {

        public static string Formatting(string text)
        {
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
        public static void Render(XGraphics gfx)
        {
            PdfDocument document = new PdfDocument();

            PdfPage page = document.AddPage();
           // XGraphics gfx = XGraphics.FromPdfPage(page);

            const bool unicode = false;

            Document doc = CreateDocument();
            PdfDocumentRenderer pdfRenderer = new PdfDocumentRenderer(unicode);
            //pdfRenderer.Document = doc;
            DocumentRenderer cunt = new DocumentRenderer(doc);
            cunt.PrepareDocument();
            //cunt.RenderObject(gfx,);
            
        }
        public static Document CreateDocument()
        {
            Document mydoc = new Document();
            Section section = mydoc.AddSection();

            Paragraph paragraph = section.AddParagraph();
            
            paragraph.Format.Font.Color = Color.FromCmyk(100, 30, 20, 50);
 
            paragraph.AddFormattedText("Hello, World!", TextFormat.Bold);
            return mydoc;
        }
        public static void ExportPDF(string text)
        {
            //string pdfFileName
            //PdfDocument OriginPDF = PdfReader.Open(pdfFileName, PdfDocumentOpenMode.ReadOnly);
            PdfDocument NewDocument = new PdfDocument();

            //create document info 
            NewDocument.Info.Title = "Translated";//OriginPDF.Info.Title + "_Translated";

            string formatted =Formatting(text);

            PdfPage[] pages = new PdfPage[3];//testing with set val 

            XGraphics[] gfx = new XGraphics[pages.Length];
            for (int i = 0;i < pages.Length; i++)
            {
                pages[i] = NewDocument.AddPage();
                gfx[i] = XGraphics.FromPdfPage(pages[i]);

            }
            string sub = "";
            if (formatted.Length > 3000)
            {
                sub = formatted.Substring(3000);
            }
            //fonts
            XFont Sfont = new XFont("Verdana", 5, XFontStyle.Italic);
            XFont font = new XFont("Verdana", 10, XFontStyle.Regular);
            //text formater
            XTextFormatter tf = new XTextFormatter(gfx[0]);
            XTextFormatter tf2 = new XTextFormatter(gfx[1]);

            //shilling wordwise 
            gfx[0].DrawString("Document translated by WordWise", Sfont, XBrushes.Red,new XRect(20, 20, 50, 50),XStringFormats.Center);

            XRect rect = new XRect(40, 100, (pages[0].Width * 9/10), (pages[0].Height *9/10));
            gfx[0].DrawRectangle(XBrushes.SeaShell, rect);
            tf.DrawString(formatted,font,XBrushes.Black,rect,XStringFormats.TopLeft);

            gfx[1].DrawRectangle(XBrushes.SeaShell, rect);
            tf2.DrawString(sub, font, XBrushes.Black, rect, XStringFormats.TopLeft);

            const string filename = "translated_doc.pdf";

            //Improve for user friendliness 
            NewDocument.Save(filename);
            
            Process.Start(filename);

        }
        public static string GetText(string pdfFileName)
        {
            PdfDocument OriginPDF = PdfReader.Open(pdfFileName, PdfDocumentOpenMode.ReadOnly);
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



