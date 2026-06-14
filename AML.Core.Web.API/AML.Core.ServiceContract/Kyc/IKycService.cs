using AML.Core.Common.StaticResource;
using AML.DTO.DTO.Common;
using AML.DTO.DTO.Kyc;
using System.Collections.Generic;

namespace AML.Core.ServiceContract.Kyc
{
    public interface IKycService
    {
        ServiceResponse<int> CreateCorporate(CorporateKycDTO kycCorporate);
        ServiceResponse<int> CreateIndividual(KycIndividualDTO kycIndividual);
        List<BusinessNatureDTO> GetBusinessType(string culture, string CustomerType, int ClientId);
        List<ProductTypeDTO> GetAllProduct(string culture, string CustomerType, int ClientId);
        List<DeliveryChannelDTO> GetAllDeliveryChannel(string culture, string CustomerType, int ClientId);

        List<DeliveryChannelDTO> get_all_mode_of_payment(string culture, int ClientId, string CategoryType);

        List<DeliveryChannelDTO> get_all_residence_status(string culture, int ClientId, string CategoryType);

        List<DeliveryChannelDTO> GetProfessionalStatus(string culture, string CustomerType, int ClientId);

        List<LegalStatusDTO> GetLegalStatus(string culture, string CustomerType, int ClientId);

        ServiceResponse<string> GetRiskTypeId(KycIndividualDTO kycIndividual, CorporateKycDTO kycCorporate, string Type,string culture, int ClientIds);
        ServiceResponse<string> GetRiskLovId(KycIndividualDTO kycIndividual, CorporateKycDTO kycCorporate, string Type, string culture, int ClientIds);
        ClientMenuRightsModelDTO GetMenuRightsByClientId(int ClientIds);

    }
}
