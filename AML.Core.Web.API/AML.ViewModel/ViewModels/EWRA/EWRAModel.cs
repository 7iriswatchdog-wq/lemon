using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;

namespace AML.ViewModel.ViewModels.EWRA
{
    public class EWRAModel
    {
        public int Id { get; set; }
        public int Category { get; set; }
        public int Type { get; set; }
        public string CustVolumeScore { get; set; }
        public string CustCountScore { get; set; }
        public string CustTransactionScore { get; set; }
        public string CustOverallScore { get; set; }
        public string CustOverallRisk { get; set; }
        public string CounterOverallScore { get; set; }
        public string CounterOverallRisk { get; set; }
        public string ProductOverallScore { get; set; }
        public string ProductOverallRisk { get; set; }
        public string JurisdictionOverallScore { get; set; }
        public string JurisdictionOverallRisk { get; set; }
        public string DeliveryOverallScore { get; set; }
        public string DeliveryOverallRisk { get; set; }
        public string QuantitativeOverallScore { get; set; }
        public string QuantitativeOverallRisk { get; set; }
        public DateTime DateOfAssessment { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string CreatedDate { get; set; }
        public List<EWRACustomerType> EWRACustomerTypes { get; set; }
        public List<EWRACustomerVolume> EWRACustomerVolume { get; set; }
        public List<EWRACustomerCount> EWRACustomerCount { get; set; }
        public List<EWRACustomerTransaction> EWRACustomerTransaction { get; set; }
        public List<EWRAProductCategory> EWRAProductCategory { get; set; }
        public List<EWRACounterPartyType> EWRACounterPartyType { get; set; }
        public List<EWRADeliveryType> EWRADeliveryType { get; set; }
        public List<EWRAJurisdiction> EWRAJurisdiction { get; set; }
        public SelectList MainNationalityLists { get; set; }
        public SelectList QuantitativeList { get; set; }
        public string isVolume { get; set; }
        public string isCount { get; set; }
        public string isTransaction { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public List<EWRAQualitativeModel> EWRAQualitativeModel { get; set; }
        public string QualitativeOverallScore { get; set; }
        public string QualitativeOverallRating { get; set; }
        public string OverallScore { get; set; }
        public string OverallRisk { get; set; }
        public string QCustOverallScore { get; set; }
        public string QCustOverallRisk { get; set; }
        public string QCounterOverallScore { get; set; }
        public string QCounterOverallRisk { get; set; }
        public string QProductOverallScore { get; set; }
        public string QProductOverallRisk { get; set; }
        public string QJurisdictionOverallScore { get; set; }
        public string QJurisdictionOverallRisk { get; set; }
        public string QDeliveryOverallScore { get; set; }
        public string QDeliveryOverallRisk { get; set; }
        public int QId { get; set; }
        public string QCreatedBy { get; set; }
        public DateTime QCreatedOn { get; set; }
        public string QCreatedDate { get; set; }
        public int QuantitativeId { get; set; }
        public string QuantitativeName { get; set; }
        public int SaveType { get; set; }
        public List<EWRAProductList> EWRAProductList { get; set; }
        public int clientId { get; set; }
        //public int QID { get; set; }
    }
    public class EWRACustomerType
    {
        public int CustTypeId { get; set; }
        public string CustType { get; set; }
        public int CategoryId { get; set; }
        public int isActive { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }

        public bool isDeletedInUI { get; set; }
       
    }
    public class EWRACustomerVolume
    {
        public int CategoryId { get; set; }
        public int TypeId { get; set; }
        public string Type { get; set; }
        public string High { get; set; }
        public string Medium { get; set; }
        public string Low { get; set; }
        public string Total { get; set; }
        public string RiskScore { get; set; }
        public int Status { get; set; }
    }
    public class EWRACustomerCount
    {
        public int CategoryId { get; set; }
        public int TypeId { get; set; }
        public string Type { get; set; }
        public string High { get; set; }
        public string Medium { get; set; }
        public string Low { get; set; }
        public string Total { get; set; }
        public string RiskScore { get; set; }
        public int Status { get; set; }
    }
    public class EWRACustomerTransaction
    {
        public int CategoryId { get; set; }
        public int TypeId { get; set; }
        public string Type { get; set; }
        public string High { get; set; }
        public string Medium { get; set; }
        public string Low { get; set; }
        public string Total { get; set; }
        public string RiskScore { get; set; }
        public int Status { get; set; }
    }
    public class EWRAProductCategory
    {
        public int CategoryId { get; set; }
        public string Category { get; set; }
        public int isActive { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public List<EWRAProductList> EWRAProductList { get; set; }
    }
    public class EWRAProductList
    {
        public int ProductId { get; set; }
        public string Product { get; set; }
        public int isActive { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string High { get; set; }
        public string Medium { get; set; }
        public string Low { get; set; }
        public string Total { get; set; }
        public string RiskScore { get; set; }
        public int Status { get; set; }
    }
    public class EWRACounterPartyType
    {
        public int TypeId { get; set; }
        public string Type { get; set; }
        public int isActive { get; set; }
        public List<EWRACounterPartyList> EWRACounterPartyList { get; set; }
    }
    public class EWRACounterPartyList
    {
        public int Id { get; set; }
        public string CounterParty { get; set; }
        public int CountryId { get; set; }
        public string Country { get; set; }
        public string CountryRiskScore { get; set; }
        public int CountryRiskRating { get; set; }
        public string CountryRiskLevel { get; set; }
        public int FatfRiskRating { get; set; }
        public int isActive { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string selectedRisk { get; set; }
        public int selectedRiskId { get; set; }
        public string ExpenseAmount { get; set; }
        public string OverrideApplied { get; set; }
    }
    public class EWRADeliveryType
    {
        public int DeliveryTypeId { get; set; }
        public string DeliveryType { get; set; }
        public string Cash { get; set; }
        public string Bank { get; set; }
        public string RiskLevel { get; set; }
        public string RiskScore { get; set; }
    }
    public class EWRAJurisdiction
    {
        public int CountryId { get; set; }
        public string Country { get; set; }
        public string CountryRiskScore { get; set; }
        public string CountryRiskRating { get; set; }
        public int FatfRiskRating { get; set; }
        public string CountryRiskLevel { get; set; }
        public string LedgerAmount { get; set; }
        public int isActive { get; set; }
    }
    public class EWRAQualitativeModel
    {
        public int RiskTypeId { get; set; }
        public string RiskType { get; set; }
        public List<EWRARiskModel> EWRARiskModel { get; set; }
        //public List<InherentRiskModel> InherentRiskModel { get; set; }
        //public List<KeyControlModel> KeyControlModel { get; set; }
        //public List<ResidualRiskModel> ResidualRiskModel { get; set; }
        public string InherentImpactAvg { get; set; }
        public string InherentLikelihoodAvg { get; set; }
        public int InherentRiskScoreAvg { get; set; }
        public string InherentRiskRatingAvg { get; set; }
        public string ResidualImpactAvg { get; set; }
        public string ResidualLikelihoodAvg { get; set; }
        public int ResidualRiskScoreAvg { get; set; }
        public string ResidualRiskRatingAvg { get; set; }
        public int KeyControlScoreAvg { get; set; }
        public string KeyControlRatingAvg { get; set; }
        public string QualitativeOverallScore { get; set; }
        public string QualitativeOverallRating { get; set; }
        public string OverallScore { get; set; }
        public string OverallRisk { get; set; }
        public string QCustOverallScore { get; set; }
        public string QCustOverallRisk { get; set; }
        public string QCounterOverallScore { get; set; }
        public string QCounterOverallRisk { get; set; }
        public string QProductOverallScore { get; set; }
        public string QProductOverallRisk { get; set; }
        public string QJurisdictionOverallScore { get; set; }
        public string QJurisdictionOverallRisk { get; set; }
        public string QDeliveryOverallScore { get; set; }
        public string QDeliveryOverallRisk { get; set; }
        public string QCreatedBy { get; set; }
        public DateTime QCreatedOn { get; set; }
        public int QuantitativeId { get; set; }
        public string QuantitativeName { get; set; }
        public int QId { get; set; }
    }
    public class EWRARiskModel
    {
        public int DescriptionId { get; set; }
        public string RiskDescription { get; set; }
        public int IsActive { get; set; }
        public List<RiskDescriptionData> RiskDescriptionData { get; set; }
        public List<InherentRiskModel> InherentRiskModelData { get; set; }
        public List<KeyControlModel> KeyControlModelData { get; set; }
        public List<ResidualRiskModel> ResidualRiskModelData { get; set; }
    }
    public class RiskDescriptionData
    {
        public int DescriptionId { get; set; }
        public string RiskDescription { get; set; }
        public int IsActive { get; set; }
        public bool isDeletedInUI { get; set; }
        public int TypeId { get; set; }
    }
    public class InherentRiskModel
    {
        public int InherentRiskId { get; set; }
        public string InherentRiskDescription { get; set; }
        public int IsActive { get; set; }
        public int InherentImpactId { get; set; }
        public string InherentImpact { get; set; }
        public int InherentLikelihoodId { get; set; }
        public string InherentLikelihood { get; set; }
        public string InherentRiskRating { get; set; }
        public int InherentRiskScore { get; set; }
        public int TypeId { get; set; }
        public bool isDeletedInUI { get; set; }
    }
    public class KeyControlModel
    {
        public int KeyId { get; set; }
        public string KeyControl { get; set; }
        public int IsActive { get; set; }
        public int SelectedKeyId { get; set; }
        public string SelectedKey { get; set; }
        public string KeyRating { get; set; }
        public int KeyScore { get; set; }
        public int TypeId { get; set; }
        public bool isDeletedInUI { get; set; }
        public int KeyControlScoreId { get; set; }
        public string KeyControlScore { get; set; }
        public int ResidualRiskScore { get; set; }
    }
    public class ResidualRiskModel
    {
        public int ResidualRiskId { get; set; }
        public string ResidualRiskDescription { get; set; }
        public int IsActive { get; set; }
        public int ResidualImpactId { get; set; }
        public string ResidualImpact { get; set; }
        public int ResidualLikelihoodId { get; set; }
        public string ResidualLikelihood { get; set; }
        public string ResidualRiskRating { get; set; }
        public int ResidualRiskScore { get; set; }
        public int SelectedId { get; set; }
        public string SelectedKey { get; set; }
        public int TypeId { get; set; }
        public bool isDeletedInUI { get; set; }
    }
}
