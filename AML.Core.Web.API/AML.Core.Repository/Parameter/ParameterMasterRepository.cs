using AML.Core.Common.StaticResource;
using AML.DTO.DTO.Parameter;
using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace AML.Core.Repository.Parameter
{
    public class ParameterMasterRepository : BaseRepository, IParameterMasterRepository
    {
        public ParameterMasterRepository(IConfiguration configuration, IHttpContextAccessor context) : base(configuration, context)
        {

        }

        //public ServiceResponse<List<ParamDTO>> GetAllParameter(int paraCode)
        //{
        //    ServiceResponse<List<ParamDTO>> serviceResponse = new ServiceResponse<List<ParamDTO>>();
        //    try
        //    {
        //        DynamicParameters parameters = new DynamicParameters();
        //    }
        //}
    }
}
