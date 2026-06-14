using AML.Core.Common.StaticResource;
using AML.Core.RepositoryContract.CustomerCase;
using AML.Core.ServiceContract.CaseDocument;
using AML.DTO.DTO.CaseDocument;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace AML.Core.Service.CaseDocument
{
    public class CaseDocumentService : BaseService, ICaseDocumentService
    {
        ICaseDocumentRepository _CaseDocumentRepository;
        public CaseDocumentService(ICaseDocumentRepository CaseDocumentRepository, IConfiguration configuration,
            IHostingEnvironment environment) : base(CaseDocumentRepository, configuration)
        {
            _CaseDocumentRepository = CaseDocumentRepository;
        }

        public ServiceResponse<int> Create(CaseDocumentDTO _CaseDocumentDTO)
        {
            return _CaseDocumentRepository.Create(_CaseDocumentDTO);
        }

        public CaseDocumentDTO GetDetails(int Id)
        {
            //Perform business requirements here
            return _CaseDocumentRepository.GetDetails(Id).Result;
        }

        public List<CaseDocumentDTO> GetAll()
        {
            //Perform business requirements here
            return _CaseDocumentRepository.GetAll().Result;
        }

     

        public List<DocumentCategoryDTO> GetAllCategories()
        {
            //Perform business requirements here
            return _CaseDocumentRepository.GetAllCategories().Result;
        }
        public List<CaseDocumentDTO> GetCaseDocumentByCaseId(int caseId)
        {
            //Perform business requirements here
            return _CaseDocumentRepository.GetDetailsByCaseId(caseId).Result;
        }
        public List<DocumentTypeDTO> GetAllTypes()
        {
            //Perform business requirements here
            return _CaseDocumentRepository.GetAllTypes().Result;
        }

        public ServiceResponse<int> Update(CaseDocumentDTO _CaseDocumentDTO)
        {
            return _CaseDocumentRepository.Update(_CaseDocumentDTO);
        }
        public ServiceResponse<int> Delete(int Id)
        {
            return _CaseDocumentRepository.Delete(Id);
        }
    }
}
