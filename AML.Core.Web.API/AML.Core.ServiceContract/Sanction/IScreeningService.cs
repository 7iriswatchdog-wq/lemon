using AML.DTO.DTO.CustomerScreening;
using AML.DTO.DTO.Sanction;

namespace AML.Core.ServiceContract.Sanction
{
    public interface IScreeningService
    {
        CustomerScreeningRS BlackListSearch(ScreeningSearchDTO model);
    }
}
