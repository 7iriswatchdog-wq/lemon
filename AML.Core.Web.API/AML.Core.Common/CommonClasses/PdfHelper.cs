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
        private static readonly Dictionary<string, List<string>> _pdfParagraphsCache = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
        private static readonly object _cacheLock = new object();

        private static List<string> GetCachedParagraphs(string filePath)
        {
            lock (_cacheLock)
            {
                if (_pdfParagraphsCache.TryGetValue(filePath, out var cached))
                {
                    return cached;
                }

                var paragraphs = new List<string>();
                if (!File.Exists(filePath))
                {
                    _pdfParagraphsCache[filePath] = paragraphs;
                    return paragraphs;
                }

                try
                {
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
                    var current = new List<string>();

                    foreach (var rawLine in fullText.Split(new[] { '\n', '\r' }))
                    {
                        string line = rawLine.Trim();
                        if (string.IsNullOrEmpty(line))
                        {
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
                    if (current.Count > 0)
                        paragraphs.Add(string.Join(" ", current));

                    if (paragraphs.Count <= 1 && paragraphs.Any())
                    {
                        paragraphs = fullText
                            .Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(l => l.Trim())
                            .Where(l => l.Length > 2)
                            .ToList();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("PDF read error: " + ex.Message);
                }

                _pdfParagraphsCache[filePath] = paragraphs;
                return paragraphs;
            }
        }

        /// <summary>
        /// Searches a PDF for the given keyword.
        /// Returns the matched paragraphs (blank-line separated sections) joined by \n\n.
        /// Each returned block is exactly one natural paragraph from the document.
        /// Returns null if no match is found.
        /// </summary>
        public static string SearchKeywordInPdf(string filePath, string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword)) return null;

            var paragraphs = GetCachedParagraphs(filePath);
            if (paragraphs == null || paragraphs.Count == 0) return null;

            var matched = paragraphs
                .Where(p => p.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0)
                .ToList();

            if (matched.Count == 0) return null;

            return string.Join("\n\n", matched);
        }

        /// <summary>
        /// Checks if a given paragraph is present in the PDF file.
        /// </summary>
        public static bool IsParagraphInPdf(string filePath, string paragraph)
        {
            if (string.IsNullOrWhiteSpace(paragraph)) return false;

            var paragraphs = GetCachedParagraphs(filePath);
            if (paragraphs == null || paragraphs.Count == 0) return false;

            string trimmedPara = paragraph.Trim();
            return paragraphs.Any(p => 
                string.Equals(p.Trim(), trimmedPara, StringComparison.OrdinalIgnoreCase) ||
                p.Contains(trimmedPara) ||
                trimmedPara.Contains(p.Trim())
            );
        }
    }
}
