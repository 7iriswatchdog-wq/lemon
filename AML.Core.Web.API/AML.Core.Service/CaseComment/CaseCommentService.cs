using AML.Core.Common.StaticResource;
using AML.Core.RepositoryContract.CustomerCase;
using AML.Core.ServiceContract.CaseComment;
using AML.DTO.DTO.CaseComment;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace AML.Core.Service.CaseComment
{
    public class CaseCommentService : BaseService, ICaseCommentService
    {
        ICaseCommentRepository _CaseCommentRepository;
        public CaseCommentService(ICaseCommentRepository CaseCommentRepository, IConfiguration configuration,
            IHostingEnvironment environment) : base(CaseCommentRepository, configuration)
        {
            _CaseCommentRepository = CaseCommentRepository;
        }

        public ServiceResponse<int> Create(CaseCommentDTO _CaseCommentDTO)
        {
            return _CaseCommentRepository.Create(_CaseCommentDTO);
        }

        public CaseCommentDTO GetDetails(int Id)
        {
            //Perform business requirements here
            return _CaseCommentRepository.GetDetails(Id).Result;
        }
        public List<CaseCommentDTO> GetAllByCase(int Id)
        {
            //Perform business requirements here
            return _CaseCommentRepository.GetAllByCase(Id).Result;
        }

        public List<CaseCommentDTO> GetAll()
        {
            //Perform business requirements here
            return _CaseCommentRepository.GetAll().Result;
        }

        public ServiceResponse<int> Update(CaseCommentDTO _CaseCommentDTO)
        {
            return _CaseCommentRepository.Update(_CaseCommentDTO);
        }
        public ServiceResponse<int> Delete(int Id)
        {
            return _CaseCommentRepository.Delete(Id);
        }
    }
}
