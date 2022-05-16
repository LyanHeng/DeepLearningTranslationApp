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
using System.Text.RegularExpressions;

namespace TranslationApp.Classes
{
    public static class PdfSharpExtensions
    {

        /*public static List<List<string>> Format(string text)
        {
            //maybe need to add divide for length of string 
            //string sentence = "";
            string word = "";
            
            List<string> sentence = new List<string>();
            List<List<string>> divided = new List<List<string>>();
            foreach (var i in text)
            {
                word += i.ToString();
                //sentence += i.ToString();

                if (i.ToString() == "")//if there is a blank space 
                {
                    sentence.Add(word);
                    word = null;
                }
                if (i.ToString() == "." || i.ToString() == "!" || i.ToString() == "?" || sentence.Count ==100 )
                {
                    divided.Add(sentence);
                    sentence = null;
                }
                //divided.Add(tobe);
            }
            return divided; //returns list of strings divided by punctuation 
        }*/
        public static List<string> Format(string text)
        {
            //maybe need to add divide for length of string 
            //string sentence = "";
            string word = "";
            string test = "";
            string pain = "";
            List<string> sentence = new List<string>();
            //List<List<string>> divided = new List<List<string>>();
            List<string> divided = new List<string>();
            foreach (var i in text)
            {
                word += i.ToString();
                //sentence += i.ToString();

                if (i.ToString() == " ")//if there is a blank space 
                {
                    sentence.Add(word);
                    //sentence.Add("\r\n");
                    word = "";

                }
                if (i.ToString() == "." || i.ToString() == "!" || i.ToString() == "?" || sentence.Count == 15)
                //if(sentence.Count == 20)
                {
                    //divided.Add(sentence.);
                    foreach(var plz in sentence)
                    {
                        test += plz.ToString();
                    }
                    divided.Add(test);
                    test = "";
                    sentence.Clear();
                }
                //divided.Add(tobe);
            }
            //divided = sentence;
            return divided; //returns list of strings divided by punctuation 
        }
        public static List<string> Fmat(string text)
        {
            int count = 0;
            List<string> sentences = new List<string>();
            var regex2 = new Regex(@"((?<=(\.)*(\r\n)+).{20}|(?<=\.(\s)+)).{20}", RegexOptions.Compiled);
            List<string> matchList = new List<string>();
            foreach (Match match in regex2.Matches(text))
            {
                matchList.Add(match.Value);
            }
            for (int i = matchList.Count - 1; i > 0; i--)
            {
                //int positionOfNewline = substring.LastIndexOf(matchList[i]);
                int positionOfNewline = text.LastIndexOf(matchList[i]);
                //newstring = text.Substring(0, positionOfNewline);
                //text = text.Substring(positionOfNewline, text.Length - positionOfNewline);
                string part = text.Substring(positionOfNewline,text.Length - positionOfNewline);
                //newstring = text.Substring(0, positionOfNewline);
                sentences.Add(part);
               
            }
            return sentences;
        }
        public static void ExportPDF(string text)
        {
            //string pdfFileName
            //PdfDocument OriginPDF = PdfReader.Open(pdfFileName, PdfDocumentOpenMode.ReadOnly);
            PdfDocument NewDocument = new PdfDocument();
            //create document info 
            //string name = OriginPDF.Info.Title + "_Translated";//not sure about keeping this
            NewDocument.Info.Title = "a Title";//OriginPDF.Info.Title + "_Translated";


            //List<List<string>> division = Format(text); //-> Disabled for now
            //List<string> division = Format(text);
            List<string> division = Fmat(text);
            //List<string> division = new List<string>();
            //division.Add(text); //only left like this until sentence handling can be fixed
            /*template for proper implementaion
             * a new page is created for each page of doc
             * cannot be further implemented due to there being no way of preserving original structure
             * export to pdf will need to be reworked for original structure to be preserved
             * To be further touched upon in SEM 1 final sprint
            */
            PdfPage[] pages = new PdfPage[3];//testing with set val 
                                             //PdfPage page = NewDocument.AddPage();

            XGraphics[] gfx = new XGraphics[pages.Length];
            for (int i = 0;i < pages.Length; i++)
            {
                pages[i] = NewDocument.AddPage();
                gfx[i] = XGraphics.FromPdfPage(pages[i]);
                /*for (int z = 0; z < i; z++)
                {
                    gfx[z] = XGraphics.FromPdfPage(pages[i]);
                }*/

            }

            //XGraphics gfx = XGraphics.FromPdfPage(pages[i]);
            //fonts
            XFont Sfont = new XFont("Verdana", 5, XFontStyle.Italic);
            XFont font = new XFont("Verdana", 4, XFontStyle.Regular);

            //maybe add draw for title here, or remove need for original file 

            //shilling wordwise 
            //XGraphics titlefx = XGraphics.FromPdfPage(pages[0]); 
            gfx[0].DrawString("Document translated by WordWise", Sfont, XBrushes.Red,new XRect(20, 20, 50, 50),XStringFormats.Center);
            //draw text to pdf 
            int lineH = 10;
            int pageNum = 0;
            division.Reverse();
            foreach (var i in division)
            {
                if(lineH == pages[pageNum].Height || lineH > pages[pageNum].Height)
                {
                    pageNum++;
                    lineH = 10;
                }
                XRect rect = new XRect(15, lineH, pages[pageNum].Width/10 ,pages[pageNum].Height/5 );
                //gfx[pageNum].DrawString(i, font, XBrushes.Black, rect, XStringFormats.CenterLeft);
                gfx[pageNum].DrawString(i, font, XBrushes.Black, rect, XStringFormats.CenterLeft);
                lineH += 15;
            }
            const string filename = "test.pdf";
            //needs to be changed
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



