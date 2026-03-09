using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace AML.Core.Common.CommonClasses
{
    public class PdfHelper
    {
        /// <summary>
        /// Searches a PDF for the given keyword.
        /// Returns the matched paragraphs (blank-line separated sections) joined by \n\n.
        /// Each returned block is exactly one natural paragraph from the document.
        /// Returns null if no match is found.
        /// </summary>
        public static string SearchKeywordInPdf(string filePath, string keyword)
        {
            if (!File.Exists(filePath)) return null;
            if (string.IsNullOrWhiteSpace(keyword)) return null;

            try
            {
                // 1. Extract all pages into a single string
                var sb = new StringBuilder();
                using (var reader = new PdfReader(filePath))
                {
                    for (int i = 1; i <= reader.NumberOfPages; i++)
                    {
                        string pageText = PdfTextExtractor.GetTextFromPage(reader, i, new SimpleTextExtractionStrategy());
                        sb.AppendLine(pageText);
                    }
                }

                string fullText = sb.ToString();

                // 2. Group consecutive non-empty lines into paragraphs.
                //    A blank line signals the end of one paragraph and start of the next.
                var paragraphs = new List<string>();
                var current   = new List<string>();

                foreach (var rawLine in fullText.Split(new[] { '\n', '\r' }))
                {
                    string line = rawLine.Trim();
                    if (string.IsNullOrEmpty(line))
                    {
                        // Blank line = paragraph boundary
                        if (current.Count > 0)
                        {
                            paragraphs.Add(string.Join(" ", current));
                            current.Clear();
                        }
                    }
                    else
                    {
                        current.Add(line);
                    }
                }
                // Flush last paragraph
                if (current.Count > 0)
                    paragraphs.Add(string.Join(" ", current));

                // 3. If the entire document came back as ONE paragraph (no blank lines),
                //    treat each individual line as its own paragraph instead.
                if (paragraphs.Count <= 1 && paragraphs.Any())
                {
                    paragraphs = fullText
                        .Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(l => l.Trim())
                        .Where(l => l.Length > 2) // ignore stray single chars
                        .ToList();
                }

                // 4. Find paragraphs that contain the keyword (case-insensitive)
                var matched = paragraphs
                    .Where(p => p.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0)
                    .ToList();

                if (matched.Count == 0) return null;

                // 5. Return each matched paragraph separated by double newline
                return string.Join("\n\n", matched);
            }
            catch (Exception ex)
            {
                Console.WriteLine("PDF search error: " + ex.Message);
                return null;
            }
        }
    }
}
