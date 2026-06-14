using AML.Core.DataContract.Authentication;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading.Tasks;
using NLog;

namespace AML.Core.Common.StaticResource
{
    public static class AMLUtility
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static T ObjToType<T>(object source)
        {
            try
            {
                if (source != null)
                {
                    var json = JsonConvert.SerializeObject(source);
                    var jobj = JObject.Parse(json);
                    return jobj.ToObject<T>();
                }
                return default(T);
            }
            catch (Exception ex) { Logger.Error(ex, "Error in ObjToType"); return default(T); }
        }
        public static T ObjToModel<T>(object source)
        {
            try
            {
                if (source != null)
                {
                    return JsonConvert.DeserializeObject<T>(source.ToString());
                }
                return default(T);
            }
            catch { return default(T); }
        }
        public static List<T> ToModelList<T>(this List<dynamic> list)
        {
            List<T> rlist = new List<T>();
            try
            {
                foreach (dynamic d in list)
                {
                    rlist.Add(ObjToType<T>(d));
                }
            }
            catch { }
            return rlist;
        }
        public static T ToModel<T>(this object obj)
        {
            return ObjToType<T>(obj);
        }
        public static T CastModel<T>(this Object myobj)
        {
            Type objectType = myobj.GetType();
            Type target = typeof(T);
            var x = Activator.CreateInstance(target, false);
            var z = from source in objectType.GetMembers().ToList()
                    where source.MemberType == MemberTypes.Property
                    select source;
            var d = from source in target.GetMembers().ToList()
                    where source.MemberType == MemberTypes.Property
                    select source;
            List<MemberInfo> members = d.Where(memberInfo => d.Select(c => c.Name)
               .ToList().Contains(memberInfo.Name)).ToList();
            PropertyInfo propertyInfo;
            object value;
            foreach (var memberInfo in members)
            {
                propertyInfo = typeof(T).GetProperty(memberInfo.Name);
                value = myobj.GetType().GetProperty(memberInfo.Name).GetValue(myobj, null);

                propertyInfo.SetValue(x, value, null);
            }
            return (T)x;
        }
        public static ServiceResponse<T> ResponseToType<T>(this object source)
        {
            try
            {
                var jobj = JObject.Parse(source.ToString());
                ServiceResponse<T> obj = new ServiceResponse<T>
                {
                    Message = jobj.Value<string>("message"),
                    Information = jobj.Value<string>("information"),
                    Status = jobj.Value<int>("status"),
                    Result = typeof(T).IsPrimitive ? (T)Convert.ChangeType(jobj["result"], typeof(T)) : ObjToType<T>(jobj["result"])
                };
                return obj;
            }
            catch { return default(ServiceResponse<T>); }

        }
        public static int GetRandom(int min, int max)
        {
            return RandomNumberGenerator.GetInt32(min, max);
        }

        public static DateTime? ParseDate(this object data)
        {
            DateTime defaultDate = DateTime.MinValue;
            try
            {
                var ci = new CultureInfo("en-US");
                //        var formats = new[] { "M-d-yyyy", "yyyy-MM-dd", "dd-MM-yyyy", "MM-dd-yyyy", "M.d.yyyy", "dd.MM.yyyy", "MM.dd.yyyy", "M/d/yyyy h:mm:ss tt", "d/M/yyyy h:mm:ss tt", "MM/dd/yyyy", "dd/MM/yyyy", "MM/dd/yyyy h:mm:ss tt", "dd/MM/yyyy h:mm:ss tt", "yyyy-MM-dd h:mm:ss tt", "yyyy-MM-dd h:mm:ss" }
                //.Union(ci.DateTimeFormat.GetAllDateTimePatterns()).ToArray();
                var formats = new[] {
                    "dd-MM-yyyy",
                    "d/MM/yyyy hh:mm:ss",
                    "d-MM-yyyy hh:mm:ss"
                }
        .Union(ci.DateTimeFormat.GetAllDateTimePatterns()).ToArray();
                List<string> defaultDbDts = new List<string> { "0000-00-00", "0000-00-00 00:00:00", "1/1/0001", "0001-01-01", "01-01-0001", "1/1/0001 12:00:00 AM" };
                if (data != null && !defaultDbDts.Contains(data.ParseString()))
                {
                    defaultDate = DateTime.ParseExact(data.ParseString(), formats, CultureInfo.InvariantCulture);
                    return defaultDate;
                }
                else
                    return DateTime.MinValue;
            }
            catch { return null; }
        }
        public static int ParseInt(this object data)
        {
            int defaultVal = 0;
            try
            {
                if (data != null)
                { int.TryParse(Convert.ToString(data), out defaultVal); }
            }
            catch { }
            return defaultVal;

        }
        public static byte? ParseByte(this object data)
        {
            byte defaultVal = 0;
            try
            {
                if (data != null)
                { byte.TryParse(Convert.ToString(data), out defaultVal); return defaultVal; }
            }
            catch { return null; }
            return defaultVal;

        }
        public static float ParseFloat(this object data)
        {
            float defaultVal = 0F;
            try
            {
                if (data != null)
                {
                    float.TryParse(Convert.ToString(data), out defaultVal);
                }
            }
            catch { }
            return defaultVal;
        }

