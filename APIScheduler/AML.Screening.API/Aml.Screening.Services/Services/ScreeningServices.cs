using Aml.Screening.Common.Algorithms;
using Aml.Screening.DataContracts.Dtos;
using Aml.Screening.MongoModal.Modal;
using Aml.Screening.MongoModal.UnitOfWork;
using Aml.Screening.ServiceContracts.ServiceContracts;
using Aml.Screening.Services.Extensions;
using Aml.Screening.Services.LocalServices;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualBasic;
using MongoDB.Bson;
using MongoDB.Driver;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static Aml.Screening.MongoModal.Modal.NAMELIST;

namespace Aml.Screening.Services.Services
{
    public class ScreeningServices : IScreeningServices
    {
        /// <summary>
        /// The unit of work black list
        /// </summary>
        private readonly UnitOfWork_BlackListRepository _unitOfWorkBlackList;
        /// <summary>
        /// The unit of work name list
        /// </summary>
        private readonly UnitOfWork_NameListRepository _unitOfWorkNameList;
        /// <summary>
        /// The unit of work case log
        /// </summary>
        private readonly UnitOfWork_CaseLogRepository _unitOfWorkCaseLog;
        /// <summary>
        /// The predicate black list
        /// </summary>
        private Expression<Func<BLACKLIST, bool>> _predicate_blackList = null;
        /// <summary>
        /// The predicate bame list
        /// </summary>
        private Expression<Func<NAMELIST, bool>> _predicate_nameList = null;
        /// <summary>
        /// The predicate
        /// </summary>
        private Expression<Func<CASELOG, bool>> _predicate = null;
        /// <summary>
        /// The fuzzy services
        /// </summary>
        private FuzzyServices fuzzyServices;

        /// <summary>
        /// The case services
        /// </summary>
        private CaseLogServices caseServices;
        /// <summary>
        /// Initializes a new instance of the <see cref="ScreeningServices"/> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// 
        
        public ScreeningServices(IConfiguration configuration)
        {
            _unitOfWorkBlackList = new UnitOfWork_BlackListRepository(configuration);
            _unitOfWorkNameList = new UnitOfWork_NameListRepository(configuration);
            _unitOfWorkCaseLog = new UnitOfWork_CaseLogRepository(configuration);
            fuzzyServices = new FuzzyServices(configuration);
            caseServices = new CaseLogServices(configuration);
        }





        /// <summary>
        /// Checks the alias name nationality.
        /// </summary>
        /// <param name="sReq">The s req.</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public Task<ScreeningResponse> CheckAliasNameNationality(CustomerScreenDto sReq)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Checks the name of the exact.
        /// </summary>
        /// <param name="sReq">The s req.</param>
        /// <returns></returns>
        //public async Task<CASELOGMATCH> CheckExactName(CustomerScreenDto dto)
        //{
        //    try
        //    {

        //        List<CASELOGMATCH> lst = new List<CASELOGMATCH>();

        //        FilterDefinition<NAMELIST> filter;

        //        var status = Builders<NAMELIST>.Filter.Eq(x => x.STATUS,"A");
        //        var fullname= Builders<NAMELIST>.Filter.Eq(x => x.FULLNAME,dto.CustomerFullName);
        //        var catergory = Builders<NAMELIST>.Filter.Or(
        //                                 Builders<NAMELIST>.Filter.Eq("CATEGORY", dto.CustomerType),
        //                                 Builders<NAMELIST>.Filter.Eq("CATEGORY", "ENTITY"));

        //        filter = Builders<NAMELIST>.Filter.And(status, fullname, catergory);


        //        //_predicate_nameList = x => x.STATUS.Equals("A");
        //        //_predicate_nameList = _predicate_nameList.And(z => z.FULLNAME.Equals(dto.CustomerFullName));
        //        //_predicate_nameList = _predicate_nameList.And(z => z.CATEGORY.Equals(dto.CustomerType));
        //        //_predicate_nameList =_predicate_nameList.Or(z => z.CATEGORY.Equals("ENTITY"));
        //        Console.WriteLine($"connecting to mongo db, checking with exact name: {DateTime.Now}");
        //        Console.WriteLine($"Caseid: {dto.CaseId}");
        //        Console.WriteLine($"CustomerName: {dto.CustomerFullName}");
        //        Console.WriteLine($"CustomerType: { dto.CustomerType}");
        //        Console.WriteLine($"whitelisting: {dto.WWhitelisting}");
        //        Console.WriteLine($"whitelistingdate: {dto.WhitelistingDate}");

        //        //LogFile("connecting to mongo db, checking with id", dto);
        //        //var result = await _unitOfWorkNameList.FindAllLimitAsync(filter);

        //        var result = await _unitOfWorkNameList.FindOneAsync(filter);

        //        if (result != null)
        //        {
        //            Console.WriteLine($"After getting results from Mongo db {DateTime.Now}");
        //            Console.WriteLine($"Matchuid : { result.UID}");
        //            Console.WriteLine($"MATCHNAME : {result.FULLNAME}");
        //            Console.WriteLine($"Matchuid : {result.CATEGORY}");

        //            //LogFile3("After getting results from Mongo db", result);
        //            CASELOG caseLog = new CASELOG();
        //            caseLog.CASEID = dto.CaseId;

        //            var caseLogMatch = new CASELOGMATCH
        //            {
        //                MATCHUID = result.UID,
        //                MATCHNAME = result.FULLNAME,
        //                MATCHSCORE = 100,
        //                MATCHCATEGORY = (result.CATEGORY != null) ? result.CATEGORY : String.Empty,
        //                MATCHTYPE = (result.TYPE != null) ? result.TYPE : string.Empty,
        //                MATCHNATIONALITY = (result.NATIONALITY.ToUpper() != null) ? result.NATIONALITY.ToUpper() : String.Empty,
        //                MATCHDOB = (result.DOB.Count() > 0) ? result.DOB.Select(x => x.DOB).FirstOrDefault() : String.Empty,
        //                MATCHIDNO = (result.IDDETAILS.Count() > 0) ? result.IDDETAILS.Select(x => x.IDNUMBER).FirstOrDefault() : String.Empty,
        //            };
        //            lst.Add(caseLogMatch);

        //            _predicate = x => x.CASEID.Equals(dto.CaseId);
        //            var result2 = (await _unitOfWorkCaseLog.FindAllAsync(_predicate)).ToList().LastOrDefault();
        //            if (result2 != null && result2.MATCHRECORDS != null)
        //            {
        //                foreach (var item in result2.MATCHRECORDS)
        //                {
        //                    var caseLogMatch2 = new CASELOGMATCH
        //                    {
        //                        MATCHUID = item.MATCHUID,
        //                        MATCHSCORE = item.MATCHSCORE,
        //                        MATCHNAME = item.MATCHNAME,
        //                        MATCHCATEGORY = (item.MATCHCATEGORY != null) ? item.MATCHCATEGORY : String.Empty,
        //                        MATCHTYPE = (item.MATCHTYPE != null) ? item.MATCHTYPE : String.Empty,
        //                        MATCHNATIONALITY = (item.MATCHNATIONALITY != null) ? item.MATCHNATIONALITY : String.Empty,
        //                        MATCHDOB = (item.MATCHDOB != null) ? item.MATCHDOB : String.Empty,
        //                        MATCHIDNO = (item.MATCHIDNO != null) ? item.MATCHIDNO : String.Empty
        //                    };
        //                    lst.Add(caseLogMatch2);
        //                }

        //                caseLog._id = result2._id;
        //                caseLog.MATCHRECORDS = lst;
        //            }


