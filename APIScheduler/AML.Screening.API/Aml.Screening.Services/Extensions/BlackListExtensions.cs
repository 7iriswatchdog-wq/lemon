using Aml.Screening.DataContracts.Dtos;
using Aml.Screening.MongoModal.Modal;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace Aml.Screening.Services.Extensions
{
    public static class BlackListExtensions
    {
        public static BLACKLIST ToBlackListDto(this WatchList dto)
        {
            try
            {
                BLACKLIST modal = new BLACKLIST();
                modal.FULLNAME = dto.FULLNAME;
                if (!string.IsNullOrWhiteSpace(dto.NATIONALITY))
                {
                    List<NATIONALITYLIST> natList = new List<NATIONALITYLIST>();
                    NATIONALITYLIST nat = new NATIONALITYLIST()
                    {
                        NATCOUNTRY = dto.NATIONALITY.Trim().ToUpper()
                    };
                    natList.Add(nat);
                    modal.NATIONALITYLIST = natList;
                };
                if (!string.IsNullOrWhiteSpace(dto.DOB))
                {
                    List<DOBLIST> dobList = new List<DOBLIST>();
                    DOBLIST dob = new DOBLIST()
                    {
                        DOB = dto.DOB.Trim().ToUpper()
                    };
                    dobList.Add(dob);
                    modal.DOBLIST = dobList;
                };
                if (!string.IsNullOrWhiteSpace(dto.IDNUMBER))
                {
                    List<IDLIST> dobList = new List<IDLIST>();
                    IDLIST dob = new IDLIST()
                    {
                        IDNUMBER = dto.IDNUMBER.Trim().ToUpper()
                    };
                    dobList.Add(dob);
                    modal.IDLIST = dobList;
                };
                modal.CATEGORY = !string.IsNullOrWhiteSpace(dto.CATEGORY) ? dto.CATEGORY : string.Empty;
                modal.TYPE = !string.IsNullOrWhiteSpace(dto.TYPE) ? dto.TYPE : "NA";
                if(string.IsNullOrWhiteSpace(modal.UID))
                modal.UID = dto.TYPE+"-"+ Guid.NewGuid().ToString().Substring(0, 5);
                modal.FIRST_NAME = "";
                modal.MIDDLE_NAME = "";
                modal.LAST_NAME = "";
                modal.CREATEDON = DateTime.Now.ToString("dd-MM-yyyy");                
                modal.CLIENTID = dto.CLIENTID; 
                modal.REMARKS= dto.REMARKS;
                return modal;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}