        public static double ParseDouble(this object data)
        {
            double defaultVal = 0;
            try
            {
                if (data != null)
                {
                    double.TryParse(Convert.ToString(data), out defaultVal);
                }
            }
            catch { }
            return defaultVal;
        }
        public static long ParseLong(this object data)
        {
            long defaultVal = 0;
            try
            {
                if (data != null)
                {
                    long.TryParse(Convert.ToString(data), out defaultVal);
                }
            }
            catch { }
            return defaultVal;
        }

        public static string ParseString(this object data)
        {
            try
            {
                if (data != null)
                    return data.ToString();
                else
                    return string.Empty;
            }
            catch { return string.Empty; }
        }
        public static bool IsNullOrEmpty(this object data)
        {
            try
            {
                if (data != null)
                    return string.IsNullOrEmpty(data.ToString());
                else
                    return true;
            }
            catch { return true; }
        }
        public static bool IsNotNullOrEmpty(this object data)
        {
            try
            {
                if (data != null)
                    return !string.IsNullOrEmpty(data.ToString());
                else
                    return false;
            }
            catch { return false; }
        }

        public static bool IsEqual(this object data, object obj2)
        {
            try
            {
                if (data != null)
                    return data.Equals(obj2);
                else
                    return false;
            }
            catch { return false; }
        }
        public static string ConvertStringArrayToCommaSeparatedString(string[] stringArray)
        {
            return stringArray != null ? string.Join(",", stringArray) : string.Empty;
        }
        public static string[] StringToArray(this string value, char seprator = ',')
        {
            return value.IsNotNullOrEmpty() ? value.Split(seprator) : null;
        }
        /// <summary>
        /// Convert date to my sql date format
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        /// <returns></returns>

        public static string ToMysqlDateFormat(this object date)
        {
            try
            {
                var formats = new[] { "dd-MM-yyyy h:mm:ss tt", "dd-MM-yyyy h:mm:ss", "MM/dd/yyyy h:mm:ss tt", "M/d/yyyy hh:mm:ss tt", "dd-MM-yyyy HH:mm", "dd-MM-yyyy hh:mm:ss", "dd-MM-yyyy HH:mm:ss" };
                var date_result = DateTime.ParseExact(date.ParseString(), formats, CultureInfo.InvariantCulture);
                return string.Format("{0:yyyy-MM-dd}", date_result);
            }
            catch
            {
                return "0001-01-01"; //Default MySql Date Format
            }
        }
        /// <summary>
        /// Converts DateTime object to MySql DateTime
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>

        public static string ToMySqlDateTime(this object date)
        {
            try
            {
                var formats = new[] { "dd-MM-yyyy h:mm:ss tt", "dd-MM-yyyy h:mm:ss", "MM/dd/yyyy h:mm:ss tt", "M/d/yyyy hh:mm:ss tt", "dd-MM-yyyy HH:mm", "dd-MM-yyyy hh:mm:ss", "dd-MM-yyyy HH:mm:ss" };
                var date_result = DateTime.ParseExact(date.ParseString(), formats, CultureInfo.InvariantCulture);
                return string.Format("{0:yyyy-MM-dd HH:mm:ss}", date_result);
            }
            catch
            {
                return "0000-00-00 00:00"; //Default MySql Date Format
            }
        }
        /// <summary>
        /// Convert date to my sql date format
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>