        //            var insResult = await caseServices.Add(caseLog);
        //            return caseLogMatch;
        //        }
        //        return new CASELOGMATCH
        //        {
        //            MATCHUID = string.Empty
        //        };

        //    }
        //    catch (Exception ex)
        //    {
        //        return new CASELOGMATCH
        //        {
        //            MATCHUID = string.Empty
        //        };
        //    }
        //}

        public async Task<CASELOGMATCH> CheckExactName(CustomerScreenDto dto)
        {
            try
            {
                List<CASELOGMATCH> lst = new List<CASELOGMATCH>();
                DateTime whitelistDate;

                // ✅ Filters
                //var status = Builders<NAMELIST>.Filter.Eq(x => x.STATUS, "A");

                //var fullname = Builders<NAMELIST>.Filter.Eq(x => x.FULLNAME, dto.CustomerFullName);

                //var category = Builders<NAMELIST>.Filter.Or(
                //                    Builders<NAMELIST>.Filter.Eq("CATEGORY", dto.CustomerType),
                //                    Builders<NAMELIST>.Filter.Eq("CATEGORY", "ENTITY")
                //               );

                //var filter = Builders<NAMELIST>.Filter.And(status, fullname, category);

                //Expression<Func<NAMELIST, bool>> filter = x =>
                //        x.STATUS == "A" &&
                //        x.FULLNAME == dto.CustomerFullName &&
                //        (x.CATEGORY == dto.CustomerType || x.CATEGORY == "ENTITY");

                Expression<Func<NAMELIST, bool>> filter = x =>
                        x.FULLNAME == dto.CustomerFullName &&
                        (x.CATEGORY == dto.CustomerType || x.CATEGORY == "ENTITY");

                Console.WriteLine($"connecting to mongo db, checking with exact name: {DateTime.Now}");
                Console.WriteLine($"Caseid: {dto.CaseId}");
                Console.WriteLine($"CustomerName: {dto.CustomerFullName}");
                Console.WriteLine($"CustomerType: {dto.CustomerType}");
                Console.WriteLine($"whitelisting: {dto.WWhitelisting}");
                Console.WriteLine($"whitelistingdate: {dto.WhitelistingDate}");

                // ✅ Get ALL matching records
                var results = (await _unitOfWorkNameList.FindAllAsync(filter)).ToList();

                // ✅ Apply Whitelist filtering
                

                // ✅ Whitelisting formats (handles ALL cases)
               
                if (dto.WWhitelisting?.Equals("YES", StringComparison.OrdinalIgnoreCase) == true
                    && DateTime.TryParse(dto.WhitelistingDate, out whitelistDate))
                {
                    var formats = new[]
                       {
                            "dd-MMM-yy h:mm:ss tt",   // ✅ your current format
                            "dd-MMM-yyyy h:mm:ss tt",
                            "dd/MM/yyyy HH:mm:ss",
                            "dd-MM-yyyy HH:mm:ss"
                        };
                    results = results.Where(x =>
                    {
                        if (!string.IsNullOrEmpty(x.CREATEDON) &&
                            DateTime.TryParseExact(
                                x.CREATEDON,
                                formats, // ✅ multiple formats
                                CultureInfo.InvariantCulture,
                                DateTimeStyles.None,
                                out var createdDate))
                        {
                            return createdDate.Date >= whitelistDate.Date;
                        }
                        return false;
                    }).ToList();
                }

                if (results != null && results.Any())
                {
                    CASELOG caseLog = new CASELOG();
                    caseLog.CASEID = dto.CaseId;

                    Console.WriteLine($"Total Matches After Filter: {results.Count}");

                    // ✅ Loop through ALL filtered matches
                    foreach (var result in results)
                    {
                        var caseLogMatch = new CASELOGMATCH
                        {
                            MATCHUID = result.UID,
                            MATCHNAME = result.FULLNAME,
                            MATCHSCORE = 100,
                            MATCHCATEGORY = result.CATEGORY ?? string.Empty,
                            MATCHTYPE = result.TYPE ?? string.Empty,
                            MATCHNATIONALITY = result.NATIONALITY != null ? result.NATIONALITY.ToUpper() : string.Empty,
                            MATCHDOB = result.DOB?.Count > 0 ? result.DOB.Select(x => x.DOB).FirstOrDefault() : string.Empty,
                            MATCHIDNO = result.IDDETAILS?.Count > 0 ? result.IDDETAILS.Select(x => x.IDNUMBER).FirstOrDefault() : string.Empty,
                            FLAGTYPE="OM",
                            STATUS=result.STATUS
                        };

                        lst.Add(caseLogMatch);
                    }

                    // ✅ Merge with existing CASELOG
                    _predicate = x => x.CASEID.Equals(dto.CaseId);
                    var result2 = (await _unitOfWorkCaseLog.FindAllAsync(_predicate)).LastOrDefault();

                    if (result2 != null && result2.MATCHRECORDS != null)
                    {
                        foreach (var item in result2.MATCHRECORDS)
                        {
                            lst.Add(new CASELOGMATCH
                            {
                                MATCHUID = item.MATCHUID,
                                MATCHSCORE = item.MATCHSCORE,
                                MATCHNAME = item.MATCHNAME,
                                MATCHCATEGORY = item.MATCHCATEGORY ?? string.Empty,
                                MATCHTYPE = item.MATCHTYPE ?? string.Empty,
                                MATCHNATIONALITY = item.MATCHNATIONALITY ?? string.Empty,
                                MATCHDOB = item.MATCHDOB ?? string.Empty,
                                MATCHIDNO = item.MATCHIDNO ?? string.Empty,
                                STATUS = item.STATUS
                            });
                        }

                        caseLog._id = result2._id;
                    }

                    caseLog.MATCHRECORDS = lst;

                    await caseServices.Add(caseLog);

                    return lst.FirstOrDefault(); // keep method signature same
                }

                return new CASELOGMATCH
                {
                    MATCHUID = string.Empty
                };
            }
            catch (Exception)
            {
                return new CASELOGMATCH
                {
                    MATCHUID = string.Empty
                };
            }
        }

