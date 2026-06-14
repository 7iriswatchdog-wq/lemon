using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace AML.DTO.DTO.EWRA
{
    public class EWRAConfigModelDTO
    {
        public int ClientId { get; set; }
        public List<EWRAQuantitativeConfigModelDTO> EWRAQuantitativeConfigModel { get; set; }
        public List<EWRAQualitativeConfigModelDTO> EWRAQualitativeConfigModel { get; set; }
    }
    public class EWRAQuantitativeConfigModelDTO
    {
        public int ClientId { get; set; }
        public int CustTypeId { get; set; }
        public string CustType { get; set; }
        public int CategoryId { get; set; }
        public int isActive { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public List<EWRACustomerTypeDTO> CustomerType { get; set; }
        public List<EWRACustomerTypeDTO> AvailableCustomerType { get; set; }
        public List<EWRACustomerTypeDTO> CustomerTypeList { get; set; }
        public List<EWRACustomerTypeDTO> AddedCustomerType { get; set; }
        public List<EWRACounterPartyListDTO> AddedCounterparty { get; set; }
        public List<EWRACounterPartyListDTO> EWRACounterPartyType { get; set; }

    }
    public class EWRAQualitativeConfigModelDTO
    {
        [Column("RiskTypeId")]
        public int RiskTypeId { get; set; }
        [Column("RiskType")]
        public string RiskType { get; set; }
        //public SelectList EWRARiskTypeList { get; set; }
        public List<EWRARiskModelDTO> EWRARiskModel { get; set; }
        [Column("DescriptionId")]
        public int DescriptionId { get; set; }
        [Column("RiskDescription")]
        public string RiskDescription { get; set; }
        //public SelectList EWRARiskDescriptionList { get; set; }
        public List<RiskDescriptionDataDTO> RiskDescriptionData { get; set; }
        public List<InherentRiskModelDTO> InherentRiskModelData { get; set; }
        public List<KeyControlModelDTO> KeyControlModelData { get; set; }
        public List<ResidualRiskModelDTO> ResidualRiskModelData { get; set; }
        public List<RiskDescriptionDataDTO> AvailableRiskDescription { get; set; }
        public List<RiskDescriptionDataDTO> AddedRiskDescription { get; set; }
        public List<InherentRiskModelDTO> AvailableInherentRisk { get; set; }
        public List<InherentRiskModelDTO> AddedInherentRisk { get; set; }
        public List<KeyControlModelDTO> AvailableKey { get; set; }
        public List<KeyControlModelDTO> AddedKey { get; set; }
        public List<ResidualRiskModelDTO> AvailableResidualRisk { get; set; }
        public List<ResidualRiskModelDTO> AddedResidualRisk { get; set; }
        public int ClientId { get; set; }
    }
}