        public static string ToMysqlDateFormat(this DateTime? date)
        {
            try
            {
                if (date.HasValue)
                {
                    var formats = new[] { "dd/MM/yyyy", "dd/MM/yyyy h:mm:ss tt", "dd/MM/yyyy hh:mm:ss tt", "dd/MM/yyyy h:mm:ss" };
                    var date_result = DateTime.ParseExact(date.Value.Date.ParseString(), formats, CultureInfo.InvariantCulture);
                    return string.Format("{0:yyyy-MM-dd}", date_result);
                }
                else
                    return "0000-00-00"; //Default MySql Date Format 
            }
            catch
            {
                return "0000-00-00"; //Default MySql Date Format
            }
        }
        /// <summary>
        /// Converts DateTime to String Formatted as "dd-MM-yyyy"
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static string ToUIDDateFormat(this object date, string defaultValue = "")
        {
            //try
            //{
            //    var ci = new CultureInfo("en-US");
            //    var formats = new[] { "yyyy-MM-dd", "dd-MM-yyyy", "dd-MM-yyyy h:mm:ss tt", "dd/MM/yyyy h:mm:ss", "dd-MM-yyyy h:mm:ss", "MM/dd/yyyy", "MM/dd/yyyy h:mm:ss tt", "MM/dd/yyyy 00:00:00", "M/d/yyyy 00:00:00", "M/d/yyyy hh:mm:ss tt", "dd-MMM-yy h:mm:ss tt", "dd/MM/yyyy HH:mm:ss", "dd-MM-yyyy HH:mm:ss" };
            //    List<string> defaultDbDts = new List<string> { "0000-00-00", "0000-00-00 00:00:00", "01/01/0001", "01/01/0001 00:00:00", "0001-01-01", "1/1/0001 12:00:00 AM", "1/1/1970 12:00:00 AM", "1970-01-01", "01-01-1970", "01-01-0001", "01/01/1970 00:00:00", "1/1/1970 00:00:00", "01-01-0001 00:00:00", "01-01-1970 00:00:00", "1970-01-01 00:00:00" };
            //    //if (date != null && Convert.ToString(date) != "" && !defaultDbDts.Contains(date.ParseString()))
            //    //{
            //    //    return DateTime.ParseExact(date.ParseString(), formats, CultureInfo.InvariantCulture).ToString("dd-MM-yyyy");
            //    //}
            //    return defaultValue;
            //}
            //catch (Exception ex)
            //{
            //    string mesage = ex.Message;
            //    return defaultValue;
            //}
            try
            {
                var ci = new CultureInfo("en-US");
                var formats = new[] { "yyyy-MM-dd", "dd-MM-yyyy", "dd-MM-yyyy h:mm:ss tt", "dd/MM/yyyy h:mm:ss", "dd-MM-yyyy h:mm:ss", "MM/dd/yyyy", "MM/dd/yyyy h:mm:ss tt", "MM/dd/yyyy 00:00:00", "M/d/yyyy 00:00:00", "M/d/yyyy hh:mm:ss tt", "dd-MMM-yy h:mm:ss tt", "dd/MM/yyyy HH:mm:ss", "dd-MM-yyyy HH:mm:ss" };
                List<string> defaultDbDts = new List<string> { "0000-00-00", "0000-00-00 00:00:00", "01/01/0001", "01/01/0001 00:00:00", "0001-01-01", "1/1/0001 12:00:00 AM", "1/1/1970 12:00:00 AM", "1970-01-01", "01-01-1970", "01-01-0001", "01/01/1970 00:00:00", "1/1/1970 00:00:00", "01-01-0001 00:00:00", "01-01-1970 00:00:00", "1970-01-01 00:00:00" };
                if (date != null && Convert.ToString(date) != "" && !defaultDbDts.Contains(date.ParseString()))
                {
                    //var dateformat= DateTime.ParseExact(date.ParseString(), formats, CultureInfo.InvariantCulture).ToString("dd-MM-yyyy");
                    var dateformat = DateTime.ParseExact(date.ParseString(), "dd-MM-yyyy", CultureInfo.InvariantCulture).ToString();
                    return dateformat;
                }
                return defaultValue;
            }
            catch (Exception ex)
            {
                string mesage = ex.Message;
                return defaultValue;
            }
        }


