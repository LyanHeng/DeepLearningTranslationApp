using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfSharp.Pdf.Content;
using PdfSharp.Pdf.Content.Objects;
using PdfSharp.Pdf.IO;

namespace TranslationApp.Classes
{
    public static class PdfSharpExtensions
    {

        public static void ExportPDF(string pdfFileName, string text )
        {
            PdfDocument OriginPDF = PdfReader.Open(pdfFileName, PdfDocumentOpenMode.ReadOnly);
            PdfDocument NewDocument = new PdfDocument();
            //create document info 
            string name = OriginPDF.Info.Title + "_Translated";//not sure about keeping this
            NewDocument.Info.Title = OriginPDF.Info.Title + "_Translated";

            //Will need to add loop for adding pages but leave for now
            PdfPage page = NewDocument.AddPage();
            XGraphics gfx = XGraphics.FromPdfPage(page);
            XFont Sfont = new XFont("Verdana", 5, XFontStyle.Italic);
            XFont font = new XFont("Verdana", 10, XFontStyle.Regular);

            //formatting looks bad atm 
            gfx.DrawString("Document translated by WordWise", Sfont, XBrushes.Red,
            new XRect(20, 20, 50, 50),
            XStringFormats.Center);

            gfx.DrawString(text, font, XBrushes.Black,
            new XRect(10, 10, page.Width/3, page.Height/3),
            XStringFormats.CenterLeft);
            const string filename = "test.pdf";
            NewDocument.Save(filename);
            Process.Start(filename);

        }
        public static string GetText(string pdfFileName)
        {
            PdfDocument OriginPDF = PdfReader.Open(pdfFileName, PdfDocumentOpenMode.ReadOnly);
            string sentence = "";
            /*
             * Still TODO:
             * figure out how to split strings and keep some semblance of structure to the document 
             * Will need to look under the hood of the document reading part 
             * clean up file and remove unecassary code
             * 
            */
            foreach (PdfPage page in OriginPDF.Pages)
            {
                PdfDictionary.PdfStream stream = page.Contents.Elements.GetDictionary(0).Stream;
                var content = ContentReader.ReadContent(page);
                //var result = new StringBuilder();
                var text = ExtractText(content);

                foreach (var i in text)
                {
                    //add paragraph/structure handling here
                    //unrealistic to do it properly on this end i beleive 
                    sentence += i.ToString();
                }
                /*OLD (maybe) redudant loop for reading PDFS
                 * using (var _document = PdfReader.Open(pdfFileName, PdfDocumentOpenMode.ReadOnly))
                {
                    //PdfDictionary.PdfStream stream = 
                    var result = new StringBuilder();
                    foreach (var page in _document.Pages.OfType<PdfPage>())
                    {
                        ExtractText(ContentReader.ReadContent(page), result);
                        result.AppendLine();
                    }
                    return result.ToString();

                }*/
            }
            //maybe string builder??
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

        

