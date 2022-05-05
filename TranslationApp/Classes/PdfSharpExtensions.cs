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

        public static List<string> Format(string text)
        {
            //maybe need to add divide for length of string 
            string tobe = "";
            List<string> divided = new List<string>();
            foreach(var i in text)
            {                
                tobe += i.ToString(); 
                if(i.ToString() == "." || i.ToString() == "!" || i.ToString() == "?")
                {
                    divided.Add(tobe);                    
                    tobe = null;
                }
            }
            return divided; //returns list of strings divided by punctuation 
        }
        public static void ExportPDF(string pdfFileName, string text )
        {
            PdfDocument OriginPDF = PdfReader.Open(pdfFileName, PdfDocumentOpenMode.ReadOnly);
            PdfDocument NewDocument = new PdfDocument();
            //create document info 
            string name = OriginPDF.Info.Title + "_Translated";//not sure about keeping this
            NewDocument.Info.Title = OriginPDF.Info.Title + "_Translated";

            
            List<string> division = Format(text);

            /*template for proper implementaion
             * a new page is created for each page of doc
             * cannot be further implemented due to there being no way of preserving original structure
             * export to pdf will need to be reworked for original structure to be preserved
             * To be further touched upon in SEM 1 final sprint
            foreach (PdfPage pages in OriginPDF.Pages)
            { 
                PdfPage a_page = NewDocument.AddPage();
                XGraphics a_gfx = XGraphics.FromPdfPage(a_page);
                //draw strings
                //do next page 
                //
            }
            */
            PdfPage page = NewDocument.AddPage();
            XGraphics gfx = XGraphics.FromPdfPage(page);
            XFont Sfont = new XFont("Verdana", 5, XFontStyle.Italic);
            XFont font = new XFont("Verdana", 10, XFontStyle.Regular);
            //for testing string divider only 
            XRect rect1 = new XRect(10, 10, page.Width / 3, page.Height / 3);
            XRect rect2 = new XRect(10, 20, page.Width / 3, page.Height / 3);
            //to do divide the page 
            //consult http://www.pdfsharp.net/wiki/TextLayout-sample.ashx
            //formatting looks bad atm

            //maybe add draw for title here, or remove need for original file 

            //shilling wordwise 
            gfx.DrawString("Document translated by WordWise", Sfont, XBrushes.Red,
            new XRect(20, 20, 50, 50),
            XStringFormats.Center);
            int y = 10;
            foreach (var i in division)
            {
                XRect rect = new XRect(10, y, page.Width / 10, page.Height / 5);
                gfx.DrawString(i, font, XBrushes.Black, rect, XStringFormats.CenterLeft);
                y = y + 10;
            }
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
             * Change Export to PDF to be done in this section 
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
            }
            return sentence;
        }
        //could be used with export to txt functioanlity ?
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

        

