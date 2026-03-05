using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace AML.DTO.DTO.EWRA
{
    public class EWRAModelDTO
    {
        [Column("Id")]
        public int Id { get; set; }
        public int Category { get; set; }
        public int Type { get; set; }
        [Column("Cust_Volume_Score")]
        public string CustVolumeScore { get; set; }
        [Column("Cust_Count_Score")]
        public string CustCountScore { get; set; }
        [Column("Cust_Transaction_Score")]
        public string CustTransactionScore { get; set; }
        [Column("Cust_Overall_Score")]
        public string CustOverallScore { get; set; }
        [Column("Cust_Overall_Risk")]
        public string CustOverallRisk { get; set; }
        [Column("Counter_Overall_Score")]
        public string CounterOverallScore { get; set; }
        [Column("Counter_Overall_Risk")]
        public string CounterOverallRisk { get; set; }
        [Column("Products_Overall_Score")]
        public string ProductOverallScore { get; set; }
        [Column("Products_Overall_Risk")]
        public string ProductOverallRisk { get; set; }
        [Column("Jurisdiction_Overall_Score")]
        public string JurisdictionOverallScore { get; set; }
        [Column("Jurisdiction_Overall_Risk")]
        public string JurisdictionOverallRisk { get; set; }
        [Column("Delivery_Overall_Score")]
        public string DeliveryOverallScore { get; set; }
        [Column("Delivery_Overall_Risk")]
        public string DeliveryOverallRisk { get; set; }
        [Column("Overall_Score")]
        public string QuantitativeOverallScore { get; set; }
        [Column("Overall_Risk")]
        public string QuantitativeOverallRisk { get; set; }
        public DateTime DateOfAssessment { get; set; }
        [Column("CreatedBy")]
        public string CreatedBy { get; set; }
        [Column("CreatedOn")]
        public DateTime CreatedOn { get; set; }
        [Column("CreatedDate")]
        public string CreatedDate { get; set; }
        public List<EWRACustomerTypeDTO> EWRACustomerTypes { get; set; }
        public List<EWRACustomerVolumeDTO> EWRACustomerVolume { get; set; }
        public List<EWRACustomerCountDTO> EWRACustomerCount { get; set; }
        public List<EWRACustomerTransactionDTO> EWRACustomerTransaction { get; set; }
        public List<EWRAProductCategoryDTO> EWRAProductCategory { get; set; }
        public List<EWRACounterPartyTypeDTO> EWRACounterPartyType { get; set; }
        public List<EWRADeliveryTypeDTO> EWRADeliveryType { get; set; }
        public List<EWRAJurisdictionDTO> EWRAJurisdiction { get; set; }
        //public SelectList MainNationalityLists { get; set; }
        //public SelectList QuantitativeList { get; set; }
        public string isVolume { get; set; }
        public string isCount { get; set; }
        public string isTransaction { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public List<EWRAProductListDTO> EWRAProductList { get; set; }
        public List<EWRAQualitativeModelDTO> EWRAQualitativeModel { get; set; }
        [Column("QualitativeOverallScore")]
        public string QualitativeOverallScore { get; set; }
        [Column("QualitativeOverallRating")]
        public string QualitativeOverallRating { get; set; }
        [Column("Overall_Score")]
        public string OverallScore { get; set; }
        [Column("Overall_Risk")]
        public string OverallRisk { get; set; }
        [Column("QCustOverallScore")]
        public string QCustOverallScore { get; set; }
        [Column("QCustOverallRisk")]
        public string QCustOverallRisk { get; set; }
        [Column("QCounterOverallScore")]
        public string QCounterOverallScore { get; set; }
        [Column("QCounterOverallRisk")]
        public string QCounterOverallRisk { get; set; }
        [Column("QProductOverallScore")]
        public string QProductOverallScore { get; set; }
        [Column("QProductOverallRisk")]
        public string QProductOverallRisk { get; set; }
        [Column("QJurisdictionOverallScore")]
        public string QJurisdictionOverallScore { get; set; }
        [Column("QJurisdictionOverallRisk")]
        public string QJurisdictionOverallRisk { get; set; }
        [Column("QDeliveryOverallScore")]
        public string QDeliveryOverallScore { get; set; }
        [Column("QDeliveryOverallRisk")]
        public string QDeliveryOverallRisk { get; set; }
        [Column("QId")]
        public int QId { get; set; }

        [Column("QCreatedBy")]
        public string QCreatedBy { get; set; }
        [Column("QCreatedOn")]
        public DateTime QCreatedOn { get; set; }

        [Column("QCreatedDate")]
        public string QCreatedDate { get; set; }
        [Column("QuantitativeId")]
        public int QuantitativeId { get; set; }
        [Column("QuantitativeName")]
        public string QuantitativeName { get; set; }
        public int SaveType { get; set; }
        public int clientId { get; set; }
    }
    public class EWRACustomerTypeDTO
    {
        [Column("TypeId")]
        public int CustTypeId { get; set; }
        [Column("Type")]
        public string CustType { get; set; }
        [Column("CategoryId")]
        public int CategoryId { get; set; }
        [Column("isActive")]
        public int isActive { get; set; }
        [Column("CreatedBy")]
        public int CreatedBy { get; set; }
        [Column("CreatedOn")]
        public DateTime CreatedOn { get; set; }
        public bool isDeletedInUI { get; set; }
        public int ClientId { get; set; }
    }

    public class EWRACustomerVolumeDTO
    {
        [Column("Category_Id")]
        public int CategoryId { get; set; }
        [Column("Type_Id")]
        public int TypeId { get; set; }
        [Column("Type")]
        public string Type { get; set; }
        [Column("High")]
        public string High { get; set; }
        [Column("Medium")]
        public string Medium { get; set; }
        [Column("Low")]
        public string Low { get; set; }
        [Column("Total")]
        public string Total { get; set; }
        [Column("Risk_Score")]
        public string RiskScore { get; set; }
        public int Status { get; set; }
    }
    public class EWRACustomerCountDTO
    {
        [Column("Category_Id")]
        public int CategoryId { get; set; }
        [Column("Type_Id")]
        public int TypeId { get; set; }
        [Column("Type")]
        public string Type { get; set; }
        [Column("High")]
        public string High { get; set; }
        [Column("Medium")]
        public string Medium { get; set; }
        [Column("Low")]
        public string Low { get; set; }
        [Column("Total")]
        public string Total { get; set; }
        [Column("Risk_Score")]
        public string RiskScore { get; set; }
        public int Status { get; set; }
    }
    public class EWRACustomerTransactionDTO
    {
        [Column("Category_Id")]
        public int CategoryId { get; set; }
        [Column("Type_Id")]
        public int TypeId { get; set; }
        [Column("Type")]
        public string Type { get; set; }
        [Column("High")]
        public string High { get; set; }
        [Column("Medium")]
        public string Medium { get; set; }
        [Column("Low")]
        public string Low { get; set; }
        [Column("Total")]
        public string Total { get; set; }
        [Column("Risk_Score")]
        public string RiskScore { get; set; }
        public int Status { get; set; }
    }
    public class EWRAProductCategoryDTO
    {
        [Column("Category_Id")]
        public int CategoryId { get; set; }
        [Column("Category")]
        public string Category { get; set; }
        [Column("isActive")]
        public int isActive { get; set; }
        [Column("Created_By")]
        public int CreatedBy { get; set; }
        [Column("Created_On")]
        public DateTime CreatedOn { get; set; }
        public List<EWRAProductListDTO> EWRAProductList { get; set; }
    }
    public class EWRAProductListDTO
    {
        [Column("Product_Id")]
        public int ProductId { get; set; }
        [Column("Product")]
        public string Product { get; set; }
        [Column("isActive")]
        public int isActive { get; set; }
        [Column("Created_By")]
        public int CreatedBy { get; set; }
        [Column("Created_On")]
        public DateTime CreatedOn { get; set; }
        [Column("High")]
        public string High { get; set; }
        [Column("Medium")]
        public string Medium { get; set; }
        [Column("Low")]
        public string Low { get; set; }
        [Column("Total")]
        public string Total { get; set; }
        [Column("Risk_Score")]
        public string RiskScore { get; set; }
        public int Status { get; set; }
    }
    public class EWRACounterPartyTypeDTO
    {
        [Column("Type_Id")]
        public int TypeId { get; set; }
        [Column("Type")]
        public string Type { get; set; }
        [Column("isActive")]
        public int isActive { get; set; }
        public List<EWRACounterPartyListDTO> EWRACounterPartyList { get; set; }
    }
    public class EWRACounterPartyListDTO
    {
        [Column("Id")]
        public int Id { get; set; }
        [Column("Counter_party")]
        public string CounterParty { get; set; }
        [Column("country_id")]
        public int CountryId { get; set; }
        [Column("country")]
        public string Country { get; set; }
        [Column("RiskScore")]
        public string CountryRiskScore { get; set; }
        [Column("RiskRating")]
        public int CountryRiskRating { get; set; }
        [Column("Country_Risk_Level")]
        public string CountryRiskLevel { get; set; }
        [Column("FATFRiskRating")]
        public int FatfRiskRating { get; set; }
        [Column("isActive")]
        public int isActive { get; set; }
        [Column("Created_By")]
        public int CreatedBy { get; set; }
        [Column("Created_On")]
        public DateTime CreatedOn { get; set; }
        [Column("EDD_Risk")]
        public string selectedRisk { get; set; }
        [Column("EDD_Risk_Id")]
        public int selectedRiskId { get; set; }
        [Column("Exp_Amount")]
        public string ExpenseAmount { get; set; }
        [Column("Override")]
        public string OverrideApplied { get; set; }
        public List<EWRACounterPartyListDTO> EWRACounterPartyList { get; set; }
        public int ClientId { get; set; }
    }
    public class EWRADeliveryTypeDTO
    {
        [Column("Delivery_Type_Id")]
        public int DeliveryTypeId { get; set; }
        [Column("Delivery_Type")]
        public string DeliveryType { get; set; }
        [Column("Cash")]
        public string Cash { get; set; }
        [Column("Bank")]
        public string Bank { get; set; }
        [Column("Risk_Level")]
        public string RiskLevel { get; set; }
        [Column("Risk_Score")]
        public string RiskScore { get; set; }
    }
    public class EWRAJurisdictionDTO
    {
        [Column("country_id")]
        public int CountryId { get; set; }
        [Column("country")]
        public string Country { get; set; }
        [Column("RiskScore")]
        public string CountryRiskScore { get; set; }
        [Column("RiskRating")]
        public string CountryRiskRating { get; set; }
        [Column("RiskLevel")]
        public string CountryRiskLevel { get; set; }
        [Column("Exp_Amount")]
        public string LedgerAmount { get; set; }
        [Column("FATFRiskRating")]
        public int FatfRiskRating { get; set; }
        [Column("isActive")]
        public int isActive { get; set; }
    }
    public class EWRAQualitativeModelDTO
    {
        [Column("RiskTypeId")]
        public int RiskTypeId { get; set; }
        [Column("RiskType")]
        public string RiskType { get; set; }
        public List<EWRARiskModelDTO> EWRARiskModel { get; set; }
        //public List<InherentRiskModelDTO> InherentRiskModel { get; set; }
        //public List<KeyControlModelDTO> KeyControlModel { get; set; }
        //public List<ResidualRiskModelDTO> ResidualRiskModel { get; set; }
        [Column("InherentImpactAvg")]
        public string InherentImpactAvg { get; set; }
        [Column("InherentLikelihoodAvg")]
        public string InherentLikelihoodAvg { get; set; }
        [Column("InherentRiskScoreAvg")]
        public int InherentRiskScoreAvg { get; set; }
        [Column("InherentRiskRatingAvg")]
        public string InherentRiskRatingAvg { get; set; }
        public string ResidualImpactAvg { get; set; }
        public string ResidualLikelihoodAvg { get; set; }
        [Column("ResidualRiskScoreAvg")]
        public int ResidualRiskScoreAvg { get; set; }
        [Column("ResidualRiskRatingAvg")]
        public string ResidualRiskRatingAvg { get; set; }
        [Column("KeyControlScoreAvg")]
        public int KeyControlScoreAvg { get; set; }
        [Column("KeyControlRatingAvg")]
        public string KeyControlRatingAvg { get; set; }
        [Column("QualitativeOverallScore")]
        public string QualitativeOverallScore { get; set; }
        [Column("QualitativeOverallRating")]
        public string QualitativeOverallRating { get; set; }
        [Column("OverallScore")]
        public string OverallScore { get; set; }
        [Column("OverallRisk")]
        public string OverallRisk { get; set; }
        [Column("QCustOverallScore")]
        public string QCustOverallScore { get; set; }
        [Column("QCustOverallRisk")]
        public string QCustOverallRisk { get; set; }
        [Column("QCounterOverallScore")]
        public string QCounterOverallScore { get; set; }
        [Column("QCounterOverallRisk")]
        public string QCounterOverallRisk { get; set; }
        [Column("QProductOverallScore")]
        public string QProductOverallScore { get; set; }
        [Column("QProductOverallRisk")]
        public string QProductOverallRisk { get; set; }
        [Column("QJurisdictionOverallScore")]
        public string QJurisdictionOverallScore { get; set; }
        [Column("QJurisdictionOverallRisk")]
        public string QJurisdictionOverallRisk { get; set; }
        [Column("QDeliveryOverallScore")]
        public string QDeliveryOverallScore { get; set; }
        [Column("QDeliveryOverallRisk")]
        public string QDeliveryOverallRisk { get; set; }
        [Column("QId")]
        public int QId { get; set; }

        [Column("QCreatedBy")]
        public string QCreatedBy { get; set; }
        [Column("QCreatedOn")]
        public DateTime QCreatedOn { get; set; }
        public int QuantitativeId { get; set; }
        public string QuantitativeName { get; set; }
    }
    public class EWRARiskModelDTO
    {
        [Column("DescriptionId")]
        public int DescriptionId { get; set; }
        [Column("RiskDescription")]
        public string RiskDescription { get; set; }
        [Column("IsActive")]
        public int IsActive { get; set; }
        public List<RiskDescriptionDataDTO> RiskDescriptionData { get; set; }
        public List<InherentRiskModelDTO> InherentRiskModelData { get; set; }
        public List<KeyControlModelDTO> KeyControlModelData { get; set; }
        public List<ResidualRiskModelDTO> ResidualRiskModelData { get; set; }
    }
    public class RiskDescriptionDataDTO
    {
        [Column("DescriptionId")]
        public int DescriptionId { get; set; }
        [Column("RiskDescription")]
        public string RiskDescription { get; set; }
        [Column("IsActive")]
        public int IsActive { get; set; }
        public bool isDeletedInUI { get; set; }
        public int TypeId { get; set; }
        public int ClientId { get; set; }
    }
    public class InherentRiskModelDTO
    {
        public int ClientId { get; set; }
        [Column("InherentRiskId")]
        public int InherentRiskId { get; set; }
        [Column("InherentRiskDescription")]
        public string InherentRiskDescription { get; set; }
        [Column("IsActive")]
        public int IsActive { get; set; }
        [Column("InherentImpactId")]
        public int InherentImpactId { get; set; }
        [Column("InherentImpact")]
        public string InherentImpact { get; set; }
        [Column("InherentLikelihoodId")]
        public int InherentLikelihoodId { get; set; }
        [Column("InherentLikelihood")]
        public string InherentLikelihood { get; set; }
        [Column("InherentRiskRating")]
        public string InherentRiskRating { get; set; }
        [Column("InherentRiskScore")]
        public int InherentRiskScore { get; set; }
        public int TypeId { get; set; }
        public bool isDeletedInUI { get; set; }
    }
    public class KeyControlModelDTO
    {
        public int ClientId { get; set; }
        [Column("KeyId")]
        public int KeyId { get; set; }
        [Column("KeyControl")]
        public string KeyControl { get; set; }
        [Column("IsActive")]
        public int IsActive { get; set; }
        [Column("SelectedKeyId")]
        public int SelectedKeyId { get; set; }
        [Column("SelectedKey")]
        public string SelectedKey { get; set; }
        [Column("KeyRating")]
        public string KeyRating { get; set; }
        public int KeyScore { get; set; }
        public int TypeId { get; set; }
        public bool isDeletedInUI { get; set; }

        [Column("Residualriskscore")]
        public string Residualriskscore { get; set; }
    }
    public class ResidualRiskModelDTO
    {
        public int ClientId { get; set; }
        [Column("ResidualRiskId")]
        public int ResidualRiskId { get; set; }
        [Column("ResidualRiskDescription")]
        public string ResidualRiskDescription { get; set; }
        [Column("IsActive")]
        public int IsActive { get; set; }
        public int ResidualImpactId { get; set; }
        public string ResidualImpact { get; set; }
        public int ResidualLikelihoodId { get; set; }
        public string ResidualLikelihood { get; set; }
        public string ResidualRiskRating { get; set; }
        public int ResidualRiskScore { get; set; }
        [Column("SelectedKeyId")]
        public int SelectedId { get; set; }
        [Column("SelectedKey")]
        public string SelectedKey { get; set; }
        public int TypeId { get; set; }
        public bool isDeletedInUI { get; set; }
    }
}
