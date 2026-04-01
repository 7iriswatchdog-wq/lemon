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
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

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
                _logger.LogInformation($"[OCR] Starting upload for {files.Count} file(s)");

                foreach (var file in files)
                {
                    if (file.Length == 0) continue;

                    var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                    _logger.LogInformation($"[OCR] Processing file: {file.FileName} ({extension}, {file.Length} bytes)");
                    
                    using var ms = new MemoryStream();
                    await file.CopyToAsync(ms);
                    ms.Position = 0;

                    if (extension == ".pdf")
                    {
                        var pdfBase64List = ProcessPdf(ms.ToArray());
                        _logger.LogInformation($"[OCR] PDF converted to {pdfBase64List.Count} image(s)");
                        base64Images.AddRange(pdfBase64List);
                    }
                    else if (extension == ".jpg" || extension == ".jpeg" || extension == ".png" || extension == ".bmp" || extension == ".webp")
                    {
                        var imgBase64 = ProcessImage(ms);
                        base64Images.Add(imgBase64);
                    }
                    else
                    {
                        _logger.LogWarning($"[OCR] Unsupported file format: {extension}");
                    }
                }

                if (base64Images.Count == 0)
                {
                    return BadRequest(new { error = "No valid images or PDFs found to process." });
                }

                _logger.LogInformation($"[OCR] Calling RunPod API with {base64Images.Count} images...");
                var extractedData = await CallRunPodOpenAiApi(base64Images);
<<<<<<< HEAD
                Console.WriteLine(JsonConvert.SerializeObject(extractedData, Formatting.Indented));
=======
                _logger.LogInformation("[OCR] Successfully received extraction result.");
