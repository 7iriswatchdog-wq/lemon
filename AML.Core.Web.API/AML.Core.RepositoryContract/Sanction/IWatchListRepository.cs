using AML.Core.Common.StaticResource;
using AML.DTO.DTO.Sanction;
using System;
using System.Collections.Generic;
using System.Text;

namespace AML.Core.RepositoryContract.Sanction
{
    public interface IWatchListRepository: IBaseRepository
    {
        ServiceResponse<int> Create(WatchListDTO _watchListDTO);
        ServiceResponse<List<WatchListDTO>> GetAll();
    }
}