        public async Task<IEnumerable<CASELOGMATCH>> CheckFuzzyName(CustomerScreenDto dto)
        {
            try
            {

                var result = await fuzzyServices.FuzzySearchOffline(dto);
                if (result != null)
                {
                    if (!string.IsNullOrWhiteSpace(result.CASEID))
                    {
                        var insResult = await caseServices.Add(result);
                    }
                    return result.MATCHRECORDS;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<CASELOGMATCH> CheckId(CustomerScreenDto dto)
        {
            try
            {



                _predicate_blackList = x => x.STATUS.Equals("A");
                _predicate_blackList = _predicate_blackList.And(z => z.IDLIST.Any(o => o.IDNUMBER.Equals(dto.CustomerIdNumber)));
                Console.WriteLine($"connecting to mongo db, checking with id : {DateTime.Now}");
                //LogFile("connecting to mongo db, checking with id", dto);
                Console.WriteLine($"case id : { dto.CaseId}");
                Console.WriteLine($"customername : {dto.CustomerFullName}");
                Console.WriteLine($"customertype :  {dto.CustomerType}");
                Console.WriteLine($"customeridnumber : {dto.CustomerIdNumber}");
                //Console.WriteLine("case id", dto.CaseId);

                var result = await _unitOfWorkBlackList.FindOneAsync(predicate: _predicate_blackList);

                if (result != null)
                {
                    Console.WriteLine($"After getting results from Mongo db  : {DateTime.Now}");
                    Console.WriteLine($"Matchuid : {result.UID}");
                    Console.WriteLine($"MATCHNAME :  {result.FULLNAME}");
                    Console.WriteLine($"MATCHCATEGORY :  {result.CATEGORY}");

                    //LogFile2("After getting results from Mongo db", result);
                    CASELOG caseLog = new CASELOG();
                    caseLog.CASEID = dto.CaseId;
                    List<CASELOGMATCH> lst = new List<CASELOGMATCH>();
                    var caseLogMatch = new CASELOGMATCH
                    {
                        MATCHUID = result.UID,
                        MATCHSCORE = 100,
                        MATCHNAME = result.FULLNAME,
                        MATCHCATEGORY = (result.CATEGORY != null) ? result.CATEGORY : String.Empty,
                        MATCHTYPE = (result.TYPE != null) ? result.TYPE : String.Empty,
                        MATCHNATIONALITY = (result.NATIONALITYLIST.Count() > 0) ? result.NATIONALITYLIST.Select(x => x.NATCOUNTRY).FirstOrDefault() : String.Empty,
                        MATCHDOB = (result.DOBLIST.Count() > 0) ? result.DOBLIST.Select(x => x.DOB).FirstOrDefault() : String.Empty,
                        MATCHIDNO = (result.IDLIST.Count() > 0) ? result.IDLIST.Select(x => x.IDNUMBER).FirstOrDefault() : String.Empty,
                    };
                    lst.Add(caseLogMatch);




                    caseLog.MATCHRECORDS = lst;
                    var insResult = await caseServices.Add(caseLog);
                    return caseLogMatch;
                }
                return new CASELOGMATCH
                {
                    MATCHUID = string.Empty
                };
            }
            catch (Exception ex)
            {
                return new CASELOGMATCH
                {
                    MATCHUID = string.Empty
                };
            }
        }

        public Task<ScreeningResponse> CheckNameDob(CustomerScreenDto sReq)
        {
            throw new NotImplementedException();
        }

        public Task<ScreeningResponse> CheckNameNationality(CustomerScreenDto sReq)
        {
            throw new NotImplementedException();
        }

        public async Task<ScreeningResponse> CheckNameNationalityDob(CustomerScreenDto sReq)
        {
            try
            {
                _predicate_nameList = x => x.FULLNAME.Equals(sReq.CustomerFullName);
                _predicate_nameList = _predicate_nameList.And(x => x.NATIONALITY.Equals(sReq.CustomerNationality));
                _predicate_nameList = _predicate_nameList.And(x => x.DOB.Any(z => z.DOB.Equals(sReq.CustomerDob)));
                var result = await _unitOfWorkNameList.FindOneAsync(predicate: _predicate_nameList);
                return result != null ? new ScreeningResponse
                {
                    IsPositive = true,
                    MatchUID = result.UID,
                    MatchName = result.FULLNAME,
                    AuthFlag = "P",
                    MatchCategory = result.CATEGORY,
                    MatchCount = "1",
                    MatchPercentage = "100%",
                    MatchRisk = "HighRisk",
                    MatchType = result.TYPE
                } : new ScreeningResponse
                {
                    IsPositive = false
                }
                ;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public string Normalize(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            return System.Text.RegularExpressions.Regex
                .Replace(input.Trim(), @"\s+", " ")  // replace multiple spaces with single
                .ToLower();
        }
        #region Casual name Search
        public async Task<List<CASELOGMATCH>> SearchNameNationalityDob(CustomerScreenDto sReq)
        {
            try
            {

                var pattern = System.Text.RegularExpressions.Regex
    .Replace(sReq.CustomerFullName.Trim(), @"\s+", "\\s+");

                _predicate_nameList = x =>
                    System.Text.RegularExpressions.Regex.IsMatch(
                        x.FULLNAME, pattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                // ✅ Nationality filter (only if not null/empty)
                if (!string.IsNullOrWhiteSpace(sReq.CustomerNationality))
                {
                    _predicate_nameList = _predicate_nameList.And(x =>
                        x.NATIONALITY != null &&
                        x.NATIONALITY.Equals(sReq.CustomerNationality));
                }

                // ✅ DOB filter (only if not null/empty)
                if (!string.IsNullOrWhiteSpace(sReq.CustomerDob))
                {
                    _predicate_nameList = _predicate_nameList.And(x =>
                        x.DOB.Any(z => z.DOB != null && z.DOB.Equals(sReq.CustomerDob)));
                }
                if (sReq.CustomerType == "CORPORATE")
                {
                    _predicate_nameList = _predicate_nameList.And(x =>
                        x.CATEGORY == "CORPORATE" || x.CATEGORY == "ENTITY");
                }
                else if (sReq.CustomerType == "Individual")
                {
                    _predicate_nameList = _predicate_nameList.And(x =>
                        x.CATEGORY == "INDIVIDUAL");
                }
                _predicate_nameList = _predicate_nameList.And(x =>
                        x.TYPE == "UN" || x.TYPE == "UAE IEC LIST");
               
                                       
                //_predicate_nameList = x => x.FULLNAME.Equals(sReq.CustomerFullName);
                //_predicate_nameList = _predicate_nameList.And(x => x.NATIONALITY.Equals(sReq.CustomerNationality));
                //_predicate_nameList = _predicate_nameList.And(x => x.DOB.Any(z => z.DOB.Equals(sReq.CustomerDob)));
                var result = (await _unitOfWorkNameList.FindAllAsync(predicate: _predicate_nameList)).ToList();
                List<CASELOGMATCH> lst = new List<CASELOGMATCH>();
                if (result.Count() > 0)
                {
                    foreach (var item in result)
                    {
                        //item.MATCHSCORE = 100;
                        var caseLogMatch = new CASELOGMATCH
                        {
                            MATCHUID = item.UID,
                            MATCHNAME = item.FULLNAME,
                            MATCHSCORE = 100,
                            MATCHCATEGORY = (item.CATEGORY != null) ? item.CATEGORY : String.Empty,
                            MATCHTYPE = (item.TYPE != null) ? item.TYPE : string.Empty,
                            MATCHNATIONALITY = (item.NATIONALITY.ToUpper() != null) ? item.NATIONALITY.ToUpper() : String.Empty,
                            MATCHDOB = (item.DOB.Count() > 0) ? item.DOB.Select(x => x.DOB).FirstOrDefault() : String.Empty,
                            MATCHIDNO = (item.IDDETAILS.Count() > 0) ? item.IDDETAILS.Select(x => x.IDNUMBER).FirstOrDefault() : String.Empty,
                            STATUS=item.STATUS,
                        };
                        //NAMELIST dto = new NAMELIST
                        //{
                        //    MATCHUID = item.UID,
                        //    MATCHSCORE = 100,
                        //    MATCHNAME = item.FULLNAME,
                        //    MATCHCATEGORY = (item.CATEGORY != null) ? item.CATEGORY : String.Empty,
                        //    MATCHTYPE = (item.TYPE != null) ? item.TYPE : String.Empty,
                        //    NATIONALITY = (item.NATIONALITY != null) ? item.NATIONALITY : String.Empty,
                        //    MATCHDOB = (item.DOB.Count() > 0) ? item.DOB.Select(x => x.DOB).FirstOrDefault() : String.Empty,
                        //    CLIENTID = item.CLIENTID,
                        //    MATCHIDNUMBER = (item.IDDETAILS.Count() > 0) ? item.IDDETAILS.Select(x => x.IDNUMBER).FirstOrDefault() : String.Empty
                        //};
                        lst.Add(caseLogMatch);
                    }
                }
                return lst;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return new List<CASELOGMATCH>();
                //return new List<BlackListDto>();
            }
        }
        public async Task<List<CASELOGMATCH>> SearchNameDob(CustomerScreenDto sReq)
        {
            try
            {
                _predicate_nameList = x => x.FULLNAME.Equals(sReq.CustomerFullName);
                _predicate_nameList = _predicate_nameList.And(x => x.DOB.Any(z => z.DOB.Equals(sReq.CustomerDob)));
                var result = (await _unitOfWorkNameList.FindAllAsync(predicate: _predicate_nameList)).ToList();
                List<CASELOGMATCH> lst = new List<CASELOGMATCH>();
                if (result.Count() > 0)
                {
                    foreach (var item in result)
                    {

                        item.MATCHSCORE = 100;
                        var caseLogMatch = new CASELOGMATCH
                        {
                            //MATCHUID = item.UID,
                            //MATCHNAME = item.FULLNAME,
                            MATCHSCORE = 100,
                            //MATCHCATEGORY = (item.CATEGORY != null) ? item.CATEGORY : String.Empty,
                            //MATCHTYPE = (item.TYPE != null) ? item.TYPE : string.Empty,
                            //MATCHNATIONALITY = (item.NATIONALITY.ToUpper() != null) ? item.NATIONALITY.ToUpper() : String.Empty,
                            //MATCHDOB = (item.DOB.Count() > 0) ? item.DOB.Select(x => x.DOB).FirstOrDefault() : String.Empty,
                            //MATCHIDNO = (item.IDDETAILS.Count() > 0) ? item.IDDETAILS.Select(x => x.IDNUMBER).FirstOrDefault() : String.Empty,
                        };
                        //BlackListDto dto = new BlackListDto
                        //{
                        //    MATCHUID = item.UID,
                        //    MATCHSCORE = 100,
                        //    MATCHNAME = item.FULLNAME,
                        //    MATCHCATEGORY = (item.CATEGORY != null) ? item.CATEGORY : String.Empty,
                        //    MATCHTYPE = (item.TYPE != null) ? item.TYPE : String.Empty,
                        //    NATIONALITY = (item.NATIONALITY != null) ? item.NATIONALITY : String.Empty,
                        //    MATCHDOB = (item.DOB.Count() > 0) ? item.DOB.Select(x => x.DOB).FirstOrDefault() : String.Empty,
                        //    CLIENTID = item.CLIENTID,
                        //    MATCHIDNUMBER = (item.IDDETAILS.Count() > 0) ? item.IDDETAILS.Select(x => x.IDNUMBER).FirstOrDefault() : String.Empty
                        //};
                        lst.Add(caseLogMatch);
                    }
                }
                return lst;
            }
            catch (Exception ex)
            {
                return new List<CASELOGMATCH>();
                //return new List<BlackListDto>();
            }
        }


        public async Task<List<CASELOGMATCH>> SearchNameNationality(CustomerScreenDto sReq)
        {
            try
            {
                _predicate_nameList = x => x.FULLNAME.Equals(sReq.CustomerFullName);
                _predicate_nameList = _predicate_nameList.And(x => x.NATIONALITY.Equals(sReq.CustomerNationality));
                var result = (await _unitOfWorkNameList.FindAllAsync(predicate: _predicate_nameList)).ToList();
                List<NAMELIST> lst = new List<NAMELIST>();
                List<CASELOGMATCH> cASELOGMATCHes = new List<CASELOGMATCH>();
                if (result.Count() > 0)
                {
                    foreach (var item in result)
                    {

                        //item.MATCHSCORE = 100;
                        var caseLogMatch = new CASELOGMATCH
                        {
                            //MATCHUID = item.UID,
                            //MATCHNAME = item.FULLNAME,
                            MATCHSCORE = 100,
                            //MATCHCATEGORY = (item.CATEGORY != null) ? item.CATEGORY : String.Empty,
                            //MATCHTYPE = (item.TYPE != null) ? item.TYPE : string.Empty,
                            //MATCHNATIONALITY = (item.NATIONALITY.ToUpper() != null) ? item.NATIONALITY.ToUpper() : String.Empty,
                            //MATCHDOB = (item.DOB.Count() > 0) ? item.DOB.Select(x => x.DOB).FirstOrDefault() : String.Empty,
                            //MATCHIDNO = (item.IDDETAILS.Count() > 0) ? item.IDDETAILS.Select(x => x.IDNUMBER).FirstOrDefault() : String.Empty,
                        };
                        //BlackListDto dto = new BlackListDto
                        //{
                        //    MATCHUID = item.UID,
                        //    MATCHSCORE = 100,
                        //    MATCHNAME = item.FULLNAME,
                        //    MATCHCATEGORY = (item.CATEGORY != null) ? item.CATEGORY : String.Empty,
                        //    MATCHTYPE = (item.TYPE != null) ? item.TYPE : String.Empty,
                        //    NATIONALITY = (item.NATIONALITY != null) ? item.NATIONALITY : String.Empty,
                        //    MATCHDOB = (item.DOB.Count() > 0) ? item.DOB.Select(x => x.DOB).FirstOrDefault() : String.Empty,
                        //    CLIENTID = item.CLIENTID,
                        //    MATCHIDNUMBER = (item.IDDETAILS.Count() > 0) ? item.IDDETAILS.Select(x => x.IDNUMBER).FirstOrDefault() : String.Empty
                        //};
                        cASELOGMATCHes.Add(caseLogMatch);
                    }
                }
                return cASELOGMATCHes;
            }
            catch (Exception ex)
            {
                return new List<CASELOGMATCH>();
            }
        }

        //hided by sanjana
        //public async Task<List<BlackListDto>> SearchNameNationality(CustomerScreenDto sReq)
        //{
        //    try
        //    {
        //        _predicate_nameList = x => x.FULLNAME.Equals(sReq.CustomerFullName);
        //        _predicate_nameList = _predicate_nameList.And(x => x.NATIONALITY.Equals(sReq.CustomerNationality));
        //        var result = (await _unitOfWorkNameList.FindAllAsync(predicate: _predicate_nameList)).ToList();
        //        List<BlackListDto> lst = new List<BlackListDto>();
        //        if (result.Count() > 0)
        //        {
        //            foreach (var item in result)
        //            {
        //                BlackListDto dto = new BlackListDto
        //                {
        //                    MATCHUID = item.UID,
        //                    MATCHSCORE = 100,
        //                    MATCHNAME = item.FULLNAME,
        //                    MATCHCATEGORY = (item.CATEGORY != null) ? item.CATEGORY : String.Empty,
        //                    MATCHTYPE = (item.TYPE != null) ? item.TYPE : String.Empty,
        //                    NATIONALITY = (item.NATIONALITY != null) ? item.NATIONALITY : String.Empty,
        //                    MATCHDOB = (item.DOB.Count() > 0) ? item.DOB.Select(x => x.DOB).FirstOrDefault() : String.Empty,
        //                    CLIENTID = item.CLIENTID,
        //                    MATCHIDNUMBER = (item.IDDETAILS.Count() > 0) ? item.IDDETAILS.Select(x => x.IDNUMBER).FirstOrDefault() : String.Empty
        //                };
        //                lst.Add(dto);
        //            }
        //        }
        //        return lst;
        //    }
        //    catch (Exception ex)
        //    {
        //        return new List<BlackListDto>();
        //    }
        //}



        public async Task<List<CASELOGMATCH>> SearchFuzzy(CustomerScreenDto dto)
        {
            try
            {
                var result = await fuzzyServices.SearchFuzzy(dto);
                return result;
            }
            catch (Exception ex)
            {
                return new List<CASELOGMATCH>();
            }
        }

        //hided by sanjana
        //public async Task<List<BlackListDto>> SearchFuzzy(CustomerScreenDto dto)
        //{
        //    try
        //    {
        //        var result = await fuzzyServices.SearchFuzzy(dto);
        //        return result;
        //    }
        //    catch (Exception ex)
        //    {
        //        return new List<BlackListDto>();
        //    }
        //}
        //public async Task<List<BlackListDto>> SearchFuzzyAKA(BlackListDto dto)
        //{
        //    try
        //    {
        //        var result = await fuzzyServices.SearchFuzzyAKA(dto);
        //        return result;
        //    }
        //    catch (Exception ex)
        //    {
        //        return new List<BlackListDto>();
        //    }
        //}
        public async Task<IEnumerable<FuzzyFilterDto>> GetFuzzyMatches(CustomerScreenDto dto)
        {
            try
            {
                var result = await fuzzyServices.FuzzySearchOffline(dto);
                return null;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion
        public async Task<ScreeningResponse> GetBlackListByUID(CustomerScreenDto sReq)
        {
            try
            {
                _predicate_blackList = x => x.STATUS.Equals("A");
                _predicate_blackList = _predicate_blackList.And(z => z.IDLIST.Any(o => o.IDNUMBER.Equals(sReq.CustomerIdNumber)));
                var result = await _unitOfWorkBlackList.FindOneAsync(predicate: _predicate_blackList);
                return result != null ? new ScreeningResponse
                {
                    IsPositive = true,
                    MatchUID = result.UID,
                    MatchName = result.FULLNAME,
                    AuthFlag = "P",
                    MatchCategory = result.CATEGORY,
                    MatchCount = "1",
                    MatchPercentage = "100%",
                    MatchRisk = "HighRisk",
                    MatchType = result.TYPE,
                    MatchRemarks = Resource.CustomerIDMatch
                } : new ScreeningResponse
                {
                    IsPositive = false
                }
                                ;
            }
            catch (Exception ex)
            {
                return new ScreeningResponse
                {
                    IsPositive = false
                };
            }
        }
        public async Task<List<CASELOGMATCH>> SearchBlacklist(SearchDto dto)
        {
            try
            {
                var param = dto.ToOfflineCustomerScreenDto();
                if (dto.SEARCHTYPE == "E")
                    return await SearchNameNationalityDob(param);
                else if (dto.SEARCHTYPE == "P")
                {
                    var res = await SearchNameNationality(param);
                    if (res.Count() > 0) return res;
                    else
                        return await SearchNameDob(param);
                }

                else
                    return await SearchFuzzy(param);
            }
            catch (Exception)
            {

                throw;
            }
            return null;
        }

        //public async Task<List<BlackListDto>> SearchBlacklist(SearchDto dto)
        //{
        //    try
        //    {
        //        var param = dto.ToOfflineCustomerScreenDto();
        //        if (dto.SEARCHTYPE == "E")
        //            return await SearchNameNationalityDob(param);
        //        else if (dto.SEARCHTYPE == "P")
        //        {
        //            var res = await SearchNameNationality(param);
        //            if (res.Count() > 0) return res;
        //            else
        //                return await SearchNameDob(param);
        //        }

        //        else
        //            return await SearchFuzzy(param);
        //    }
        //    catch (Exception)
        //    {

        //        throw;
        //    }
        //    return null;
        //}

        //public async Task<List<BlackListDto>> GetAliasNames(BlackListDto dto)
        //{
        //    try
        //    {

        //        return await SearchFuzzyAKA(dto);
        //    }

        //    catch (Exception)
        //    {

        //        throw;
        //    }
        //    return null;
        //}
        internal async Task<ServiceResponse> AddBlackList(BLACKLIST per)
        {
            try
            {
                CultureInfo en = new CultureInfo("en-US");
                #region Find & Delete
                _predicate_nameList = x => x.UID.Equals(per.UID);
                var depF = await _unitOfWorkNameList.FindAllAsync(_predicate_nameList);
                if (depF.Count() > 0)
                {
                    //Exists remove and add new updated item
                    var delres = _unitOfWorkNameList.DeleteMAny(_predicate_nameList);
                }
                #endregion                
                #region Name & Fuzzy
                per.FULLNAME = Regex.Replace(Regex.Replace(per.FULLNAME.Trim().ToUpper(), @"[-]", " "), @"[^\w\d\s]", "");
                per.FIRST_NAME = Regex.Replace(Regex.Replace(per.FIRST_NAME.Trim(), @"[-]", " "), @"[^\w\d\s]", "");
                per.LAST_NAME = Regex.Replace(Regex.Replace(per.LAST_NAME.Trim(), @"[-]", " "), @"[^\w\d\s]", "");
                per.MIDDLE_NAME = Regex.Replace(Regex.Replace(per.MIDDLE_NAME.Trim(), @"[-]", " "), @"[^\w\d\s]", "");
                string[] fname = per.FULLNAME.Trim().Split(' ');
                var S_dxName = new List<string>();
                foreach (var names in fname)
                {
                    string S_Fname = Soundex.SoundexText(names.Trim().ToUpper());
                    S_dxName.Add(S_Fname);

                }
                #endregion
                #region IDList
                var ID_Details = new List<IDDETAIL>();
                if (per.IDLIST != null && per.IDLIST.Count() > 0)
                {
                    foreach (var iddet in per.IDLIST)
                    {
                        IDDETAIL docID = new IDDETAIL
                        {
                            IDTYPE = iddet.IDTYPE,
                            IDNUMBER = iddet.IDNUMBER
                        };
                        ID_Details.Add(docID);
                    }
                }
                #endregion
                #region Nationality
                string Nationality = "";
                if (per.NATIONALITYLIST != null && per.NATIONALITYLIST.Count > 0)
                {
                    if (per.NATIONALITYLIST.FirstOrDefault() != null)
                        Nationality = per.NATIONALITYLIST.FirstOrDefault().NATCOUNTRY;
                }
                #endregion
                #region DOBlist
                var dxDOB = new List<DOBLIST>();
                if (per.DOBLIST != null && per.DOBLIST.Count() > 0)
                {
                    foreach (var item in per.DOBLIST)
                    {
                        dxDOB.Add(new DOBLIST
                        {
                            DOBUID = item.DOBUID,
                            DOB = item.DOB,
                            AGE = item.AGE,
                            ASOFDATE = item.ASOFDATE,
                            DOBMAINENTRY = item.DOBMAINENTRY,
                            ISDECEASED = item.ISDECEASED
                        });
                    }
                }
                #endregion
                string id = "0";
                NAMELIST docnew = new NAMELIST
                {
                    P_id = "0",
                    UID = per.UID,
                    CATEGORY = string.IsNullOrEmpty(per.CATEGORY) ? "" : per.CATEGORY.Trim().ToUpper(),
                    SUBCATEGORY = string.IsNullOrEmpty(per.SUBCATEGORY) ? "" : per.SUBCATEGORY.Trim().ToUpper(),
                    FULLNAME = string.IsNullOrEmpty(per.FULLNAME) ? "" : per.FULLNAME.Trim().ToUpper(),
                    FULLNAME_NL = string.IsNullOrEmpty(per.FULLALIAS) ? "" : per.FULLALIAS.Trim(),
                    SOUNDEX = S_dxName,
                    FIRSTNAME = string.IsNullOrEmpty(per.FIRST_NAME) ? "" : per.FIRST_NAME.Trim().ToUpper(),
                    LASTNAME = string.IsNullOrEmpty(per.LAST_NAME) ? "" : per.LAST_NAME.Trim().ToUpper(),
                    IDDETAILS = ID_Details,
                    NATIONALITY = Nationality,
                    DOB = dxDOB,
                    CREATEDON = DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss"),
                    TYPE = string.IsNullOrEmpty(per.TYPE) ? "" : per.TYPE.ToUpper(),
                    STATUS = "A",
                    CLIENTID = per.CLIENTID,
                    REMARKS = string.IsNullOrEmpty(per.REMARKS) ? "" : per.REMARKS.ToUpper()
                };
                _unitOfWorkNameList.InsertOne(docnew);
                id = docnew._id.ToString();
                //Alias Conversation
                #region Aliaslist Conversation for soundlike matching

                if (per.AKALIST != null && per.AKALIST.Count() > 0)
                {
                    foreach (var item in per.AKALIST)
                    {
                        var S_dxAKA = new List<string>();
                        string FULLName = item.AKAFIRST_NAME.Trim().ToUpper() + " " + item.AKALAST_NAME.Trim().ToUpper();
                        FULLName = Regex.Replace(Regex.Replace(FULLName.Trim().ToUpper(), @"[-]", " "), @"[^\w\d\s]", "");
                        string[] AKaname = FULLName.Trim().Split(' ');
                        foreach (var names in AKaname)
                        {
                            string S_Fname = Soundex.SoundexText(names.Trim().ToUpper());
                            S_dxAKA.Add(S_Fname);
                        }
                        NAMELIST docalias = new NAMELIST
                        {
                            P_id = "0",
                            UID = per.UID,
                            CATEGORY = string.IsNullOrEmpty(per.CATEGORY) ? "" : per.CATEGORY.Trim().ToUpper(),
                            SUBCATEGORY = string.IsNullOrEmpty(per.SUBCATEGORY) ? "" : per.SUBCATEGORY.Trim().ToUpper(),
                            FULLNAME = string.IsNullOrEmpty(per.FULLNAME) ? "" : per.FULLNAME.Trim().ToUpper(),
                            FULLNAME_NL = string.IsNullOrEmpty(per.FULLALIAS) ? "" : per.FULLALIAS.Trim(),
                            SOUNDEX = S_dxName,
                            FIRSTNAME = string.IsNullOrEmpty(item.AKAFIRST_NAME) ? "" : item.AKAFIRST_NAME.Trim().ToUpper(),
                            LASTNAME = string.IsNullOrEmpty(item.AKALAST_NAME) ? "" : item.AKALAST_NAME.Trim().ToUpper(),
                            IDDETAILS = ID_Details,
                            NATIONALITY = Nationality,
                            DOB = dxDOB,
                            CREATEDON = DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss"),
                            TYPE = string.IsNullOrEmpty(per.TYPE) ? "" : per.TYPE.ToUpper(),
                            STATUS = "A",
                            CLIENTID = per.CLIENTID,
                            REMARKS = string.IsNullOrEmpty(per.REMARKS) ? "" : per.REMARKS.ToUpper(),
                        };
                        _unitOfWorkNameList.InsertOne(docalias);
                    }
                }
                #endregion
                var blresult = await InsertBlackList(per, id);
                return new ServiceResponse()
                {
                    ResponseCode = "200",
                    ResponseMessage = "Process Success"
                };
            }
            catch (Exception ex)
            {
                return new ServiceResponse()
                {
                    ResponseCode = "500",
                    ResponseMessage = "Internal Exception"
                };
            }
        }

        public async Task<string> InsertBlackList(BLACKLIST Per, string Obid)
        {
            try
            {
                CultureInfo en = new CultureInfo("en-US");
                #region Find & Delete
                _predicate_blackList = x => x.UID.Equals(Per.UID);
                var depF = await _unitOfWorkBlackList.FindAllAsync(_predicate_blackList);
                if (depF.Count() > 0)
                {
                    Per.CREATEDDATE = depF.FirstOrDefault() != null ? depF.First().CREATEDDATE : DateTime.UtcNow;
                    //Exists remove and add new updated item
                    var delres = _unitOfWorkBlackList.DeleteMAny(_predicate_blackList);
                }
                else
                {
                    Per.CREATEDDATE = DateTime.UtcNow;
                }
                #endregion                
                #region Aliaslist
                var dxAKA = new List<AKALIST>();
                if (Per.AKALIST != null && Per.AKALIST.Count() > 0)
                {
                    foreach (var item in Per.AKALIST)
                    {
                        dxAKA.Add(new AKALIST
                        {
                            AKAUID = item.AKAUID,
                            AKATYPE = string.IsNullOrEmpty(item.AKATYPE) ? "" : item.AKATYPE.ToUpper(),
                            AKACATEGORY = string.IsNullOrEmpty(item.AKACATEGORY) ? "" : item.AKACATEGORY.ToUpper(),
                            AKAFIRST_NAME = string.IsNullOrEmpty(item.AKAFIRST_NAME) ? "" : item.AKAFIRST_NAME.ToUpper(),
                            AKALAST_NAME = string.IsNullOrEmpty(item.AKALAST_NAME) ? "" : item.AKALAST_NAME.ToUpper()
                        });
                    }
                }
                #endregion
                #region IDlist
                var dxIDS = new List<IDLIST>();
                if (Per.IDLIST != null && Per.IDLIST.Count() > 0)
                {
                    foreach (var item in Per.IDLIST)
                    {
                        dxIDS.Add(new IDLIST
                        {
                            IDUID = item.IDUID,
                            IDTYPE = item.IDTYPE,
                            IDNUMBER = item.IDNUMBER,
                            IDISSUEDATE = item.IDISSUEDATE,
                            IDEXPIRYDATE = item.IDEXPIRYDATE,
                            IDCOUNTRY = item.IDCOUNTRY
                        });
                    }
                }
                #endregion
                #region Natlist
                var dxNAT = new List<NATIONALITYLIST>();
                if (Per.NATIONALITYLIST != null && Per.NATIONALITYLIST.Count() > 0)
                {
                    foreach (var item in Per.NATIONALITYLIST)
                    {
                        dxNAT.Add(new NATIONALITYLIST
                        {
                            NATUID = item.NATUID,
                            NATCOUNTRY = item.NATCOUNTRY,
                            NATMAINENTRY = item.NATMAINENTRY
                        });
                    }
                }
                #endregion
                #region DOBlist
                var dxDOB = new List<DOBLIST>();
                if (Per.DOBLIST != null && Per.DOBLIST.Count() > 0)
                {
                    foreach (var item in Per.DOBLIST)
                    {
                        dxDOB.Add(new DOBLIST
                        {
                            DOBUID = item.DOBUID,
                            DOB = item.DOB,
                            AGE = item.AGE,
                            ASOFDATE = item.ASOFDATE,
                            DOBMAINENTRY = item.DOBMAINENTRY,
                            ISDECEASED = item.ISDECEASED
                        });
                    }
                }
                #endregion
                #region Addrslist
                var dxADDR = new List<ADDRESSLIST>();
                if (Per.ADDRESSLIST != null && Per.ADDRESSLIST.Count() > 0)
                {
                    foreach (var item in Per.ADDRESSLIST)
                    {
                        dxADDR.Add(new ADDRESSLIST
                        {
                            ADDRESSUID = item.ADDRESSUID,
                            ADDRESS1 = item.ADDRESS1,
                            ADDRESS2 = item.ADDRESS2,
                            ADDRESS3 = item.ADDRESS3,
                            ADDRESSCITY = item.ADDRESSCITY,
                            ADDRESSCOUNTRY = item.ADDRESSCOUNTRY,
                            ADDRESSPOCODE = item.ADDRESSPOCODE,
                            ADDRESSSTATE = item.ADDRESSSTATE
                        });
                    }
                }
                #endregion
                #region Citizlist
                var dxCIT = new List<CITIZENSHIPLIST>();
                if (Per.CITIZENSHIPLIST != null && Per.CITIZENSHIPLIST.Count() > 0)
                {
                    foreach (var item in Per.CITIZENSHIPLIST)
                    {
                        dxCIT.Add(new CITIZENSHIPLIST
                        {
                            CITUID = item.CITUID,
                            CITCOUNTRY = item.CITCOUNTRY,
                            CITMAINENTRY = item.CITMAINENTRY
                        });
                    }
                }
                #endregion
                #region POBlist
                var dxPOB = new List<PLACEOBLIST>();
                if (Per.PLACEOBLIST != null && Per.PLACEOBLIST.Count() > 0)
                {
                    foreach (var item in Per.PLACEOBLIST)
                    {
                        dxPOB.Add(new PLACEOBLIST
                        {
                            POBUID = item.POBUID,
                            POBMAINENTRY = item.POBMAINENTRY,
                            POB = item.POB
                        });
                    }
                }
                #endregion
                #region INFOlist
                var dxINFO = new List<ADDINFOLIST>();
                if (Per.ADDINFOLIST != null && Per.ADDINFOLIST.Count() > 0)
                {
                    foreach (var item in Per.ADDINFOLIST)
                    {
                        dxINFO.Add(new ADDINFOLIST
                        {
                            ADDINFO1 = item.ADDINFO1,
                            ADDINFO2 = item.ADDINFO2,
                            ADDINFO3 = item.ADDINFO3,
                            ADDINFO4 = item.ADDINFO4,
                            ADDINFO5 = item.ADDINFO5,
                            ADDINFO6 = item.ADDINFO6,
                            ADDINFO7 = item.ADDINFO7,
                            ADDINFO8 = item.ADDINFO8
                        });
                    }
                }
                #endregion
                BLACKLIST doc = new BLACKLIST
                {
                    _id = ObjectId.Parse(Obid),
                    UID = Per.UID,
                    SUBCATEGORY = string.IsNullOrEmpty(Per.SUBCATEGORY) ? "" : Per.SUBCATEGORY.ToUpper(),
                    CATEGORY = string.IsNullOrEmpty(Per.CATEGORY) ? "" : Per.CATEGORY.ToUpper(),
                    FIRST_NAME = string.IsNullOrEmpty(Per.FIRST_NAME) ? "" : Per.FIRST_NAME.ToUpper(),
                    LAST_NAME = string.IsNullOrEmpty(Per.LAST_NAME) ? "" : Per.LAST_NAME.ToUpper(),
                    MIDDLE_NAME = string.IsNullOrEmpty(Per.MIDDLE_NAME) ? "" : Per.MIDDLE_NAME.ToUpper(),
                    FULLNAME = string.IsNullOrEmpty(Per.FULLNAME) ? "" : Per.FULLNAME.Trim().ToUpper(),
                    FULLNAME_AR = string.IsNullOrEmpty(Per.FULLALIAS) ? "" : Per.FULLALIAS.Trim(),
                    CREATEDON = Per.CREATEDON,
                    UPDATEDON = Per.UPDATEDON,
                    CREATEDDATE = Per.CREATEDDATE,
                    UPDATEDDATE = DateTime.UtcNow,
                    TITLE = string.IsNullOrEmpty(Per.TITLE) ? "" : Per.TITLE.ToUpper(),
                    POSITION = string.IsNullOrEmpty(Per.POSITION) ? "" : Per.POSITION.ToUpper(),
                    REMARKS = string.IsNullOrEmpty(Per.REMARKS) ? "" : Per.REMARKS.ToUpper(),
                    TYPE = string.IsNullOrEmpty(Per.TYPE) ? "" : Per.TYPE.ToUpper(),
                    PROGRAMLIST = Per.PROGRAMLIST,
                    IDLIST = dxIDS,
                    AKALIST = dxAKA,
                    ADDRESSLIST = dxADDR,
                    NATIONALITYLIST = dxNAT,
                    CITIZENSHIPLIST = dxCIT,
                    DOBLIST = dxDOB,
                    PLACEOBLIST = dxPOB,
                    ADDINFOLIST = dxINFO,
                    STATUS = "A",
                    CLIENTID = Per.CLIENTID

                };
                var res = _unitOfWorkBlackList.InsertOne(doc);
                return "200";
            }
            catch (Exception ex)
            {
                return "500";
            }
        }

        public async Task<ServiceResponse> DeleteBlackList(WatchlistRequestDto Per)
        {
            try
            {
                CultureInfo en = new CultureInfo("en-US");
                #region Find & Delete
                _predicate_blackList = x => x.UID.Equals(Per.UID);
                _predicate_nameList = x => x.UID.Equals(Per.UID);

                //var result = (await _unitOfWorkNameList.FindAllAsync(predicate: _predicate_blackList)).ToList().FirstOrDefault();




                var depF = _unitOfWorkBlackList.FindOne(predicate: _predicate_blackList);
                var namelist = _unitOfWorkNameList.FindOne(predicate: _predicate_nameList);
                if (depF != null || namelist != null)
                {
                    _unitOfWorkBlackList.DeleteById(depF._id);
                    _unitOfWorkNameList.DeleteById(namelist._id);
                }
                //var blacklist =  _unitOfWorkBlackList.DeleteById(_predicate_blackList);
                //var depF = _unitOfWorkNameList.FindOne(_predicate_nameList);

                //    //Exists remove and add new updated item
                //var delres = _unitOfWorkBlackList.DeleteById(_predicate_blackList);


                #endregion

                return new ServiceResponse()
                {
                    ResponseCode = "200",
                    ResponseMessage = "Process Success"
                };
            }
            catch (Exception ex)
            {
                return new ServiceResponse()
                {
                    ResponseCode = "500",
                    ResponseMessage = "Process UnSuccess"
                };
            }
        }


        public async Task<IEnumerable<WSearchResult>> GetWatchlistByFilter(WSearchDto dto)
        {
            try
            {
                List<WSearchResult> searchRes = new List<WSearchResult>();
                string typ = dto.Type.ToUpper().Trim();
                DateTime std = DateTime.ParseExact(dto.StartDate + " 00:00:00", "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                DateTime end = DateTime.ParseExact(dto.EndDate + " 23:59:59", "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                if (typ == "OFAC") {
                    _predicate_blackList = z => z.TYPE == typ;
                }
                if (typ != "OFAC")
                {
                    if (typ == "UN")
                    {
                        _predicate_blackList = x => DateTime.Parse(x.CREATEDON) >= std.AddHours(-5.5);
                        _predicate_blackList = _predicate_blackList.And(z => DateTime.Parse(z.CREATEDON) <= end);
                    }
                    else
                    {
                        _predicate_blackList = x => x.CREATEDDATE >= std.AddHours(-5.5);
                        _predicate_blackList = _predicate_blackList.And(z => z.CREATEDDATE <= end);
                    }
                    _predicate_blackList = _predicate_blackList.And(z => z.TYPE == typ);
                }
                if (dto.client_id != 0)
                {
                    _predicate_blackList = _predicate_blackList.And(z => z.CLIENTID == dto.client_id);
                }
                var result = (await _unitOfWorkBlackList.FindAllAsync(_predicate_blackList)).ToList();
                if (result.Count() > 0)
                {
                    foreach (var item in result)
                    {
                        WSearchResult ws = new WSearchResult();
                        ws.FULLNAME = item.FULLNAME;
                        ws.TYPE = item.TYPE;
                        ws.NATIONALITY = (item.NATIONALITYLIST.Count() > 0) ? item.NATIONALITYLIST.Select(x => x.NATCOUNTRY).FirstOrDefault() : String.Empty;
                        ws.DOB = (item.DOBLIST.Count() > 0) ? item.DOBLIST.Select(x => x.DOB).FirstOrDefault() : String.Empty;
                        ws.UID = item.UID;
                        ws.IDNUMBER = (item.IDLIST.Count() > 0) ? item.IDLIST.Select(x => x.IDNUMBER).FirstOrDefault() : String.Empty;
                        ws.CATEGORY = item.CATEGORY;
                        ws.CREATEDON = item.CREATEDON;
                        ws.REMARKS = item.REMARKS;
                        searchRes.Add(ws);
                    }
                }
                return searchRes;
            }
            catch (Exception ex)
            {
                //log exception
                return null;
            }
        }

        public async Task<WSearchResult> GetWatchlistByUID(WatchlistRequestDto dto)
        {
            try
            {
                WSearchResult searchRes = new WSearchResult();

                _predicate_blackList = x => x.UID.Equals(dto.UID);
                _predicate_blackList = _predicate_blackList.And(z => z.CLIENTID == dto.client_id);
                //_predicate_blackList = x => x.UID.Equals(Per.UID);
                _predicate_nameList = x => x.UID.Equals(dto.UID);
                _predicate_nameList = _predicate_nameList.And(z => z.CLIENTID == dto.client_id);
                var result = _unitOfWorkBlackList.FindOne(predicate: _predicate_blackList);
                var result2 = _unitOfWorkNameList.FindOne(predicate: _predicate_nameList);
                if (result != null)
                {

                    //WSearchResult ws = new WSearchResult();
                    searchRes.FULLNAME = result.FULLNAME;
                    searchRes.TYPE = result.TYPE;
                    searchRes.NATIONALITY = (result.NATIONALITYLIST.Count() > 0) ? result.NATIONALITYLIST.Select(x => x.NATCOUNTRY).FirstOrDefault() : String.Empty;
                    searchRes.DOB = (result.DOBLIST.Count() > 0) ? result.DOBLIST.Select(x => x.DOB).FirstOrDefault() : String.Empty;
                    searchRes.UID = result.UID;
                    searchRes.IDNUMBER = (result.IDLIST.Count() > 0) ? result.IDLIST.Select(x => x.IDNUMBER).FirstOrDefault() : String.Empty;
                    searchRes.CATEGORY = result.CATEGORY;
                    searchRes.CREATEDON = result.CREATEDON;
                    searchRes.REMARKS = result.REMARKS;
                }
                return searchRes;
            }
            catch (Exception ex)
            {
                //log exception
                return null;
            }
        }

        public async Task<ServiceResponse> UpdateBlackList(WatchListModel watchlistmodeldto)
        {

            try
            {
                _predicate_blackList = x => x.UID.Equals(watchlistmodeldto.UID);
                _predicate_nameList = x => x.UID.Equals(watchlistmodeldto.UID);
                List<NATIONALITYLIST> natList = new List<NATIONALITYLIST>();
                List<DOBLIST> dobList = new List<DOBLIST>();
                List<IDDETAIL> iddetail = new List<IDDETAIL>();
                List<IDLIST> IDList = new List<IDLIST>();
                if (!string.IsNullOrWhiteSpace(watchlistmodeldto.IDNUMBER))
                {

                    IDLIST idl = new IDLIST()
                    {
                        IDNUMBER = watchlistmodeldto.IDNUMBER.Trim().ToUpper()
                    };
                    IDList.Add(idl);

                };
                if (!string.IsNullOrWhiteSpace(watchlistmodeldto.NATIONALITY))
                {

                    NATIONALITYLIST nat = new NATIONALITYLIST()
                    {
                        NATCOUNTRY = watchlistmodeldto.NATIONALITY.Trim().ToUpper()
                    };
                    natList.Add(nat);

                };

                if (!string.IsNullOrWhiteSpace(watchlistmodeldto.DOB))
                {

                    DOBLIST dob = new DOBLIST()
                    {
                        DOB = watchlistmodeldto.DOB.Trim().ToUpper()
                    };
                    dobList.Add(dob);

                };

                if (!string.IsNullOrWhiteSpace(watchlistmodeldto.IDNUMBER))
                {

                    IDDETAIL ID = new IDDETAIL()
                    {
                        IDNUMBER = watchlistmodeldto.IDNUMBER.Trim().ToUpper()
                    };
                    iddetail.Add(ID);

                };

                var Blacklistupdate = Builders<BLACKLIST>.Update.Set(x => x.REMARKS, watchlistmodeldto.REMARKS)
                    .Set(x => x.CATEGORY, watchlistmodeldto.CATEGORY)
                .Set(x => x.NATIONALITYLIST, natList)
                .Set(x => x.DOBLIST, dobList)
                .Set(x => x.IDLIST, IDList)
                .Set(x => x.FULLNAME, watchlistmodeldto.FULLNAME)
                .Set(x => x.UPDATEDON, DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));

                var namelistupdate = Builders<NAMELIST>.Update.Set(x => x.REMARKS, watchlistmodeldto.REMARKS)
                    .Set(x => x.CATEGORY, watchlistmodeldto.CATEGORY)
                .Set(x => x.NATIONALITY, watchlistmodeldto.NATIONALITY)
                .Set(x => x.DOB, dobList)
                .Set(x => x.IDDETAILS, iddetail)
                .Set(x => x.FULLNAME, watchlistmodeldto.FULLNAME)
                .Set(x => x.UPDATEDON, DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));

                _unitOfWorkBlackList.UpdateOne(_predicate_blackList, Blacklistupdate);
                _unitOfWorkNameList.UpdateOne(_predicate_nameList, namelistupdate);
                return new ServiceResponse()
                {
                    ResponseCode = "200",
                    ResponseMessage = "Process Success"
                };
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                throw ex;
                return new ServiceResponse()
                {
                    ResponseCode = "200",
                    ResponseMessage = "Process Success"
                };
            }


        }
        //private async void LogFile(string message2, CustomerScreenDto data)
        //{
        //    string message = string.Format("Time: {0}", DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss tt"));
        //    message += Environment.NewLine;
        //    message += "-----------------------------------------------------------";
        //    message += Environment.NewLine;
        //    message += message2;
        //    message += Environment.NewLine;

        //    message += string.Format("CaseId: {0}", data.CaseId);
        //    message += Environment.NewLine;
        //    message += string.Format("Name: {0}", data.CustomerFullName);
        //    message += Environment.NewLine;
        //    message += string.Format("Type: {0}", data.CustomerType);
        //    message += Environment.NewLine;
        //    message += string.Format("Nationality: {0}", data.CustomerNationality);
        //    message += Environment.NewLine;
        //    message += string.Format("Dob: {0}", data.CustomerDob);
        //    message += Environment.NewLine;



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
        //private async void LogFile2(string message2, BLACKLIST data)
        //{
        //    string message = string.Format("Time: {0}", DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss tt"));
        //    message += Environment.NewLine;
        //    message += "-----------------------------------------------------------";
        //    message += Environment.NewLine;
        //    message += message2;
        //    message += Environment.NewLine;

        //    message += string.Format("CaseId UID: {0}", data.UID);
        //    message += Environment.NewLine;
        //    message += string.Format("Name: {0}", data.FULLNAME);
        //    message += Environment.NewLine;
        //    message += string.Format("Type: {0}", data.CATEGORY);
        //    message += Environment.NewLine;



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

        //private async void LogFile3(string message2, NAMELIST data)
        //{
        //    string message = string.Format("Time: {0}", DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss tt"));
        //    message += Environment.NewLine;
        //    message += "-----------------------------------------------------------";
        //    message += Environment.NewLine;
        //    message += message2;
        //    message += Environment.NewLine;

        //    message += string.Format("CaseId UID: {0}", data.UID);
        //    message += string.Format("Name: {0}", data.FULLNAME);
        //    message += string.Format("Type: {0}", data.CATEGORY);




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