        public static DateTime? ParseDB(this string date)
        {
            if (string.IsNullOrEmpty(date))
            {
                return null;
            }

            if (date == "0001-01-01" || date == "0000-00-00" || date == "01-01-0001" || date == "00-00-0000")
            {
                return null;
            }

            DateTime dt;

            var success = DateTime.TryParse(date, out dt);
            if (!success)
            {
                string[] formats = { "dd/MM/yyyy", "dd-MM-yyyy", "yyyy-MM-dd", "yyyy/MM/dd", "dd MMM yyyy", "MM/dd/yyyy" };
                success = DateTime.TryParseExact(date, formats, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out dt);
                if (!success) return null;
            }
            if (dt.Date == DateTime.MinValue)
            {
                return null;
            }

            return dt;
        }
        public static string ParseDateToString(this string date)
        {

            if (string.IsNullOrEmpty(date))
            {
                return null;
            }

            if (date == "0001-01-01" || date == "0000-00-00" || date == "01-01-0001" || date == "00-00-0000" || date == "00-01-0001")
            {
                return string.Empty;
            }

            DateTime dt;

            var success = DateTime.TryParse(date, out dt);
            if (!success)
            {
                return null;
            }
            if (dt.Date == DateTime.MinValue)
            {
                return null;
            }

            return dt.ToString("yyyy-MM-dd");
        }
        public static string ToDateFormat(this DateTime? dt)
        {
            if (dt == null)
            {
                return null;
            }

            return string.Format("{0:yyyy-MM-dd}", dt);
        }

        public static string ToDateFormat(this DateTime dt)
        {
            return string.Format("{0:yyyy-MM-dd}", dt);
        }

        public static string ToDateTimeFormat(this DateTime? dt)
        {
            if (dt == null)
            {
                return null;
            }

            return string.Format("{0:yyyy-MM-dd HH:mm:ss}", dt);
        }

        public static string ToDateTimeFormat(this DateTime dt)
        {
            return string.Format("{0:yyyy-MM-dd HH:mm:ss}", dt);
        }

