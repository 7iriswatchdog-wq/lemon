using AML.Core.Common.StaticResource;
using AML.DTO.DTO.FreeSource;
using AML.DTO.DTO.Sanction;
using AML.ViewModel.ViewModels.Sanction;
using System;
using System.Collections.Generic;
using System.Text;

namespace AML.Core.ServiceContract.Sanction
{
    public interface ISanctionService
    {
        ServiceResponse<int> Create(WatchListDTO model);
        List<WatchListDTO> GetAll();
    }
}
