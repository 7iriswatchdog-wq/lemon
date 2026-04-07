using AML.Core.Common.Algorithms;
using AML.Core.Common.StaticResource;
using AML.Core.RepositoryContract.FreeSource;
using AML.DTO.DTO.FreeSource;
using AML.OFAC;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDB.Driver.Core.Events;
using MySqlX.XDevAPI.Common;
using NLog;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using static AML.DTO.DTO.FreeSource.BlackListMongoDTO;
using static AML.DTO.DTO.FreeSource.CaseLogsMongoDTO;
using static AML.DTO.DTO.FreeSource.TransactionCaseLogsMongoDTO;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AML.Core.Repository.FreeSource
{
    public class FreeSourceRepository : MongoRepository, IFreeSourceRepository
    {
        private readonly Logger log = LogManager.GetCurrentClassLogger();

        private IMongoDatabase mongoDB;
        
        public FreeSourceRepository(IConfiguration configuration) : base(configuration)
        {
            mongoDB = GetMongoConnection();
        }

        public ServiceResponse<int> Create(BlackListMongoDTO.BLACKLIST dto)
        {
            throw new NotImplementedException();
        }

        public string InsertNameList(BLACKLIST per)
        {
            try
            {
                CultureInfo en = new CultureInfo("en-US");
                #region Find & Delete
                var Query = Builders<NAMELIST>.Filter;
                string id = "0";
                var query = Query.And(Query.Eq("UID", per.UID), Query.Eq("STATUS", "A"));
                var collection = mongoDB.GetCollection<NAMELIST>("NAMELIST");
                var depF = collection.Find(query);
                if (depF.CountDocuments() > 0)
                {
                    //Exists remove and add new updated item
                    collection.DeleteMany(query);
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
                    STATUS = "A"
                };
                collection.InsertOne(docnew);
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
                            STATUS = "A"
                        };
                        collection.InsertOne(docalias);
                    }
                }
                #endregion
                InsertBlackList(per, id);
                return "E000";
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
            return "501";

        }
        public void InsertBlackList(BLACKLIST Per, string Obid)
        {
            try
            {
                CultureInfo en = new CultureInfo("en-US");
                var Query = Builders<BLACKLIST>.Filter;
                var query = Query.And(Query.Eq("UID", Per.UID), Query.Eq("STATUS", "A"));
                var collection = mongoDB.GetCollection<BLACKLIST>("BLACKLIST");
                var depF = collection.Find(query);
                if (depF.CountDocuments() > 0)
                {
                    //Exists remove and add new updated item
                    collection.DeleteMany(query);
                }
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
                    STATUS = "A"
                };
                collection.InsertOne(doc);
            }
            catch (Exception ex)
            {
                log.Error(ex);
                throw ex;
            }
        }

        public string ReadFile(string filePath)
        {
            OfacParser ofac = new OfacParser();
            return ofac.ReadOfacXmlData(filePath);
        }
        public (bool exists, List<NAMELIST> response, List<NAMELIST> response2) SearchNameList(string Name, bool isCorporate,int Clientid)
        {

            string cleanedString = GetCleanedString(Name,true);

            

            Console.WriteLine("string2: " + cleanedString);


            FilterDefinitionBuilder<NAMELIST> Query = Builders<NAMELIST>.Filter;
            /*Code added by sanjana*/
            FilterDefinitionBuilder<NAMELIST> Query2 = Builders<NAMELIST>.Filter;
            FilterDefinition<NAMELIST> query = Query.And(
                                Query.Or(
                                        Query.Eq("FULLNAME", Name.ToUpper()),
                                        Query.Eq("FULLNAME_NL", Name.ToUpper())
                                ),
                                Query.Or(
                                        Query.Eq("TYPE", "BL"),
                                        Query.Eq("TYPE", "CBWL"),
                                        Query.Eq("TYPE", "INTERNAL")
                                       
                                ),
                                Query.Eq("CATEGORY", (isCorporate ? "CORPORATE"   : "INDIVIDUAL")),
                                Query.Eq("CLIENTID", Clientid)
                               
                        );
          /*Code added by sanjana*/
            FilterDefinition<NAMELIST> query2 = Query2.And(
                                Query2.Or(
                                        Query2.Eq("FULLNAME", cleanedString.ToUpper()),
                                        Query2.Eq("FULLNAME_NL", cleanedString.ToUpper())
                                ),
                                Query2.Or(
                                        Query2.Eq("TYPE", "UN"),
                                        Query2.Eq("TYPE", "OFAC"),
                                        Query2.Eq("TYPE", "UAE IEC LIST")
                                ),
                                 Query2.Or(
                                        Query2.Eq("CATEGORY", isCorporate ? "CORPORATE" : "INDIVIDUAL"),
                                       Query2.Eq("CATEGORY", isCorporate ? "ENTITY" : "INDIVIDUAL")
                                )
                                
                        );

            IMongoCollection<NAMELIST> collection = mongoDB.GetCollection<NAMELIST>("NAMELIST");
            IFindFluent<NAMELIST, NAMELIST> depF = collection.Find(query);
            /*Code added by sanjana*/
            IFindFluent<NAMELIST, NAMELIST> depF2 = collection.Find(query2);

            log.Debug($"MongoDB,searching the name");
			log.Debug(depF.CountDocuments());
            log.Debug(depF2.CountDocuments());

            if (depF.CountDocuments() > 0)
            {
				log.Debug($"exists:true");
				return (exists: true, response: depF.ToList(), depF2.ToList());
				log.Debug($"after searching:");
			}
            /*Code added by sanjana*/
            if (depF2.CountDocuments() > 0)
            {
                log.Debug($"exists:true");
                return (exists: true, response: depF.ToList(), depF2.ToList());
                log.Debug($"after searching:");
            }
            log.Debug($"exists:false");
			return (exists: false, response: null,null);
        }

        public List<NAMELIST> GetRecordsByCreatedDate(string createdDate,string type)
        {
            try
            {
                DateTime createdDate1 = DateTime.ParseExact(
                    createdDate,
                    "yyyy-MM-dd",
                    CultureInfo.InvariantCulture
                );

                var dateString = createdDate1.ToString("dd/MM/yyyy");

                var filter = Builders<NAMELIST>.Filter.And(
                    Builders<NAMELIST>.Filter.Regex(
                        x => x.CREATEDON,
                        new BsonRegularExpression("^" + dateString)
                    ),
                    Builders<NAMELIST>.Filter.Eq(x => x.TYPE, type.ToUpper())
                );

                var collection = mongoDB.GetCollection<NAMELIST>("NAMELIST");

                var data = collection.Find(filter)
                .Project(x => new NAMELIST
                {
                    FULLNAME = x.FULLNAME,
                    CATEGORY = x.CATEGORY
                })
                .ToList();

                return data;
        }
    catch (Exception)
    {
        return new List<NAMELIST>();
    }
}

        //public (bool exists, NAMELIST response, List<NAMELIST> response2) SearchFuzzylist(string Name, bool isCorporate, int Clientid) 
        //{
        //    string[] nameList = Name.Trim().Split(' ').Where(s => s != "" && s.Length > 2).Distinct().ToArray();
        //    if (nameList.Length == 0)
        //        return (exists: false, response: null, null);
        //    var soundCodes = new List<string>();
        //    foreach (var names in nameList)
        //    {
        //        string sound = Soundex.SoundexText(GetCleanedString(names));
        //        soundCodes.Add(sound);
        //    }
        //    var soundFilter = Builders<NAMELIST>.Filter.All("SOUNDEX", soundCodes);

        //    //FilterDefinitionBuilder<NAMELIST> Query2 = Builders<NAMELIST>.Filter;
        //    //FilterDefinition<NAMELIST> query2 = Query2.And(
        //    //    Query2.Or(
        //    //        Query2.Eq("SOUNDEX", soundFilter)
        //    //        ),
        //    //    Query2.Or(
        //    //                            Query2.Eq("CATEGORY", isCorporate ? "CORPORATE" : "INDIVIDUAL"),
        //    //                           Query2.Eq("CATEGORY", isCorporate ? "ENTITY" : "INDIVIDUAL")
        //    //                    ));
        //    FilterDefinition<NAMELIST> filter;

        //    filter = Builders<NAMELIST>.Filter.And(soundFilter);
        //    IMongoCollection<NAMELIST> collection = mongoDB.GetCollection<NAMELIST>("NAMELIST");
        //    var result = collection.Find(filter).ToListAsync();

        //    var activeFilter = Builders<NAMELIST>.Filter.Eq(x => x.STATUS, "A");
        //    var clientidFilter = Builders<NAMELIST>.Filter.Or(
        //                             Builders<NAMELIST>.Filter.Eq("CLIENTID", screen.ClientId),
        //                             Builders<NAMELIST>.Filter.Exists("CLIENTID", false),
        //                             Builders<NAMELIST>.Filter.Eq("CLIENTID", -1)
        //                         ); var soundFilter = Builders<NAMELIST>.Filter.All("SOUNDEX", soundCodes);
        //    var nationalityEmptyFilter = Builders<NAMELIST>.Filter.Eq(x => x.NATIONALITY, "");
        //    var nationalityFilter = Builders<NAMELIST>.Filter.Where(x => x.NATIONALITY.ToUpper() == screen.CustomerNationality.ToUpper());
        //    var customertypeFilter = Builders<NAMELIST>.Filter.Eq(x => x.CATEGORY, screen.CustomerType);

        //    FilterDefinition<NAMELIST> filter;
        //    if (!string.IsNullOrEmpty(screen.CustomerNationality))
        //    {
        //        filter = Builders<NAMELIST>.Filter.And(activeFilter, clientidFilter, soundFilter, customertypeFilter, Builders<NAMELIST>.Filter.Or(nationalityEmptyFilter, nationalityFilter));
        //    }
        //    else
        //    {
        //        filter = Builders<NAMELIST>.Filter.And(activeFilter, clientidFilter, soundFilter, customertypeFilter);
        //    }
        //    var result = await _unitOfWorkNameList.FindAllLimitAsync(filter);




        //    return (exists: false, response: null, null);
        //}

        private string GetCleanedString(string name, bool toupper = true)
        {
            if (string.IsNullOrEmpty(name)) { return ""; }

            if (toupper) { name = name.ToUpper(); }

            name = name.Trim();

            name = Regex.Replace(name, @"\s+", " ");
            name = Regex.Replace(name, @"[^A-Z\d\s]", " ");

            return name;
        }

        public bool InsertCaseLog(CASELOG model)
        {
            try
            {
                log.Info($"Inserting case with id {model.CASEID} to case log");
                var collection = mongoDB.GetCollection<CASELOG>("CASELOG");

                #region MatchRecord List
                var dxmatchRecord = new List<MatchRecordsDTO>();
                if (model.MATCHRECORDS != null && model.MATCHRECORDS.Count() > 0)
                {
                    foreach (var item in model.MATCHRECORDS)
                    {
                        dxmatchRecord.Add(new MatchRecordsDTO
                        {
                            MATCHUID = string.IsNullOrEmpty(item.MATCHUID) ? "" : item.MATCHUID.Trim().ToUpper(),
                            MATCHTYPE = string.IsNullOrEmpty(item.MATCHTYPE) ? "" : item.MATCHTYPE.Trim().ToUpper(),
                            MATCHCATEGORY = string.IsNullOrEmpty(item.MATCHCATEGORY) ? "" : item.MATCHCATEGORY.Trim().ToUpper(),
                            MATCHNAME = string.IsNullOrEmpty(item.MATCHNAME) ? "" : item.MATCHNAME.Trim().ToUpper(),
                            MATCHSCORE = item.MATCHSCORE,
                            MATCHNATIONALITY = string.IsNullOrEmpty(item.MATCHNATIONALITY) ? "" : item.MATCHNATIONALITY.Trim().ToUpper(),
                            MATCHIDNO = string.IsNullOrEmpty(item.MATCHIDNO) ? "" : item.MATCHIDNO.Trim().ToUpper(),
                            MATCHDOB = item.MATCHDOB,
                            MATCHRESOURCESID= string.IsNullOrEmpty(item.MATCHRESOURCESID) ? "" : item.MATCHRESOURCESID,
                            REMARKS= string.IsNullOrEmpty(item.REMARKS) ? "" : item.REMARKS,
                            MATCHDATASETS=string.IsNullOrEmpty(item.MATCHDATASETS) ? "--":item.MATCHDATASETS,
                            MATCHGENDER=string.IsNullOrEmpty(item.MATCHGENDER) ? "":item.MATCHGENDER
                        });
                    }
                }
                #endregion
                CASELOG docnew = new CASELOG
                {
                    CASEID = model.CASEID,
                    MATCHRECORDS = dxmatchRecord
                };
                collection.InsertOne(docnew);

                log.Info("Inserted record successfully");

                return true;
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
            return false;
        }
        public bool InsertCaseLogUnderThreshold(CASELOG model)
        {
            try
            {
                log.Info($"Inserting case with id {model.CASEID} to case log under threshold");

                var collection = mongoDB.GetCollection<CASELOG>("CASELOG_UNDER_THRESHOLD");

                #region MatchRecord List
                var dxmatchRecord = new List<MatchRecordsDTO>();
                if (model.MATCHRECORDS != null && model.MATCHRECORDS.Count() > 0)
                {
                    foreach (var item in model.MATCHRECORDS)
                    {
                        dxmatchRecord.Add(new MatchRecordsDTO
                        {
                            MATCHUID = string.IsNullOrEmpty(item.MATCHUID) ? "" : item.MATCHUID.Trim().ToUpper(),
                            MATCHTYPE = string.IsNullOrEmpty(item.MATCHTYPE) ? "" : item.MATCHTYPE.Trim().ToUpper(),
                            MATCHCATEGORY = string.IsNullOrEmpty(item.MATCHCATEGORY) ? "" : item.MATCHCATEGORY.Trim().ToUpper(),
                            MATCHNAME = string.IsNullOrEmpty(item.MATCHNAME) ? "" : item.MATCHNAME.Trim().ToUpper(),
                            MATCHSCORE = item.MATCHSCORE,
                            MATCHNATIONALITY = string.IsNullOrEmpty(item.MATCHNATIONALITY) ? "" : item.MATCHNATIONALITY.Trim().ToUpper(),
                            MATCHIDNO = string.IsNullOrEmpty(item.MATCHIDNO) ? "" : item.MATCHIDNO.Trim().ToUpper(),
                            MATCHDOB = item.MATCHDOB
                        });
                    }
                }
                #endregion
                CASELOG docnew = new CASELOG
                {
                    CASEID = model.CASEID,
                    MATCHRECORDS = dxmatchRecord
                };
                collection.InsertOne(docnew);

                log.Info("Inserted record successfully");

                return true;
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
            return false;
        }
        public string GetDetailsByCaseId(int caseId)
        {
            try
            {
                var collection = mongoDB.GetCollection<CASELOG>("CASELOG_UNDER_THRESHOLD");
                var res = mongoDB.GetCollection<BsonDocument>("CASELOG_UNDER_THRESHOLD");
                var filter = Builders<BsonDocument>.Filter.Eq("CASEID", caseId.ToString());

                var doc = res.Find(filter).ToList().LastOrDefault();
                if (doc != null)
                {
                    var elem1 = doc.Elements.ElementAt(2).Value.ToJson();
                    return elem1;

                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
            return null;
        }
        public string GetPendingDetailsByCaseId(int caseId)
        {
            try
            {
                var collection = mongoDB.GetCollection<CASELOG>("CASELOG");
                var res = mongoDB.GetCollection<BsonDocument>("CASELOG");
                var filter = Builders<BsonDocument>.Filter.Eq("CASEID", caseId.ToString());

                var doc = res.Find(filter).ToList().LastOrDefault();
                var elem1 = doc.Elements.ElementAt(2).Value.ToJson();
                return elem1;
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
            return null;
        }
        public bool UpdateCaseLogRemarks(CASELOG model)
        {

            try
            {
                var collection = mongoDB.GetCollection<CASELOG>("CASELOG");

                #region MatchRecord List
                var dxmatchRecord = new List<MatchRecordsDTO>();
                if (model.MATCHRECORDS != null && model.MATCHRECORDS.Count() > 0)
                {
                    int i = 0;
                    foreach (var item in model.MATCHRECORDS)
                    {
                        dxmatchRecord.Add(new MatchRecordsDTO
                        {
                            MATCHUID = string.IsNullOrEmpty(item.MATCHUID) ? "" : item.MATCHUID.Trim().ToUpper(),
                            MATCHTYPE = string.IsNullOrEmpty(item.MATCHTYPE) ? "" : item.MATCHTYPE.Trim().ToUpper(),
                            MATCHCATEGORY = string.IsNullOrEmpty(item.MATCHCATEGORY) ? "" : item.MATCHCATEGORY.Trim().ToUpper(),
                            MATCHNAME = string.IsNullOrEmpty(item.MATCHNAME) ? "" : item.MATCHNAME.Trim().ToUpper(),
                            MATCHSCORE = item.MATCHSCORE,
                            MATCHNATIONALITY = string.IsNullOrEmpty(item.MATCHNATIONALITY) ? "" : item.MATCHNATIONALITY.Trim().ToUpper(),
                            MATCHIDNO = string.IsNullOrEmpty(item.MATCHIDNO) ? "" : item.MATCHIDNO.Trim().ToUpper(),
                            MATCHDOB = item.MATCHDOB
                        });
                        //     Builders<Poll>.Update.Set(x => x.Votes[-1].Name, vote.Name),
                        var caseidfilter = Builders<CASELOG>.Filter.Eq("CASEID", model.CASEID);
                        var descfilter = Builders<CASELOG>.Sort.Descending(val=>val._id);
                        //   var matchrecordfilter = Builders<CASELOG>.Filter.Eq("MATCHRECORDS", i.ToString());
                        //var update = Builders<CASELOG>.Update.Set("MATCHRECORDS.$.REMARKS", model.MATCHRECORDS[i].REMARKS);
                        var update = Builders<CASELOG>.Update.Set(x => x.MATCHRECORDS[i].REMARKS, model.MATCHRECORDS[i].REMARKS).Set(x => x.MATCHRECORDS[i].SEARCHTYPES, model.MATCHRECORDS[i].SEARCHTYPES);
                        var result = collection.FindOneAndUpdate(caseidfilter, update, new FindOneAndUpdateOptions<CASELOG, CASELOG>() { Sort = descfilter });


                        //var builder = Builders<CASELOG>.Filter;
                        //var filter = builder.Eq("CASEID", model.CASEID) & builder.ElemMatch(student => student.MATCHUID, x => x == 80);

                        //var builder = Builders<Student>.Update;
                        //var update = builder.Set(student => student.Grades[-1], 82);

                        //var result = collection.UpdateOne(filter, update);

                        i++;




                    }
                }
                #endregion
                CASELOG docnew = new CASELOG
                {
                    CASEID = model.CASEID,
                    MATCHRECORDS = dxmatchRecord
                };


                return true;
            }
            catch (Exception ex)
            {
                log.Error(ex);
                return false;
            }

        }

        //transaction screening
        public bool InsertTransactionCaseLog(TRANSACTION_CASELOG model)
        {
            try
            {
                var collection = mongoDB.GetCollection<TRANSACTION_CASELOG>("TRANSACTION_CASELOG");

                #region MatchRecord List
                var dxmatchRecord = new List<MatchTranRecordsDTO>();
                if (model.MATCHRECORDS != null && model.MATCHRECORDS.Count() > 0)
                {
                    foreach (var item in model.MATCHRECORDS)
                    {
                        dxmatchRecord.Add(new MatchTranRecordsDTO
                        {
                            MATCHUID = string.IsNullOrEmpty(item.MATCHUID) ? "" : item.MATCHUID.Trim().ToUpper(),
                            MATCHTYPE = string.IsNullOrEmpty(item.MATCHTYPE) ? "" : item.MATCHTYPE.Trim().ToUpper(),
                            MATCHCATEGORY = string.IsNullOrEmpty(item.MATCHCATEGORY) ? "" : item.MATCHCATEGORY.Trim().ToUpper(),
                            MATCHNAME = string.IsNullOrEmpty(item.MATCHNAME) ? "" : item.MATCHNAME.Trim().ToUpper(),
                            MATCHSCORE = item.MATCHSCORE,
                            MATCHNATIONALITY = string.IsNullOrEmpty(item.MATCHNATIONALITY) ? "" : item.MATCHNATIONALITY.Trim().ToUpper(),
                            MATCHIDNO = string.IsNullOrEmpty(item.MATCHIDNO) ? "" : item.MATCHIDNO.Trim().ToUpper(),
                            MATCHDOB = item.MATCHDOB
                        });
                    }
                }
                #endregion
                TRANSACTION_CASELOG docnew = new TRANSACTION_CASELOG
                {

                    TRANREFNO = model.TRANREFNO,
                    CASEID = model.CASEID,

                    MATCHRECORDS = dxmatchRecord
                };
                collection.InsertOne(docnew);

                return true;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);

            }
            return false;
        }

        public bool UpdateTranCaseLogRemarks(TRANSACTION_CASELOG model)
        {

            try
            {
                var collection = mongoDB.GetCollection<TRANSACTION_CASELOG>("TRANSACTION_CASELOG");

                #region MatchRecord List
                var dxmatchRecord = new List<MatchTranRecordsDTO>();
                if (model.MATCHRECORDS != null && model.MATCHRECORDS.Count() > 0)
                {
                    int i = 0;
                    foreach (var item in model.MATCHRECORDS)
                    {
                        dxmatchRecord.Add(new MatchTranRecordsDTO
                        {
                            MATCHUID = string.IsNullOrEmpty(item.MATCHUID) ? "" : item.MATCHUID.Trim().ToUpper(),
                            MATCHTYPE = string.IsNullOrEmpty(item.MATCHTYPE) ? "" : item.MATCHTYPE.Trim().ToUpper(),
                            MATCHCATEGORY = string.IsNullOrEmpty(item.MATCHCATEGORY) ? "" : item.MATCHCATEGORY.Trim().ToUpper(),
                            MATCHNAME = string.IsNullOrEmpty(item.MATCHNAME) ? "" : item.MATCHNAME.Trim().ToUpper(),
                            MATCHSCORE = item.MATCHSCORE,
                            MATCHNATIONALITY = string.IsNullOrEmpty(item.MATCHNATIONALITY) ? "" : item.MATCHNATIONALITY.Trim().ToUpper(),
                            MATCHIDNO = string.IsNullOrEmpty(item.MATCHIDNO) ? "" : item.MATCHIDNO.Trim().ToUpper(),
                            MATCHDOB = item.MATCHDOB
                        });

                        var caseidfilter = Builders<TRANSACTION_CASELOG>.Filter.Eq("CASEID", model.CASEID);

                        var update = Builders<TRANSACTION_CASELOG>.Update.Set(x => x.MATCHRECORDS[i].REMARKS, model.MATCHRECORDS[i].REMARKS);
                        var result = collection.UpdateOne(caseidfilter, update);
                        i++;
                    }
                }
                #endregion
                TRANSACTION_CASELOG docnew = new TRANSACTION_CASELOG
                {
                    CASEID = model.CASEID,
                    MATCHRECORDS = dxmatchRecord
                };


                return true;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                throw ex;
                return false;
            }

        }
    }
}