        /// <summary>
        /// Converts DB Datetime to .NET Datetime object
        /// </summary>
        /// <param name="date"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static string ToUIDDateTime(this object date, string customFormat = "dd-MM-yyyy h:mm:ss tt", string defaultValue = "")
        {
            try
            {
                var ci = new CultureInfo("en-US");
                var formats = new[] { "yyyy-MM-dd", "yyyy-MM-dd HH:mm:ss", "yyyy-MM-dd hh:mm:ss tt", "dd-MM-yyyy", "dd/MM/yyyy h:mm:ss tt", "dd/MM/yyyy h:mm:ss", "dd-MM-yyyy h:mm:ss tt", "MM/dd/yyyy", "MM/dd/yyyy h:mm:ss tt", "MM/dd/yyyy h:mm:ss", "M/d/yyyy h:mm:ss", "M/d/yyyy h:mm:ss tt", "M/d/yyyy hh:mm:ss tt", "dd-MM-yyyy hh:mm:ss" };
                List<string> defaultDbDts = new List<string> { "0000-00-00", "0000-00-00 00:00:00", "01/01/0001", "01/01/0001 00:00:00", "0001-01-01", "1/1/0001 12:00:00 AM", "1970-01-01", "01-01-1970", "01-01-0001", "01/01/1970 00:00:00" };
                if (date != null && !defaultDbDts.Contains(date.ParseString()))
                {
                    return DateTime.ParseExact(date.ParseString(), formats, CultureInfo.InvariantCulture).ToString(customFormat);
                }
                return defaultValue;
            }
            catch (Exception ex)
            {
                string mesage = ex.Message;
                return defaultValue;
            }
        }
        public static string ParseDBDate(this object data, string date_format, string default_value)
        {

            try
            {
                var ci = new CultureInfo("en-US");
                var formats = new[] { "yyyy-MM-dd", "M-d-yyyy", "dd-MM-yyyy", "MM-dd-yyyy", "M.d.yyyy", "dd.MM.yyyy", "MM.dd.yyyy", "M/d/yyyy h:mm:ss tt", "d/M/yyyy h:mm:ss tt", "MM/dd/yyyy", "MM/dd/yyyy h:mm:ss tt", "dd/MM/yyyy h:mm:ss tt", "dd-MM-yyyy h:mm:ss tt", "dd/MM/yyyy" }
        .Union(ci.DateTimeFormat.GetAllDateTimePatterns()).ToArray();
                List<string> defaultDbDts = new List<string> { "0000-00-00", "0000-00-00 00:00:00", "1/1/0001", "0001-01-01", "01-01-0001", "1/1/0001 12:00:00 AM" };
                if (data != null && !defaultDbDts.Contains(data.ParseString()))
                {
                    if (data.ParseString().Contains("00:00:00"))
                        return DateTime.ParseExact(data.ParseString().Replace("00:00:00", "").Trim(), formats, CultureInfo.InvariantCulture).ToString(date_format);
                    else
                        return DateTime.ParseExact(data.ParseString().Trim(), formats, CultureInfo.InvariantCulture).ToString(date_format);
                }
                else
                    return default_value;
            }
            catch { return default_value; }
        }
        public static IEnumerable<T> Flatten<T>(this IEnumerable<T> sequence, Func<T, IEnumerable<T>> childFetcher)
        {
            var itemsToYield = new Queue<T>(sequence);
            while (itemsToYield.Count > 0)
            {
                var item = itemsToYield.Dequeue();
                yield return item;

                var children = childFetcher(item);
                if (children != null)
                {
                    foreach (var child in children)
                    {
                        itemsToYield.Enqueue(child);
                    }
                }
            }
        }
        public static IEnumerable<T> SelectManyRecursive<T>(this IEnumerable<T> source, Func<T, IEnumerable<T>> selector)
        {
            var result = source.SelectMany(selector);
            if (!result.Any())
            {
                return result;
            }
            return result.Concat(result.SelectManyRecursive(selector));
        }
        public static IEnumerable<T> GetValues<T>()
        {
            return Enum.GetValues(typeof(T)).Cast<T>();
        }
        public static bool FromStringToBool(this object property)
        {
            List<string> inputString = new List<string> { "no", "", "0" };
            if (inputString.Contains(property))
            {
                return false;
            }
            else return true;
        }
        public static string ToYesNoText(this int property)
        {
            if (property == 1)
                return "Yes";
            else if (property == 0)
                return "No";
            else return "N/A";
        }
        public static string ToGenderName(this string value)
        {
            if (value == "m")
                return "Male";
            else if (value == "f")
                return "Female";
            else return "N/A";
        }
        public static string ToMaritalStatus(this string value)
        {
            if (value == "m")
                return "Married";
            else if (value == "s")
                return "Single";
            else return "N/A";
        }


        public static string DecimalToWords(decimal number)
        {
            if (number == 0)
                return "zero";

            if (number < 0)
                return "minus " + DecimalToWords(Math.Abs(number));

            string words = "";

            int intPortion = (int)number;
            decimal fraction = (number - intPortion) * 100;
            int decPortion = (int)fraction;

            words = NumberToWords(intPortion);
            if (decPortion > 0)
            {
                words += " and ";
                words += NumberToWords(decPortion);
            }
            return words;
        }
        public static string NumberToWords(this int number)
        {
            if (number == 0)
                return "zero";

            if (number < 0)
                return "minus " + NumberToWords(Math.Abs(number));

            string words = "";

            if ((number / 1000000) > 0)
            {
                words += NumberToWords(number / 1000000) + " million ";
                number %= 1000000;
            }

            if ((number / 1000) > 0)
            {
                words += NumberToWords(number / 1000) + " thousand ";
                number %= 1000;
            }

            if ((number / 100) > 0)
            {
                words += NumberToWords(number / 100) + " hundred ";
                number %= 100;
            }

            if (number > 0)
            {
                if (words != "")
                    words += "and ";

                var unitsMap = new[] { "zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine", "ten", "eleven", "twelve", "thirteen", "fourteen", "fifteen", "sixteen", "seventeen", "eighteen", "nineteen" };
                var tensMap = new[] { "zero", "ten", "twenty", "thirty", "forty", "fifty", "sixty", "seventy", "eighty", "ninety" };

                if (number < 20)
                    words += unitsMap[number];
                else
                {
                    words += tensMap[number / 10];
                    if ((number % 10) > 0)
                        words += "-" + unitsMap[number % 10];
                }
            }

            return words;
        }