>>>>>>> 2677cfc92ab8cca49cda6f13b76a5d905b216946
                return Ok(extractedData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing OCR upload");
                return StatusCode(500, new 
                { 
                    error = "Internal server error during OCR processing.",
                    exception = ex.Message,
                    stackTrace = ex.StackTrace,
                    innerException = ex.InnerException?.Message
                });
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
<<<<<<< HEAD
=======
            var endpointId = _configuration["RunPod:EndpointId"];
            var apiKey = _configuration["RunPod:ApiKey"];
>>>>>>> 2677cfc92ab8cca49cda6f13b76a5d905b216946

            Console.WriteLine($"endpointId: {endpointId}");
            Console.WriteLine($"apiKey: {apiKey}");
            Console.WriteLine("endpointId: " + endpointId);
            Console.WriteLine("apiKey: " + apiKey);
            Console.WriteLine("endpointId: {0}", endpointId);
            Console.WriteLine("apiKey: {0}", apiKey);
            if (string.IsNullOrEmpty(endpointId) || string.IsNullOrEmpty(apiKey))
            {
                Console.WriteLine("RunPod EndpointId or ApiKey is not configured.");
                throw new InvalidOperationException("RunPod EndpointId or ApiKey is not configured.");
            }

            var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromMinutes(10); // RunPod vision models can take >100s on cold start
            var baseUrl = $"https://api.runpod.ai/v2/{endpointId}/openai/v1/chat/completions";

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            var systemPrompt =
    "You are a document data extraction engine. Analyze the provided identification document image(s) and extract the requested fields into a single JSON object.\n\n" +
    "The document may be from ANY country and may be any document type: passport, national ID card, Emirates ID, residence permit (Iqama, Cédula, etc.), driving licence, labour card, or similar.\n\n" +
    "OUTPUT FIELDS (all must be present):\n" +
    "  firstname, middlename, lastname, dob, document_number, issue_date, expiry_date, gender, nationality, profession, profession_type, employer_name, isGolden\n\n" +

    "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n" +
    "RULE 1 — COMPLETENESS\n" +
    "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n" +
    "Every field in OUTPUT FIELDS MUST appear in the JSON. If a value is absent or unreadable, set it to null. Never omit a key.\n\n" +

    "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n" +
    "RULE 2 — NO HALLUCINATION (CRITICAL)\n" +
    "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n" +
    "Extract ONLY values that are EXPLICITLY and VISIBLY printed on the document.\n" +
    "  ✗ Do NOT infer, assume, translate, or guess any value.\n" +
    "  ✗ Do NOT use MRZ lines (the machine-readable zone at the bottom, e.g. P<INDRAVINDRAN<<RAMYA<<<) as the source for any field except as a cross-check for nationality.\n" +
    "  ✗ If a field does not appear on the document, return null.\n\n" +

    "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n" +
    "RULE 3 — DOCUMENT NUMBER\n" +
    "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n" +
    "Read from a clearly labeled field such as: 'Passport No', 'ID Number', 'Document No', 'Card No', 'Residence No', 'File No', 'رقم الهوية', 'رقم جواز السفر'.\n" +
    "Do NOT extract barcode values or MRZ alphanumeric sequences as the document number.\n\n" +

    "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n" +
    "RULE 4 — NAME EXTRACTION\n" +
    "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n" +
    "Names may appear in different structures across countries and languages. Follow these guidelines:\n\n" +
    "  a) firstname: Extract the given name / prename / first name. Labels include 'Given Name(s)', 'First Name', 'Prenom', 'Vorname', 'Nombre', 'الاسم الأول', etc.\n" +
    "  b) lastname: Extract the surname / family name. Labels include 'Surname', 'Family Name', 'Last Name', 'Nom', 'اللقب', 'الاسم الأخير', etc.\n" +
    "  c) middlename: Extract the middle name if clearly and separately labeled. If no middle name label exists, set to null.\n\n" +
    "  SPECIAL CASES:\n" +
    "  • If the document shows a single unseparated full name with no separate surname label (e.g. 'Full Name: JOHN MICHAEL DOE'), place the entire value in firstname and set lastname and middlename to null.\n" +
    "  • DUPLICATE WORDS: If a name contains repeated words exactly as printed (e.g. 'JOHN JOHN DOE', 'MUHAMMAD MUHAMMAD ALI'), preserve ALL repeated words exactly as they appear. Do NOT remove or deduplicate them.\n" +
    "  • Arabic documents: The name order is typically: Given name → Father's name → Grandfather's name → Family name. Map accordingly unless the document uses explicit English-style labels.\n" +
    "  • Hyphenated or compound names: Keep hyphens and spacing as printed.\n\n" +

    "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n" +
    "RULE 5 — DATES\n" +
    "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n" +
    "Format ALL dates as YYYY-MM-DD. Read only from clearly labeled date fields:\n" +
    "  - dob: 'Date of Birth', 'تاريخ الميلاد', 'D.O.B', 'Birthdate', 'Né(e) le', etc.\n" +
    "  - issue_date: 'Date of Issue', 'تاريخ الإصدار', 'Issued', 'Valid From', etc.\n" +
    "  - expiry_date: 'Date of Expiry', 'Valid Until', 'Expiry', 'تاريخ الانتهاء', 'Expires', etc.\n\n" +

    "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n" +
    "RULE 6 — GENDER & NATIONALITY\n" +
    "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n" +
    "  - gender: Return ONLY 'M' or 'F'. The label may say 'Sex', 'Gender', 'الجنس', 'Sexo', etc.\n" +
    "  - nationality: ALWAYS return the correct 3-letter ISO 3166-1 alpha-3 code.\n" +
    "    If the document shows a nationality adjective or non-standard code, convert it:\n" +
    "    ✗ IMPORTANT: NEVER return non-standard or hallucinated codes like 'SLK', 'UAE', 'KSA', 'UK', 'SRI'. These are WRONG.\n" +
    "    ✗ SRI LANKA SPECIAL RULE: Standard SRI LANKAN documents must ALWAYS map to 'LKA'. Never 'SRI', never 'SLK'.\n" +
    "    Correct reference table for common documents:\n" +
    "      INDIAN / IND → IND       SRI LANKAN / SRI / SLK → LKA   PAKISTANI / PAK → PAK\n" +
    "      FILIPINO / FIL → PHL     BENGALI / BANGLADESHI → BGD      NEPALI → NPL\n" +
    "      EMIRATI / UAE / ARB → ARE  SAUDI / KSA → SAU              EGYPTIAN / EGY → EGY\n" +
    "      BRITISH / UK / GBR → GBR  AMERICAN / USA → USA            CHINESE → CHN\n" +
    "      GERMAN → DEU              FRENCH → FRA                    SPANISH → ESP\n" +
    "      JORDANIAN → JOR           LEBANESE → LBN                  SYRIAN → SYR\n" +
    "      ETHIOPIAN → ETH           NIGERIAN → NGA                  KENYAN → KEN\n" +
    "      INDONESIAN → IDN          MALAYSIAN → MYS                 THAI → THA\n\n" +

    "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n" +
    "RULE 7 — PROFESSION, PROFESSION TYPE & EMPLOYER\n" +
    "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n" +
    "  profession:\n" +
    "    Extract ONLY if the document has an explicit field labeled: 'Profession', 'Occupation', 'Job Title', 'Designation', 'Employment', 'المهنة', 'الوظيفة', etc.\n" +
    "    Documents that commonly carry this field: Emirates ID, GCC residence permits, Saudi Iqama, labour cards, work permits.\n" +
    "    Standard passports do NOT carry profession — return null unless the label is visibly present.\n\n" +
    "  profession_type:\n" +
    "    Must be STRICTLY one of: 'Salaried in private sector' | 'Salaried in public sector' | 'Self employed' | 'Freelance' | 'Non Salaried (Dependent)' — or null.\n" +
    "    Determination priority:\n" +
    "    ① Explicitly stated on the document → use the closest matching value from the list above.\n" +
    "    ② Not stated, but employer_name is known → INFER:\n" +
    "         Government / Ministry / Police / Military / Municipality / Public Hospital / Public University → 'Salaried in public sector'\n" +
    "         Private company (LLC, Ltd, Inc, Corp, FZCO, Trading, Consultants, Brokers, Group) → 'Salaried in private sector'\n" +
    "         Own practice (independent Doctor, Lawyer, Architect, Consultant) → 'Self employed'\n" +
    "         Profession label = 'Freelancer' / 'Freelance' → 'Freelance'\n" +
    "         Student / Housewife / Dependent / Retired / Unemployed → 'Non Salaried (Dependent)'\n" +
    "    ③ Insufficient context → null.\n\n" +
    "  employer_name:\n" +
    "    Extract ONLY from a field explicitly labeled: 'Employer', 'Employer Name', 'Company', 'Sponsored by', 'Organisation', 'صاحب العمل', 'جهة العمل'.\n" +
    "    ⚠️ CRITICAL: These are NOT employer fields — never use them as employer_name:\n" +
    "       'Place of Issue' / 'Issued at' / 'Place of Birth' / 'مكان الإصدار' / 'مكان الميلاد'\n" +
    "       City names (Mumbai, Cochin, Dubai, Riyadh, London) appearing under place fields.\n" +
    "    Return null if no explicit employer label is found.\n\n" +

    "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n" +
    "RULE 8 — GOLDEN VISA\n" +
    "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n" +
    "Set isGolden to 'Yes' ONLY if the text 'Golden' or 'Golden Visa' is explicitly printed on the document. Otherwise always 'No'. This field must NEVER be null.\n\n" +

    "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n" +
    "WORKED EXAMPLES\n" +
    "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n" +
    "Indian passport: firstname='RAMYA', lastname='RAVINDRAN', middlename=null, nationality='IND', employer_name=null (COCHIN is Place of Issue — not employer), profession=null, profession_type=null\n" +
    "UAE Emirates ID (private employer): profession='OPERATIONS MANAGER', employer_name='LUXFOLIO MORTGAGE BROKERS L.L.C', profession_type='Salaried in private sector'\n" +
    "UAE Emirates ID (ministry): employer_name='MINISTRY OF HEALTH', profession_type='Salaried in public sector'\n" +
    "Duplicate name: document shows 'JOHN JOHN DOE' → firstname='JOHN JOHN', lastname='DOE' (do NOT reduce to 'JOHN')\n" +
    "Single-block name: document shows only 'JUAN DELA CRUZ' with no separate surname → firstname='JUAN DELA CRUZ', lastname=null, middlename=null\n\n" +

    "Return ONLY the raw JSON object. No markdown, no explanation, no code fences.";

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
            _logger.LogInformation($"[OCR] RunPod Request Payload Size: {jsonPayload.Length} bytes");
            _logger.LogInformation($"[OCR] Model: {payload.model}, Max Tokens: {payload.max_tokens}");
            
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(baseUrl, content);
            _logger.LogInformation($"[OCR] RunPod Response Status: {response.StatusCode}");

            var responseJson = await response.Content.ReadAsStringAsync();
            _logger.LogInformation($"[OCR] Raw RunPod Response Body: {responseJson}");

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"RunPod API Error ({response.StatusCode}): {responseJson}");
            }

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
                var result = JsonSerializer.Deserialize<object>(cleanJson);
                _logger.LogInformation("[OCR] JSON parsed successfully.");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[OCR] Failed to parse cleaned JSON. Raw cleaned content: {cleanJson}");
                // Fallback: return the raw cleaned string if it fails to parse as an object
                return new { raw_output = cleanJson };
            }
        }
    }
}
