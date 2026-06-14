using AML.DTO.DTO.CustomerCase;
using AML.DTO.DTO.Kyc;
using System;
using System.Collections.Generic;
using System.Text;

namespace AML.Core.ServiceContract.CustomerMaster
{
    public interface ICustomerMasterService : IBaseService
    {
        IEnumerable<CustomerMasterDTO> GetPaginatedCustomerMaster(int pgIndex, int pageSize, string sortCol, string s_dir, string filterTxt,int clientId);
        CustomerMasterDTO GetDetailsById(int Id);

        List<CustomerMasterDTO> GetDetailsBySearch(string Id,string cust_type, int clientId);
        int Insert(CustomerMasterDTO model);
        int Update(CustomerMasterDTO model);

        int UpdateCustomerMaster(CustomerMasterDTO model);
        int UpdateWhiteList(int id, string isWhiteListed);
        void UpdateGroupInfo(int numericId, string groupId, string groupRisk, int isDuplicate = 0, string groupEntityOf = null);
        IEnumerable<CustomerCaseDTO> GetAllGroupEntities(int clientId);
        int InsertCustomerWhiteListLogs(int customerMasterID, string customerID, int createdBy, string isWhiteListed);
        int Delete(string Id);
        int Undelete(string Id);
        KycIndividualDTO GetAll(string CustomerId);
        CorporateKycDTO GetAllCorporate(string CustomerId);
        List<GroupEntityDTO> GetAllGroupEntities(string CustomerId);
        List<PersonDetailsDTO> GetAllPartners(string CustomerId);
        List<PersonDetailsDTO> GetAllSeniorManagement(string CustomerId);
        List<PersonDetailsDTO> GetAllSignatories(string CustomerId);
    }
}
