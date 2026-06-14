using AML.Core.Common.StaticResource;
using AML.DTO.DTO.CorporateShareholder;

namespace AML.Core.RepositoryContract.CorporateShareholder
{
    public interface ICorporateShareholderRepository
    {
        ServiceResponse<CorporateShareholderDTO> GetDetails(int Id);
        ServiceResponse<int> Create(CorporateShareholderDTO model);
        ServiceResponse<int> CreateShareHolder(CorporateShareholderDTO model);
        ServiceResponse<int> DeleteByCorporateID(string corporateID);
    }
}
