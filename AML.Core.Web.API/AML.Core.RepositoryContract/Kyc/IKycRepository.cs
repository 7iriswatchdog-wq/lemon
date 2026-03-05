using AML.Core.Common.StaticResource;
using AML.DTO.DTO.Common;
using AML.DTO.DTO.Kyc;
using System.Collections.Generic;

namespace AML.Core.RepositoryContract.Kyc
{
    public interface IKycRepository
    {
        ServiceResponse<int> CreateCorporate(CorporateKycDTO kycCorporate);
        ServiceResponse<int> CreateIndividual(KycIndividualDTO kycIndividual);
        ServiceResponse<string> GetRiskTypeId(KycIndividualDTO kycIndividual, CorporateKycDTO kycCorporate, string Type,string culture, int ClientIds);
        ServiceResponse<List<BusinessNatureDTO>> GetBusinessType(string culture, string CustomerType, int ClientId);
        ServiceResponse<List<ProductTypeDTO>> GetAllProduct(string culture, string CustomerType, int ClientId);
        ServiceResponse<List<DeliveryChannelDTO>> GetAllDeliveryChannel(string culture, string CustomerType, int ClientId);

        ServiceResponse<List<DeliveryChannelDTO>> GetProfessionalStatus(string culture, string CustomerType, int ClientId);

        ServiceResponse<List<DeliveryChannelDTO>> get_all_mode_of_payment(string culture, int ClientId, string CategoryType);

        ServiceResponse<List<DeliveryChannelDTO>> get_all_residence_status(string culture, int ClientId, string CategoryType);
        ServiceResponse<List<LegalStatusDTO>> GetLegalStatus(string culture, string CustomerType, int ClientId);

        ServiceResponse<ClientMenuRightsModelDTO> GetMenuRightsByClientId(int ClientIds);
        ServiceResponse<string> GetRiskLovId(KycIndividualDTO kycIndividual, CorporateKycDTO kycCorporate, string Type, string culture, int ClientIds);


    }

}
