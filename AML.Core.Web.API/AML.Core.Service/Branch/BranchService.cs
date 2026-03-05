using System;
using System.Collections.Generic;
using System.Text;
using AML.Core.ServiceContract.Branch;
using AML.Core.Common.StaticResource;
using AML.Core.RepositoryContract.Branch;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;
using AML.DTO.DTO.Branch;

namespace AML.Core.Service.Branch
{
    public class BranchService : BaseService, IBranchService
    {
        IBranchRepository _branchRepository;
        public BranchService(IBranchRepository branchRepository, IConfiguration configuration, IHostingEnvironment environment) : base(branchRepository, configuration)
        {
            _branchRepository = branchRepository;
        }

        public ServiceResponse<int> Create(BranchDTO _branchDTO)
        {
            //Perform business requirements here
            //_branchDTO.CreatedBy = 1; // Needto change this with loggedin User Id
            return _branchRepository.Create(_branchDTO);
        }

        public BranchDTO GetDetails(int Id)
        {
            //Perform business requirements here
            return _branchRepository.GetDetails(Id).Result;
        }

        public List<BranchDTO> GetAll(int clientId)
        {
            //Perform business requirements here
            return _branchRepository.GetAll(clientId).Result;
        }

        public ServiceResponse<int> Update(BranchDTO _branchDTO)
        {
            //Perform business requirements here
            _branchDTO.UpdatedBy = 1; // Needto change this with loggedin User Id
            return _branchRepository.Update(_branchDTO);
        }
        public ServiceResponse<int> Delete(int Id)
        {
            return _branchRepository.Delete(Id);
        }
    }
}
