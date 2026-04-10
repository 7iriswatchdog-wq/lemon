using AML.Core.Common.StaticResource;
using AML.DTO.DTO.Common;
using AML.DTO.DTO.CustomerCase;
using AML.DTO.DTO.EtlBatch;
using AML.DTO.DTO.Kyc;
using AML.DTO.DTO.Risk;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace AML.Core.RepositoryContract.CustomerCase
{
    public interface ICustomerMasterRepository: IBaseRepository
    {
        ServiceResponse<string> Create(CustomerMasterDTO _CustomerMasterDTO);

        ServiceResponse<string> CreateScreening(CustomerMasterDTO _CustomerMasterDTO);

        ServiceResponse<string> GetCustomerId(string CustomerId);


        ServiceResponse<string> CreatePrefix(CustomerMasterDTO _CustomerMasterDTO);
        ServiceResponse<CustomerMasterDTO> GetDetailsById(int Id);

        ServiceResponse<List<CustomerMasterDTO>> GetDetailsBySearch(string searchterm, string cust_type, int clientId);
        ServiceResponse<List<CustomerMasterDTO>> GetCustomerMasterByCodePrefix(string prefix);
        ServiceResponse<List<CustomerMasterDTO>> GetCustomerMasterByCode(string customerType,int clientId);
        ServiceResponse<List<CustomerMasterDTO>> GetCustomerMasterByCodePrefixAndCode(string prefix, string code);
        ServiceResponse<int> Insert(CustomerMasterDTO _CustomerMasterDTO);
        ServiceResponse<int> Update(CustomerMasterDTO _CustomerMasterDTO);
        IEnumerable<CustomerMasterDTO> GetPaginatedCustomerMaster(int pgIndex, int pageSize, string sortCol, string s_dir, string filterTxt,int clientId);
        ServiceResponse<int> UpdateWhiteList(int id, string isWhiteListed);

        ServiceResponse<int> InsertCustomerWhiteListLogs(int customerMasterID, string customerID, int createdBy, string isWhiteListed);

        ServiceResponse<string> CreateCustomerMasterShareholder(CustomerMasterDTO _CustomerMasterDTO, string corporateID, int sharePercentage);
        ServiceResponse<int> Delete(string id);
        ServiceResponse<int> Undelete(string id);
        ServiceResponse<string> CreateGroupEntity(CustomerMasterDTO _CustomerMasterDTO, string CorporateType = null);
        ServiceResponse<string> CreateCustomerMasterShareholder(CustomerMasterDTO _CustomerMasterDTO, string corporateID, int sharePercentage, string Designation = null, string EmiratesId = null, string EmiratesIdExpiry = null, string PassportExpiry = null, string customerType = null);
        ServiceResponse<KycIndividualDTO> GetAll(string CustomerId);
        ServiceResponse<CorporateKycDTO> GetAllCorporate(string CustomerId);
        ServiceResponse<List<GroupEntityDTO>> GetAllGroupEntities(string CustomerId);
        ServiceResponse<List<PersonDetailsDTO>> GetAllPartners(string CustomerId);
        ServiceResponse<List<PersonDetailsDTO>> GetAllSeniorManagement(string CustomerId);
        ServiceResponse<List<PersonDetailsDTO>> GetAllSignatories(string CustomerId);
        //Risk Bulk Upload

        ServiceResponse<string> GetRiskTypeId(RiskBulkDTO _RiskBulkDTO);
        ServiceResponse<string> GetRisklovId(RiskBulkDTO _RiskBulkDTO);
        //ServiceResponse<string> CreateBulkrisk(RiskDTO _RiskDTO);

        ServiceResponse<int> CreateBulkrisk(RiskDTO _RiskDTO);

        ServiceResponse<int> CreateCorpCustomerBulkRisk(RiskCorpCustomerDTO _RiskDTO);

        ServiceResponse<string> CreateShareholdersData(ShareholderDTO _shareholderDTO);


        ServiceResponse<string> DeleteShareholders(int id);


        ServiceResponse<string> DeletePendingShareholders(string companyCode);
        ServiceResponse<string> UpdateParentId(int id, int? parentId);




    }
}