        public static string AddInnerSpaces(this string input)
        {
            return string.Join(string.Empty, input.Select((x, i) => (
              char.IsUpper(x) && i > 0 &&
              (char.IsLower(input[i - 1]) || (i < input.Count() - 1 && char.IsLower(input[i + 1])))
         ) ? " " + x : x.ToString()));
        }

        public static string ArrayToString(this object[] input, char separator = ',')
        {
            if (input == null)
                return string.Empty;
            else
                return string.Join(separator, input);
        }

        public static string CapitalizedText(this string stringValue)
        {
            if (IsNotNullOrEmpty(stringValue))
            {
                return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(stringValue);
            }
            else
                return stringValue;
        }
        public static string CapitalizedFirstLetter(this string stringValue)
        {
            if (IsNotNullOrEmpty(stringValue))
            {
                return stringValue.First().ToString().ToUpper() + stringValue.Substring(1);
            }
            else
                return stringValue;
        }
        public static string FormatJsonText(string jsonString)
        {
            using var doc = JsonDocument.Parse(
                jsonString,
                new JsonDocumentOptions
                {
                    AllowTrailingCommas = true
                }
            );
            MemoryStream memoryStream = new MemoryStream();
            using (
                var utf8JsonWriter = new Utf8JsonWriter(
                    memoryStream,
                    new JsonWriterOptions
                    {
                        Indented = true
                    }
                )
            )
            {
                doc.WriteTo(utf8JsonWriter);
            }
            return new System.Text.UTF8Encoding()
                .GetString(memoryStream.ToArray());
        }
        public static string FormatJsonToPlainText(string jsonString)
        {
            char[] arr = { (char)34, (char)58, ' ', (char)34 };
            char[] arrNum = { (char)34, (char)58, ' ' };
            string formattedString = FormatJsonText(jsonString);
            formattedString = formattedString.Replace('[', ' ').Replace(']', ' ');
            formattedString = formattedString.Replace(new string(arr), " => ").Replace(new string(arrNum), " => ");
            formattedString = formattedString.Replace('"', ' ');
            formattedString = formattedString.Replace("{", "---------------------------------");
            formattedString = formattedString.Replace("},", "");
            formattedString = formattedString.Replace("}", "");
            return formattedString;
        }

        public static async Task<string> ScreeningAPICall(dynamic model, string url, string baseURL)
        {
            string result = string.Empty;
            #region POST Content Setter
            string postContent = JsonConvert.SerializeObject(model);
            var buffer = Encoding.UTF8.GetBytes(postContent);
            var byteContent = new ByteArrayContent(buffer);
            byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            #endregion
            #region POST REQUEST
            using (HttpClient client = new HttpClient { Timeout = TimeSpan.FromSeconds(30) })
            {
                //Add Token Headers            
                try
                {
                    client.DefaultRequestHeaders.Add("authUser", model.AuthToken.UserId.ToString());
                    client.DefaultRequestHeaders.Add("authCompany", model.AuthToken.CompanyId.ToString());
                    client.DefaultRequestHeaders.Add("authKey", model.AuthToken.AuthKey.ToString());
                }
                catch { }
                client.BaseAddress = new Uri(baseURL);
                string fullUrl = baseURL.TrimEnd('/') + "/" + url.TrimStart('/');
                HttpResponseMessage response = await client.PostAsync(fullUrl, byteContent);
                if (response.IsSuccessStatusCode)
                {
                    result = await response.Content.ReadAsStringAsync();
                }
            }
            #endregion
            return result;
        }
        public static async Task<string> C6ScreeningAPICall(dynamic model, string url, string baseURL)
        {
            string result = string.Empty;
            
            // New token generation
            TokenRS token =  CreateC6Token(ScreeningService.C6AUTHENTICATION, baseURL, model.Username);
            model.CompanyID = token.user.id;
            string postContent1 = JsonConvert.SerializeObject(model);
            var buffer1 = Encoding.UTF8.GetBytes(postContent1);
            var byteContent1 = new ByteArrayContent(buffer1);
            byteContent1.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            using (HttpClient client1 = new HttpClient { Timeout = TimeSpan.FromSeconds(30) })
            {
                try
                {
                    client1.DefaultRequestHeaders.Add("Authorization", "Bearer " + token.user.token);
                }
                catch { }
                client1.BaseAddress = new Uri(baseURL);
                string fullUrl = baseURL.TrimEnd('/') + "/" + url.TrimStart('/');
                HttpResponseMessage response = await client1.PostAsync(fullUrl, byteContent1);
                if (response.IsSuccessStatusCode)
                {
                    result = await response.Content.ReadAsStringAsync();
                }
                else
                {
                    result = await response.Content.ReadAsStringAsync();
                }
            }
            return result;
        }

