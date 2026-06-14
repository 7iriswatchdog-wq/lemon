using AML.Core.Common.StaticResource;
using AML.ViewModel.ViewModels.FreeSource;
using static AML.DTO.DTO.FreeSource.BlackListMongoDTO;

namespace AML.Core.ServiceContract.FreeSource
{
    public interface IFreeSourceService
    {
        ServiceResponse<int> Create(BLACKLIST dto);
        string ReadFile(string FilePath, string Source);
    }
}
