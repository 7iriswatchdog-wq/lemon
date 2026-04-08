using AML.Core.Common.StaticResource;
using AML.DTO.DTO.FreeSource;
using System;
using System.Collections.Generic;
using System.Text;
using static AML.DTO.DTO.FreeSource.BlackListMongoDTO;
using static AML.DTO.DTO.FreeSource.CaseLogsMongoDTO;
using static AML.DTO.DTO.FreeSource.TransactionCaseLogsMongoDTO;

namespace AML.Core.RepositoryContract.FreeSource
{
    public interface IFreeSourceRepository
    {
        ServiceResponse<int> Create(BLACKLIST dto);
        string ReadFile(string filePath);
        string InsertNameList(BLACKLIST per);
        bool InsertCaseLog(CASELOG model);
        bool InsertCaseLogUnderThreshold(CASELOG model);
        string GetDetailsByCaseId(int caseid);
        string GetPendingDetailsByCaseId(int caseid);
        bool UpdateCaseLogRemarks(CASELOG model);
        (bool exists, List<NAMELIST> response, List<NAMELIST> response2) SearchNameList(string name, bool v,int clientid);

        public List<NAMELIST> GetRecordsByCreatedDate(string createdDate,string type);
        //(bool exists, NAMELIST response, List<NAMELIST> response2) SearchFuzzylist(string name, bool v, int clientid);
        //Transaction Screening
        bool InsertTransactionCaseLog(TRANSACTION_CASELOG model);
        bool UpdateTranCaseLogRemarks(TRANSACTION_CASELOG model);


        //End Transaction Screening
    }
}