        public static async Task<string> WEBScreeningAPICall(dynamic model,string url,string baseURL,string username,string password)
        {
            string result = string.Empty;

            string postContent = JsonConvert.SerializeObject(model);
            var buffer = Encoding.UTF8.GetBytes(postContent);
            var byteContent = new ByteArrayContent(buffer);
            byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            using (HttpClient client = new HttpClient { Timeout = TimeSpan.FromSeconds(30) })
            {
                try
                {
                    var authToken = Convert.ToBase64String(
                        Encoding.ASCII.GetBytes($"{username}:{password}")
                    );

                    client.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Basic", authToken);

                    string fullUrl = baseURL.TrimEnd('/') + "/" + url.TrimStart('/');

                    HttpResponseMessage response =
                        await client.PostAsync(fullUrl, byteContent);

                    result = await response.Content.ReadAsStringAsync();
                }
                catch (Exception ex)
                {
                    result = ex.Message;
                }
            }

            return result;
        }

        public static TokenRS CreateC6Token(string url, string baseURL, string username)
        {
            TokenRS res = new TokenRS();
            TokenRQ model = new TokenRQ();
            dynamic result = string.Empty;
            model.username = username;
            #region POST Content Setter
            string postContent = JsonConvert.SerializeObject(model);
            var buffer = Encoding.UTF8.GetBytes(postContent);
            var byteContent = new ByteArrayContent(buffer);
            byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            #endregion
            #region POST REQUEST
            using (HttpClient client = new HttpClient { Timeout = TimeSpan.FromSeconds(30) })
            {
                client.BaseAddress = new Uri(baseURL);
                string fullUrl = baseURL.TrimEnd('/') + "/" + url.TrimStart('/');
                HttpResponseMessage response = client.PostAsync(fullUrl, byteContent).Result;
                if (response.IsSuccessStatusCode)
                {
                    result = response.Content.ReadAsStringAsync();

                    if (result != null && !string.IsNullOrEmpty(result.Result))
                    {
                        res = JsonConvert.DeserializeObject<TokenRS>(result.Result);
                    }
                }
                else
                {
                    result = response.Content.ReadAsStringAsync();


                    res = JsonConvert.DeserializeObject<TokenRS>(result.Result);
                    res.status = 400;
                }
            }
            #endregion
            return res;
        }

        public static async Task<string> PostAsync(dynamic model, string url, string baseURL)
        {
            string result = string.Empty;
            #region POST Content Setter
            string postContent = JsonConvert.SerializeObject(model);
            var buffer = Encoding.UTF8.GetBytes(postContent);
            var byteContent = new ByteArrayContent(buffer);
            byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            #endregion
            #region POST REQUEST
            using (HttpClient client = new HttpClient { Timeout = TimeSpan.FromSeconds(30) })
            {
                //Add Token Headers            
                try
                {
                    client.DefaultRequestHeaders.Add("authUser", model.AuthToken.UserId.ToString());
                    client.DefaultRequestHeaders.Add("authCompany", model.AuthToken.CompanyId.ToString());
                    client.DefaultRequestHeaders.Add("authKey", model.AuthToken.AuthKey.ToString());
                }
                catch (Exception ex)
                {
                }
                client.BaseAddress = new Uri(baseURL);
                Console.WriteLine(baseURL + url);
                HttpResponseMessage response = await client.PostAsync(baseURL + url, byteContent);
                Console.WriteLine(response.StatusCode);
                if (response.IsSuccessStatusCode)
                {
                    result = await response.Content.ReadAsStringAsync();
                }
            }
            #endregion
            return result;
        }

    }
}
