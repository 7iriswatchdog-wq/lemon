using AML.Core.RepositoryContract.CustomerCase;
using AML.Core.ServiceContract.CustomerMaster;
using AML.DTO.DTO.CustomerCase;
using AML.DTO.DTO.Kyc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace AML.Core.Service.CustomerMaster
{
    public class CustomerMasterService: BaseService, ICustomerMasterService
    {
        ICustomerMasterRepository _customerMasterRepository;
        public CustomerMasterService(ICustomerMasterRepository customerMasterRepository, IConfiguration configuration) : base(customerMasterRepository, configuration)
        {
            _customerMasterRepository = customerMasterRepository;
        }

        public IEnumerable<CustomerMasterDTO> GetPaginatedCustomerMaster(int pgIndex, int pageSize, string sortCol, string s_dir, string filterTxt,int clientId)
        {
            return _customerMasterRepository.GetPaginatedCustomerMaster(pgIndex, pageSize, sortCol, s_dir, filterTxt,clientId);
        }

        public CustomerMasterDTO GetDetailsById(int Id)
        {
            return _customerMasterRepository.GetDetailsById(Id).Result;
        }
        public List<CustomerMasterDTO> GetDetailsBySearch(string searchterm, string cust_type, int clientId)
        {
            return _customerMasterRepository.GetDetailsBySearch(searchterm, cust_type, clientId).Result;
        }

        public int Insert(CustomerMasterDTO model)
        {
            if (model.CustomerType == "INDIVIDUAL")
            {
                model.CustomerType = "I";
            }
            else if (model.CustomerType == "CORPORATE")
            {
                model.CustomerType = "C";
            }
            else
            {
                model.CustomerType = "S";
            }

            return _customerMasterRepository.Insert(model).Result;
        }

        public int Update(CustomerMasterDTO model)
        {
            if (model.CustomerType == "INDIVIDUAL")
            {
                model.CustomerType = "I";
            }
            else if (model.CustomerType == "CORPORATE")
            {
                model.CustomerType = "C";
            }
            else
            {
                model.CustomerType = "S";
            }

            return _customerMasterRepository.Update(model).Result;
        }

        public int UpdateWhiteList(int id, string isWhiteListed)
        {
            return _customerMasterRepository.UpdateWhiteList(id, isWhiteListed).Result;
        }

        public int InsertCustomerWhiteListLogs(int customerMasterID, string customerID, int createdBy, string isWhiteListed)
        {
            return _customerMasterRepository.InsertCustomerWhiteListLogs(customerMasterID, customerID, createdBy, isWhiteListed).Result;
        }
        public int Delete(string id)
        {
            Console.WriteLine(string.Format("cms Id : {0}", id));
            return _customerMasterRepository.Delete(id).Result;
        }
        public int Undelete(string id)
        {
            Console.WriteLine(string.Format("cms Id : {0}", id));
            return _customerMasterRepository.Undelete(id).Result;
        }
        public KycIndividualDTO GetAll(string CustomerId)
        {

            return _customerMasterRepository.GetAll(CustomerId).Result;
        }
        public CorporateKycDTO GetAllCorporate(string CustomerId)
        {
            return _customerMasterRepository.GetAllCorporate(CustomerId).Result;
        }
        public List<GroupEntityDTO> GetAllGroupEntities(string CustomerId)
        {
            return _customerMasterRepository.GetAllGroupEntities(CustomerId).Result;
        }
        public List<PersonDetailsDTO> GetAllPartners(string CustomerId)
        {
            return _customerMasterRepository.GetAllPartners(CustomerId).Result;
        }
        public List<PersonDetailsDTO> GetAllSeniorManagement(string CustomerId)
        {
            return _customerMasterRepository.GetAllSeniorManagement(CustomerId).Result;
        }
        public List<PersonDetailsDTO> GetAllSignatories(string CustomerId)
        {
            return _customerMasterRepository.GetAllSignatories(CustomerId).Result;
        }
    }
}
