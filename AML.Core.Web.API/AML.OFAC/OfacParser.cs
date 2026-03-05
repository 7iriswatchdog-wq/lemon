using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;


namespace AML.OFAC
{
    public class OfacParser
    {

        internal void ReadOfacXmlData(string downloadpath)
        {
            try
            {
                int i = 0;
                int j = 0;
                using (XmlReader reader = XmlReader.Create(downloadpath))
                {
                    while (reader.Read())
                    {
                        switch (reader.NodeType)
                        {
                            case XmlNodeType.Element:
                                switch (reader.LocalName.ToLower())
                                {
                                    case "sdnentry":
                                        string retval = ReadXmldata(reader.ReadOuterXml());
                                        if (retval == "E000")
                                        {
                                            i++;
                                        }
                                        j++;
                                        break;
                                }
                                break;
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public BLACKLIST ReadXmldata(string XmlData)
        {
            XDocument xDocument = new XDocument();
            try
            {
                xDocument = XDocument.Parse(XmlData);
                XNamespace ns = xDocument.Root.Attribute("xmlns").Value;
                BLACKLIST blackList =
                    (from e in xDocument.Elements(ns + "sdnEntry")
                     select new BLACKLIST
                     {
                         UID = "OFAC-" + (string)e.Element(ns + "uid"),
                         FIRST_NAME = string.IsNullOrEmpty((string)e.Element(ns + "firstName")) ? "" : ((string)e.Element(ns + "firstName")).ToUpper(),
                         LAST_NAME = string.IsNullOrEmpty((string)e.Element(ns + "lastName")) ? "" : ((string)e.Element(ns + "lastName")).ToUpper(),
                         FULLNAME = Regex.Replace(Regex.Replace(((string.IsNullOrEmpty((string)e.Element(ns + "firstName")) ? "" : ((string)e.Element(ns + "firstName")).ToUpper() + " ") +
                          (string.IsNullOrEmpty((string)e.Element(ns + "lastName")) ? "" : ((string)e.Element(ns + "lastName")).ToUpper())).Trim(), @"[-]", " "), @"[^\w\d\s]", ""),
                         MIDDLE_NAME = "",
                         REMARKS = string.IsNullOrEmpty((string)e.Element(ns + "remarks")) ? "" : (string)e.Element(ns + "remarks"),
                         ADDRESSLIST = ReadAddresslist(e, ns),
                         IDLIST = ReadIDlist(e, ns),
                         AKALIST = ReadAKAlist(e, ns),
                         PROGRAMLIST = ReadProgramlist(e, ns),
                         NATIONALITYLIST = ReadNationalitylist(e, ns),
                         CITIZENSHIPLIST = ReadCitizenlist(e, ns),
                         DOBLIST = ReadDOBlist(e, ns),
                         PLACEOBLIST = ReadPOBlist(e, ns),
                         ADDINFOLIST = ReadADINFOlist(e, ns),
                         CREATEDON = "",
                         UPDATEDON = "",
                         CATEGORY = string.IsNullOrEmpty((string)e.Element(ns + "sdnType")) ? "" : (string)e.Element(ns + "sdnType"),
                         SUBCATEGORY = "",
                         TITLE = string.IsNullOrEmpty((string)e.Element(ns + "title")) ? "" : (string)e.Element(ns + "title"),
                         TYPE = "OFAC"
                     }).FirstOrDefault();
                //DataAccessServices service = new DataAccessServices();
                //string result = service.InsertNameList(blackList);
                return blackList;
            }
            catch (Exception ex)
            { throw; }
        }
        public List<PROGRAMLIST> ReadProgramlist(XElement e, XNamespace ns)
        {
            List<PROGRAMLIST> lst = new List<PROGRAMLIST>();
            try
            {
                lst = (from p in e.Elements(ns + "programList")
                       select new PROGRAMLIST
                       {
                           PROGRAM = (string)p.Element(ns + "program")
                       }).ToList();
                if (lst != null && lst.Count > 0)
                {
                    StringBuilder sbPgm = new StringBuilder();
                    foreach (var item in lst)
                    {
                        if (sbPgm.ToString() != "")
                        {
                            sbPgm.Append(",");
                        }
                        sbPgm.Append(string.IsNullOrEmpty(item.PROGRAM) ? "" : item.PROGRAM.ToUpper());
                    }
                    //return sbPgm.ToString();

                }
                return lst;
            }
            catch (Exception ex)
            { throw; }
        }
        public List<IDLIST> ReadIDlist(XElement e, XNamespace ns)
        {
            List<IDLIST> lst = new List<IDLIST>();
            try
            {
                lst = (from p in e.Elements(ns + "idList").Elements(ns + "id")
                       select new IDLIST
                       {
                           IDUID = (string)p.Element(ns + "uid"),
                           IDCOUNTRY = (string)p.Element(ns + "idCountry"),
                           IDEXPIRYDATE = (string)p.Element(ns + "expirationDate"),
                           IDISSUEDATE = (string)p.Element(ns + "issueDate"),
                           IDTYPE = (string)p.Element(ns + "idType"),
                           IDNUMBER = (string)p.Element(ns + "idNumber")
                       }).ToList();
                if (lst.Count > 0)
                {
                    return lst.ToList();
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            { throw; }
        }
        public List<AKALIST> ReadAKAlist(XElement e, XNamespace ns)
        {
            List<AKALIST> lst = new List<AKALIST>();
            try
            {
                lst = (from p in e.Elements(ns + "akaList").Elements(ns + "aka")
                       select new AKALIST
                       {
                           AKAUID = (string)p.Element(ns + "uid"),
                           AKATYPE = (string)p.Element(ns + "type"),
                           AKACATEGORY = (string)p.Element(ns + "category"),
                           AKAFIRST_NAME = string.IsNullOrEmpty((string)p.Element(ns + "firstName")) ? "" : ((string)p.Element(ns + "firstName")).ToUpper(),
                           AKALAST_NAME = string.IsNullOrEmpty((string)p.Element(ns + "lastName")) ? "" : ((string)p.Element(ns + "lastName")).ToUpper()
                       }).ToList();
                if (lst.Count > 0)
                {
                    return lst.ToList();
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            { throw; }
        }
        public List<ADDRESSLIST> ReadAddresslist(XElement e, XNamespace ns)
        {
            List<ADDRESSLIST> lst = new List<ADDRESSLIST>();
            try
            {
                lst = (from p in e.Elements(ns + "addressList").Elements(ns + "address")
                       select new ADDRESSLIST
                       {
                           ADDRESSUID = (string)p.Element(ns + "uid"),
                           ADDRESS1 = (string)p.Element(ns + "address1"),
                           ADDRESS2 = (string)p.Element(ns + "address2"),
                           ADDRESS3 = (string)p.Element(ns + "address3"),
                           ADDRESSCITY = (string)p.Element(ns + "city"),
                           ADDRESSSTATE = (string)p.Element(ns + "stateOrProvince"),
                           ADDRESSPOCODE = (string)p.Element(ns + "postalCode"),
                           ADDRESSCOUNTRY = (string)p.Element(ns + "country")
                       }).ToList();
                if (lst.Count > 0)
                {
                    return lst.ToList();
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            { throw; }
        }
        public List<NATIONALITYLIST> ReadNationalitylist(XElement e, XNamespace ns)
        {
            List<NATIONALITYLIST> lst = new List<NATIONALITYLIST>();
            try
            {
                lst = (from p in e.Elements(ns + "nationalityList").Elements(ns + "nationality")
                       select new NATIONALITYLIST
                       {
                           NATUID = (string)p.Element(ns + "uid"),
                           NATCOUNTRY = string.IsNullOrEmpty((string)p.Element(ns + "country")) ? "" : (string)p.Element(ns + "country").Value.ToUpper(),
                           NATMAINENTRY = (string)p.Element(ns + "mainEntry")
                       }).ToList();
                if (lst.Count > 0)
                {
                    return lst.ToList();
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            { throw; }
        }
        public List<CITIZENSHIPLIST> ReadCitizenlist(XElement e, XNamespace ns)
        {
            List<CITIZENSHIPLIST> lst = new List<CITIZENSHIPLIST>();
            try
            {
                lst = (from p in e.Elements(ns + "citizenshipList").Elements(ns + "citizenship")
                       select new CITIZENSHIPLIST
                       {
                           CITUID = (string)p.Element(ns + "uid"),
                           CITCOUNTRY = (string)p.Element(ns + "country"),
                           CITMAINENTRY = (string)p.Element(ns + "mainEntry")
                       }).ToList();

                if (lst.Count > 0)
                {
                    return lst.ToList();
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            { throw; }
        }
        public List<DOBLIST> ReadDOBlist(XElement e, XNamespace ns)
        {
            List<DOBLIST> lst = new List<DOBLIST>();
            try
            {
                lst = (from p in e.Elements(ns + "dateOfBirthList").Elements(ns + "dateOfBirthItem")
                       select new DOBLIST
                       {
                           DOBUID = (string)p.Element(ns + "uid"),
                           DOB = (string)p.Element(ns + "dateOfBirth"),
                           DOBMAINENTRY = (string)p.Element(ns + "mainEntry"),
                           ASOFDATE = "",
                           ISDECEASED = "",
                           AGE = ""
                       }).ToList();
                if (lst.Count > 0)
                {
                    return lst.ToList();
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            { throw; }
        }
        public List<PLACEOBLIST> ReadPOBlist(XElement e, XNamespace ns)
        {
            List<PLACEOBLIST> lst = new List<PLACEOBLIST>();
            try
            {
                lst = (from p in e.Elements(ns + "placeOfBirthList").Elements(ns + "placeOfBirthItem")
                       select new PLACEOBLIST
                       {
                           POBUID = (string)p.Element(ns + "uid"),
                           POB = (string)p.Element(ns + "placeOfBirth"),
                           POBMAINENTRY = (string)p.Element(ns + "mainEntry")
                       }).ToList();
                if (lst.Count > 0)
                {
                    return lst.ToList();
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            { throw; }
        }
        public List<ADDINFOLIST> ReadADINFOlist(XElement e, XNamespace ns)
        {
            List<ADDINFOLIST> lst = new List<ADDINFOLIST>();
            try
            {
                lst = (from p in e.Elements(ns + "vesselInfo")
                       select new ADDINFOLIST
                       {
                           ADDINFO1 = (string)p.Element(ns + "callSign"),
                           ADDINFO2 = (string)p.Element(ns + "vesselType"),
                           ADDINFO3 = (string)p.Element(ns + "vesselFlag"),
                           ADDINFO4 = (string)p.Element(ns + "vesselOwner"),
                           ADDINFO5 = (string)p.Element(ns + "tonnage"),
                           ADDINFO6 = (string)p.Element(ns + "grossRegisteredTonnage")
                       }).ToList();
                if (lst.Count > 0)
                {
                    return lst.ToList();
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            { throw; }
        }
    }
}
