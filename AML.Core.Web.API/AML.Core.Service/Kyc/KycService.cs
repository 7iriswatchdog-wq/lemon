using AML.Core.Common.StaticResource;
using AML.Core.RepositoryContract.Kyc;
using AML.Core.ServiceContract.Kyc;
using AML.DTO.DTO.Common;
using AML.DTO.DTO.Kyc;
using System.Collections.Generic;

namespace AML.Core.Service.Kyc
{
    public class KycService : IKycService
    {
        private IKycRepository _kycRepository;
        public KycService(IKycRepository kycRepository)
        {
            _kycRepository = kycRepository;
        }
        public ServiceResponse<int> CreateCorporate(CorporateKycDTO kycCorporate)
        {
            return _kycRepository.CreateCorporate(kycCorporate);
        }
        public ServiceResponse<int> CreateIndividual(KycIndividualDTO kycIndividual)
        {
            return _kycRepository.CreateIndividual(kycIndividual);
        }
        public ServiceResponse<string> GetRiskTypeId(KycIndividualDTO kycIndividual, CorporateKycDTO kycCorporate, string Type,string culture, int ClientIds)
        {
            return _kycRepository.GetRiskTypeId(kycIndividual, kycCorporate, Type,culture, ClientIds);

        }
        public ServiceResponse<string> GetRiskLovId(KycIndividualDTO kycIndividual, CorporateKycDTO kycCorporate, string Type, string culture, int ClientIds)
        {
            return _kycRepository.GetRiskLovId(kycIndividual, kycCorporate, Type, culture, ClientIds);

        }
        public List<BusinessNatureDTO> GetBusinessType(string culture, string CustomerType, int ClientId)
        {
            return _kycRepository.GetBusinessType(culture, CustomerType, ClientId).Result;
        }
        public List<ProductTypeDTO> GetAllProduct(string culture, string CustomerType, int ClientId)
        {
            return _kycRepository.GetAllProduct(culture, CustomerType, ClientId).Result;
        }
        public List<DeliveryChannelDTO> GetAllDeliveryChannel(string culture, string CustomerType, int ClientId)
        {
            return _kycRepository.GetAllDeliveryChannel(culture, CustomerType, ClientId).Result;
        }

        public List<DeliveryChannelDTO> GetProfessionalStatus(string culture, string CustomerType, int ClientId)
        {
            return _kycRepository.GetProfessionalStatus(culture, CustomerType, ClientId).Result;
        }

        public List<LegalStatusDTO> GetLegalStatus(string culture, string CustomerType, int ClientId)
        {
            return _kycRepository.GetLegalStatus(culture, CustomerType, ClientId).Result;
        }

        public List<DeliveryChannelDTO> get_all_mode_of_payment(string culture, int ClientId, string CategoryType)
        {
            return _kycRepository.get_all_mode_of_payment(culture, ClientId, CategoryType).Result;
        }

        public List<DeliveryChannelDTO> get_all_residence_status(string culture, int ClientId, string CategoryType)
        {
            return _kycRepository.get_all_residence_status(culture, ClientId, CategoryType).Result;
        }

        public ClientMenuRightsModelDTO GetMenuRightsByClientId(int ClientIds)
        {
            //Perform business requirements here
            return _kycRepository.GetMenuRightsByClientId(ClientIds).Result;
        }
    }
}
