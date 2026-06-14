using Aml.Screening.Common.Algorithms;
using Aml.Screening.DataContracts.Dtos;
using Aml.Screening.MongoModal.Modal;
using Aml.Screening.MongoModal.UnitOfWork;

using Microsoft.Extensions.Configuration;

using MongoDB.Bson;
using MongoDB.Driver;

using Newtonsoft.Json;

using AnyAscii;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace Aml.Screening.Services.LocalServices
{
    /// <summary>
    /// 
    /// </summary>
    public class FuzzyServices
    {
        /// <summary>
        /// The unit of work name list
        /// </summary>
        private readonly UnitOfWork_NameListRepository _unitOfWorkNameList;
        private readonly DoubleMetaphone _metaphoneBuilder;
        /// <summary>
        /// The unit of work case log
        /// </summary>
        private readonly UnitOfWork_CaseLogRepository _unitOfWorkCaseLog;
        /// <summary>
        /// The predicate
        /// </summary>
        private Expression<Func<NAMELIST, bool>> predicate = null;
        /// <summary>
        /// The predicate
        /// </summary>
        private Expression<Func<CASELOG, bool>> _predicate = null;
        /// <summary>
        /// Initializes a new instance of the <see cref="FuzzyServices"/> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// 
        
        public FuzzyServices(IConfiguration configuration)
        {
            _unitOfWorkNameList = new UnitOfWork_NameListRepository(configuration);
            _unitOfWorkCaseLog = new UnitOfWork_CaseLogRepository(configuration);
            _metaphoneBuilder = new DoubleMetaphone();
        }

        private double BoostScore(double score, double bias = 1.0)
        {
            return score + (score * (1 - score) * bias);
        }

        private double GetWeightedScore(List<(double, double)> scoresAndWeights)
        {
            var scores = new List<double>();
            var sumOfWeights = 0.0;

            foreach (var scoreAndWeight in scoresAndWeights)
            {
                var (score, weight) = scoreAndWeight;

                sumOfWeights += weight;

                scores.Add(score * weight);
            }

            return scores.Aggregate(0.0, (p, c) => p + c) / sumOfWeights;
        }

        private string GetCleanedString(string name, bool toupper = false)
        {
            name = name.Transliterate().ToLower();

            name = Regex.Replace(name, @"[^a-z\d\s]", " ");

            if (toupper)
            {
                name = name.ToUpper();
            }

            name = Regex.Replace(name, @"\s+", " ");

            return name.Trim();
        }

        private string CleanText(string text, bool sorted = true)
        {
            text = text.ToLower();
            text = text.Transliterate();
            text = Regex.Replace(text, @"[^a-z\s]", " ");
            text = Regex.Replace(text, @"\s+", " ");
            text = string.Join(' ', new HashSet<string>(text.Split(" ")).ToArray());
            text = text.Trim();

            if (sorted) { text = string.Join(" ", text.Split(" ").OrderBy(val => val)); }

            return text;
        }

        private List<string> GetDblMetaphone(string name)
        {
            var tokens = new HashSet<string>();

            foreach (var token in name.Split(" "))
            {
                foreach (var metaphone in _metaphoneBuilder.BuildKeys(token))
                {
                    tokens.Add(metaphone);
                }
            }

            return tokens.OrderBy(val => val).ToList();
        }

        private List<string> GetNGrams(string text, int nGramLength)
        {
            var nGramsList = new List<string>();

            for (int i = 0; i < text.Length - 1; i++)
            {
                var nGram = i + 1 + nGramLength > text.Length ? text[i..] : text.Substring(i, nGramLength);
                nGramsList.Add(nGram);
            }

            return nGramsList;
        }

        private double GetDblScore(string firstString, string secondString)
        {
            var firstStringMetaphones = GetDblMetaphone(firstString);
            var secondStringMetaphones = GetDblMetaphone(secondString);

            var metaphoneIntersectionSize = 0;

            foreach (var metaphone in firstStringMetaphones)
            {
                if (secondStringMetaphones.Contains(metaphone))
                {
                    metaphoneIntersectionSize++;
                }
            }

            return ((double)metaphoneIntersectionSize) / Math.Min(firstStringMetaphones.Count, secondStringMetaphones.Count);
        }

        private double NGramCompare(string firstString, string secondString, int nGramLength = 3)
        {
            firstString = Regex.Replace(firstString, @"\s+", "");
            secondString = Regex.Replace(secondString, @"\s+", "");

            if (firstString == secondString) { return 1; }
            if (firstString.Length < nGramLength || secondString.Length < nGramLength) { return 0; }

            var shortString = firstString.Length <= secondString.Length ? firstString : secondString;
            var longString = firstString.Length > secondString.Length ? firstString : secondString;

            var nGramsListShort = GetNGrams(shortString, nGramLength);
            var nGramsListLong = GetNGrams(longString, nGramLength);

            var nGramsListShortLength = nGramsListShort.Count;

            var intersectionSize = 0;

            foreach (var nGram in nGramsListShort)
            {
                if (nGramsListLong.Contains(nGram))
                {
                    intersectionSize++;
                }
            }

            return ((double)intersectionSize) / nGramsListShortLength;
        }

        private double GetScore(string string1, string string2)
        {
            if (string2.Contains("ACCIRENT"))
            {
                Console.WriteLine(string2);
            }

            var cleanedString1Sorted = CleanText(string1, true);
            var cleanedString1Unsorted = CleanText(string1, false);

            var cleanedString2Sorted = CleanText(string2, true);
            var cleanedString2Unsorted = CleanText(string2, false);

            if (string.IsNullOrEmpty(cleanedString1Sorted) || string.IsNullOrEmpty(cleanedString2Sorted)) { return 0; }

            var useSortedName = NGramCompare(cleanedString1Sorted, cleanedString2Sorted, 2) > NGramCompare(cleanedString1Unsorted, cleanedString2Unsorted, 2);

            var cleanedDataName = useSortedName ? cleanedString1Sorted : cleanedString1Unsorted;
            var cleanedSearchName = useSortedName ? cleanedString2Sorted : cleanedString2Unsorted;

            var concatName = new string[] { cleanedDataName, cleanedSearchName };

            var avgSearchNameWordLength = concatName.Select(val => val.Length).Average();
            var searchNameLength = (new int[] { cleanedDataName.Split(' ').Count(), cleanedSearchName.Split(' ').Count() }).Average();

            var dblScore = GetDblScore(cleanedDataName, cleanedSearchName);
            var biGramScore = NGramCompare(cleanedDataName, cleanedSearchName, 2);
            var triGramScore = NGramCompare(cleanedDataName, cleanedSearchName, 3);
            var quadGramScore = NGramCompare(cleanedDataName, cleanedSearchName, 4);

            var biGramWeight = Math.Max(Math.Min(1 - avgSearchNameWordLength + 3, 1), 0) + (searchNameLength / 2);
            var triGramWeight = Math.Max(Math.Min(Math.Min(1 - avgSearchNameWordLength + 4, avgSearchNameWordLength - 2), 1), 0) + (searchNameLength / 2);
            var quadGramWeight = Math.Max(Math.Min(1, avgSearchNameWordLength - 3), 0) + (searchNameLength / 2);
            var dlbWeight = Math.Max(Math.Max(biGramWeight, triGramWeight), quadGramWeight) * 2 / searchNameLength;

            var shorterLength = Math.Min(cleanedSearchName.Length, cleanedDataName.Length);
            var longerLength = Math.Max(cleanedSearchName.Length, cleanedDataName.Length);

            var baseScore = GetWeightedScore(new List<(double, double)>()
            {
                (dblScore, dlbWeight),
                (biGramScore, biGramWeight),
                (triGramScore, triGramWeight),
                (quadGramScore, quadGramWeight)
            });

            var boosedScore = BoostScore(baseScore, (double)shorterLength / longerLength);

            return Math.Round(boosedScore * 100, 0);
        }

        /// <summary>
        /// Fuzzies the search.
        /// </summary>
        /// <param name="screen">The screen.</param>
        /// <returns></returns>
        public async Task<ScreeningResponse> FuzzySearch(CustomerScreenDto screen)
        {
            try
            {
                double searchScore = 60;  //To Do : Fetch screening score from Mysql DB;
                string[] nameList = screen.CustomerFullName.Trim().Split(' ').Where(s => s != "" && s.Length > 2).Distinct().ToArray();
                if (nameList.Length == 0)
                    return new ScreeningResponse
                    {
                        IsPositive = false,
                    };
                var soundCodes = new List<string>();
                foreach (var names in nameList)
                {
                    string sound = Soundex.SoundexText(GetCleanedString(names));
                    soundCodes.Add(sound);
                }
                var activeFilter = Builders<NAMELIST>.Filter.Eq(x => x.STATUS, "A");
                var soundFilter = Builders<NAMELIST>.Filter.All("SOUNDEX", soundCodes);
                var clientidFilter = Builders<NAMELIST>.Filter.Or(
                                         Builders<NAMELIST>.Filter.Eq("CLIENTID", screen.ClientId),
                                         Builders<NAMELIST>.Filter.Exists("CLIENTID", false),
                                         Builders<NAMELIST>.Filter.Eq("CLIENTID", -1)
                                     ); var filter = Builders<NAMELIST>.Filter.And(activeFilter, clientidFilter, soundFilter);
                var result = await _unitOfWorkNameList.FindAllLimitAsync(filter);
                if (result.Count() > 0)
                {
                    List<FuzzyFilterDto> fzzyList = new List<FuzzyFilterDto>();
                    foreach (var item in result)
                    {
                        FuzzyFilterDto fDto = new FuzzyFilterDto()
                        {
                            NAME = item.FULLNAME ?? string.Empty,
                            SCORE = JaroWinkler.Similarity(item.FULLNAME, screen.CustomerFullName.Trim().ToUpper()),
                            UID = (item.UID != null) ? item.UID : String.Empty,
                            CATEGORY = (item.CATEGORY != null) ? item.CATEGORY : String.Empty,
                            TYPE = (item.TYPE != null) ? item.TYPE : String.Empty,
                        };
                        fzzyList.Add(fDto);
                    }
                    var colln = fzzyList.Where(z => z.SCORE >= searchScore).OrderByDescending(o => o.SCORE).FirstOrDefault();
                    if (colln != null)
                    {
                        var matchDetails = Calculate(colln.SCORE);
                        return new ScreeningResponse
                        {
                            IsPositive = true,
                            MatchRisk = matchDetails.MatchRisk,
                            MatchPercentage = colln.SCORE.ToString(),
                            MatchCategory = colln.CATEGORY,
                            MatchName = colln.NAME,
                            MatchType = colln.TYPE,
                            MatchCount = string.Empty,
                            AuthFlag = "P",
                            MatchUID = colln.UID,
                            MatchRemarks = Resource.NameFuzzyMatch
                        };
                    }
                }
                return new ScreeningResponse
                {
                    IsPositive = false
                };

            }
            catch (Exception ex)
            {
                return new ScreeningResponse
                {
                    IsPositive = false
                };

            }
        }

        public async Task<CASELOG> FuzzySearchOffline(CustomerScreenDto screen)
        {
            try
            {
                CASELOG caseLog = new CASELOG();
                caseLog.CASEID = screen.CaseId;
                double searchScore = screen.Threshold;  //To Do : Fetch screening score from Mysql DB;
                string[] nameList = screen.CustomerFullName.Trim().Split(' ').Where(s => s != "" && s.Length > 2).Distinct().ToArray();
                if (nameList.Length == 0)
                    return null;
                var soundCodes = new List<string>();
                foreach (var names in nameList)
                {
                    string sound = Soundex.SoundexText(GetCleanedString(names));
                    soundCodes.Add(sound);
                }
                var activeFilter = Builders<NAMELIST>.Filter.Eq(x => x.STATUS, "A");
                var clientidFilter = Builders<NAMELIST>.Filter.Or(
                                         Builders<NAMELIST>.Filter.Eq("CLIENTID", screen.ClientId),
                                         Builders<NAMELIST>.Filter.Exists("CLIENTID", false),
                                         Builders<NAMELIST>.Filter.Eq("CLIENTID", -1)
                                     ); var soundFilter = Builders<NAMELIST>.Filter.All("SOUNDEX", soundCodes);
                var nationalityEmptyFilter = Builders<NAMELIST>.Filter.Eq(x => x.NATIONALITY, "");
                var nationalityFilter = Builders<NAMELIST>.Filter.Where(x => x.NATIONALITY.ToUpper() == screen.CustomerNationality.ToUpper());
                var customertypeFilter=Builders<NAMELIST>.Filter.Eq(x=>x.CATEGORY,screen.CustomerType);

                FilterDefinition<NAMELIST> filter;
                if (!string.IsNullOrEmpty(screen.CustomerNationality))
                {
                    filter = Builders<NAMELIST>.Filter.And(activeFilter, clientidFilter, soundFilter, customertypeFilter, Builders<NAMELIST>.Filter.Or(nationalityEmptyFilter, nationalityFilter));
                }
                else
                {
                    filter = Builders<NAMELIST>.Filter.And(activeFilter, clientidFilter, soundFilter, customertypeFilter);
                }
                Console.WriteLine($"connecting to mongo db, checking with id { DateTime.Now}");
                Console.WriteLine($"connecting to mongo db, checking with id {screen.CaseId}");

                //LogFile("connecting to mongo db, checking with id", screen.CaseId);
                var result = await _unitOfWorkNameList.FindAllLimitAsync(filter);
                
                if (result.Count() > 0)
                {
                    Console.WriteLine($"After getting results from Mongo db :  { DateTime.Now}");
                    Console.WriteLine($"Fuzzycountafterresult :  { result.Count()}");


                    List<CASELOGMATCH> lst = new List<CASELOGMATCH>();
                    foreach (var item in result)
                    {
                        //LogFile("After getting results from Mongo db", item.UID ?? String.Empty);

                        //if (item ) 
                        //{
                        CASELOGMATCH match = new CASELOGMATCH()
                            {
                                MATCHNAME = item.FULLNAME ?? string.Empty,
                                MATCHSCORE = GetScore(item.FULLNAME.Trim().ToUpper(), screen.CustomerFullName.Trim().ToUpper()),
                                MATCHUID = item.UID ?? String.Empty,
                                MATCHCATEGORY = item.CATEGORY ?? String.Empty,
                                MATCHTYPE = item.TYPE ?? String.Empty,
                                MATCHNATIONALITY = item.NATIONALITY ?? String.Empty,
                                MATCHDOB = (item.DOB.Count() > 0) ? item.DOB.Select(x => x.DOB).FirstOrDefault() : String.Empty,
                                MATCHIDNO = (item.IDDETAILS.Count() > 0) ? item.IDDETAILS.Select(x => x.IDNUMBER).FirstOrDefault() : String.Empty,
                            };
                        if (match.MATCHSCORE > 70) 
                        {
                            lst.Add(match);
                        }
                           
                        //}
                       
                    }
                    //LogFile("After getting results from Mongo db", lst);
                    _predicate = x => x.CASEID.Equals(screen.CaseId);
                    var result2 = (await _unitOfWorkCaseLog.FindAllAsync(_predicate)).ToList().LastOrDefault();
                    if (result2 != null && result2.MATCHRECORDS != null)
                    {
                        foreach (var item in result2.MATCHRECORDS)
                        {
                            var caseLogMatch2 = new CASELOGMATCH
                            {
                                MATCHUID = item.MATCHUID,
                                MATCHSCORE = item.MATCHSCORE,
                                MATCHNAME = item.MATCHNAME,
                                MATCHCATEGORY = (item.MATCHCATEGORY != null) ? item.MATCHCATEGORY : String.Empty,
                                MATCHTYPE = (item.MATCHTYPE != null) ? item.MATCHTYPE : String.Empty,
                                MATCHNATIONALITY = (item.MATCHNATIONALITY != null) ? item.MATCHNATIONALITY : String.Empty,
                                MATCHDOB = (item.MATCHDOB != null) ? item.MATCHDOB : String.Empty,
                                MATCHIDNO = (item.MATCHIDNO != null) ? item.MATCHIDNO : String.Empty
                            };
                            if (caseLogMatch2.MATCHSCORE > 70)
                            {
                                lst.Add(caseLogMatch2);
                            }
                           
                        }

                        var caseResult = lst.Where(z => z.MATCHSCORE >= searchScore).OrderByDescending(o => o.MATCHSCORE).Take(25).ToList();
                        if (caseResult.Count() > 0)
                        {

                            caseLog.MATCHRECORDS = lst;
                             caseLog._id = result2._id;
                        }
                    }

                    return caseLog;
                }

                return null;
               

            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                return null;
            }
        }

        public class CountryList
        {
            public List<CountryType> CountryTypes { get; set; }
        }
        public class CountryType
        {
            public string Num_code { get; set; }
            public string Alpha_2_code { get; set; }
            public string Alpha_3_code { get; set; }
            public string En_short_name { get; set; }
            public string Nationality { get; set; }
        }


        private string GetFullCountryName(string isoCode)
        {
            string body;

            using (StreamReader reader = new StreamReader(@"countries.json"))
            {
                body = reader.ReadToEnd();
            };

            var countryList = JsonConvert.DeserializeObject<CountryList>(body);

            var countryItem = countryList.CountryTypes.Where(val => val.Alpha_2_code == isoCode || val.Alpha_3_code == isoCode).ToList();

            return countryItem.FirstOrDefault()?.En_short_name;
        }

        private int GetScore1(string source, string target)
        {
            // Option 1: FuzzySharp
            return FuzzySharp.Fuzz.Ratio(source, target);

            // Option 2: Jaro-Winkler
            // return (int)(JaroWinkler.Similarity(source, target) * 100);
        }


        public async Task<List<CASELOGMATCH>> SearchFuzzy(CustomerScreenDto screen)
        {
            try
            {
                if (screen.CustomerNationality?.Length == 2 || screen.CustomerNationality?.Length == 3)
                {
                    screen.CustomerNationality = GetFullCountryName(screen.CustomerNationality);
                }

                screen.CustomerFullName = GetCleanedString(screen.CustomerFullName, true);

                if (string.IsNullOrWhiteSpace(screen.CustomerFullName))
                    return new List<CASELOGMATCH>();

                string[] nameList = screen.CustomerFullName.Split(' ')
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .Distinct()
                    .ToArray();

                if (nameList.Length == 0)
                    return new List<CASELOGMATCH>();

                var soundCodes = nameList
                    .Select(name => Soundex.SoundexText(GetCleanedString(name)))
                    .ToList();

                var activeFilter = Builders<NAMELIST>.Filter.Eq(x => x.STATUS, "A");

                var clientidFilter = Builders<NAMELIST>.Filter.Or(
                    Builders<NAMELIST>.Filter.Eq("CLIENTID", screen.ClientId),
                    Builders<NAMELIST>.Filter.Exists("CLIENTID", false),
                    Builders<NAMELIST>.Filter.Eq("CLIENTID", -1)
                );

                var soundFilter = Builders<NAMELIST>.Filter.In("SOUNDEX", soundCodes);

                var nationalityEmptyFilter = Builders<NAMELIST>.Filter.Eq(x => x.NATIONALITY, "");
                var nationalityFilter = Builders<NAMELIST>.Filter.Where(x => x.NATIONALITY.ToUpper() == screen.CustomerNationality.ToUpper());

                bool isCorporate = screen.CustomerType == "C";

                var categoryFilter = Builders<NAMELIST>.Filter.Or(
                    Builders<NAMELIST>.Filter.Eq("CATEGORY", isCorporate ? "CORPORATE" : "INDIVIDUAL"),
                    Builders<NAMELIST>.Filter.Eq("CATEGORY", isCorporate ? "ENTITY" : "INDIVIDUAL")
                );

                FilterDefinition<NAMELIST> filter;

                if (!string.IsNullOrEmpty(screen.CustomerNationality))
                {
                    filter = Builders<NAMELIST>.Filter.And(
                        activeFilter, clientidFilter, soundFilter, categoryFilter,
                        Builders<NAMELIST>.Filter.Or(nationalityEmptyFilter, nationalityFilter));
                }
                else
                {
                    filter = Builders<NAMELIST>.Filter.And(activeFilter, clientidFilter, soundFilter, categoryFilter);
                }

                var result = _unitOfWorkNameList.FindAll(filter);

                if (!result.Any())
                    return new List<CASELOGMATCH>();

                List<CASELOGMATCH> finalMatches = new List<CASELOGMATCH>();

                foreach (var item in result)
                {
                    string matchId = string.IsNullOrWhiteSpace(item.UID)
                        ? string.Empty
                        : (item.UID.StartsWith("UN") ? item.SUBCATEGORY : item.UID);

                    string matchName = item.FULLNAME ?? string.Empty;

                    int scoreFull = GetScore1(item.FULLNAME?.Trim().ToUpper() ?? "", screen.CustomerFullName);
                    int scoreFirst = GetScore1(item.FIRSTNAME?.Trim().ToUpper() ?? "", screen.CustomerFullName);
                    int scoreLast = GetScore1(item.LASTNAME?.Trim().ToUpper() ?? "", screen.CustomerFullName);

                    int maxScore = Math.Max(scoreFull, Math.Max(scoreFirst, scoreLast));
                    int akaBestScore = 0;

                    if (item.AKALIST != null)
                    {
                        foreach (var aka in item.AKALIST)
                        {
                            string akaName = $"{aka.AKAFIRST_NAME} {aka.AKALAST_NAME}".Trim().ToUpper();
                            int akaScore = GetScore1(akaName, screen.CustomerFullName);
                            if (akaScore > akaBestScore)
                                akaBestScore = akaScore;
                        }
                    }

                    int finalScore = Math.Max(maxScore, akaBestScore);
                    if (finalScore >= screen.Threshold)
                    {
                        var caseLogMatch = new CASELOGMATCH
                        {
                            MATCHUID = item.UID,
                            MATCHNAME = item.FULLNAME,
                            MATCHSCORE = finalScore,
                            MATCHCATEGORY = (item.CATEGORY != null) ? item.CATEGORY : String.Empty,
                            MATCHTYPE = (item.TYPE != null) ? item.TYPE : string.Empty,
                            MATCHNATIONALITY = (item.NATIONALITY.ToUpper() != null) ? item.NATIONALITY.ToUpper() : String.Empty,
                            MATCHDOB = (item.DOB.Count() > 0) ? item.DOB.Select(x => x.DOB).FirstOrDefault() : String.Empty,
                            MATCHIDNO = (item.IDDETAILS.Count() > 0) ? item.IDDETAILS.Select(x => x.IDNUMBER).FirstOrDefault() : String.Empty
                        };
                        finalMatches.Add(caseLogMatch);
                    }



                    //if (finalScore >= screen.Threshold)
                    //{
                    //    item.MATCHSCORE = finalScore;
                    //    item.UID = matchId;
                    //    item.FULLNAME = matchName;
                    //    finalMatches.Add(item);
                    //}
                }

                return finalMatches
                    .OrderByDescending(x => x.MATCHSCORE)
                    .Take(50)
                    .ToList();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                return new List<CASELOGMATCH>();
            }
        }

        //public async Task<List<NAMELIST>> SearchFuzzy(CustomerScreenDto screen)
        //{
        //    try
        //    {
        //        if (screen.CustomerNationality?.Length == 2 || screen.CustomerNationality?.Length == 3)
        //        {
        //            screen.CustomerNationality = GetFullCountryName(screen.CustomerNationality);
        //        }

        //        screen.CustomerFullName = GetCleanedString(screen.CustomerFullName, true);

        //        double searchScore = 0;  //To Do : Fetch screening score from Mysql DB;
        //        string[] nameList = screen.CustomerFullName.Split(' ').Where(s => s != "").Distinct().ToArray();
        //        if (nameList.Length == 0)
        //            return null;
        //        var soundCodes = new List<string>();
        //        foreach (var names in nameList)
        //        {
        //            string sound = Soundex.SoundexText(GetCleanedString(names));
        //            soundCodes.Add(sound);
        //        }
        //        var activeFilter = Builders<NAMELIST>.Filter.Eq(x => x.STATUS, "A");
        //        var clientidFilter = Builders<NAMELIST>.Filter.Or(
        //                                 Builders<NAMELIST>.Filter.Eq("CLIENTID", screen.ClientId),
        //                                 Builders<NAMELIST>.Filter.Exists("CLIENTID", false),
        //                                 Builders<NAMELIST>.Filter.Eq("CLIENTID", -1)
        //                             );
        //        var soundFilter = Builders<NAMELIST>.Filter.In("SOUNDEX", soundCodes);
        //        //var AkasoundFilter = Builders<NAMELIST>.Filter.In("AKALIST.AKASOUNDEX", soundCodes);
        //        var nationalityEmptyFilter = Builders<NAMELIST>.Filter.Eq(x => x.NATIONALITY, "");
        //        var nationalityFilter = Builders<NAMELIST>.Filter.Where(x => x.NATIONALITY.ToUpper() == screen.CustomerNationality.ToUpper());
        //        bool corporate = false;

        //        if (screen.CustomerType == "C")
        //        {
        //            corporate = true;
        //        }

        //        var Category = Builders<NAMELIST>.Filter.Or(
        //                                 Builders<NAMELIST>.Filter.Eq("CATEGORY", corporate ? "CORPORATE" : "INDIVIDUAL"),
        //                                 Builders<NAMELIST>.Filter.Eq("CATEGORY", corporate ? "ENTITY" : "INDIVIDUAL")
        //                                 );

        //        FilterDefinition<NAMELIST> filter;
        //        if (!string.IsNullOrEmpty(screen.CustomerNationality))
        //        {
        //            filter = Builders<NAMELIST>.Filter.And(activeFilter, clientidFilter, soundFilter, Category, Builders<NAMELIST>.Filter.Or(nationalityEmptyFilter, nationalityFilter));
        //        }
        //        else
        //        {
        //            filter = Builders<NAMELIST>.Filter.And(activeFilter, clientidFilter, soundFilter, Category);
        //        }

        //        var result = _unitOfWorkNameList.FindAll(filter);
        //        var MatchID = "";
        //        var MatchName = "";

        //        if (result.Count() > 0)
        //        {
        //            List<NAMELIST> lst = new List<NAMELIST>();
        //            //List<BlackListDto> lst = new List<BlackListDto>();
        //            foreach (var item in result)
        //            {
        //                MatchID = (item.UID == null) ? String.Empty : (item.UID.Substring(0, 2) == "UN") ? item.SUBCATEGORY : item.UID;

        //                MatchName = item.FULLNAME ?? string.Empty;


        //                if (item.FULLNAME.Contains(screen.CustomerFullName.Trim().ToUpper()) || item.FIRSTNAME.Contains(screen.CustomerFullName.Trim().ToUpper()) || item.LASTNAME.Contains(screen.CustomerFullName.Trim().ToUpper()))
        //                {


        //                    List<AKALISTDTO> myList = new List<AKALISTDTO>();

        //                    if (item.AKALIST != null)
        //                    {
        //                        foreach (var AKA in item.AKALIST)
        //                        {
        //                            myList.Add(new AKALISTDTO
        //                            {
        //                                AKAUID = AKA.AKAUID,
        //                                AKACATEGORY = AKA.AKACATEGORY,
        //                                AKAFIRST_NAME = AKA.AKAFIRST_NAME,
        //                                AKALAST_NAME = AKA.AKALAST_NAME,
        //                                AKATYPE = AKA.AKATYPE

        //                            });

        //                        }
        //                    }


        //                    NAMELIST match = new NAMELIST()
        //                    {
        //                        FULLNAME = item.FULLNAME ?? string.Empty,
        //                        MATCHSCORE = GetScore(item.FULLNAME.Trim().ToUpper(), screen.CustomerFullName.Trim().ToUpper()),
        //                        //MATCHUID = (item.UID != null) ? item.UID : String.Empty,
        //                        UID = MatchID,
        //                        CATEGORY = (item.CATEGORY != null) ? item.CATEGORY : String.Empty,
        //                        TYPE = (item.TYPE != null) ? item.TYPE : String.Empty,
        //                        NATIONALITY = (item.NATIONALITY != null) ? item.NATIONALITY : String.Empty,
        //                        DOB = item.DOB,
        //                        IDDETAILS = item.IDDETAILS,
        //                        AKALIST = item.AKALIST,
        //                    };
        //                    lst.Add(item);

        //                }
        //                else
        //                {

        //                    //var score1 = Math.Round(JaroWinkler.Similarity(item.LASTNAME, screen.CustomerFullName.Trim().ToUpper()));
        //                    //var score2 = Math.Round(JaroWinkler.Similarity(item.FIRSTNAME, screen.CustomerFullName.Trim().ToUpper()));
        //                    //var score3 = Math.Round(JaroWinkler.Similarity(item.FULLNAME, screen.CustomerFullName.Trim().ToUpper()));


        //                    // item.MATCHSCORE = GetScore(item.FULLNAME.Trim().ToUpper(), screen.CustomerFullName.Trim().ToUpper());

        //                    //Searching for name match in the Alias Names and adding it to the resultset
        //                    List<AKALISTDTO> myList = new List<AKALISTDTO>();


        //                    //if (item.AKALIST != null)
        //                    //{
        //                    //    foreach (var AKA in item.AKALIST)
        //                    //    {
        //                    //        myList.Add(new AKALISTDTO
        //                    //        {
        //                    //            AKAUID = AKA.AKAUID,
        //                    //            AKACATEGORY = AKA.AKACATEGORY,
        //                    //            AKAFIRST_NAME = AKA.AKAFIRST_NAME,
        //                    //            AKALAST_NAME = AKA.AKALAST_NAME,
        //                    //            AKATYPE = AKA.AKATYPE

        //                    //        });

        //                    //        var AKAName = AKA.AKAFIRST_NAME + " " + AKA.AKALAST_NAME;
        //                    //            string AKANameMatch = AKAName + " " + "(Alias Name);   " + MatchName + "  " + "(Original Name)" ?? string.Empty;

        //                    //        BlackListDto matchAKA = new BlackListDto()
        //                    //        {
        //                    //            //MATCHNAME = AKAName + " " + "(Alias Name) "  + MatchName + "  " + "(Original Name)" ?? string.Empty,
        //                    //            MATCHNAME = AKANameMatch,
        //                    //            MATCHSCORE = GetScore(AKAName?.Trim()?.ToUpper(), screen.CustomerFullName?.Trim()?.ToUpper()),
        //                    //            MATCHUID = MatchID,
        //                    //            MATCHCATEGORY = (item.CATEGORY != null) ? item.CATEGORY : String.Empty,
        //                    //            MATCHTYPE = (item.TYPE != null) ? item.TYPE : String.Empty,
        //                    //            NATIONALITY = (item.NATIONALITY != null) ? item.NATIONALITY : String.Empty,
        //                    //            MATCHDOB = (item.DOB.Count() > 0) ? item.DOB.Select(x => x.DOB).FirstOrDefault() : String.Empty,
        //                    //            MATCHIDNUMBER = (item.IDDETAILS.Count() > 0) ? item.IDDETAILS.Select(x => x.IDNUMBER).FirstOrDefault() : String.Empty,
        //                    //            AKANAMES = myList
        //                    //        };
        //                    //        lst.Add(matchAKA);

        //                    //    }
        //                    //}


        //                    NAMELIST match = new NAMELIST()
        //                    {
        //                        FULLNAME = item.FULLNAME ?? string.Empty,
        //                        MATCHSCORE = GetScore(item.FULLNAME.Trim().ToUpper(), screen.CustomerFullName.Trim().ToUpper()),
        //                        //MATCHUID = (item.UID != null) ? item.UID : String.Empty,
        //                        UID = MatchID,
        //                        CATEGORY = (item.CATEGORY != null) ? item.CATEGORY : String.Empty,
        //                        TYPE = (item.TYPE != null) ? item.TYPE : String.Empty,
        //                        NATIONALITY = (item.NATIONALITY != null) ? item.NATIONALITY : String.Empty,
        //                        DOB = item.DOB,
        //                        IDDETAILS = item.IDDETAILS,
        //                        AKALIST = item.AKALIST,
        //                        //MATCHNAME = item.FULLNAME ?? string.Empty,
        //                        //MATCHSCORE = Math.Max(Math.Round(JaroWinkler.Similarity(item.FULLNAME, screen.CustomerFullName.Trim().ToUpper())), Math.Max(Math.Round(JaroWinkler.Similarity(item.FIRSTNAME, screen.CustomerFullName.Trim().ToUpper())), Math.Round(JaroWinkler.Similarity(item.LASTNAME, screen.CustomerFullName.Trim().ToUpper())))),
        //                        //MATCHUID = (item.UID != null) ? item.UID : String.Empty,

        //                        //MATCHNAME = MatchName ?? string.Empty,
        //                        //MATCHSCORE = GetScore(MatchName.Trim().ToUpper(), screen.CustomerFullName.Trim().ToUpper()),
        //                        //MATCHUID = MatchID,
        //                        //MATCHCATEGORY = (item.CATEGORY != null) ? item.CATEGORY : String.Empty,
        //                        //MATCHTYPE = (item.TYPE != null) ? item.TYPE : String.Empty,
        //                        //NATIONALITY = (item.NATIONALITY != null) ? item.NATIONALITY : String.Empty,
        //                        //MATCHDOB = (item.DOB.Count() > 0) ? item.DOB.Select(x => x.DOB).FirstOrDefault() : String.Empty,
        //                        //MATCHIDNUMBER = (item.IDDETAILS.Count() > 0) ? item.IDDETAILS.Select(x => x.IDNUMBER).FirstOrDefault() : String.Empty,
        //                        //AKANAMES = myList
        //                    };
        //                    lst.Add(item);
        //                    //}
        //                }
        //            }
        //            //var lstResult = lst
        //            //    .Where(z => z.MATCHSCORE >= searchScore && z.MATCHSCORE >= screen.Threshold)
        //            //    .GroupBy(x=>x.MATCHUID).Select(x=>x.First())
        //            //    .OrderByDescending(o => o.MATCHSCORE)
        //            //    .Take(50).ToList();

        //            var lstResult = lst
        //                .Where(z => z.MATCHSCORE >= searchScore && z.MATCHSCORE >= screen.Threshold)
        //                .OrderByDescending(o => o.MATCHSCORE)
        //                .Take(50).ToList();
        //            return lstResult;
        //        }

        //        return new List<NAMELIST>();
        //        //return new List<BlackListDto>();

        //    }
        //    catch (Exception ex)
        //    {
        //        Console.Error.WriteLine(ex);
        //        return new List<NAMELIST>();

        //        //return new List<BlackListDto>();
        //    }
        //}

        //Hided By sanjana
        //public async Task<List<BlackListDto>> SearchFuzzy(CustomerScreenDto screen)
        //{
        //    try
        //    {
        //        if (screen.CustomerNationality?.Length == 2 || screen.CustomerNationality?.Length == 3)
        //        {
        //            screen.CustomerNationality = GetFullCountryName(screen.CustomerNationality);
        //        }

        //        screen.CustomerFullName = GetCleanedString(screen.CustomerFullName, true);

        //        double searchScore = 0;  //To Do : Fetch screening score from Mysql DB;
        //        string[] nameList = screen.CustomerFullName.Split(' ').Where(s => s != "").Distinct().ToArray();
        //        if (nameList.Length == 0)
        //            return null;
        //        var soundCodes = new List<string>();
        //        foreach (var names in nameList)
        //        {
        //            string sound = Soundex.SoundexText(GetCleanedString(names));
        //            soundCodes.Add(sound);
        //        }
        //        var activeFilter = Builders<NAMELIST>.Filter.Eq(x => x.STATUS, "A");
        //        var clientidFilter = Builders<NAMELIST>.Filter.Or(
        //                                 Builders<NAMELIST>.Filter.Eq("CLIENTID", screen.ClientId),
        //                                 Builders<NAMELIST>.Filter.Exists("CLIENTID", false),
        //                                 Builders<NAMELIST>.Filter.Eq("CLIENTID", -1)
        //                             );
        //        var soundFilter = Builders<NAMELIST>.Filter.In("SOUNDEX", soundCodes);
        //        //var AkasoundFilter = Builders<NAMELIST>.Filter.In("AKALIST.AKASOUNDEX", soundCodes);
        //        var nationalityEmptyFilter = Builders<NAMELIST>.Filter.Eq(x => x.NATIONALITY, "");
        //        var nationalityFilter = Builders<NAMELIST>.Filter.Where(x => x.NATIONALITY.ToUpper() == screen.CustomerNationality.ToUpper());
        //        bool corporate = false;

        //        if (screen.CustomerType == "C")
        //        {
        //            corporate = true;
        //        }

        //        var Category = Builders<NAMELIST>.Filter.Or(
        //                                 Builders<NAMELIST>.Filter.Eq("CATEGORY", corporate ? "CORPORATE" : "INDIVIDUAL"),
        //                                 Builders<NAMELIST>.Filter.Eq("CATEGORY", corporate ? "ENTITY" : "INDIVIDUAL")
        //                                 );

        //        FilterDefinition<NAMELIST> filter;
        //        if (!string.IsNullOrEmpty(screen.CustomerNationality))
        //        {
        //            filter = Builders<NAMELIST>.Filter.And(activeFilter, clientidFilter, soundFilter, Category, Builders<NAMELIST>.Filter.Or(nationalityEmptyFilter, nationalityFilter));
        //        }
        //        else
        //        {
        //            filter = Builders<NAMELIST>.Filter.And(activeFilter, clientidFilter, soundFilter, Category);
        //        }

        //        var result = _unitOfWorkNameList.FindAll(filter);
        //        var MatchID = "";
        //        var MatchName = "";

        //        if (result.Count() > 0)
        //        {
        //           
        //            List<BlackListDto> lst = new List<BlackListDto>();
        //            foreach (var item in result)
        //            {
        //                MatchID = (item.UID == null) ? String.Empty : (item.UID.Substring(0, 2) == "UN") ? item.SUBCATEGORY : item.UID;

        //                MatchName = item.FULLNAME ?? string.Empty;


        //                if (item.FULLNAME.Contains(screen.CustomerFullName.Trim().ToUpper()) || item.FIRSTNAME.Contains(screen.CustomerFullName.Trim().ToUpper()) || item.LASTNAME.Contains(screen.CustomerFullName.Trim().ToUpper()))
        //                {

        //                   
        //    List<AKALISTDTO> myList = new List<AKALISTDTO>();

        //        if (item.AKALIST != null)
        //        {
        //            foreach (var AKA in item.AKALIST)
        //            {
        //                myList.Add(new AKALISTDTO
        //                {
        //                    AKAUID = AKA.AKAUID,
        //                    AKACATEGORY = AKA.AKACATEGORY,
        //                    AKAFIRST_NAME = AKA.AKAFIRST_NAME,
        //                    AKALAST_NAME = AKA.AKALAST_NAME,
        //                    AKATYPE = AKA.AKATYPE

        //});

        //            }
        //        }

        //        BlackListDto match = new BlackListDto()
        //        {
        //            MATCHNAME = item.FULLNAME ?? string.Empty,
        //            MATCHSCORE = GetScore(item.FULLNAME.Trim().ToUpper(), screen.CustomerFullName.Trim().ToUpper()),
        //            //MATCHUID = (item.UID != null) ? item.UID : String.Empty,
        //            MATCHUID = MatchID,
        //            MATCHCATEGORY = (item.CATEGORY != null) ? item.CATEGORY : String.Empty,
        //            MATCHTYPE = (item.TYPE != null) ? item.TYPE : String.Empty,
        //            NATIONALITY = (item.NATIONALITY != null) ? item.NATIONALITY : String.Empty,
        //            MATCHDOB = (item.DOB.Count() > 0) ? item.DOB.Select(x => x.DOB).FirstOrDefault() : String.Empty,
        //            MATCHIDNUMBER = (item.IDDETAILS.Count() > 0) ? item.IDDETAILS.Select(x => x.IDNUMBER).FirstOrDefault() : String.Empty,
        //            AKANAMES = myList,
        //        };
        //                    lst.Add(lst);

        //                }
        //                else
        //                {

        //                    //var score1 = Math.Round(JaroWinkler.Similarity(item.LASTNAME, screen.CustomerFullName.Trim().ToUpper()));
        //                    //var score2 = Math.Round(JaroWinkler.Similarity(item.FIRSTNAME, screen.CustomerFullName.Trim().ToUpper()));
        //                    //var score3 = Math.Round(JaroWinkler.Similarity(item.FULLNAME, screen.CustomerFullName.Trim().ToUpper()));


        //                    
        //                    //Searching for name match in the Alias Names and adding it to the resultset
        //                    //List<AKALISTDTO> myList = new List<AKALISTDTO>();


        //                    //if (item.AKALIST != null)
        //                    //{
        //                    //    foreach (var AKA in item.AKALIST)
        //                    //    {
        //                    //        myList.Add(new AKALISTDTO
        //                    //        {
        //                    //            AKAUID = AKA.AKAUID,
        //                    //            AKACATEGORY = AKA.AKACATEGORY,
        //                    //            AKAFIRST_NAME = AKA.AKAFIRST_NAME,
        //                    //            AKALAST_NAME = AKA.AKALAST_NAME,
        //                    //            AKATYPE = AKA.AKATYPE

        //                    //        });

        //                    //        var AKAName = AKA.AKAFIRST_NAME + " " + AKA.AKALAST_NAME;
        //                    //        string AKANameMatch = AKAName + " " + "(Alias Name);   " + MatchName + "  " + "(Original Name)" ?? string.Empty;

        //                    //        BlackListDto matchAKA = new BlackListDto()
        //                    //        {
        //                    //            //MATCHNAME = AKAName + " " + "(Alias Name) "  + MatchName + "  " + "(Original Name)" ?? string.Empty,
        //                    //            MATCHNAME = AKANameMatch,
        //                    //            MATCHSCORE = GetScore(AKAName?.Trim()?.ToUpper(), screen.CustomerFullName?.Trim()?.ToUpper()),
        //                    //            MATCHUID = MatchID,
        //                    //            MATCHCATEGORY = (item.CATEGORY != null) ? item.CATEGORY : String.Empty,
        //                    //            MATCHTYPE = (item.TYPE != null) ? item.TYPE : String.Empty,
        //                    //            NATIONALITY = (item.NATIONALITY != null) ? item.NATIONALITY : String.Empty,
        //                    //            MATCHDOB = (item.DOB.Count() > 0) ? item.DOB.Select(x => x.DOB).FirstOrDefault() : String.Empty,
        //                    //            MATCHIDNUMBER = (item.IDDETAILS.Count() > 0) ? item.IDDETAILS.Select(x => x.IDNUMBER).FirstOrDefault() : String.Empty,
        //                    //            AKANAMES = myList
        //                    //        };
        //                    //        lst.Add(matchAKA);

        //                    //    }
        //                    //}


        //                    //BlackListDto match = new BlackListDto()
        //                    //{
        //                    //    //MATCHNAME = item.FULLNAME ?? string.Empty,
        //                    //    //MATCHSCORE = Math.Max(Math.Round(JaroWinkler.Similarity(item.FULLNAME, screen.CustomerFullName.Trim().ToUpper())), Math.Max(Math.Round(JaroWinkler.Similarity(item.FIRSTNAME, screen.CustomerFullName.Trim().ToUpper())), Math.Round(JaroWinkler.Similarity(item.LASTNAME, screen.CustomerFullName.Trim().ToUpper())))),
        //                    //    //MATCHUID = (item.UID != null) ? item.UID : String.Empty,

        //                    //    MATCHNAME = MatchName ?? string.Empty,
        //                    //    MATCHSCORE = GetScore(MatchName.Trim().ToUpper(), screen.CustomerFullName.Trim().ToUpper()),
        //                    //    MATCHUID = MatchID,
        //                    //    MATCHCATEGORY = (item.CATEGORY != null) ? item.CATEGORY : String.Empty,
        //                    //    MATCHTYPE = (item.TYPE != null) ? item.TYPE : String.Empty,
        //                    //    NATIONALITY = (item.NATIONALITY != null) ? item.NATIONALITY : String.Empty,
        //                    //    MATCHDOB = (item.DOB.Count() > 0) ? item.DOB.Select(x => x.DOB).FirstOrDefault() : String.Empty,
        //                    //    MATCHIDNUMBER = (item.IDDETAILS.Count() > 0) ? item.IDDETAILS.Select(x => x.IDNUMBER).FirstOrDefault() : String.Empty,
        //                    //    AKANAMES = myList
        //                    //};
        //                    lst.Add(lst);
        //                    //}
        //                }
        //            }
        //            //var lstResult = lst
        //            //    .Where(z => z.MATCHSCORE >= searchScore && z.MATCHSCORE >= screen.Threshold)
        //            //    .GroupBy(x=>x.MATCHUID).Select(x=>x.First())
        //            //    .OrderByDescending(o => o.MATCHSCORE)
        //            //    .Take(50).ToList();

        //            var lstResult = lst
        //                .Where(z => z.MATCHSCORE >= searchScore && z.MATCHSCORE >= screen.Threshold)
        //                .OrderByDescending(o => o.MATCHSCORE)
        //                .Take(50).ToList();
        //            return lstResult;
        //        }

        //        
        //        return new List<BlackListDto>();

        //    }
        //    catch (Exception ex)
        //    {
        //        Console.Error.WriteLine(ex);
        //        

        //        return new List<BlackListDto>();
        //    }
        //}




        /// <summary>
        /// Calculates the specified aml score.
        /// </summary>
        /// <param name="amlScore">The aml score.</param>
        /// <returns></returns>
        public static MatchDto Calculate(double amlScore)
        {
            try
            {
                MatchDto dto = new MatchDto();
                if (amlScore >= 100)
                {
                    dto.MatchRisk = "High Risk";
                    dto.MatchRange = "100%";
                }
                else if (amlScore >= 90 && amlScore < 100)
                {
                    dto.MatchRisk = "Medium Risk";
                    dto.MatchRange = "90%";
                }
                else if (amlScore >= 80 && amlScore < 90)
                {
                    dto.MatchRisk = "Low Risk";
                    dto.MatchRange = "80%";
                }
                else
                {
                    dto.MatchRisk = "Risk below 80%";
                    dto.MatchRange = "Below 80%";
                }
                return dto;
            }
            catch (Exception ex)
            {
                return new MatchDto
                {
                    MatchRange = "0",
                    MatchRisk = "0"
                };
            }

        }

        //private async void LogFile(string message2, object data)
        //{
        //    string data3 = bool.TryParse(data.ToString(), out bool result) ? result.ToString() : "";
        //    string message = string.Format("Time: {0}", DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss tt"));
        //    message += Environment.NewLine;
        //    message += "-----------------------------------------------------------";
        //    message += Environment.NewLine;
        //    message += message2;
        //    message += Environment.NewLine;

        //    message += string.Format("Message: {0}", data3);



        //    if (!System.IO.File.Exists(@"APISchedulerLogs.txt"))
        //    {
        //        using (System.IO.StreamWriter sw = System.IO.File.CreateText(@"APISchedulerLogs.txt"))
        //        {
        //            sw.WriteLine(message);
        //        }
        //    }
        //    else
        //    {
        //        using (System.IO.StreamWriter sw = System.IO.File.AppendText(@"APISchedulerLogs.txt"))
        //        {
        //            sw.WriteLine(message);
        //        }
        //    }



        //}
    }
}
