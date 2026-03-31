using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Docnet.Core;
using Docnet.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace AML.Web.Controllers.Ocr
{
    [Route("api/ocr")]
    [ApiController]
    public class OcrController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<OcrController> _logger;

        public OcrController(IConfiguration configuration, IHttpClientFactory httpClientFactory, ILogger<OcrController> logger)
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> Upload([FromForm] List<IFormFile> files)
        {
            if (files == null || files.Count == 0)
            {
                return BadRequest(new { error = "No files were uploaded." });
            }

            try
            {
                var base64Images = new List<string>();

                foreach (var file in files)
                {
                    if (file.Length == 0) continue;

                    var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                    using var ms = new MemoryStream();
                    await file.CopyToAsync(ms);
                    ms.Position = 0;

                    if (extension == ".pdf")
                    {
                        var pdfBase64List = ProcessPdf(ms.ToArray());
                        base64Images.AddRange(pdfBase64List);
                    }
                    else if (extension == ".jpg" || extension == ".jpeg" || extension == ".png" || extension == ".bmp" || extension == ".webp")
                    {
                        var imgBase64 = ProcessImage(ms);
                        base64Images.Add(imgBase64);
                    }
                    else
                    {
                        _logger.LogWarning($"Unsupported file format: {extension}");
                    }
                }

                if (base64Images.Count == 0)
                {
                    return BadRequest(new { error = "No valid images or PDFs found to process." });
                }

                var extractedData = await CallRunPodOpenAiApi(base64Images);
                return Ok(extractedData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing OCR upload");
                return StatusCode(500, new { error = "Internal server error during OCR processing." });
            }
        }

        private List<string> ProcessPdf(byte[] pdfBytes)
        {
            var results = new List<string>();

            using (var docReader = DocLib.Instance.GetDocReader(pdfBytes, new PageDimensions(2.0)))
            {
                var pageCount = docReader.GetPageCount();
                for (int i = 0; i < pageCount; i++)
                {
                    using (var pageReader = docReader.GetPageReader(i))
                    {
                        var width = pageReader.GetPageWidth();
                        var height = pageReader.GetPageHeight();
                        var rawBytes = pageReader.GetImage(); // BGRA format

                        using (var image = Image.LoadPixelData<Bgra32>(rawBytes, width, height))
                        {
                            results.Add(ResizeAndEncodeToJpeg(image));
                        }
                    }
                }
            }
            return results;
        }

        private string ProcessImage(MemoryStream ms)
        {
            using (var image = Image.Load(ms))
            {
                return ResizeAndEncodeToJpeg(image);
            }
        }

        private string ResizeAndEncodeToJpeg(Image image)
        {
            const int maxSize = 1280;
            if (image.Width > maxSize || image.Height > maxSize)
            {
                var resizeOptions = new ResizeOptions
                {
                    Size = new Size(maxSize, maxSize),
                    Mode = ResizeMode.Max,
                    Sampler = KnownResamplers.Lanczos3
                };
                image.Mutate(x => x.Resize(resizeOptions));
            }

            using var outMs = new MemoryStream();
            image.SaveAsJpeg(outMs, new JpegEncoder { Quality = 85 });
            return Convert.ToBase64String(outMs.ToArray());
        }

        private async Task<object> CallRunPodOpenAiApi(List<string> base64Images)
        {
            var endpointId = _configuration["RunPod:EndpointId"];
            var apiKey = _configuration["RunPod:ApiKey"];

            if (string.IsNullOrEmpty(endpointId) || string.IsNullOrEmpty(apiKey))
            {
                throw new InvalidOperationException("RunPod EndpointId or ApiKey is not configured.");
            }

            var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromMinutes(10); // RunPod vision models can take >100s on cold start
            var baseUrl = $"https://api.runpod.ai/v2/{endpointId}/openai/v1/chat/completions";

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            var systemPrompt = "Analyze this identification document and extract data into a JSON object. " +
    "FIELDS: firstname, middlename, lastname, dob, document_number, issue_date, expiry_date, gender, nationality, profession, profession_type, employer_name, isGolden." +
    "\nIMPORTANT RULES:\n" +
    "1. All fields in the FIELDS list MUST be present in the output JSON. If a value is not found on the document, set its value to `null`.\n" +
    "2. document_number: Look for the clear visual label 'Passport No', 'ID Number', or 'Document No'. Do NOT use the starts of MRZ lines (e.g., P<IND or similar) as the document number.\n" +
    "3. firstname: Extract the given name(s) or prename. If the document displays a single full name (e.g., 'JOHN DOE'), place it entirely in this field (e.g., 'JOHN DOE') and set 'middlename' and 'lastname' to `null`. If the name is clearly separated (e.g., 'JOHN MICHAEL DOE'), extract 'JOHN'. If a name has repeated words (e.g., 'JOHN JOHN DOE'), strictly keep all repeated words as they appear.\n" +
    "4. middlename: Extract any middle name(s). If not present, set to `null`.\n" +
    "5. lastname: Extract the surname(s). If not present or if the `firstname` field contains the single full name, set to `null`.\n" +
    "6. Formatting: Dates as YYYY-MM-DD, e.g., '1990-01-01'. Gender as M/F. Nationality as 3-letter code, e.g., 'IND'.\n" +
    "7. EXAMPLES:\n" +
    "   - For document_number: 'C1234567' (e.g., from 'Passport No C1234567')\n" +
    "   - For firstname: 'JOHN' (e.g., from 'Given Names: JOHN MICHAEL', or 'Full Name: JANE DOE' -> 'JANE DOE')\n" +
    "   - For middlename: 'MICHAEL' (e.g., from 'Given Names: JOHN MICHAEL') or null\n" +
    "   - For lastname: 'DOE' (e.g., from 'Surname: DOE') or null\n" +
    "   - For dob: '1990-01-01' (e.g., from 'Date of Birth 01/01/1990')\n" +
    "   - For issue_date: '2020-03-15' (e.g., from 'Date of Issue 15 MAR 2020')\n" +
    "   - For expiry_date: '2030-03-15' (e.g., from 'Date of Expiry 15 MAR 2030')\n" +
    "   - For gender: 'M' (e.g., from 'Sex: M')\n" +
    "   - For nationality: 'LKA' (e.g., from 'Nationality Sri Lanks')\n" +
    "   - For profession: 'Sales Officer' (e.g., from 'Employment or Profession or Work or Occupation/')\n" +
    "   - For profession_type: Strictly one from the following options - 'Salaried in private sector', 'Salaried in public sector', 'Self employed', 'Freelance', 'Non Salaried (Dependent)' or null\n" +
    "   - For employer_name: 'Google' (e.g., from 'Employer: Google') or null\n" +
    "   - For isGolden: 'Yes' if 'Golden' is explicitly mentioned on the document, otherwise 'No'. Strictly not null - only 'Yes' or 'No'\n" +
    "Return ONLY the JSON object and no other text or markdown.";

            var messages = new List<object>
            {
                new { role = "system", content = systemPrompt }
            };

            var userContent = new List<object>
            {
                new { type = "text", text = "Extract information from these documents into a JSON object matching the requested schema." }
            };

            foreach (var b64 in base64Images)
            {
                userContent.Add(new
                {
                    type = "image_url",
                    image_url = new { url = $"data:image/jpeg;base64,{b64}" }
                });
            }

            messages.Add(new { role = "user", content = userContent });

            var payload = new
            {
                model = "qwen/qwen2.5-vl-7b-instruct",
                messages = messages,
                max_tokens = 1024,
                temperature = 0.0
            };

            var jsonPayload = JsonSerializer.Serialize(payload);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(baseUrl, content);
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync();
            using var document = JsonDocument.Parse(responseJson);

            var rawMessage = document.RootElement
                                     .GetProperty("choices")[0]
                                     .GetProperty("message")
                                     .GetProperty("content")
                                     .GetString() ?? "";

            // Strip out markdown ```json ... ``` blocks if standard models fail to obey the "no markdown" prompt.
            var cleanJson = Regex.Replace(rawMessage, @"```json\s*", "");
            cleanJson = Regex.Replace(cleanJson, @"\s*```", "");

            try
            {
                return JsonSerializer.Deserialize<object>(cleanJson);
            }
            catch
            {
                // Fallback: return the raw cleaned string if it fails to parse as an object
                return new { raw_output = cleanJson };
            }
        }
    }
}
