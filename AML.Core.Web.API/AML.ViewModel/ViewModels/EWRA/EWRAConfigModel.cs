using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;

namespace AML.ViewModel.ViewModels.EWRA
{
    public class EWRAConfigModel
    {
        public int ClientId { get; set; }
        public List<EWRAQuantitativeConfigModel> EWRAQuantitativeConfigModel { get; set; }
        public List<EWRAQualitativeConfigModel> EWRAQualitativeConfigModel { get; set; }
    }
    public class EWRAQuantitativeConfigModel
    {
        public int ClientId { get; set; }
        public int CustTypeId { get; set; }
        public string CustType { get; set; }
        public int CategoryId { get; set; }
        public int isActive { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public List<EWRACustomerType> CustomerType { get; set; }
        public List<EWRACustomerType> AvailableCustomerType { get; set; }
        public List<EWRACustomerType> CustomerTypeList { get; set; }
        public List<EWRACustomerType> AddedCustomerType { get; set; }
        public List<EWRACounterPartyType> EWRACounterPartyType { get; set; }
        public List<EWRACounterPartyList> AddedCounterparty { get; set; }
       
        public SelectList Nationalities { get; set; }
    }
    public class EWRAQualitativeConfigModel
    {
        public int ClientId { get; set; }
        public int RiskTypeId { get; set; }
        public string RiskType { get; set; }
        public SelectList EWRARiskTypeList { get; set; }
        public List<EWRARiskModel> EWRARiskModel { get; set; }
        public int DescriptionId { get; set; }
        public string RiskDescription { get; set; }
        public SelectList EWRARiskDescriptionList { get; set; }
        public List<RiskDescriptionData> RiskDescriptionData { get; set; }
        public List<InherentRiskModel> InherentRiskModelData { get; set; }
        public List<KeyControlModel> KeyControlModelData { get; set; }
        public List<ResidualRiskModel> ResidualRiskModelData { get; set; }
        public List<RiskDescriptionData> AvailableRiskDescription { get; set; }
        public List<RiskDescriptionData> AddedRiskDescription { get; set; }
        public List<InherentRiskModel> AvailableInherentRisk { get; set; }
        public List<InherentRiskModel> AddedInherentRisk { get; set; }
        public List<KeyControlModel> AvailableKey { get; set; }
        public List<KeyControlModel> AddedKey { get; set; }
        public List<ResidualRiskModel> AvailableResidualRisk { get; set; }
        public List<ResidualRiskModel> AddedResidualRisk { get; set; }
    }
}
