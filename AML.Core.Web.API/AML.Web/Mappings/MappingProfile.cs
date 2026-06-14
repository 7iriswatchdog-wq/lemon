using AML.DTO.DTO.Branch;
using AML.DTO.DTO.CaseAssignment;
using AML.DTO.DTO.CaseComment;
using AML.DTO.DTO.CaseDocument;
using AML.DTO.DTO.CodesMaster;
using AML.DTO.DTO.Common;
using AML.DTO.DTO.Country;
using AML.DTO.DTO.CustomerCase;
using AML.DTO.DTO.CustomerCategory;
using AML.DTO.DTO.Department;
using AML.DTO.DTO.Designation;
using AML.DTO.DTO.EtlBatch;
using AML.DTO.DTO.EWRA;
using AML.DTO.DTO.IdentityType;
using AML.DTO.DTO.InternalWatchListExcel;
using AML.DTO.DTO.Kyc;
using AML.DTO.DTO.LovMaster;
using AML.DTO.DTO.ProdMaster;
using AML.DTO.DTO.ProliferationFinance;
using AML.DTO.DTO.Report;
using AML.DTO.DTO.Risk;
using AML.DTO.DTO.RiskV2;
using AML.DTO.DTO.Sanction;
using AML.DTO.DTO.TransactionMonitor;
using AML.DTO.DTO.TransactionScreening;
using AML.DTO.DTO.User;
using AML.DTO.DTO.UserAccess;
using AML.DTO.DTO.UserGroup;
using AML.DTO.DTO.VisaType;
using AML.ViewModel.ViewModels.Branch;
using AML.ViewModel.ViewModels.CaseAssignment;
using AML.ViewModel.ViewModels.CaseComment;
using AML.ViewModel.ViewModels.CaseDocument;
using AML.ViewModel.ViewModels.CodesMaster;
using AML.ViewModel.ViewModels.Common;
using AML.ViewModel.ViewModels.Corporate;
using AML.ViewModel.ViewModels.Country;
using AML.ViewModel.ViewModels.CustomerCase;
using AML.ViewModel.ViewModels.CustomerCategory;
using AML.ViewModel.ViewModels.CustomerMaster;
using AML.ViewModel.ViewModels.Department;
using AML.ViewModel.ViewModels.Designation;
using AML.ViewModel.ViewModels.EtlBatch;
using AML.ViewModel.ViewModels.EWRA;
using AML.ViewModel.ViewModels.IdentityType;
using AML.ViewModel.ViewModels.InternalWathcList;
using AML.ViewModel.ViewModels.Kyc;
using AML.ViewModel.ViewModels.LovMasterModel;
using AML.ViewModel.ViewModels.ProductMaster;
using AML.ViewModel.ViewModels.ProliferationFinance;
using AML.ViewModel.ViewModels.Report;
using AML.ViewModel.ViewModels.Risk;
using AML.ViewModel.ViewModels.RiskAPI;
using AML.ViewModel.ViewModels.RiskV2;
using AML.ViewModel.ViewModels.Sanction;
using AML.ViewModel.ViewModels.TransactionMonitor;
using AML.ViewModel.ViewModels.TransactionScreening;
using AML.ViewModel.ViewModels.User;
using AML.ViewModel.ViewModels.UserAccess;
using AML.ViewModel.ViewModels.UserGroup;
using AML.ViewModel.ViewModels.VisaType;
using AML.Web.Controllers.Reports;
using AML.Core.ServiceContract.CaseStudio;
using AML.Core.Common.StaticResource;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AML.Web.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            ShouldMapField = fieldInfo => true;
            ShouldMapProperty = propertyInfo => true;
            CreateMap<DepartmentModel, DepartmentDTO>();
            CreateMap<DepartmentDTO, DepartmentModel>();
            CreateMap<BranchModel, BranchDTO>();
            CreateMap<BranchDTO, BranchModel>();
            CreateMap<DesignationModel, DesignationDTO>();
            CreateMap<DesignationDTO, DesignationModel>();
            CreateMap<UserGroupModel, UserGroupDTO>();
            CreateMap<UserGroupDTO, UserGroupModel>();
            CreateMap<VisaTypeModel, VisaTypeDTO>();
            CreateMap<VisaTypeDTO, VisaTypeModel>();
            CreateMap<IdentityTypeModel, IdentityTypeDTO>();
            CreateMap<IdentityTypeDTO, IdentityTypeModel>();
            CreateMap<CountryModel, CountryDTO>();
            CreateMap<CountryDTO, CountryModel>();
            CreateMap<UserModel, UserDTO>();
            CreateMap<UserDTO, UserModel>();
            CreateMap<UserDetailModel, UserDetailDTO>();
            CreateMap<UserDetailDTO, UserDetailModel>();
            CreateMap<UserGroupRightModel, UserGroupRightDTO>();
            CreateMap<UserGroupRightDTO, UserGroupRightModel>();
            CreateMap<UserGroupRightDetailsModel, UserGroupRightDetailsDTO>();
            CreateMap<UserGroupRightDetailsDTO, UserGroupRightDetailsModel>();
            CreateMap<ModuleModel, ModuleDTO>();
            CreateMap<ModuleDTO, ModuleModel>();
            CreateMap<FunctionalityModel, FunctionalityDTO>();
            CreateMap<FunctionalityDTO, FunctionalityModel>();
            
            // Global Date Converters to prevent "String was not recognized as a valid DateTime"
            CreateMap<string, DateTime>().ConvertUsing(s => AMLUtility.ParseDB(s) ?? new DateTime(1900, 1, 1));
            CreateMap<string, DateTime?>().ConvertUsing(s => AMLUtility.ParseDB(s));

            CreateMap<CaseModel, CustomerCaseDTO>()
                .ForMember(dest => dest.DOB, opt => opt.MapFrom(src => src.DOB.ParseDB().GetValueOrDefault()))
                .ForMember(dest => dest.PassportIssueDate, opt => opt.MapFrom(src => src.PassportIssueDate.ParseDB()))
                .ForMember(dest => dest.PassportExpiryDate, opt => opt.MapFrom(src => src.PassportExpiryDate.ParseDB()))
                .ForMember(dest => dest.EmiratesIdIssueDate, opt => opt.MapFrom(src => src.EmiratesIdIssueDate.ParseDB()))
                .ForMember(dest => dest.EmiratesIdExpiryDate, opt => opt.MapFrom(src => src.EmiratesIdExpiryDate.ParseDB()))
                .ForMember(dest => dest.EstablishmentDate, opt => opt.MapFrom(src => src.EstablishmentDate))
                .ForMember(dest => dest.UpdatedOnDB, opt => opt.MapFrom(src => src.UpdatedOnDB.ParseDB()));
            CreateMap<ScreenCaseModel, CustomerCaseDTO>();
            CreateMap<CustomerCaseDTO, CaseModel>()
                .ForMember(dest => dest.ParentId, opt => opt.MapFrom(src => src.ParentID))
                .ForMember(dest => dest.UpdatedOnSortKey, opt => opt.MapFrom(src => src.UpdatedOnDB ?? src.CreatedOnDB));
            CreateMap<CaseCommentDTO, CaseModel>()
                .ForMember(dest => dest.Comments, opt => opt.MapFrom(src => src.Comment))
                .ForMember(dest => dest.MatchType, opt => opt.MapFrom(src => src.CommentType))
                .ForMember(dest => dest.CreatedOn, opt => opt.MapFrom(src => src.CreatedOnDB ?? DateTime.MinValue));
            CreateMap<CustomerApiModel, CustomerCaseDTO>();
            CreateMap<CustomerCaseDTO, CustomerApiModel>();
            CreateMap<CustomerCaseApiModel, CustomerCaseDTO>();
            CreateMap<CustomerCaseDTO, CustomerCaseApiModel>();
            CreateMap<ETLReport, ETLDataLoadReportDTO>();
            CreateMap<ETLDataLoadReportDTO, ETLReport>();
            CreateMap<ETLDataLoadReportDTO, CaseModel>();
            CreateMap<CaseModel, ETLDataLoadReportDTO> ();
            CreateMap<EtlBatchDTO, EtlBatchModel>();
            CreateMap<EtlBatchModel, EtlBatchDTO> ();
            CreateMap<DocumentsModel, DocumentsDTO>();
            CreateMap<DocumentsDTO, DocumentsModel>();
            CreateMap<CustomerExcelData, CustomerExcelDTO>();
            CreateMap<CustomerExcelDTO, CustomerExcelData>();
            CreateMap<CustomerCategoryModel, CustomerCategoryDTO>();
            CreateMap<CustomerCategoryDTO, CustomerCategoryModel>();
            CreateMap<SanctionWatchModel, WatchListDTO>();
            CreateMap<WatchListDTO, SanctionWatchModel>();
            CreateMap<CaseAssignmentModel, CaseAssignmentDTO>();
            CreateMap<CaseAssignmentDTO, CaseAssignmentModel>();
            CreateMap<CaseCommentModel, CaseCommentDTO>();
            CreateMap<CaseCommentDTO, CaseCommentModel>();
            // DTO ? Model
            CreateMap<CaseDocumentDTO, CaseDocumentModel>()
                .ForMember(dest => dest.Document, opt => opt.Ignore()) // IFormFile does not exist in DTO
                .ForMember(dest => dest.CreatedOn, opt => opt.MapFrom(src => src.CreatedOnDB))
                .ForMember(dest => dest.IssuedDate, opt => opt.MapFrom(src => src.IssuedDateOnDB))
                .ForMember(dest => dest.ExpiryDate, opt => opt.MapFrom(src => src.ExpiryDateOnDB));

            // Model ? DTO (for saving/updating back to DB)
            CreateMap<CaseDocumentModel, CaseDocumentDTO>()
    .ForMember(dest => dest.CreatedOnDB, opt => opt.MapFrom(src => src.CreatedOn))
    .ForMember(dest => dest.IssuedDateOnDB, opt => opt.MapFrom(src => src.IssuedDate))
    .ForMember(dest => dest.ExpiryDateOnDB, opt => opt.MapFrom(src => src.ExpiryDate));


            CreateMap<DocumentTypeModel, DocumentTypeDTO>();
            CreateMap<DocumentTypeDTO, DocumentTypeModel>();
            CreateMap<DocumentCategoryModel, DocumentCategoryDTO>();
            CreateMap<DocumentCategoryDTO, DocumentCategoryModel>();
            CreateMap<CaseReportListModel, CaseReportListDTO>();
            CreateMap<CaseReportListDTO, CaseReportListModel>();
            CreateMap<ExcelData, InternalWatcListExcelDTO>(); 
            CreateMap<InternalWatcListExcelDTO, ExcelData>();
            CreateMap<UploadLogsListDTO, UploadLogsListModel>();
            CreateMap<UploadLogsListModel, UploadLogsListDTO>();

            CreateMap<DigiSchedulerLogModel, DigiSchedulerLogsDTO>();
            CreateMap<DigiSchedulerLogsDTO, DigiSchedulerLogModel>();

            CreateMap<CorporateScreeningModel, CorporateScreeningDTO>();
            CreateMap<CorporateScreeningDTO, CorporateScreeningModel>();

            CreateMap<CorporateDetailsModel, CorporateScreeningDTO>();
            CreateMap<CorporateScreeningDTO, CorporateDetailsModel>();

            CreateMap<CustomeDetailsModel, CustomeDetailsDTO>();
            CreateMap<CustomeDetailsDTO, CustomeDetailsModel>();
            CreateMap<RiskDTO, RiskModel>();
            CreateMap<RiskModel, RiskDTO>();
            CreateMap<RiskCorpCustomerDTO, RiskCorpCustomerModel>();
            CreateMap<RiskCorpCustomerModel, RiskCorpCustomerDTO>();

            CreateMap<RiskDTOV2, RiskModelV2>();
            CreateMap<RiskModelV2, RiskDTOV2>();
            CreateMap<RiskCorpCustomerDTOV2, RiskCorpCustomerModelV2>();
            CreateMap<RiskCorpCustomerModelV2, RiskCorpCustomerDTOV2>();

            CreateMap<LovMasterDTO, LovMasterModel>();
            CreateMap<LovMasterModel, LovMasterDTO>();
            CreateMap<RIskConfigurationMasterDTO, RIskConfigurationMasterModel>();
            CreateMap<RIskConfigurationMasterModel, RIskConfigurationMasterDTO>();
            CreateMap<RiskItemsDTO, RiskItemsModel>();
            CreateMap<RiskItemsModel, RiskItemsDTO>();
            CreateMap<RiskTypeCategoryDTO, RiskTypeCategoryModel>();
            CreateMap<RiskTypeCategoryModel, RiskTypeCategoryDTO>();

            CreateMap<RiskItemsDTOV2, RiskItemsModelV2>();
            CreateMap<RiskItemsModelV2, RiskItemsDTOV2>();
            CreateMap<RiskTypeCategoryDTOV2, RiskTypeCategoryModelV2>();
            CreateMap<RiskTypeCategoryModelV2, RiskTypeCategoryDTOV2>();

            CreateMap<RIskConfigurationMasterDTOV2, RIskConfigurationMasterModelV2>();
            CreateMap<RIskConfigurationMasterModelV2, RIskConfigurationMasterDTOV2>();

            CreateMap<RiskTypeDTO, RiskTypeModel>();
            CreateMap<RiskTypeModel, RiskTypeDTO>();

            CreateMap<RiskTypeDTOV2, RiskTypeModelV2>();
            CreateMap<RiskTypeModelV2, RiskTypeDTOV2>();


            CreateMap<RiskAssessmentBankModel, RiskAssessmentBankDTO>();
            CreateMap<RiskAssessmentBankDTO, RiskAssessmentBankModel>();
            CreateMap<RiskAssessmentVendorModel, RiskAssessmentVendorDTO>();
            CreateMap<RiskAssessmentVendorDTO, RiskAssessmentVendorModel>();

            CreateMap<RiskAssessmentBankModelV2, RiskAssessmentBankDTOV2>();
            CreateMap<RiskAssessmentBankDTOV2, RiskAssessmentBankModelV2>();
            CreateMap<RiskAssessmentVendorModelV2, RiskAssessmentVendorDTOV2>();
            CreateMap<RiskAssessmentVendorDTOV2, RiskAssessmentVendorModelV2>();

            CreateMap<SanctionScreeningModel, SanctionScreeningLogDTO>();
            CreateMap<SanctionScreeningLogDTO, SanctionScreeningModel>();

            CreateMap<SanctionScreeningLogModel, SanctionScreeningLogDTO>();
            CreateMap<SanctionScreeningLogDTO, SanctionScreeningLogModel>();
            CreateMap<CustomerMasterDTO, CustomerMasterModel>()
                .ForMember(dest => dest.IsDuplicate, opt => opt.MapFrom(src => src.IsDuplicate == 1));
            CreateMap<CustomerMasterModel, CustomerMasterDTO>()
                .ForMember(dest => dest.IsDuplicate, opt => opt.MapFrom(src => src.IsDuplicate ? 1 : 0));


            CreateMap<RiskReportModel, RiskReportDTO>();
            CreateMap<RiskReportDTO, RiskReportModel>();
            CreateMap<RiskReportModel, RiskReportDTO>();
            CreateMap<RiskReportDTO, RiskReportModel>();
            CreateMap<RiskSummaryDTO, RiskSummaryModel>();
            CreateMap<RiskSummaryModel, RiskSummaryDTO>();

            CreateMap<RiskReportModelV2, RiskReportDTOV2>();
            CreateMap<RiskReportDTOV2, RiskReportModelV2>();
            CreateMap<RiskSummaryDTOV2, RiskSummaryModelV2>();
            CreateMap<RiskSummaryModelV2, RiskSummaryDTOV2>();

            CreateMap<UserPasswordLogModel, UserPasswordLogModelDTO>();
            CreateMap<UserPasswordLogModelDTO, UserPasswordLogModel>(); 

                CreateMap<CorporateExcelData , CorporateExcelDTO>();
            CreateMap<CorporateExcelDTO, CorporateExcelData>();


            CreateMap<TMSCaseDTO, TMSCase>();
            CreateMap<TMSCase, TMSCaseDTO>();
            CreateMap<TMSCaseDTO, TMSCaseNewModel>();
            CreateMap<TMSCaseNewModel, TMSCaseDTO>();


            CreateMap<TMSRulesMasterDTO, TMSNewRulesModel>();
            CreateMap<TMSNewRulesModel, TMSRulesMasterDTO>();



            CreateMap<TMSCustomerTypeModelDTO, TMSCustomerTypeModel>();
            CreateMap<TMSCustomerTypeModel, TMSCustomerTypeModelDTO>();

            CreateMap<TMSDraftLogModelDTO, TMSDraftLogModel>();
            CreateMap<TMSDraftLogModel, TMSDraftLogModelDTO>();


            CreateMap<TMSTranTypeDTO, TMSTranType>();
            CreateMap<TMSTranType, TMSTranTypeDTO>();

            CreateMap<TMSRulesMasterDTO, TMSNewRulesModel>();
            CreateMap<TMSNewRulesModel, TMSRulesMasterDTO>();
            CreateMap<TMSRulesDetailsDTO, TMSRulesDetailsModel>();
            CreateMap<TMSRulesDetailsModel, TMSRulesDetailsDTO>();
            CreateMap<TMSNewRulesNamesDTO, TMSNewRulesNamesModel>();
            CreateMap<TMSNewRulesNamesModel, TMSNewRulesNamesDTO>();
            CreateMap<TMSFieldsmodelDTO, TMSFieldsNamesmodel>();
            CreateMap<TMSFieldsNamesmodel, TMSFieldsmodelDTO>();

            CreateMap<ProliferationFinanceCaseDTO, ProliferationFinanceModel>();
            CreateMap<ProliferationFinanceModel, ProliferationFinanceCaseDTO>();




            CreateMap<kycapimodel, CustomerCaseDTO>();
            CreateMap<CustomerCaseDTO, kycapimodel>();

            CreateMap<KycIndividualDTO, kycapimodel>();
            CreateMap<kycapimodel, KycIndividualDTO>();

            CreateMap<CorporateKycDTO, kycapimodel>();
            CreateMap<kycapimodel, CorporateKycDTO>();


            CreateMap<CorporateKycModel, CorporateKycDTO>();
            CreateMap<CorporateKycDTO, CorporateKycModel>();


            CreateMap<BranchKycDTO, Branch>();
            CreateMap<Branch, BranchKycDTO>();



            CreateMap<KycIndividualDTO, KycIndividualModel>();
            CreateMap<KycIndividualModel, KycIndividualDTO>();

            //CreateMap<PassportDetailsDTO, PassportDetails>();
            //CreateMap<PassportDetails, PassportDetailsDTO>(); 



            //CreateMap<RiskConfigResultModel, RiskConfigResultDTO>();
            //CreateMap<RiskConfigResultDTO, RiskConfigResultModel>();
            //CreateMap<RiskTypeResultModel, RiskTypeResultDTO>();
            //CreateMap<RiskTypeResultDTO, RiskTypeResultModel>();
            //CreateMap<RiskItemResultModel, RiskItemResultDTO>();
            //CreateMap<RiskItemResultDTO, RiskItemResultModel>();
            CreateMap<CorporateKycModel, CorporateKycDTO>();
            CreateMap<CorporateKycDTO, CorporateKycModel>();
            CreateMap<AddressDTO, Address>();
            CreateMap<Address, AddressDTO>();
            CreateMap<LicenseDTO, License>();
            CreateMap<License, LicenseDTO>();
            CreateMap<BranchKycDTO, Branch>();
            CreateMap<Branch, BranchKycDTO>();
            CreateMap<PersonDetailsDTO, PersonDetails>();
            CreateMap<PersonDetails, PersonDetailsDTO>();
            CreateMap<PEPDetailDTO, PEPDetail>();
            CreateMap<PEPDetail, PEPDetailDTO>();
            CreateMap<BankDetailDTO, BankDetail>();
            CreateMap<BankDetail, BankDetailDTO>();
            CreateMap<GroupEntityDTO, GroupEntity>();
            CreateMap<GroupEntity, GroupEntityDTO>();

            CreateMap<KycIndividualDTO, KycIndividualModel>();
            CreateMap<KycIndividualModel, KycIndividualDTO>();
            CreateMap<GroupEntityDTO, GroupEntity>();
            CreateMap<GroupEntity, GroupEntityDTO>();
            CreateMap<BusinessNature, BusinessNatureDTO>();
            CreateMap<BusinessNatureDTO, BusinessNature>();
            CreateMap<LegalStatusDTO, LegalStatusModel>();
            CreateMap<LegalStatusModel, LegalStatusDTO>();
            CreateMap<DeliveryChannelDTO, DeliveryChannel>();
            CreateMap<DeliveryChannel, DeliveryChannelDTO>();
            CreateMap<ProductType, ProductTypeDTO>();
            CreateMap<ProductTypeDTO, ProductType>();

            CreateMap<CorporateDetailsModel, CorporateKycDTO>();
            CreateMap<CorporateKycDTO, CorporateDetailsModel>();
            CreateMap<CustomeDetailsModel, KycIndividualDTO>();
            CreateMap<KycIndividualDTO, CustomeDetailsModel>();
            CreateMap<CustomerExcelData, KycIndividualDTO>();
            CreateMap<KycIndividualDTO, CustomerExcelData>();
            CreateMap<CorporateExcelData, CorporateKycDTO>();
            CreateMap<CorporateKycDTO, CorporateExcelData>();
            CreateMap<CaseModel, KycIndividualDTO>();
            CreateMap<KycIndividualDTO, CaseModel>();
            //CreateMap<ClientMasterDTO, CorporateKycModel>();
            //CreateMap<CorporateKycModel, ClientMasterDTO>();
            CreateMap<Menumodel, ClientMenuRightsModelDTO>();
            CreateMap<ClientMenuRightsModelDTO, Menumodel>();
            CreateMap<CustomeDetailsModel, KycIndividualDTO>();
            CreateMap<KycIndividualDTO, CustomeDetailsModel>();




            CreateMap<PendingTransactionsModel, PendingTransactionsDTO>();
            CreateMap<PendingTransactionsDTO, PendingTransactionsModel>();
            CreateMap<PendingCasesModel, PendingCasesDTO>();
            CreateMap<PendingCasesDTO, PendingCasesModel>();
            CreateMap<TranScreenDTO, TranScreenCaseModel>();
            CreateMap<TranScreenCaseModel, TranScreenDTO>();
            CreateMap<TransactionCaseDocumentModel, TransactionCaseDocumentDTO>();
            CreateMap<TransactionCaseDocumentDTO, TransactionCaseDocumentModel>();
            CreateMap<TransactionCaseCommentModel, TransactionCaseCommentDTO>();
            CreateMap<TransactionCaseCommentDTO, TransactionCaseCommentModel>();
            CreateMap<IsWhiteListedCheckModel, IsWhiteListedCheckDTO>();
            CreateMap<IsWhiteListedCheckDTO, IsWhiteListedCheckModel>();

            CreateMap<CustomerCaseDTO, CustomerCaseWithShareholderApiModel>();
            CreateMap<CustomerCaseWithShareholderApiModel, CustomerCaseDTO>();

            CreateMap<TranScreenDTO, ScreeningModel>();
            CreateMap<ScreeningModel, TranScreenDTO>();
            CreateMap<CodesTableModel, CodesTableDTO>();
            CreateMap<CodesTableDTO, CodesTableModel>();


            //EWRA

            CreateMap<EWRACustomerType, EWRACustomerTypeDTO>();
            CreateMap<EWRACustomerTypeDTO, EWRACustomerType>();

            CreateMap<EWRAModel, EWRAModelDTO>();
            CreateMap<EWRAModelDTO, EWRAModel>();

            CreateMap<EWRAModel, EWRAQualitativeModelDTO>();
            CreateMap<EWRAQualitativeModelDTO, EWRAModel>();


            CreateMap<EWRACustomerVolume, EWRACustomerVolumeDTO>();
            CreateMap<EWRACustomerVolumeDTO, EWRACustomerVolume>();

            CreateMap<EWRACustomerCount, EWRACustomerCountDTO>();
            CreateMap<EWRACustomerCountDTO, EWRACustomerCount>();

            CreateMap<EWRACustomerTransaction, EWRACustomerTransactionDTO>();
            CreateMap<EWRACustomerTransactionDTO, EWRACustomerTransaction>();

            CreateMap<EWRAProductCategory, EWRAProductCategoryDTO>();
            CreateMap<EWRAProductCategoryDTO, EWRAProductCategory>();

            CreateMap<EWRAProductList, EWRAProductListDTO>();
            CreateMap<EWRAProductListDTO, EWRAProductList>();

            CreateMap<EWRACounterPartyType, EWRACounterPartyTypeDTO>();
            CreateMap<EWRACounterPartyTypeDTO, EWRACounterPartyType>();

            CreateMap<EWRACounterPartyList, EWRACounterPartyListDTO>();
            CreateMap<EWRACounterPartyListDTO, EWRACounterPartyList>();

            CreateMap<EWRADeliveryType, EWRADeliveryTypeDTO>();
            CreateMap<EWRADeliveryTypeDTO, EWRADeliveryType>();

            CreateMap<EWRAJurisdiction, EWRAJurisdictionDTO>();
            CreateMap<EWRAJurisdictionDTO, EWRAJurisdiction>();

            CreateMap<EWRAQualitativeModel, EWRAQualitativeModelDTO>();
            CreateMap<EWRAQualitativeModelDTO, EWRAQualitativeModel>();

            CreateMap<EWRARiskModel, EWRARiskModelDTO>();
            CreateMap<EWRARiskModelDTO, EWRARiskModel>();

            CreateMap<EWRAConfigModel, EWRAConfigModelDTO>();
            CreateMap<EWRAConfigModelDTO, EWRAConfigModel>();

            CreateMap<EWRAQualitativeConfigModel, EWRAQualitativeConfigModelDTO>();
            CreateMap<EWRAQualitativeConfigModelDTO, EWRAQualitativeConfigModel>();

            CreateMap<EWRAQuantitativeConfigModel, EWRAQuantitativeConfigModelDTO>();
            CreateMap<EWRAQuantitativeConfigModelDTO, EWRAQuantitativeConfigModel>();

            CreateMap<InherentRiskModel, InherentRiskModelDTO>();
            CreateMap<InherentRiskModelDTO, InherentRiskModel>();

            CreateMap<KeyControlModel, KeyControlModelDTO>();
            CreateMap<KeyControlModelDTO, KeyControlModel>();

            CreateMap<ResidualRiskModel, ResidualRiskModelDTO>();
            CreateMap<ResidualRiskModelDTO, ResidualRiskModel>();
            CreateMap<EWRACounterPartyList, EWRAConfigModelDTO>();
            CreateMap<EWRAConfigModelDTO, EWRACounterPartyList>();

            CreateMap<EWRACounterPartyType, EWRACounterPartyListDTO>();
            CreateMap<EWRACounterPartyListDTO, EWRACounterPartyType>();
            CreateMap<RiskDescriptionData, RiskDescriptionDataDTO>();
            CreateMap<RiskDescriptionDataDTO, RiskDescriptionData>();

            CreateMap<ProdRiskConfigurationModel, ProdRiskConfigurationMasterDTO>();
            CreateMap<ProdRiskConfigurationMasterDTO, ProdRiskConfigurationModel>();

            CreateMap<ProdRiskTypeCategoryModel, ProdRiskTypeCategoryDTO>();
            CreateMap<ProdRiskTypeCategoryDTO, ProdRiskTypeCategoryModel>();

            CreateMap<ProdRiskItemsModel, ProdRiskItemsDTO>();
            CreateMap<ProdRiskItemsDTO, ProdRiskItemsModel>();

            CreateMap<ProductRiskModel, ProductRiskDTO>();
            CreateMap<ProductRiskDTO, ProductRiskModel>();

            CreateMap<ProductRiskReportModel, ProductRiskReportDTO>();
            CreateMap<ProductRiskReportDTO, ProductRiskReportModel>();

            CreateMap<TransactionMonitorAPIModel, TransactionMonitorAPIDTO>();
            CreateMap<TransactionMonitorAPIDTO, TransactionMonitorAPIModel>();

            CreateMap<TransactionMonitorAPIDTO, TMSNewMasterDTO>();
            CreateMap<TMSNewMasterDTO, TransactionMonitorAPIDTO>();

            CreateMap<clientSearchDTO, clientSearch>();
            CreateMap<clientSearch, clientSearchDTO>();

            CreateMap<ShareholderDTO, ShareholderModel>()
                .ForMember(dest => dest.IsDuplicate, opt => opt.MapFrom(src => src.IsDuplicate == 1));
            CreateMap<ShareholderModel, ShareholderDTO>()
                .ForMember(dest => dest.IsDuplicate, opt => opt.MapFrom(src => src.IsDuplicate ? 1 : 0));

            CreateMap<ShareholderDTO, CustomerCaseDTO>();
            CreateMap<CustomerCaseDTO, ShareholderDTO>();

            CreateMap<ShareholderModel, CustomerCaseDTO>();
            CreateMap<CustomerCaseDTO, ShareholderModel>();

            CreateMap<CustomerMasterDTO, CustomerCaseDTO>();
            CreateMap<CustomerCaseDTO, CustomerMasterDTO>();

            CreateMap<ScreeningDatabaseLogsModel, CaseReportRequestDTO>();
            CreateMap<CaseReportRequestDTO, ScreeningDatabaseLogsModel>();

            CreateMap<ScreeningDatabaseLogsModel, ScreeningDatabaseLogDTO>();
            CreateMap<ScreeningDatabaseLogDTO, ScreeningDatabaseLogsModel>();

            CreateMap<DatasetUpdateLogsModel, CaseReportRequestDTO>();
            CreateMap<CaseReportRequestDTO, DatasetUpdateLogsModel>();

            CreateMap<DatasetUpdateLogsModel, DatasetUpdateLogDTO>();
            CreateMap<DatasetUpdateLogDTO, DatasetUpdateLogsModel>();

            CreateMap<CaseStudioNode, CustomerCaseDTO>()
                .ForMember(dest => dest.IsWhiteListed, opt => opt.MapFrom(src => "NO"))
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CustomerType, opt => opt.MapFrom(src => (src.Type == "C" || src.Type == "corporate") ? "C" : "I"))
                .ForMember(dest => dest.MatchCategory, opt => opt.MapFrom(src => (src.Type == "C" || src.Type == "corporate" || (src.ShareholderType != null && (src.ShareholderType == "Corporate" || src.ShareholderType.EndsWith("_Corp")))) ? "CORPORATE" : "INDIVIDUAL"))
                .ForMember(dest => dest.FlagType, opt => opt.MapFrom(src => (src.IsRoot || src.IsMainEntity) ? "Main Entity" : AML.Web.Helpers.CaseStudioTypeHelper.GetFlagTypeShortcode(!string.IsNullOrEmpty(src.FlagType) ? src.FlagType : (src.Type == "C" || src.Type == "corporate" ? "CSH" : "ISH"))))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => (src.IsRoot || src.IsMainEntity) ? ((src.Type == "C" || src.Type == "corporate") ? "Corporate" : "Individual") : ((src.Type == "C" || src.Type == "corporate") ? "Corporate_Corp" : "Individual_Ind")))
                .ForMember(dest => dest.IsDelete, opt => opt.MapFrom(src => 0))
                .ForMember(dest => dest.Nationality, opt => opt.MapFrom(src => src.Nationality))
                .ForMember(dest => dest.DOB, opt => opt.MapFrom(src => (src != null ? src.Dob : null).ParseDB() ?? new DateTime(1900, 1, 1)))
                .ForMember(dest => dest.CustDOB, opt => opt.MapFrom(src => src != null ? src.Dob : string.Empty))
                .ForMember(dest => dest.Threshold, opt => opt.MapFrom(src => src != null ? src.MatchThreshold : 0))
                .ForMember(dest => dest.Designation, opt => opt.MapFrom(src => src != null ? src.Designation : string.Empty))
                .ForMember(dest => dest.ResidenceStatus, opt => opt.MapFrom(src => src != null ? src.Residence : string.Empty))
                .ForMember(dest => dest.Modeofpayment, opt => opt.MapFrom(src => src != null ? src.Modeofpayment : string.Empty))
                .ForMember(dest => dest.OccupatinTypeTxt, opt => opt.MapFrom(src => src != null ? src.Profession : string.Empty))
                .ForMember(dest => dest.BusinessType, opt => opt.MapFrom(src => src != null ? src.BusinessType : string.Empty))
                .ForMember(dest => dest.EntityTypeTxt, opt => opt.MapFrom(src => src != null ? src.EntityTypeTxt : string.Empty))
                .ForMember(dest => dest.SOWSOFCountry, opt => opt.MapFrom(src => src != null ? src.Sowsofcountry : string.Empty))
                .ForMember(dest => dest.Employer, opt => opt.MapFrom(src => src != null ? src.Employer : string.Empty))
                .ForMember(dest => dest.EmployerIndustry, opt => opt.MapFrom(src => src != null ? src.EmployerIndustry : string.Empty))
                .ForMember(dest => dest.EmployerSector, opt => opt.MapFrom(src => src != null ? src.EmployerSector : string.Empty))
                .ForMember(dest => dest.PassportId, opt => opt.MapFrom(src => src != null ? src.PassportId : string.Empty))
                .ForMember(dest => dest.EmiratesIdNumber, opt => opt.MapFrom(src => src != null ? src.EmiratesIdNumber : string.Empty))
                .ForMember(dest => dest.ProductRefNo, opt => opt.MapFrom(src => src != null ? src.ProductRefNo : string.Empty))
                .ForMember(dest => dest.ProductValue, opt => opt.MapFrom(src => (src != null && src.ProductValue.HasValue) ? src.ProductValue.Value.ToString() : "0"))
                .ForMember(dest => dest.CustomerIdType, opt => opt.MapFrom(src => src.SelectedIdTypes != null ? string.Join(", ", src.SelectedIdTypes) : (src.Type == "C" ? "License No" : "")))
                .ForMember(dest => dest.PassportIssueDate, opt => opt.MapFrom(src => src.PassportIssueDate))
                .ForMember(dest => dest.CustPassportIssueDate, opt => opt.MapFrom(src => src.PassportIssueDate))
                .ForMember(dest => dest.PassportExpiryDate, opt => opt.MapFrom(src => src.PassportExpiryDate))
                .ForMember(dest => dest.CustPassportExpiryDate, opt => opt.MapFrom(src => src.PassportExpiryDate))
                .ForMember(dest => dest.EmiratesIdIssueDate, opt => opt.MapFrom(src => src.EmiratesIdIssueDate))
                .ForMember(dest => dest.CustEmiratesIdIssueDate, opt => opt.MapFrom(src => src.EmiratesIdIssueDate))
                .ForMember(dest => dest.EmiratesIdExpiryDate, opt => opt.MapFrom(src => src.EmiratesIdExpiryDate))
                .ForMember(dest => dest.CustEmiratesIdExpiryDate, opt => opt.MapFrom(src => src.EmiratesIdExpiryDate))
                .ForMember(dest => dest.EstablishmentDate, opt => opt.MapFrom(src => src.RegistrationDate))
                .ForMember(dest => dest.Residence, opt => opt.MapFrom(src => src.Residence))
                .ForMember(dest => dest.PlaceOfBirth, opt => opt.MapFrom(src => src.PlaceOfBirth))
                .ForMember(dest => dest.GoldenVisa, opt => opt.MapFrom(src => src.GoldenVisa))
                .ForMember(dest => dest.CounterParty, opt => opt.MapFrom(src => src.CounterParty))
                .ForMember(dest => dest.CounterPartyName, opt => opt.MapFrom(src => src.CounterPartyName))
                .ForMember(dest => dest.CIFNumber, opt => opt.MapFrom(src => src.Cif))
                .ForMember(dest => dest.CustomerIdNumber, opt => opt.MapFrom(src => !string.IsNullOrEmpty(src.PassportId) ? src.PassportId : src.EmiratesIdNumber))
                .ForMember(dest => dest.Version, opt => opt.MapFrom(src => 1))
                .ForMember(dest => dest.Tradelicense, opt => opt.MapFrom(src => src.TradeLicence))
                .ForMember(dest => dest.TradeLicenseAuthority, opt => opt.MapFrom(src => src.TradeLicenseAuthority))
                .ForMember(dest => dest.ScreeningOptions, opt => opt.MapFrom(src => string.Empty))
                .ForMember(dest => dest.IsPep, opt => opt.MapFrom(src => src.ScreeningSources != null && (src.ScreeningSources.Contains("1") || src.ScreeningSources.Contains("PEP"))))
                .ForMember(dest => dest.IsSan, opt => opt.MapFrom(src => src.ScreeningSources != null && (src.ScreeningSources.Contains("1") || src.ScreeningSources.Contains("Sanction"))))
                .ForMember(dest => dest.IsRre, opt => opt.MapFrom(src => src.ScreeningSources != null && (src.ScreeningSources.Contains("1") || src.ScreeningSources.Contains("Reputational Risk Exposure"))))
                .ForMember(dest => dest.IsIns, opt => opt.MapFrom(src => src.ScreeningSources != null && (src.ScreeningSources.Contains("1") || src.ScreeningSources.Contains("Insolvency (UK & Ireland)"))))
                .ForMember(dest => dest.IsDd, opt => opt.MapFrom(src => src.ScreeningSources != null && (src.ScreeningSources.Contains("1") || src.ScreeningSources.Contains("Disqualified Director (UK Only)"))))
                .ForMember(dest => dest.IsPoi, opt => opt.MapFrom(src => src.ScreeningSources != null && (src.ScreeningSources.Contains("1") || src.ScreeningSources.Contains("Profile of Interest"))))
                .ForMember(dest => dest.IsRel, opt => opt.MapFrom(src => src.ScreeningSources != null && (src.ScreeningSources.Contains("1") || src.ScreeningSources.Contains("Regulatory Enforcement List"))))
                .ForMember(dest => dest.GroupId, opt => opt.MapFrom(src => src.GroupId))
                .ForMember(dest => dest.GroupRisk, opt => opt.MapFrom(src => src.GroupRisk))
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.ProductName))
                .ForMember(dest => dest.DeliveryChannelName, opt => opt.MapFrom(src => src.DeliveryChannel))
                .ForMember(dest => dest.Source, opt => opt.MapFrom(src => "KYC6"))
                .ForMember(dest => dest.IsWhiteListed, opt => opt.MapFrom(src => "NO"))
                .ForMember(dest => dest.CaseChangeStatus, opt => opt.MapFrom(src => string.Empty))
                .ForMember(dest => dest.IsDuplicate, opt => opt.MapFrom(src => src.IsDuplicate ? 1 : 0))
                .ForMember(dest => dest.CreatedOn, opt => opt.MapFrom(src => DateTime.Now));

            
            CreateMap<CaseStudioNode, KycIndividualDTO>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.Name ?? string.Format("{0} {1}", src.FirstName, src.LastName).Trim()))
                .ForMember(dest => dest.DOB, opt => opt.MapFrom(src => src.Dob))
                .ForMember(dest => dest.Nationality, opt => opt.MapFrom(src => src.Nationality))
                .ForMember(dest => dest.ResidenceStatus, opt => opt.MapFrom(src => src.Residence))
                .ForMember(dest => dest.PlaceOfBirth, opt => opt.MapFrom(src => src.PlaceOfBirth))
                .ForMember(dest => dest.OccupatinTypeTxt, opt => opt.MapFrom(src => src.Profession))
                .ForMember(dest => dest.EmployerName, opt => opt.MapFrom(src => src.Employer))
                .ForMember(dest => dest.CustomerIdType, opt => opt.MapFrom(src => !string.IsNullOrEmpty(src.PassportId) ? "Passport" : "Emirates ID"))
                .ForMember(dest => dest.CustomerIdNumber, opt => opt.MapFrom(src => !string.IsNullOrEmpty(src.PassportId) ? src.PassportId : src.EmiratesIdNumber))
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.ProductName))
                .ForMember(dest => dest.DeliveryChannelName, opt => opt.MapFrom(src => src.DeliveryChannel))
                .ForMember(dest => dest.ModeOfPayment, opt => opt.MapFrom(src => src.Modeofpayment));

            CreateMap<CaseStudioNode, CorporateKycDTO>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.DateofIncorporation, opt => opt.MapFrom(src => src.RegistrationDate))
                .ForMember(dest => dest.PlaceofIncorporation, opt => opt.MapFrom(src => src.Nationality))
                .ForMember(dest => dest.EntityTypeTxt, opt => opt.MapFrom(src => src.EntityTypeTxt))
                .ForMember(dest => dest.BusinessType, opt => opt.MapFrom(src => src.BusinessType))
                .ForMember(dest => dest.LicenseIssueDate, opt => opt.MapFrom(src => src.EmiratesIdIssueDate))
                .ForMember(dest => dest.LicenseExpiryDate, opt => opt.MapFrom(src => src.EmiratesIdExpiryDate))
                .ForMember(dest => dest.LicenseNumber, opt => opt.MapFrom(src => src.TradeLicence))
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.ProductName))
                .ForMember(dest => dest.DeliveryChannelName, opt => opt.MapFrom(src => src.DeliveryChannel))
                .ForMember(dest => dest.Modeofpayment, opt => opt.MapFrom(src => src.Modeofpayment));

        }
    }
}
