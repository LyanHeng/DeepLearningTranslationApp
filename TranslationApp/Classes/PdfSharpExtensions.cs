using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PdfSharp.Pdf;
using PdfSharp.Pdf.Content;
using PdfSharp.Pdf.Content.Objects;
using PdfSharp.Pdf.IO;

namespace TranslationApp.Classes
{
    public static class PdfSharpExtensions
    {

        public static string GetText(string pdfFileName)
        {
            PdfDocument SamplePdf = PdfReader.Open(pdfFileName, PdfDocumentOpenMode.ReadOnly);
            string sentence = "";
            // PdfPage SamplePage = SamplePdf.Pages[0];
            //PdfDictionary.PdfStream stream = SamplePage.Contents.Elements.GetDictionary(0).Stream;

            /*var content = ContentReader.ReadContent(SamplePage);
            string sentence = "";
            var text = ExtractText(content);
            foreach(var i in text)
            {
            //string.Join(sentence);
                 sentence += i.ToString();

            }*/
            //Console.WriteLine(text);
            //return sentence;
            //int count = 0;//SamplePdf.Pages.Count;
            foreach (PdfPage page in SamplePdf.Pages)
            {
                PdfPage aPage = SamplePdf.Pages[count];
                PdfDictionary.PdfStream stream = aPage.Contents.Elements.GetDictionary(count).Stream;
               // count++;
                var content = ContentReader.ReadContent(aPage);
                var text = ExtractText(content);

                foreach (var i in text)
                {
                    sentence += i.ToString();
                }
                /*using (var _document = PdfReader.Open(pdfFileName, PdfDocumentOpenMode.ReadOnly))
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
            return sentence;
        }

        #region CObject Visitor
        private static void ExtractText(CObject obj, StringBuilder target)
        {
            if (obj is CArray)
                ExtractText((CArray)obj, target);
            else if (obj is CComment)
                ExtractText((CComment)obj, target);
            else if (obj is CInteger)
                ExtractText((CInteger)obj, target);
            else if (obj is CName)
                ExtractText((CName)obj, target);
            else if (obj is CNumber)
                ExtractText((CNumber)obj, target);
            else if (obj is COperator)
                ExtractText((COperator)obj, target);
            else if (obj is CReal)
                ExtractText((CReal)obj, target);
            else if (obj is CSequence)
                ExtractText((CSequence)obj, target);
            else if (obj is CString)
                ExtractText((CString)obj, target);
            else
                throw new NotImplementedException(obj.GetType().AssemblyQualifiedName);
        }
        private static void ExtractText(CArray obj, StringBuilder target)
        {
            foreach (var element in obj)
            {
                ExtractText(element, target);
            }
        }
        private static void ExtractText(CComment obj, StringBuilder target) { /* nothing */ }
        private static void ExtractText(CInteger obj, StringBuilder target) { /* nothing */ }
        private static void ExtractText(CName obj, StringBuilder target) { /* nothing */ }
        private static void ExtractText(CNumber obj, StringBuilder target) { /* nothing */ }
        private static void ExtractText(COperator obj, StringBuilder target)
        {
            if (obj.OpCode.OpCodeName == OpCodeName.Tj || obj.OpCode.OpCodeName == OpCodeName.TJ)
            {
                foreach (var element in obj.Operands)
                {
                    ExtractText(element, target);
                }
                target.Append(" ");
            }
        }
        private static void ExtractText(CReal obj, StringBuilder target) { /* nothing */ }
        private static void ExtractText(CSequence obj, StringBuilder target)
        {
            foreach (var element in obj)
            {
                ExtractText(element, target);
            }
        }
        private static void ExtractText(CString obj, StringBuilder target)
        {
            target.Append(obj.Value);
        }
        #endregion


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
