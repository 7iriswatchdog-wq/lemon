using AML.DTO.DTO.FreeSource;
using System.Collections.Generic;
using static AML.DTO.DTO.FreeSource.BlackListMongoDTO;

namespace AML.Core.RepositoryContract.InternalWatchList
{
    public interface IInternalWatchListMongoRepository
    {
        bool InsertBlockList(NAMELIST entity);
        List<NAMELIST> GetBlockListLogs(string startDate, string endDate, string source, int skip, int take, out int totalRecords);
    }
}
